using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{

    public GameObject gameOver;
    public GameObject gameWin; // Vẫn cần tham chiếu để hủy đăng ký event, nhưng không dùng để SetActive(true)
    bool gameIsOver;

    void Start()
    {
        // Đăng ký sự kiện thua
        Guard.OnGuardHasSpottedPlayer += ShowGameLose;

        // *********************************************************
        // KHẮC PHỤC LỖI NULL REFERENCE: KIỂM TRA PLAYER TRƯỚC
        // *********************************************************
        Player player = FindFirstObjectByType<Player>();

        if (player != null)
        {
            player.OnReachEndOfLevel += ShowGameWin;
        }
        // *********************************************************
    }

    void Update()
    {
        if (gameIsOver)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SceneManager.LoadScene(0); // Quay về Main Menu
            }
        }
    }

    // ************ CHỈ TẢI SCENE KHI THẮNG ************
    void ShowGameWin()
    {
        if (gameIsOver) return;

        gameIsOver = true;

        // Hủy đăng ký event trước khi chuyển Scene
        Guard.OnGuardHasSpottedPlayer -= ShowGameLose;
        Player player = FindFirstObjectByType<Player>();
        if (player != null)
        {
            player.OnReachEndOfLevel -= ShowGameWin;
        }

        // TẢI SCENE 2 ĐỂ HIỂN THỊ MÀN HÌNH THẮNG
        SceneManager.LoadScene(2);
    }
    // ***************************************************

    void ShowGameLose()
    {
        OnGameOver(gameOver);
    }

    // ************ LOGIC THUA (Chỉ hiển thị UI trên Scene hiện tại) ************
    void OnGameOver(GameObject gameOverScreen)
    {
        if (gameIsOver) return;

        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);
        }

        gameIsOver = true;

        // Mở khóa chuột khi THUA
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Hủy đăng ký event
        Guard.OnGuardHasSpottedPlayer -= ShowGameLose;
        Player player = FindFirstObjectByType<Player>();
        if (player != null)
        {
            player.OnReachEndOfLevel -= ShowGameWin;
        }
    }
}