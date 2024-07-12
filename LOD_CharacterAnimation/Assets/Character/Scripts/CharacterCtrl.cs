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


    public float followDelay = 0.1f; // 딜레이 시간 (초 단위)
    private Quaternion targetRotation;
    private float followTimer;

    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private Animator anim;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        targetRotation = transform.rotation;
        followTimer = 0f;
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
            direction.y = 0;

            if (direction != Vector3.zero)
            {
                Quaternion newTargetRotation = Quaternion.LookRotation(direction);

                if (targetRotation != newTargetRotation)
                {
                    targetRotation = newTargetRotation;
                    followTimer = 0f;
                }

                followTimer += Time.deltaTime;

                if (followTimer > followDelay)
                {
                    // 부드럽게 회전
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }
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

            float currentSpeed = isRunning ? runSpeed : walkSpeed;

            Vector3 inputDirection = new Vector3(horizontal, 0, vertical);
            inputDirection = Vector3.ClampMagnitude(inputDirection, 1);

            if (inputDirection.magnitude > 0.01f)
            {
                moveDirection = inputDirection * currentSpeed;

                Vector3 localInputDirection = transform.InverseTransformDirection(moveDirection);
                float maxMagnitude = isRunning ? 6f : 4f;
                localInputDirection.x = Mathf.Clamp(localInputDirection.x, -maxMagnitude, maxMagnitude);
                localInputDirection.z = Mathf.Clamp(localInputDirection.z, -maxMagnitude, maxMagnitude);

                float targetX = localInputDirection.x;
                float targetY = localInputDirection.z;

                float currentX = anim.GetFloat("x");
                float currentY = anim.GetFloat("y");

                float newX = Mathf.Lerp(currentX, targetX, smoothBlend * Time.deltaTime);
                float newY = Mathf.Lerp(currentY, targetY, smoothBlend * Time.deltaTime);

                anim.SetFloat("x", newX);
                anim.SetFloat("y", newY);
            }
            else
            {
                // 멈추기
                float currentX = anim.GetFloat("x");
                float currentY = anim.GetFloat("y");

                float newX = Mathf.Lerp(currentX, 0, stopSmoothBlend * Time.deltaTime);
                float newY = Mathf.Lerp(currentY, 0, stopSmoothBlend * Time.deltaTime);
                anim.SetFloat("x", newX);
                anim.SetFloat("y", newY);
            }
        }

        moveDirection.y -= gravity * Time.deltaTime;

        characterController.Move(moveDirection * Time.deltaTime);
    }
}
