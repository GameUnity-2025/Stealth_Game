using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Cần Text

public class StageScreen : MonoBehaviour
{
    // Không cần dùng waitTime nữa nếu chuyển sang dùng nút
    // public float waitTime = 3f; 
    public Text nextText;

    // Thêm biến cho nút Về Menu (Nếu có)
    // public Button backToMenuButton; 

    void Start()
    {
        Time.timeScale = 1f;

        int nextIndex = PlayerPrefs.GetInt("NextLevelIndex", -1);
        int total = SceneManager.sceneCountInBuildSettings;

        // Scene hiện tại có thể là "stage" screen
        // int current = SceneManager.GetActiveScene().buildIndex;

        bool hasNextLevel = (nextIndex != -1 && nextIndex < total);

        // ✅ Đổi text để gợi ý bấm nút
        if (nextText != null)
        {
            // Gợi ý cho người dùng bấm nút Next Level/Try Again trên màn hình
            nextText.text = hasNextLevel ?
                "Next Level" :
                "Back to Menu / Try Again";
        }
    }

    void Update()
    {
        // BỎ HOÀN TOÀN logic Input.GetKeyDown(KeyCode.Space) và Input.touchCount > 0
        // Việc chuyển màn sẽ do các hàm Public được gọi bởi UI Button đảm nhiệm.
    }

    // ===== HÀM CÔNG KHAI CHO NÚT NEXT LEVEL / TRY AGAIN =====
    public void OnNextLevelButtonClick()
    {
        LoadNextLevel();
    }

    // ===== HÀM RIÊNG ĐỂ XỬ LÝ CHUYỂN MÀN =====
    void LoadNextLevel()
    {
        int nextIndex = PlayerPrefs.GetInt("NextLevelIndex", -1);
        int total = SceneManager.sceneCountInBuildSettings;

        // Nếu còn level kế (sau khi thắng)
        if (nextIndex != -1 && nextIndex < total)
        {
            SceneManager.LoadScene(nextIndex);
        }
        else
        {
            // Nếu đã hết level (nextIndex == -1) hoặc lỗi, về Menu (Scene 0)
            SceneManager.LoadScene("Menu");
        }
    }

    // ===== HÀM CÔNG KHAI CHO NÚT VỀ MENU (Nếu có nút riêng biệt) =====
    public void BackToMenu()
    {
        SceneManager.LoadScene("Menu"); // Về Menu (Hoặc LoadScene(0))
    }
}