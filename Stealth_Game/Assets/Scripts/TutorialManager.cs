using UnityEngine;
using UnityEngine.UI; // <-- Quan trọng: Phải có dòng này cho RawImage
using TMPro; // Phải có dòng này cho Text

public class TutorialManager : MonoBehaviour
{
    // Cấu trúc để lưu trữ thông tin cho mỗi bước hướng dẫn
    [System.Serializable]
    public struct TutorialStep
    {
        [TextArea(3, 10)]
        public string textContent;
        public Texture2D imageContent; // <-- Sẽ nhận file .png hoặc .jpg
    }

    // === KÉO VÀO TRONG INSPECTOR ===
    [Header("UI Components")]
    public GameObject tutorialPanel;
    public TextMeshProUGUI tutorialText;
    public RawImage tutorialRawImage; // <-- Sẽ nhận đối tượng RawImage từ Hierarchy

    public GameObject nextButton;
    public GameObject closeButton;

    [Header("Tutorial Content")]
    public TutorialStep[] tutorialSteps;

    private int currentStep = 0;

    void Start()
    {
        // Khi scene bắt đầu, chúng ta sẽ bật panel và dừng game
        tutorialPanel.SetActive(true);
        Time.timeScale = 0f;

        // Bắt đầu ở bước đầu tiên
        currentStep = 0;
        UpdateTutorialUI();
    }

    // Hàm này cập nhật nội dung Text, Image và hiển thị đúng nút (Next/Close)
    private void UpdateTutorialUI()
    {
        if (currentStep >= 0 && currentStep < tutorialSteps.Length)
        {
            // Cập nhật nội dung text
            tutorialText.text = tutorialSteps[currentStep].textContent;

            // Cập nhật hình ảnh (Texture) cho RawImage
            if (tutorialRawImage != null)
            {
                Texture2D textureToShow = tutorialSteps[currentStep].imageContent;

                if (textureToShow != null)
                {
                    // Có ảnh cho bước này
                    tutorialRawImage.gameObject.SetActive(true);
                    tutorialRawImage.texture = textureToShow; // Gán texture cho RawImage
                }
                else
                {
                    // Không có ảnh, ẩn nó đi
                    tutorialRawImage.gameObject.SetActive(false);
                }
            }

            // Cập nhật các nút
            if (currentStep == tutorialSteps.Length - 1)
            {
                // Đây là bước cuối cùng
                nextButton.SetActive(false);
                closeButton.SetActive(true);
            }
            else
            {
                // Đây chưa phải bước cuối cùng
                nextButton.SetActive(true);
                closeButton.SetActive(false);
            }
        }
    }

    // == GỌI TỪ NÚT "NEXT" ==
    public void GoToNextStep()
    {
        if (currentStep < tutorialSteps.Length - 1)
        {
            currentStep++;
            UpdateTutorialUI();
        }
    }

    // == GỌI TỪ NÚT "CLOSE" ==
    public void CloseTutorial()
    {
        tutorialPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}