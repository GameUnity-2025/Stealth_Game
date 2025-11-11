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

    [Header("Jump Settings")]
    public float jumpForce = 5f;

    [Header("Look / Camera")]
    public float mouseSensitivity = 2f;
    public Transform cameraTransform;

    [Header("Mobile Input")]
    public JoystickController mobileJoystick;
    // Điều chỉnh touchSensitivity thấp hơn để cảm ứng không quá nhạy
    public float touchSensitivity = 0.05f;

    private bool isJumpPressedThisFrame = false;
    private CharacterController controller;
    private float smoothInputMagnitude;
    private float smoothMoveVelocity;
    private Vector3 moveVelocity;
    private float pitch = 0f;
    private bool canMove = true;

    // Gravity
    private float verticalVelocity = 0f;
    public float gravity = -9.81f;
    public float groundedGravity = -5f; // Lực giữ đất

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            Debug.LogError("Player: CharacterController missing!");
        }
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        // Nếu bạn Build cho Mobile, bạn nên BỎ (hoặc thay đổi) các lệnh Cursor lock này.
        // Tuy nhiên, tôi giữ nguyên cho Editor/PC:
#if UNITY_STANDALONE || UNITY_EDITOR
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
#endif

        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (!canMove) return;
        HandleLook();
        HandleMovement();

        // Đặt lại cờ Nhảy vào cuối Update
        isJumpPressedThisFrame = false;
    }

    private void HandleLook()
    {
        float lookX = 0f;
        float lookY = 0f;

        // ----------------------------------------------------
        // 1. Xử lý Input Chuột (PC/Editor)
        // ----------------------------------------------------
#if UNITY_STANDALONE || UNITY_EDITOR
        // Bỏ kiểm tra Input.GetMouseButton(0) để tránh xung đột với Touch Mobile
        // Thay vào đó, dùng chuột phải để xoay nếu không có Joystick
        // Hoặc kiểm tra xem con trỏ có bị khóa không.

        // Giả định bạn muốn xoay khi chuột phải được bấm trong Editor
        if (Input.GetMouseButton(1)) // Sử dụng chuột phải (RMB) để xoay trong Editor
        {
            bool isMouseOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(-1);
            if (!isMouseOverUI)
            {
                lookX = Input.GetAxis("Mouse X") * mouseSensitivity;
                lookY = Input.GetAxis("Mouse Y") * mouseSensitivity;
            }
        }
#endif

        // ----------------------------------------------------
        // 2. Xử lý Input Chạm (Mobile)
        // ----------------------------------------------------
        if (Input.touchCount > 0)
        {
            // Duyệt qua tất cả các điểm chạm để tìm điểm chạm xoay camera
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);

                // BỎ qua chạm nếu nó đang kéo Joystick (Hoặc kiểm tra theo khu vực màn hình)
                // Chúng ta sẽ kiểm tra xem điểm chạm có ở nửa bên phải màn hình không
                bool isRightSide = touch.position.x > Screen.width / 2f;

                // BỎ qua nếu chạm nằm trên UI (bao gồm cả Joystick/Nút Jump)
                bool isPointerOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId);

                // Nếu là trạng thái di chuyển VÀ nằm bên phải VÀ không nằm trên UI
                if (touch.phase == TouchPhase.Moved && isRightSide && !isPointerOverUI)
                {
                    // Đảm bảo không sử dụng touch đang kéo Joystick (nếu Joystick nằm bên trái)
                    // Nếu bạn có một Joystick cố định bên trái, kiểm tra này là đủ:
                    // if (touch.position.x > Screen.width * 0.4f) // Giả định Joystick chiếm 40% bên trái

                    lookX = touch.deltaPosition.x * mouseSensitivity * touchSensitivity;
                    lookY = touch.deltaPosition.y * mouseSensitivity * touchSensitivity;

                    // Chỉ xử lý một lần chạm để xoay
                    break;
                }
            }
        }

        // 3. Áp dụng Xoay
        transform.Rotate(Vector3.up * lookX);
        pitch -= lookY;
        pitch = Mathf.Clamp(pitch, -80f, 80f);
        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void HandleMovement()
    {
        // 1. Logic Input và Tốc độ
        float horzInput = (mobileJoystick != null) ? mobileJoystick.InputDirection.x : Input.GetAxisRaw("Horizontal");
        float vertInput = (mobileJoystick != null) ? mobileJoystick.InputDirection.y : Input.GetAxisRaw("Vertical");
        float targetInputMagnitude = (mobileJoystick != null) ? mobileJoystick.InputDirection.magnitude : new Vector2(horzInput, vertInput).normalized.magnitude;

        smoothInputMagnitude = Mathf.SmoothDamp(smoothInputMagnitude, targetInputMagnitude, ref smoothMoveVelocity, smoothMoveTime);
        Vector3 rawInputDirection = new Vector3(horzInput, 0f, vertInput).normalized;
        Vector3 moveDir = (transform.right * rawInputDirection.x + transform.forward * rawInputDirection.z).normalized;
        moveVelocity = moveDir * moveSpeed * smoothInputMagnitude;

        // LOGIC ANIMATION TỐC ĐỘ (Run/Idle)
        if (animator != null)
        {
            animator.SetFloat("Speed", smoothInputMagnitude);
        }

        // *******************************************************
        // LOGIC JUMP VÀ GRAVITY ĐÃ SỬA CHỮA (Giữ nguyên)
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

        // 5. Di chuyển CharacterController
        Vector3 finalVelocity = moveVelocity + Vector3.up * verticalVelocity;
        controller.Move(finalVelocity * Time.deltaTime);
    }

    // HÀM CÔNG KHAI CHO NÚT NHẢY UI
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
    }
}