using System.Collections;
using System.Collections.Generic;
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

    public AnimationClip animationClip;
    private VisualElement clipPanel;
    private VisualElement eventPanel;
     private Slider frameSlider;
    private Button addEventButton;


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

        //=============================================

        clipPanel = root.Q<VisualElement>("clipPanel");
        eventPanel = root.Q<VisualElement>("eventPanel");

        //=============================================

        if(_animCtr.anim != null)
        {
            CreateButtonsForAnimationClips(etcPanel);
        }

        root.Q<Button>("apply").clicked += ()=> { ResetButton(etcPanel); };

        return root;
    }

    private void ResetButton(VisualElement _panel)
    {
        _animCtr.RemoveFirstChild();
        RemoveAllButtons(_panel);
        _animCtr.Init();
        CreateButtonsForAnimationClips(_panel);
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
                //btn.clicked += () => { PlayAnimation(clip.name); };
                btn.clicked += () => { ShowClipDetails(clip); };
                _panel.Add(btn);
            }
        }
    }

    private void ShowClipDetails(AnimationClip clip)
    {
        eventPanel.Clear();
        animationClip = clip;
        frameSlider = new Slider(0, clip.length * clip.frameRate)
        {
            label = "Frame",
            showInputField = true
        };
        eventPanel.Add(frameSlider);

        addEventButton = new Button { text = "Add Event" };
        addEventButton.clicked += () => { AddAnimationEvent(clip, frameSlider.value); };
        addEventButton.clicked += () => { PlayAnimation(clip.name, frameSlider.value); };
        eventPanel.Add(addEventButton);
    }

    private void AddAnimationEvent(AnimationClip clip, float frame)
    {
        AnimationEvent animationEvent = new AnimationEvent
        {
            time = frame / clip.frameRate,
            functionName = "OnAnimationEvent" // 이벤트가 호출할 함수 이름
        };

        List<AnimationEvent> events = new List<AnimationEvent>(AnimationUtility.GetAnimationEvents(clip))
        {
            animationEvent
        };

        AnimationUtility.SetAnimationEvents(clip, events.ToArray());

        Debug.Log($"Added event at {frame / clip.frameRate} seconds to {clip.name}");
    }

    private void RemoveAllButtons(VisualElement _panel)
    {
        if (_panel != null)
        {
            _panel.Clear();
        }
    }

    private void PlayAnimation(string animationName, float _frame)
    {
        AnimationClip clip = _animCtr.GetAnimationClip(animationName);

        if (clip != null)
        {
            PlayAnimationClip(clip, _frame);
        }
        else
        {
            Debug.LogError("Animation clip not found: " + animationName);
        }
    }

    private void PlayAnimationClip(AnimationClip clip, float _frame)
    {
        AnimationMode.StartAnimationMode();
        AnimationMode.SampleAnimationClip(_animCtr.anim.gameObject, clip, 0f);

        EditorApplication.update += () =>
        {
            if (AnimationMode.InAnimationMode())
            {
                float time = (float)EditorApplication.timeSinceStartup % clip.length;
                AnimationMode.SampleAnimationClip(_animCtr.anim.gameObject, clip, _frame);
            }
        };
    }

    private void OnDisable()
    {
        if (AnimationMode.InAnimationMode())
        {
            AnimationMode.StopAnimationMode();
        }
    }
}