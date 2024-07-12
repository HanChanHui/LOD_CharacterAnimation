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

        // 이벤트 등록
        RegisterButtonEvents(buttonPanel);

        root.Q<Button>("Create").clicked += ()=> { ButtonCreate(buttonPanel); };

        return root;
    }

    private void RegisterButtonEvents(VisualElement _panel)
    {
        if (_panel != null)
        {
            var buttons = FindButtons(_panel);
            foreach (var button in buttons)
            {
                button.clicked += () => { PlayAnimation(button.text); };
            }
        }
    }

    private List<Button> FindButtons(VisualElement _panel)
    {
        List<Button> buttons = new List<Button>();

        foreach (var child in _panel.Children())
        {
            if (child is Button button)
            {
                buttons.Add(button);
            }

            buttons.AddRange(FindButtons(child));
        }

        return buttons;
    }

    private void ButtonCreate(VisualElement _panel)
    {
        if (_panel != null)
        {
            foreach (var child in _panel.Children())
            {
                if(child is VisualElement ve)
                {
                    if(ve.name == _animCtr.animType.ToString())
                    {
                        ve.Add(ButtonInit());
                    }
                }
            }
        }
    }

    private Button ButtonInit()
    {
        Button btn = new Button();
        btn.style.width = 100;
        btn.style.height = 50;
        btn.text = _animCtr.actionName;
        btn.style.fontSize = 18;
        btn.style.unityFontStyleAndWeight = FontStyle.Bold;

        btn.clicked += () => { PlayAnimation( _animCtr.actionName); };
        return btn;
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