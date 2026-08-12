using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>HomeScene の手動配置した MENU ボタンから呼び出す。</summary>
public sealed class CharacterMenuButton : MonoBehaviour
{
    [SerializeField] private string characterSceneName = "CharacterScene";

    public void OpenCharacterMenu()
    {
        HomeScenePanelState.Clear();
        PlayerPrefs.DeleteKey("CurrentSelectingSlot");
        PlayerPrefs.DeleteKey("ReturnSceneName");
        PlayerPrefs.Save();
        SceneManager.LoadScene(characterSceneName);
    }
}
