using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCtrl : MonoBehaviour
{
    [SerializeField] float walkSpeed = 5.0f;
    [SerializeField] float runSpeed = 5.0f;
    [SerializeField] float rotationSpeed = 700.0f;
    [SerializeField] float gravity = 9.81f;
    [SerializeField] float smoothBlend = 0.1f;
    [SerializeField] float stopSmoothBlend = 0.1f;
    [SerializeField] float turnThreshold = 0.5f; // 회전 각도 감지 임계값


    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private Animator anim;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        RotatePlayerToMouse();
        MovePlayer();
    }

  void RotatePlayerToMouse()
    {
        Plane playerPlane = new Plane(Vector3.up, transform.position);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (playerPlane.Raycast(ray, out float hitDist))
        {
            Vector3 targetPoint = ray.GetPoint(hitDist);
            Vector3 direction = targetPoint - transform.position;
            direction.y = 0; // Y축 회전을 방지하여 플레이어가 바닥을 보지 않도록 합니다.
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                float angle = Quaternion.Angle(transform.rotation, targetRotation);
                Debug.Log(moveDirection);
                // 회전 각도에 따라 턴 애니메이션 설정
                if (angle > turnThreshold && moveDirection.x == 0 && moveDirection.z == 0)
                {
                    anim.SetBool("IsTurning", true);
                }
                else
                {
                    anim.SetBool("IsTurning", false);
                }

                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

    void MovePlayer()
    {
        if (characterController.isGrounded)
        {
            // 입력 받기
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            bool isRunning = Input.GetKey(KeyCode.LeftShift);

            // 이동 속도 설정
            float currentSpeed = isRunning ? runSpeed : walkSpeed;

            // 이동 방향 설정
            Vector3 inputDirection = new Vector3(horizontal, 0, vertical);
            inputDirection = Vector3.ClampMagnitude(inputDirection, 1);

            if (inputDirection.magnitude > 0.01f)
            {
                // 이동 방향 설정
                moveDirection = inputDirection * currentSpeed;

                // 이동 방향과 애니메이션 방향 설정
                Vector3 localInputDirection = transform.InverseTransformDirection(moveDirection);
                float maxMagnitude = isRunning ? 2f : 1f;
                localInputDirection.x = Mathf.Clamp(localInputDirection.x , -maxMagnitude, maxMagnitude);
                localInputDirection.z = Mathf.Clamp(localInputDirection.z , -maxMagnitude, maxMagnitude);

                // 애니메이터 파라미터 설정
                anim.SetFloat("x", localInputDirection.x, smoothBlend, Time.deltaTime);
                anim.SetFloat("y", localInputDirection.z, smoothBlend, Time.deltaTime);
            }
            else
            {
                // 멈추기
                moveDirection = Vector3.zero;
                anim.SetFloat("x", 0, stopSmoothBlend, Time.deltaTime);
                anim.SetFloat("y", 0, stopSmoothBlend, Time.deltaTime);
            }
        }

        // 중력 적용
        moveDirection.y -= gravity * Time.deltaTime;

        // 이동 적용
        characterController.Move(moveDirection * Time.deltaTime);
    }
}
