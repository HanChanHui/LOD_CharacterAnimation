using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // �÷��̾ ����ٴ� Ÿ�� (�÷��̾�)
    [SerializeField] private Transform target;

    // ī�޶�� Ÿ�� ������ �Ÿ�
    public Vector3 offset;

    // ī�޶� �̵� �ӵ�
    public float smoothSpeed = 0.125f;

    void LateUpdate()
    {
        // Ÿ�� ��ġ�� �������� ���� ��ġ�� ���ϴ� ��ġ�� ����
        Vector3 desiredPosition = target.position + offset;

        // ���� ��ġ���� ���ϴ� ��ġ�� �ε巴�� �̵�
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // ī�޶� ��ġ�� �ε巴�� �̵��� ��ġ�� ����
        transform.position = smoothedPosition;
    }
}
