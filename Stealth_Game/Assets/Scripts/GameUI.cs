using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
    public GameObject gameOver;
    public GameObject gameWin;
    bool gameIsOver;

    void Start()
    {
        Guard.OnGuardHasSpottedPlayer += ShowGameLose;

        Player player = FindFirstObjectByType<Player>();
        if (player != null)
            player.OnReachEndOfLevel += ShowGameWin;
    }

    void Update()
    {
        // chỉ xử lý khi game đã kết thúc
        if (!gameIsOver) return;

        // Nếu đang hiển thị màn Lose -> Space = chơi lại màn hiện tại
        if (gameOver != null && gameOver.activeSelf && Input.GetKeyDown(KeyCode.Space))
        {
            int current = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(current);
        }

        // Nếu đang hiển thị màn Lose hoặc Win -> phím M để về menu
        if ((gameOver != null && gameOver.activeSelf || (gameWin != null && gameWin.activeSelf))
            && Input.GetKeyDown(KeyCode.M))
        {
            SceneManager.LoadScene("Menu");
        }
    }


    // ===== KHI THẮNG =====
    void ShowGameWin()
    {
        if (gameIsOver) return;
        gameIsOver = true;

        Guard.OnGuardHasSpottedPlayer -= ShowGameLose;
        Player player = FindFirstObjectByType<Player>();
        if (player != null)
            player.OnReachEndOfLevel -= ShowGameWin;

        int current = SceneManager.GetActiveScene().buildIndex;
        int total = SceneManager.sceneCountInBuildSettings;
        int next = current + 1;

        // ✅ Bỏ qua các scene trung gian như "stage" hoặc "characterselect"
        while (next < total)
        {
            string nextPath = SceneUtility.GetScenePathByBuildIndex(next).ToLower();

            // Nếu tên scene có chứa "stage" hoặc "characterselect" → bỏ qua
            if (!nextPath.Contains("stage") && !nextPath.Contains("characterselect"))
                break;

            next++;
        }


        // ✅ Nếu còn level kế thì lưu lại
        if (next < total)
        {
            PlayerPrefs.SetInt("NextLevelIndex", next);
        }
        else
        {
            PlayerPrefs.SetInt("NextLevelIndex", -1); // Hết game
        }

        PlayerPrefs.Save();

        // ✅ Chuyển sang scene “stage” (màn hình thắng)
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
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        PlayerPrefs.SetInt("NextLevelIndex", currentScene);
        PlayerPrefs.Save();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Hủy đăng ký để tránh gọi lại
        Guard.OnGuardHasSpottedPlayer -= ShowGameLose;
        Player player = FindFirstObjectByType<Player>();
        if (player != null)
            player.OnReachEndOfLevel -= ShowGameWin;
    }


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
