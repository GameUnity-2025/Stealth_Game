using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageScreen : MonoBehaviour
{
    public float waitTime = 3f;
    public Text nextText;

    void Start()
    {
        Time.timeScale = 1f;

        int nextIndex = PlayerPrefs.GetInt("NextLevelIndex", -1);
        int total = SceneManager.sceneCountInBuildSettings;
        int current = SceneManager.GetActiveScene().buildIndex;

        bool hasNextLevel = (nextIndex != -1 && nextIndex < total);

        // ✅ Đổi text
        if (nextText != null)
        {
            nextText.text = hasNextLevel ?
                "Press Space to Next Level" :
                "Press Space to Try Again";
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            LoadNextLevel();
    }

    void LoadNextLevel()
    {
        int nextIndex = PlayerPrefs.GetInt("NextLevelIndex", -1);
        int total = SceneManager.sceneCountInBuildSettings;

        if (nextIndex != -1 && nextIndex < total)
        {
            SceneManager.LoadScene(nextIndex);
        }
        else
        {
            SceneManager.LoadScene(0); // Về menu
        }
    }
}
