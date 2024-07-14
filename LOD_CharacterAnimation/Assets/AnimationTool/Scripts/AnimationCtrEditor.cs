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

    public VisualElement buttonPanel;

    public VisualElement etcPanel;


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

        buttonPanel = root.Q<VisualElement>("ButtonPanel");
        etcPanel = root.Q<VisualElement>("EtcVE");

        // 이벤트 등록
        //RegisterButtonEvents(buttonPanel);

        CreateButtonsForAnimationClips(etcPanel);

        return root;
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
                btn.clicked += () => { PlayAnimation(clip.name); };
                _panel.Add(btn);
            }
        }
    }
    

    private void PlayAnimation(string animationName)
    {
        AnimationClip clip = _animCtr.GetAnimationClip(animationName);
        Debug.Log(animationName);
        if (clip != null)
        {
            PlayAnimationClip(clip);
            Debug.Log(clip);
        }
        else
        {
            Debug.LogError("Animation clip not found: " + animationName);
        }
    }

    private void PlayAnimationClip(AnimationClip clip)
    {
        AnimationMode.StartAnimationMode();
        AnimationMode.SampleAnimationClip(_animCtr._anim.gameObject, clip, 0f);

        EditorApplication.update += () =>
        {
            if (AnimationMode.InAnimationMode())
            {
                float time = (float)EditorApplication.timeSinceStartup % clip.length;
                AnimationMode.SampleAnimationClip(_animCtr._anim.gameObject, clip, time);
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