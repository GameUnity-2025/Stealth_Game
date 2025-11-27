// 11/8/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
    public GameObject gameOver;
    public GameObject gameWin;
    

    // Thêm biến cho Nút UI Về Menu
    // Bạn nên đặt Nút Menu là con của Canvas Game Over (hoặc Game Win)
    public GameObject backToMenuButton;

    bool gameIsOver;

    void Start()
    {
        // Gắn sự kiện
        Guard.OnGuardHasSpottedPlayer += ShowGameLose;

        Player player = FindFirstObjectByType<Player>();
        if (player != null)
            player.OnReachEndOfLevel += ShowGameWin;

        
    }

    void OnDestroy()
    {
        // Đảm bảo hủy đăng ký khi GameObject bị hủy
        Guard.OnGuardHasSpottedPlayer -= ShowGameLose;
        Player player = FindFirstObjectByType<Player>();
        if (player != null)
            player.OnReachEndOfLevel -= ShowGameWin;
    }

    void Update()
    {
        // Chỉ xử lý logic UI game nếu game kết thúc
        if (!gameIsOver) return;

        // --- LOGIC XỬ LÝ CHƠI LẠI (REPLAY) ---

        

        // Xử lý logic Replay (Space cho PC/Simulator, Touch bên ngoài Joystick cho Mobile)
       

        // BỎ logic Input.GetKeyDown(KeyCode.M) và Input.touchCount >= 2 (2 chạm) 
        // Logic Về Menu sẽ được xử lý bằng hàm BackToMenu() gọi từ nút UI.
    }
    public void ReplayGame()
    {
        // Đảm bảo game đã kết thúc trước khi chơi lại
        if (!gameIsOver)
        {
            Debug.LogWarning("ReplayGame called before game ended.");
            return;
        }

        // Chơi lại màn hiện tại
        int current = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(current);
    }
    // ===== KHI THẮNG =====
    void ShowGameWin()
    {
        if (gameIsOver) return;
        gameIsOver = true;

        // Dọn dẹp sự kiện
        Guard.OnGuardHasSpottedPlayer -= ShowGameLose;
        Player player = FindFirstObjectByType<Player>();
        if (player != null)
            player.OnReachEndOfLevel -= ShowGameWin;

        // Logic lưu Level & chuyển scene "stage"
        int current = SceneManager.GetActiveScene().buildIndex;
        int total = SceneManager.sceneCountInBuildSettings;
        int next = current + 1;

        // Bỏ qua các scene trung gian (stage, characterselect)
        while (next < total)
        {
            string nextPath = SceneUtility.GetScenePathByBuildIndex(next).ToLower();
            if (!nextPath.Contains("stage") && !nextPath.Contains("characterselect"))
                break;
            next++;
        }

        if (next < total)
        {
            PlayerPrefs.SetInt("NextLevelIndex", next);
        }
        else
        {
            PlayerPrefs.SetInt("NextLevelIndex", -1); // Hết game
        }

        PlayerPrefs.Save();

        // Chuyển sang scene “stage” (màn hình thắng/chuyển màn)
        SceneManager.LoadScene("stage");
    }

    // ===== KHI THUA =====
    void ShowGameLose()
    {
        if (gameIsOver) return;
        gameIsOver = true;

        if (gameOver != null)
            gameOver.SetActive(true);

        // Lưu lại màn hiện tại để khi vào Menu và bấm Play sẽ tiếp tục từ màn này
        if (SceneManager.GetActiveScene().name == "Tutorial")
            return; // Bỏ qua tutorial
        else
        {
            int currentScene = SceneManager.GetActiveScene().buildIndex;
            PlayerPrefs.SetInt("NextLevelIndex", currentScene);
            PlayerPrefs.Save();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Dọn dẹp sự kiện
            Guard.OnGuardHasSpottedPlayer -= ShowGameLose;

            Player player = FindFirstObjectByType<Player>();
            if (player != null)
                player.OnReachEndOfLevel -= ShowGameWin;
        }
    }


    // ===== HÀM MỚI: Dùng cho Nút UI Về Menu (Public để gán vào UI) =====
    public void BackToMenu()
    {
        

        SceneManager.LoadScene("Menu");

        // Đảm bảo con trỏ chuột được mở
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Hàm gốc OnGameOver của bạn không được dùng, nên tôi giữ nguyên (có thể bạn gọi từ nơi khác)
    void OnGameOver(GameObject gameOverScreen)
    {
        if (gameIsOver) return;
        gameIsOver = true;

        if (gameOverScreen != null)
            gameOverScreen.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Guard.OnGuardHasSpottedPlayer -= ShowGameLose;
        Player player = FindFirstObjectByType<Player>();
        if (player != null)
            player.OnReachEndOfLevel -= ShowGameWin;
    }
}