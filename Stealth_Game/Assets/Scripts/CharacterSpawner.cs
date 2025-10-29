
using UnityEngine;

public class CharacterSpawner : MonoBehaviour
{
    public GameObject heroRockPrefab;
    public GameObject heroNaturePrefab;
    public GameObject heroIcePrefab;
    public Transform spawnPoint;

    void Start()
    {
        string selected = PlayerPrefs.GetString("SelectedCharacter", "HeroRock");

        GameObject prefabToSpawn = heroRockPrefab;
        switch (selected)
        {
            case "HeroNature": prefabToSpawn = heroNaturePrefab; break;
            case "HeroIce": prefabToSpawn = heroIcePrefab; break;
        }

        // 🟢 Spawn model con của Player
        GameObject model = Instantiate(prefabToSpawn, transform);
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;

        Player player = GetComponent<Player>();
        if (player != null)
        {
            Animator modelAnimator = model.GetComponentInChildren<Animator>();
            if (modelAnimator != null)
            {
                player.SetAnimator(modelAnimator);
            }
            else
            {
                Debug.LogWarning("⚠️ Model được spawn không có Animator!");
            }
        }

        // Nếu có spawnPoint, đặt Player đến vị trí spawn
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
        }
    }
}
