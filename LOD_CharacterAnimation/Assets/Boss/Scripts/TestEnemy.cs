using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEnemy : MonoBehaviour
{
    

    public void Attack()
    {
        Debug.Log("플레이어를 공격한다.");
    }

    public void Walk()
    {
        Debug.Log("플레이어에게 다가간다.");
    }

    public void Run()
    {
        Debug.Log("플레이어에게 뛰어간다.");
    }
}
