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

    private Animator _anim;

    public void Init()
    {
        _anim = GetComponent<Animator>();
        if (_anim == null)
        {
            Debug.LogError("Animator component not found!");
        }
    }

    public void AnimCross(string _name, float _soft)
    {
        if (_anim == null)
        {
            Debug.LogError("Animator not initialized!");
            return;
        }

        int animHash = Animator.StringToHash(_name);
        _anim.CrossFade(animHash, _soft);
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
}