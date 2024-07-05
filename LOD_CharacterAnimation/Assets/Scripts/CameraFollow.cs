using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // 플레이어를 따라다닐 타겟 (플레이어)
    [SerializeField] private Transform target;

    // 카메라와 타겟 사이의 거리
    public Vector3 offset;

    // 카메라 이동 속도
    public float smoothSpeed = 0.125f;

    void LateUpdate()
    {
        // 타겟 위치에 오프셋을 더한 위치를 원하는 위치로 설정
        Vector3 desiredPosition = target.position + offset;

        // 현재 위치에서 원하는 위치로 부드럽게 이동
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // 카메라 위치를 부드럽게 이동한 위치로 설정
        transform.position = smoothedPosition;
    }
}
