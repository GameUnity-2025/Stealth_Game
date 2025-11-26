using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using PinePie.SimpleJoystick;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    private Animator animator;
    public event System.Action OnReachEndOfLevel;

    [Header("Movement Settings")]
    public float moveSpeed = 7f;
    public float smoothMoveTime = 0.1f;

    [Header("Rotation Settings")]
    // Tốc độ xoay mô hình (Model) bên trong
    public float modelRotationSpeed = 800f;

    [Header("Jump Settings")]
    public float jumpForce = 5f;

    [Header("Look / Camera")]
    public float mouseSensitivity = 2f;
    public Transform cameraTransform;

    [Header("Mobile Input")]
    public JoystickController mobileJoystick;
    public float touchSensitivity = 0.1f;

    private bool isJumpPressedThisFrame = false;
    private CharacterController controller;
    private float smoothInputMagnitude;
    private float smoothMoveVelocity;
    private Vector3 moveVelocity;
    private float pitch = 0f;
    private bool canMove = true;

    // **BIẾN MỚI:** Tham chiếu đến đối tượng con chứa mô hình và Animator
    private Transform modelTransform;

    // Gravity
    private float verticalVelocity = 0f;
    public float gravity = -9.81f;
    public float groundedGravity = -5f;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            Debug.LogError("Player: CharacterController missing!");
        }
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

#if UNITY_STANDALONE || UNITY_EDITOR
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
#endif

        // **Cập nhật:** Tìm Animator và gán Model Transform
        animator = GetComponentInChildren<Animator>();
        if (animator != null)
        {
            // Model Transform là cha của Animator (hoặc chính Animator.transform nếu nó là gốc)
            modelTransform = animator.transform;
        }
    }

    private void Update()
    {
        if (!canMove) return;
        HandleLook();
        HandleMovement();
    }

    private void HandleLook()
    {
        float lookX = 0f;
        float lookY = 0f;

#if UNITY_STANDALONE || UNITY_EDITOR
        // Xử lý Input chuột trên PC/Editor
        bool isMouseOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(-1);
        if (!isMouseOverUI && Input.GetMouseButton(0))
        {
            lookX = Input.GetAxis("Mouse X") * mouseSensitivity;
            lookY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        }
#endif

        // LOGIC XỬ LÝ CẢM ỨNG ĐA CHẠM
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            bool isPointerOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId);

            if (touch.phase == TouchPhase.Moved && !isPointerOverUI && touch.position.x > Screen.width / 2f)
            {
                lookX = touch.deltaPosition.x * mouseSensitivity * touchSensitivity;
                lookY = touch.deltaPosition.y * mouseSensitivity * touchSensitivity;
                break;
            }
        }

        // **GIỮ NGUYÊN NHƯ BAN ĐẦU:** XOAY PLAYER VÀ CAMERA NGANG (Góc nhìn)
        transform.Rotate(Vector3.up * lookX);

        // Xoay Camera Pitch (lên xuống)
        pitch -= lookY;
        pitch = Mathf.Clamp(pitch, -80f, 80f);
        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void HandleMovement()
    {
        // 1. Logic Input
        float horzInput = (mobileJoystick != null) ? mobileJoystick.InputDirection.x : Input.GetAxisRaw("Horizontal");
        float vertInput = (mobileJoystick != null) ? mobileJoystick.InputDirection.y : Input.GetAxisRaw("Vertical");

        Vector3 rawInputDirection = new Vector3(horzInput, 0f, vertInput).normalized;
        float targetInputMagnitude = rawInputDirection.magnitude;

        smoothInputMagnitude = Mathf.SmoothDamp(smoothInputMagnitude, targetInputMagnitude, ref smoothMoveVelocity, smoothMoveTime);

        // Tính Vector vận tốc di chuyển (dựa trên hướng của Player (camera) và input)
        Vector3 moveDir = (transform.right * rawInputDirection.x + transform.forward * rawInputDirection.z).normalized;
        moveVelocity = moveDir * moveSpeed * smoothInputMagnitude;

        // *******************************************************
        // LOGIC XOAY MÔ HÌNH (MODEL) ĐỘC LẬP
        // *******************************************************
        if (modelTransform != null && targetInputMagnitude > 0.1f)
        {
            // Tính toán hướng di chuyển (moveDir) so với hướng forward của Player
            // Đây là hướng mà mô hình cần nhìn tới (trong Local Space của Player)
            Vector3 targetModelForward = rawInputDirection.normalized;

            // Chuyển hướng Local Space (X, Z) thành Target Rotation
            Quaternion targetRotation = Quaternion.LookRotation(targetModelForward);

            // Làm mượt việc xoay mô hình (xoay theo trục Y cục bộ)
            modelTransform.localRotation = Quaternion.RotateTowards(
                modelTransform.localRotation,
                targetRotation,
                modelRotationSpeed * Time.deltaTime
            );
        }
        else if (modelTransform != null && targetInputMagnitude <= 0.1f)
        {
            // Đảm bảo mô hình quay về hướng mặc định (forward) khi Idle
            modelTransform.localRotation = Quaternion.RotateTowards(
               modelTransform.localRotation,
               Quaternion.identity,
               modelRotationSpeed * Time.deltaTime
           );
        }

        // LOGIC ANIMATION TỐC ĐỘ (Run/Idle)
        if (animator != null)
        {
            animator.SetFloat("Speed", smoothInputMagnitude);
        }

        // *******************************************************
        // LOGIC JUMP VÀ GRAVITY (Giữ nguyên)
        // *******************************************************

        bool inputJump = Input.GetKey(KeyCode.Space) || isJumpPressedThisFrame;

        if (controller.isGrounded)
        {
            // 1. Kích hoạt Jump (Chỉ khi đang chạm đất)
            if (inputJump)
            {
                verticalVelocity = jumpForce;
                if (animator != null)
                {
                    animator.SetTrigger("JumpTrigger");
                    animator.SetBool("IsFalling", true);
                }
            }
            else
            {
                // 2. Đang chạm đất VÀ không nhảy -> Áp dụng trọng lực giữ đất & Set IsFalling FALSE
                verticalVelocity = groundedGravity;
                if (animator != null)
                {
                    animator.SetBool("IsFalling", false);
                }
            }
        }
        else // Đang trên không
        {
            // 3. Áp dụng trọng lực & Set IsFalling TRUE
            verticalVelocity += gravity * Time.deltaTime;
            if (animator != null)
            {
                animator.SetBool("IsFalling", true);
            }
        }

        // Đặt lại cờ Nhảy
        isJumpPressedThisFrame = false;

        // 5. Di chuyển CharacterController
        Vector3 finalVelocity = moveVelocity + Vector3.up * verticalVelocity;
        controller.Move(finalVelocity * Time.deltaTime);
    }

    // HÀM CÔNG KHAI CHO NÚT NHẢY UI (Giữ nguyên)
    public void OnJumpButtonDown()
    {
        if (canMove)
        {
            isJumpPressedThisFrame = true;
        }
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
    }

    public void SetAnimator(Animator anim)
    {
        this.animator = anim;
        this.modelTransform = anim.transform;
    }
}