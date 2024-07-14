using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationCtr : MonoBehaviour
{
    public enum AnimType
    {
        Walking,
        Attack,
        Death,
        Etc,
    }

    public AnimType animType;
    public string actionName;
    public float animSoft;

    public Animator _anim;

    public void Init()
    {
        _anim = GetComponentInChildren<Animator>();

        if (_anim == null)
        {
            Debug.LogError("Animator component not found!");
        }
    }

    public AnimationClip GetAnimationClip(string name)
    {
        foreach (var clip in _anim.runtimeAnimatorController.animationClips)
        {
            if (clip.name == name)
            {
                return clip;
            }
        }
        return null;
    }

    public List<AnimationClip> GetAllAnimationClips()
    {
        return new List<AnimationClip>(_anim.runtimeAnimatorController.animationClips);
    }
}