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

    public Animator anim;
    public GameObject modelPrefab;
    public RuntimeAnimatorController controller;

    public void Init()
    {
        if (transform.childCount > 0)
        {
            anim = GetComponentInChildren<Animator>();
            if (anim == null || anim.runtimeAnimatorController == null)
            {
                anim.runtimeAnimatorController = controller;
                Debug.Log($"RuntimeAnimatorController Applay {controller}");
            }
        }
        else if (modelPrefab != null && controller != null)
        {
            GameObject model = Instantiate(modelPrefab, transform);

            model.AddComponent<Animator>();
            anim = model.GetComponent<Animator>();
            Debug.Log($"Animator added to {model.name}");

            anim.runtimeAnimatorController = controller;
            Debug.Log($"RuntimeAnimatorController assigned to {model.name}");
        }
        else
        {
            Debug.Log($"No Animator");
        }
    }

    public AnimationClip GetAnimationClip(string name)
    {
        foreach (var clip in anim.runtimeAnimatorController.animationClips)
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
        return new List<AnimationClip>(anim.runtimeAnimatorController.animationClips);
    }

    public void RemoveFirstChild()
    {
        if (transform.childCount > 0)
        {
            Transform firstChild = transform.GetChild(0);
            DestroyImmediate(firstChild.gameObject);
        }
    }
}