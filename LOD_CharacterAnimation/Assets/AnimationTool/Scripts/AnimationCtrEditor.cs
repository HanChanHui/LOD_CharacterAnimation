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

    private ObjectField clipField;
    private Slider frameSlider;
    private Button startClipBtn;
    private Button stopClipBtn;

    private AnimationClip animationClip;
    public AnimationClip AnimationClip { get => animationClip; set { animationClip = value; clipField.value = value; } }
    private bool isPlaying = false;
    private float startTime = 0;
    private float currentTime = 0;

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

        clipField = root.Q<ObjectField>("animationField");
        frameSlider = root.Q<Slider>("frameSlider");
        startClipBtn = root.Q<Button>("startBtn");
        stopClipBtn = root.Q<Button>("stopBtn");

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
        //startClipBtn.clicked += () => { AddAnimationEvent(clip, frameSlider.value); };
    }

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
            currentTime = (float)(EditorApplication.timeSinceStartup - startTime) % animationClip.length;
            frameSlider.value = currentTime * animationClip.frameRate;
            AnimationMode.SampleAnimationClip(_animCtr.anim.gameObject, animationClip, currentTime);
        }
    }

    private void AddAnimationEvent(AnimationClip clip, float frame)
    {
        animationClip = clip;
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


    private void OnDisable()
    {
        if (AnimationMode.InAnimationMode())
        {
            AnimationMode.StopAnimationMode();
        }
    }
}