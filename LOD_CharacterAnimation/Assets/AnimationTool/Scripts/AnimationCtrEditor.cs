using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(AnimationCtr))]
public class AnimationCtrEditor : Editor
{
    public VisualTreeAsset visualTree;
    private AnimationCtr _animCtr;
    public VisualElement etcPanel;

    private ObjectField clipField;
    private ObjectField objectField;
    private DropdownField functionDropdown;
    private Slider frameSlider;
    private Button startClipBtn;
    private Button stopClipBtn;
    private Button addEventBtn;
    private Button deleteEventBtn;
    private ListView eventListView;

    private AnimationClip animationClip;
    public AnimationClip AnimationClip { 
        get => animationClip; 
        set { animationClip = value; clipField.value = value; UpdateEventList(); } 
    }
    private bool isPlaying = false;
    private float startTime = 0;
    private float currentTime = 0;
    private float lastEventTime = -1f;

    public override VisualElement CreateInspectorGUI()
    {
        if (!visualTree)
        {
            return base.CreateInspectorGUI();
        }
        _animCtr = (AnimationCtr)target;

        VisualElement root = new VisualElement();
        visualTree.CloneTree(root);

        _animCtr.Init();

        etcPanel = root.Q<VisualElement>("EtcVE");

        if(_animCtr.anim != null)
        {
            CreateButtonsForAnimationClips(etcPanel);
        }

        root.Q<Button>("apply").clicked += ()=> { ResetButton(etcPanel); };

        //=============================================

        clipField = root.Q<ObjectField>("animationField");
        frameSlider = root.Q<Slider>("frameSlider");
        startClipBtn = root.Q<Button>("startBtn");
        stopClipBtn = root.Q<Button>("stopBtn");
        addEventBtn = root.Q<Button>("addEventBtn");
        deleteEventBtn = root.Q<Button>("deleteEventBtn");

        objectField = root.Q<ObjectField>("objectField");
        objectField.objectType = typeof(GameObject);
        objectField.RegisterValueChangedCallback(evt =>
        {
            if (evt.newValue != null)
            {
                UpdateFunctionList(evt.newValue as GameObject);
            }
        });

        functionDropdown = root.Q<DropdownField>("functionDropdown");

        eventListView = root.Q<ListView>("eventList");
        eventListView.makeItem = () => new Label();
        eventListView.bindItem = (element, i) =>
        {
            Label label = (Label)element;
            AnimationEvent evt = (AnimationEvent)eventListView.itemsSource[i];
            label.text = $"{evt.time}: {evt.functionName}";

            deleteEventBtn.clicked += () => { RemoveAnimationEvent(i);};
        };
        eventListView.selectionType = SelectionType.Single;
        //=============================================

        return root;
    }

    #region AnimationButton
    private void ResetButton(VisualElement _panel)
    {
        _animCtr.RemoveFirstChild();
        RemoveAllButtons(_panel);
        _animCtr.Init();
        CreateButtonsForAnimationClips(_panel);
    }

    private void RemoveAllButtons(VisualElement _panel)
    {
        if (_panel != null)
        {
            _panel.Clear();
        }
    }

    private void CreateButtonsForAnimationClips(VisualElement _panel)
    {
        if (_panel != null)
        {
            List<AnimationClip> clips = _animCtr.GetAllAnimationClips();
            foreach (var clip in clips)
            {
                Button btn = new Button();
                btn.style.width = 100;
                btn.style.height = 50;
                btn.text = clip.name;
                btn.style.fontSize = 18;
                btn.style.unityFontStyleAndWeight = FontStyle.Bold;
                btn.clicked += () => { ShowClipDetails(clip); };
                _panel.Add(btn);
            }
        }
    }
    #endregion

    private void ShowClipDetails(AnimationClip clip)
    {
        AnimationClip = _animCtr.GetAnimationClip(clip.name);
        frameSlider.lowValue = 0;
        frameSlider.highValue = clip.length * clip.frameRate;
        frameSlider.RegisterValueChangedCallback( evt => 
        {
            if(animationClip != null)
            {
                currentTime = evt.newValue / animationClip.frameRate;
                AnimationMode.SampleAnimationClip(_animCtr.anim.gameObject, animationClip, currentTime);

                if (!isPlaying)
                {
                    startTime = (float)EditorApplication.timeSinceStartup - currentTime;
                }
            }
        });

        startClipBtn.clicked += () => { PlayAnimation(); };
        stopClipBtn.clicked += () => { StopAnimation(); };
        addEventBtn.clicked += () => { AddAnimationEvent(clip, currentTime); };
    }


   #region AnimationClip
     private void PlayAnimation()
     {
 
         if (animationClip != null)
         {
             PlayAnimationClip(animationClip);
         }
         else
         {
             Debug.LogError("Animation clip not found: " + animationClip.name);
         }
     }
 
     private void PlayAnimationClip(AnimationClip clip)
     {
         AnimationMode.StartAnimationMode();
         AnimationMode.SampleAnimationClip(_animCtr.anim.gameObject, clip, 0f);
         isPlaying = true;
         startTime = (float)EditorApplication.timeSinceStartup - currentTime;
         lastEventTime = -1f;
         EditorApplication.update += UpdateAnimation;
     }
 
     private void StopAnimation()
     {
         if (isPlaying)
         {
             isPlaying = false;
             EditorApplication.update -= UpdateAnimation;
             currentTime = (float)(EditorApplication.timeSinceStartup - startTime) % animationClip.length;
         }
     }
 
     void UpdateAnimation()
     {
         if (AnimationMode.InAnimationMode() && isPlaying)
         {
             float previousTime = currentTime;
             currentTime = (float)(EditorApplication.timeSinceStartup - startTime) % animationClip.length;
 
             // 애니메이션이 한 바퀴를 돌았는지 확인
             if (currentTime < previousTime)
             {
                 lastEventTime = -1f; // 애니메이션이 한 바퀴를 돌면 lastEventTime 초기화
             }
 
             frameSlider.value = currentTime * animationClip.frameRate;
             AnimationMode.SampleAnimationClip(_animCtr.anim.gameObject, animationClip, currentTime);
 
             CheckAnimationEvents(currentTime);
         }
     }
   #endregion

    #region CallAnimationClipEvent
        private void CheckAnimationEvents(float time)
        {
            foreach (var animEvent in AnimationUtility.GetAnimationEvents(animationClip))
            {
                if (Mathf.Abs(animEvent.time - time) < 0.02f  && Mathf.Abs(animEvent.time - lastEventTime) > 0.02f) // 0.02f 는 시간 오차를 고려한 값입니다.
                {
                    CallAnimationEventFunction(animEvent.functionName);
                    lastEventTime = animEvent.time;
                }
            }
        }
    
        private void CallAnimationEventFunction(string functionName)
        {
            var components = _animCtr.anim.GetComponents<MonoBehaviour>();
            foreach (var component in components)
            {
                var method = component.GetType().GetMethod(functionName);
                if (method != null)
                {
                    method.Invoke(component, null);
                    return;
                }
            }
            Debug.LogWarning($"No method named {functionName} found on any components of {_animCtr.anim.gameObject.name}");
        }
    
        private void AddAnimationEvent(AnimationClip clip, float _frame)
        {
             if (clip != null && !string.IsNullOrEmpty(functionDropdown.value))
            {
                AnimationEvent animationEvent = new AnimationEvent
                {
                    time = _frame,
                    functionName = functionDropdown.value,
                };
                Debug.Log(animationEvent.functionName);
                List<AnimationEvent> events = new List<AnimationEvent>(AnimationUtility.GetAnimationEvents(clip));
    
                // Check for duplicates
                bool eventExists = events.Exists(evt => evt.time == animationEvent.time && evt.functionName == animationEvent.functionName);
                if (!eventExists)
                {
                    events.Add(animationEvent);
                    AnimationUtility.SetAnimationEvents(clip, events.ToArray());
                    Debug.Log("Event added to clip: " + clip.name);
                    UpdateEventList();
                }
                else
                {
                    Debug.LogWarning("Event already exists at this time with the same function name.");
                }
            }
            else
            {
                Debug.LogError("No animation clip selected or function name is empty.");
            }
        }
    #endregion

    #region Event Add / Delete
        private void UpdateEventList()
        {
            if (animationClip != null)
            {
                List<AnimationEvent> events = new List<AnimationEvent>(AnimationUtility.GetAnimationEvents(animationClip));
                eventListView.itemsSource = events;
                eventListView.Rebuild();
            }
        }
    
        private void RemoveAnimationEvent(int index)
        {
            if (animationClip != null)
            {
                List<AnimationEvent> events = new List<AnimationEvent>(AnimationUtility.GetAnimationEvents(animationClip));
                if (eventListView.selectedIndex >= 0 && eventListView.selectedIndex < events.Count)
                {
                    events.RemoveAt(eventListView.selectedIndex);
                    AnimationUtility.SetAnimationEvents(animationClip, events.ToArray());
                    UpdateEventList();
                }
            }
        }
    
        private void UpdateFunctionList(GameObject selectedObject)
        {
            List<string> functionNames = new List<string>();
    
            if (selectedObject != null)
            {
                var components = selectedObject.GetComponents<MonoBehaviour>();
                foreach (var component in components)
                {
                    var methods = component.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                    foreach (var method in methods)
                    {
                        if (method.ReturnType == typeof(void) && method.GetParameters().Length == 0)
                        {
                            functionNames.Add(method.Name);
                        }
                    }
                }
            }
    
            functionDropdown.choices = functionNames;
            if (functionNames.Count > 0)
            {
                functionDropdown.value = functionNames[0];
            }
        }
    #endregion

    private void OnDisable()
    {
        if (AnimationMode.InAnimationMode())
        {
            AnimationMode.StopAnimationMode();
        }
    }
}