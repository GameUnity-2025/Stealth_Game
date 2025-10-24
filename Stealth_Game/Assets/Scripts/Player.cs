using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Bắt buộc phải có CharacterController
[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    private Animator animator;
    public event System.Action OnReachEndOfLevel;

    [Header("Movement Settings")]
    public float moveSpeed = 7f;
    public float smoothMoveTime = 0.1f;

    [Header("Jump Settings")]
    public float jumpForce = 5f; // Lực nhảy
    // Bỏ turnSpeed vì không dùng cho FPS

    [Header("Look / Camera")]
    public float mouseSensitivity = 2f; // Độ nhạy chuột
    public Transform cameraTransform; // Gán Main Camera (child) vào đây

    // Internal
    private CharacterController controller;
    private float smoothInputMagnitude;
    private float smoothMoveVelocity;
    private Vector3 moveVelocity;   // vector vận tốc ngang (x,z)
    private float pitch = 0f;       // góc nhìn dọc (vertical look)
    private bool canMove = true;

    // gravity
    private float verticalVelocity = 0f;
    public float gravity = -9.81f;
    public float groundedGravity = -0.5f; // giữ người đứng sát đất

    private void Start()
    {
        // ... (Giữ nguyên Start)
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            Debug.LogError("Player: CharacterController missing!");
        }

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (!canMove) return;

        HandleLook();
        HandleMovement();
    }

    private void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -80f, 80f);
        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    // Đã sửa hàm HandleMovement() trong Player.cs
    private void HandleMovement()
    {
        // 1. Input ngang
        Vector3 inputDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;
        float inputMag = inputDir.magnitude;

        // 2. Smooth Input
        smoothInputMagnitude = Mathf.SmoothDamp(smoothInputMagnitude, inputMag, ref smoothMoveVelocity, smoothMoveTime);

        // 3. Tính Vector di chuyển theo hướng Player (FPS)
        Vector3 moveDir = (transform.right * inputDir.x + transform.forward * inputDir.z).normalized;
        moveVelocity = moveDir * moveSpeed * smoothInputMagnitude;

        // ************************************************
        // LOGIC ANIMATION
        // ************************************************
        if (animator != null)
        {
            // Cập nhật tham số Speed (Idle/Run)
            // smoothInputMagnitude sẽ là 0 khi không di chuyển và gần 1 khi di chuyển
            animator.SetFloat("Speed", smoothInputMagnitude);
        }
        // ************************************************

        // 4. Gravity & Jump Logic
        if (controller.isGrounded)
        {
            verticalVelocity = groundedGravity;

            // --- Cập nhật Animation: Tiếp đất ---
            if (animator != null)
            {
                animator.SetBool("IsFalling", false);
            }

            // XỬ LÝ NHẢY: Nếu đang đứng trên đất và nhấn Space
            if (Input.GetKey(KeyCode.Space))
            {
                verticalVelocity = jumpForce;

                // --- Cập nhật Animation: Bắt đầu nhảy ---
                if (animator != null)
                {
                    animator.SetTrigger("JumpTrigger");
                }
            }
        }
        else 
        {
            // Áp dụng trọng lực
            verticalVelocity += gravity * Time.deltaTime;

            // --- Cập nhật Animation: Đang trên không ---
            if (animator != null)
            {
                // Kích hoạt cờ IsFalling để chạy animation Rơi/Giữ Jump
                animator.SetBool("IsFalling", true);
            }
        }

        // 5. Kết hợp vận tốc ngang và dọc
        Vector3 finalVelocity = moveVelocity + Vector3.up * verticalVelocity;

        // 6. Di chuyển bằng CharacterController
        controller.Move(finalVelocity * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Finish"))
        {
            DisableMovement();
            OnReachEndOfLevel?.Invoke();
        }
    }

    private void DisableMovement()
    {
        canMove = false;
        // Logic hiện/ẩn chuột đã được chuyển sang GameUI.cs (cho game over/win)
    }

    private void OnDestroy()
    {
        // ...
    }
}