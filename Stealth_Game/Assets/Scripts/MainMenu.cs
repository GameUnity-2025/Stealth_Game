using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Khi bấm Play, sẽ tiếp tục màn đã lưu (nếu có)
    public void PlayGame()
    {
        // 🔹 Lấy scene index đã lưu (nếu chưa có thì mặc định là 1)
        int savedIndex = PlayerPrefs.GetInt("NextLevelIndex", -1);

        if (savedIndex != -1)
        {
            // Nếu có màn đã lưu (thua dở hoặc đang ở giữa chừng)
            SceneManager.LoadScene(savedIndex);
            return;
        }

        // 🔹 Nếu chưa lưu màn nào thì load màn đầu tiên sau Menu
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int next = currentIndex + 1;
        int total = SceneManager.sceneCountInBuildSettings;

        // Bỏ qua các scene trung gian như "CharacterSelect" hoặc "Stage"
        while (next < total)
        {
            string nextPath = SceneUtility.GetScenePathByBuildIndex(next).ToLower();

            if (!nextPath.Contains("characterselect") && !nextPath.Contains("stage"))
                break;

            next++;
        }

        if (next < total)
        {
            SceneManager.LoadScene(next);
        }
        else
        {
            Debug.LogWarning("⚠️ Không có scene hợp lệ nào để load sau Menu!");
        }
    }

    // 👉 Nút Reset tiến trình nếu bạn muốn chơi lại từ đầu
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey("NextLevelIndex");
        PlayerPrefs.Save();
        Debug.Log("🔄 Tiến trình đã được reset!");
    }

    // 👉 Nút chọn nhân vật (giữ nguyên)
    public void SelectCharacter()
    {
        SceneManager.LoadScene("CharacterSelect");
    }

    // 👉 Nút thoát
    public void QuitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
    }
}
