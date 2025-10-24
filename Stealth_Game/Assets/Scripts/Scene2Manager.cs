using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene2Manager : MonoBehaviour
{
    private bool canGoToMenu = false;
    public GameObject winScreen;
    void Start()
    {
        
    // 1. Đảm bảo chuột hiện ra
    Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 2. HIỂN THỊ MÀN HÌNH THẮNG TRONG SCENE 2
        if (winScreen != null)
        {
            winScreen.SetActive(true);
        }

        canGoToMenu = true;
    }

    void Update()
    {
        if (canGoToMenu)
        {
            // 2. Chuyển về Main Menu (Scene 0) khi nhấn Space
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SceneManager.LoadScene(0);
            }
        }
    }
}