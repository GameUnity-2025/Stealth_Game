using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using PinePie.SimpleJoystick;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // --- NÂNG CẤP: Dùng Mảng (Array) ---
    [Header("Quản lý Model")]
    public GameObject[] characterModels; // Gán Rock, Ice, Nature vào đây
    private int currentModelIndex = 0; // 0=Rock, 1=Ice, 2=Nature

    private Animator animator;

    // ... (Các biến Movement, Jump, Look... giữ nguyên) ...
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
    private float verticalVelocity = 0f;
    public float gravity = -9.81f;
    public float groundedGravity = -5f;


    // --------------------------------------------------------------------
    // HÀM START (SỬA LẠI ĐỂ DÙNG MẢNG)
    // --------------------------------------------------------------------
    private void Start()
    {
        controller = GetComponent<CharacterController>();
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

#if UNITY_STANDALONE || UNITY_EDITOR
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
#endif

        // --- LOGIC KHỞI TẠO MỚI (ĐỌC TỪ PLAYERPREFS) ---

        // Tắt tất cả các model
        for (int i = 0; i < characterModels.Length; i++)
        {
            if (characterModels[i] != null) characterModels[i].SetActive(false);
        }

        // Đọc lựa chọn đã lưu từ Menu
        string selectedCharacter = PlayerPrefs.GetString("SelectedCharacter", "HeroRock");

        // Xác định index (0, 1, 2) dựa trên tên (string) đã lưu
        switch (selectedCharacter)
        {
            case "HeroIce":
                currentModelIndex = 1;
                break;
            case "HeroNature":
                currentModelIndex = 2;
                break;
            case "HeroRock":
            default:
                currentModelIndex = 0;
                break;
        }

        // Bật model chính xác dựa trên lựa chọn đã lưu
        if (characterModels.Length > currentModelIndex && characterModels[currentModelIndex] != null)
        {
            characterModels[currentModelIndex].SetActive(true);
            animator = characterModels[currentModelIndex].GetComponent<Animator>();
            UnityEngine.Debug.Log("Đã tải Player: " + selectedCharacter);
        }
        else
        {
            UnityEngine.Debug.LogError("Player CHƯA CÓ model nào được gán hoặc index lỗi!");
        }
    }


    // --------------------------------------------------------------------
    // HÀM UPDATE (CHẠY MỖI KHUNG HÌNH)
    // --------------------------------------------------------------------
    private void Update()
    {
        if (!canMove) return;
        HandleLook();
        HandleMovement();
    }

    // --------------------------------------------------------------------
    // HÀM XỬ LÝ VA CHẠM (LOGIC ĐỔI MODEL)
    // --------------------------------------------------------------------
    private void OnTriggerEnter(Collider other)
    {
        // 1. LOGIC HOÁN ĐỔI (ĐÃ GỠ BỎ CHẶN NATURE)
        if (other.CompareTag("ChangeToIce")) // Vẫn dùng Tag này
        {
            CharacterChangeStation station = other.GetComponent<CharacterChangeStation>();
            if (station == null) return; // Không phải trạm

            int stationIndex = station.GetCurrentModelIndex();
            int playerIndex = this.currentModelIndex;

            // --- CÁC DÒNG 'IF' CHẶN NATURE ĐÃ BỊ XÓA ---
            // Giờ code sẽ chạy thẳng xuống logic hoán đổi

            UnityEngine.Debug.Log("Hoán đổi!");

            // Player nhận dạng của Trạm
            this.SetModelActive(stationIndex);

            // Trạm nhận dạng (cũ) của Player
            station.SetModelActive(playerIndex);
        }

        // 2. LOGIC VỀ ĐÍCH
        if (other.CompareTag("Finish"))
        {
            DisableMovement();
            OnReachEndOfLevel?.Invoke();
        }
    }

    // --- HÀM MỚI: DÙNG ĐỂ SET PLAYER SANG DẠNG MỚI ---
    // --- HÀM SET MODEL (ĐÃ THÊM LOGIC LƯU) ---
    public void SetModelActive(int newIndex)
    {
        // Nếu Player đã là dạng đó rồi thì không làm gì
        if (newIndex == currentModelIndex || newIndex >= characterModels.Length)
        {
            return;
        }

        // Tắt model HIỆN TẠI
        if (characterModels[currentModelIndex] != null)
            characterModels[currentModelIndex].SetActive(false);

        // Cập nhật index MỚI
        currentModelIndex = newIndex;

        // Bật model MỚI
        if (characterModels[currentModelIndex] != null)
        {
            characterModels[currentModelIndex].SetActive(true);
            SetAnimator(characterModels[currentModelIndex].GetComponent<Animator>());
            UnityEngine.Debug.Log("Player đã HOÁN ĐỔI sang: " + characterModels[currentModelIndex].name);

            // --- LƯU LỰA CHỌN MỚI ---
            SaveCurrentCharacterChoice(currentModelIndex);
        }
    }

    // --- HÀM MỚI ĐỂ LƯU LỰA CHỌN ---
    private void SaveCurrentCharacterChoice(int index)
    {
        string characterName = "HeroRock"; // Mặc định

        // Ánh xạ (Map) index (int) sang tên (string)
        // (Phải khớp với script CharacterSelectUI của bạn)
        switch (index)
        {
            case 0:
                characterName = "HeroRock";
                break;
            case 1:
                characterName = "HeroIce";
                break;
            case 2:
                characterName = "HeroNature";
                break;
        }

        // Lưu vào PlayerPrefs
        PlayerPrefs.SetString("SelectedCharacter", characterName);
        PlayerPrefs.Save();
        UnityEngine.Debug.Log("Đã lưu lựa chọn mới vào PlayerPrefs: " + characterName);
    }

    // --------------------------------------------------------------------
    // CÁC HÀM CÒN LẠI (TỪ SCRIPT CŨ CỦA BẠN - Đã sao chép)
    // --------------------------------------------------------------------

    public void SetAnimator(Animator anim)
    {
        // Hàm này dùng để cập nhật biến animator
        this.animator = anim;
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
        float horzInput = (mobileJoystick != null) ? mobileJoystick.InputDirection.x : Input.GetAxisRaw("Horizontal");
        float vertInput = (mobileJoystick != null) ? mobileJoystick.InputDirection.y : Input.GetAxisRaw("Vertical");
        float targetInputMagnitude = (mobileJoystick != null) ? mobileJoystick.InputDirection.magnitude : new Vector2(horzInput, vertInput).normalized.magnitude;

        smoothInputMagnitude = Mathf.SmoothDamp(smoothInputMagnitude, targetInputMagnitude, ref smoothMoveVelocity, smoothMoveTime);
        Vector3 rawInputDirection = new Vector3(horzInput, 0f, vertInput).normalized;
        Vector3 moveDir = (transform.right * rawInputDirection.x + transform.forward * rawInputDirection.z).normalized;
        moveVelocity = moveDir * moveSpeed * smoothInputMagnitude;

        // Phải kiểm tra animator != null phòng trường hợp bị lỗi
        if (animator != null)
        {
            animator.SetFloat("Speed", smoothInputMagnitude);
        }

        bool inputJump = Input.GetKey(KeyCode.Space) || isJumpPressedThisFrame;

        if (controller.isGrounded)
        {
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
                verticalVelocity = groundedGravity;
                if (animator != null)
                {
                    animator.SetBool("IsFalling", false);
                }
            }
        }
        else // Đang trên không
        {
            verticalVelocity += gravity * Time.deltaTime;
            if (animator != null)
            {
                animator.SetBool("IsFalling", true);
            }
        }

        isJumpPressedThisFrame = false;
        Vector3 finalVelocity = moveVelocity + Vector3.up * verticalVelocity;
        controller.Move(finalVelocity * Time.deltaTime);
    }

    public void OnJumpButtonDown()
    {
        if (canMove)
        {
            isJumpPressedThisFrame = true;
        }
    }

    private void DisableMovement()
    {
        canMove = false;
    }
}