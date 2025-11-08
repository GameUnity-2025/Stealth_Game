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
    public float touchSensitivity = 0.1f;

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
    public float groundedGravity = -5f; // Đã tăng lực giữ đất để đảm bảo isGrounded = true

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

        animator = GetComponentInChildren<Animator>();
        // Debug.Log("Animator connected: " + (animator != null));
    }

    private void Update()
    {
        if (!canMove) return;
        HandleLook();
        HandleMovement();
    }

    private void HandleLook()
    {
        if (mobileJoystick != null && mobileJoystick.IsDragging)
        {
            return;
        }

        float lookX = 0f;
        float lookY = 0f;

        #if UNITY_STANDALONE || UNITY_EDITOR
        bool isMouseOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(-1);
        if (!isMouseOverUI && Input.GetMouseButton(0))
        {
             lookX = Input.GetAxis("Mouse X") * mouseSensitivity;
             lookY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        }
        #endif

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            bool isPointerOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId);
            if (touch.phase == TouchPhase.Moved && !isPointerOverUI && touch.position.x > Screen.width / 2f)
            {
                lookX = touch.deltaPosition.x * mouseSensitivity * touchSensitivity;
                lookY = touch.deltaPosition.y * mouseSensitivity * touchSensitivity;
            }
        }

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
        // LOGIC JUMP VÀ GRAVITY ĐÃ SỬA CHỮA (Tương thích Animator)
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
                    // !!! SỬA LỖI TÊN TRIGGER: Dùng "JumpTrigger"
                    animator.SetTrigger("JumpTrigger"); 
                    // Set trạng thái False ngay lập tức để chuyển sang Jump/Fall
                    animator.SetBool("IsFalling", true); // Dùng IsFalling để thể hiện đang trên không
                    // Nếu bạn có IsGrounded trong Animator:
                    // animator.SetBool("IsGrounded", false);
                }
            }
            else 
            {
                // 2. Đang chạm đất VÀ không nhảy -> Áp dụng trọng lực giữ đất & Set IsFalling FALSE
                verticalVelocity = groundedGravity; 
                if (animator != null)
                {
                    animator.SetBool("IsFalling", false); 
                    // Nếu bạn có IsGrounded trong Animator:
                    // animator.SetBool("IsGrounded", true);
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
                // Nếu bạn có IsGrounded trong Animator:
                // animator.SetBool("IsGrounded", false);
            }
        }

        // Đặt lại cờ Nhảy
        isJumpPressedThisFrame = false;

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