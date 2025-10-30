using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

    public void PlayGame()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int next = currentIndex + 1;
        int total = SceneManager.sceneCountInBuildSettings;

        // ✅ Bỏ qua các scene trung gian như "CharacterSelect" hoặc "Stage"
        while (next < total)
        {
            string nextPath = SceneUtility.GetScenePathByBuildIndex(next).ToLower();

            if (!nextPath.Contains("characterselect") && !nextPath.Contains("stage"))
                break;

            next++;
        }

        // ✅ Nếu tìm được scene hợp lệ thì load
        if (next < total)
        {
            SceneManager.LoadScene(next);
        }
        else
        {
            Debug.LogWarning("⚠️ Không có scene nào hợp lệ để load sau Menu!");
        }
    }

    public void QuitGame () {
		Debug.Log("Quit");
		Application.Quit();
	}
    // 🆕 Nút chọn nhân vật
    public void SelectCharacter()
    {
        SceneManager.LoadScene("CharacterSelect"); // tên scene bạn đã tạo
    }
}
