using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelectUI : MonoBehaviour
{
    public void SelectHeroRock()
    {
        PlayerPrefs.SetString("SelectedCharacter", "HeroRock");
        PlayerPrefs.Save();
    }

    public void SelectHeroNature()
    {
        PlayerPrefs.SetString("SelectedCharacter", "HeroNature");
        PlayerPrefs.Save();
    }
    public void SelectHeroIce()
    {
        PlayerPrefs.SetString("SelectedCharacter", "HeroIce");
        PlayerPrefs.Save();
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
