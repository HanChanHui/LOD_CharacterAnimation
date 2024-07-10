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

    public VisualElement walkingPanel;
    public VisualElement attackPanel;
    public VisualElement deathPanel;
    public VisualElement etcPanel;

    public override VisualElement CreateInspectorGUI()
    {
        if(!visualTree)
        {
            return base.CreateInspectorGUI();
        }
        _animCtr = (AnimationCtr)target;

        VisualElement root = new VisualElement();
        visualTree.CloneTree(root);

        _animCtr.Init();

        walkingPanel = root.Q<VisualElement>("Walking");
        attackPanel = root.Q<VisualElement>("Attack");
        deathPanel = root.Q<VisualElement>("Death");
        etcPanel = root.Q<VisualElement>("Etc");

        root.Q<Button>("walkBtn").clicked += ()=>{ PlayAnimation("Walk"); };
        root.Q<Button>("runBtn").clicked += ()=>{ PlayAnimation("Run"); };
        root.Q<Button>("atk1Btn").clicked += ()=>{ PlayAnimation("Attack1"); };
        root.Q<Button>("atk2Btn").clicked += ()=>{ PlayAnimation("Attack2"); };
        root.Q<Button>("atk3Btn").clicked += ()=>{ PlayAnimation("Attack3"); };
        root.Q<Button>("atk4Btn").clicked += ()=>{ PlayAnimation("Attack4"); };
        root.Q<Button>("atk5Btn").clicked += ()=>{ PlayAnimation("Attack5"); };
        root.Q<Button>("deathBtn").clicked += ()=>{ PlayAnimation("Death"); };
        root.Q<Button>("idleBtn").clicked += ()=>{ PlayAnimation("Idle"); };
        root.Q<Button>("cryBtn").clicked += ()=>{ PlayAnimation("Cry"); };
        root.Q<Button>("equipBtn").clicked += ()=>{ PlayAnimation("Equip"); };

        return root;
    }

    private void PlayAnimation(string animationName)
    {
        AnimationClip clip = _animCtr.GetAnimationClip(animationName);
        if (clip != null)
        {
            PlayAnimationClip(clip);
        }
        else
        {
            Debug.LogError("Animation clip not found: " + animationName);
        }
    }

    private void PlayAnimationClip(AnimationClip clip)
    {
        AnimationMode.StartAnimationMode();
        AnimationMode.SampleAnimationClip(_animCtr.gameObject, clip, 0f);

        EditorApplication.update += () => 
        {
            if (AnimationMode.InAnimationMode())
            {
                float time = (float)EditorApplication.timeSinceStartup % clip.length;
                AnimationMode.SampleAnimationClip(_animCtr.gameObject, clip, time);
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