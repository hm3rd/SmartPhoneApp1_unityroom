using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>HomeScene のメニュー入口と CharacterScene の管理コンポーネントを保証する。</summary>
public static class CharacterMenuBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Register()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = scene.name;
        if (sceneName == "HomeScene") EnsureHomeMenu();
        if (sceneName == "CharacterScene" && Object.FindFirstObjectByType<CharacterSceneController>() == null)
            new GameObject(nameof(CharacterSceneController)).AddComponent<CharacterSceneController>();
    }

    private static void EnsureHomeMenu()
    {
        if (Object.FindFirstObjectByType<HomeMenuController>() != null) return;
        GameObject uiCanvasObject = GameObject.Find("UICanvas");
        GameObject owner = uiCanvasObject != null
            ? uiCanvasObject
            : new GameObject(nameof(HomeMenuController));
        owner.AddComponent<HomeMenuController>();
    }
}
