using UnityEngine;

public class CharacterChangeStation : MonoBehaviour
{
    [Header("Quản lý Model Trạm")]
    public GameObject[] stationModels; // Mảng gán từ Inspector

    // --- THÊM DÒNG NÀY ---
    [Tooltip("Dạng ban đầu của trạm này: 0=Rock, 1=Ice, 2=Nature")]
    public int startingModelIndex = 1; // Mặc định là 1 (Ice)

    private int currentModelIndex = 1;

    void Start()
    {
        // Tắt tất cả các model...
        for (int i = 0; i < stationModels.Length; i++)
        {
            if (stationModels[i] != null) stationModels[i].SetActive(false);
        }

        // --- SỬA CÁC DÒNG NÀY ---
        // Bật model ban đầu DỰA THEO BIẾN MỚI
        currentModelIndex = startingModelIndex;
        if (stationModels.Length > currentModelIndex && stationModels[currentModelIndex] != null)
        {
            stationModels[currentModelIndex].SetActive(true);
        }
        else
        {
            UnityEngine.Debug.LogWarning("Index model ban đầu không hợp lệ!");
        }
    }

    // --- CÁC HÀM CÒN LẠI GIỮ NGUYÊN ---
    // (Hàm SetModelActive và GetCurrentModelIndex không cần sửa)

    public void SetModelActive(int newIndex)
    {
        if (newIndex == currentModelIndex || newIndex >= stationModels.Length)
        {
            return;
        }
        if (stationModels[currentModelIndex] != null)
            stationModels[currentModelIndex].SetActive(false);
        currentModelIndex = newIndex;
        if (stationModels[currentModelIndex] != null)
        {
            stationModels[currentModelIndex].SetActive(true);
            UnityEngine.Debug.Log("Trạm đã HOÁN ĐỔI sang: " + stationModels[currentModelIndex].name);
        }
    }
    public int GetCurrentModelIndex()
    {
        return currentModelIndex;
    }
}