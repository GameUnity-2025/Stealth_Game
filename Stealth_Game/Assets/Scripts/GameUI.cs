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
        if (gameIsOver && Input.GetKeyDown(KeyCode.Space))
            SceneManager.LoadScene(0);
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

        // ✅ Bỏ qua các scene trung gian như "stage"
        while (next < total)
        {
            string nextPath = SceneUtility.GetScenePathByBuildIndex(next).ToLower();
            if (!nextPath.Contains("stage"))
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
        OnGameOver(gameOver);
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
