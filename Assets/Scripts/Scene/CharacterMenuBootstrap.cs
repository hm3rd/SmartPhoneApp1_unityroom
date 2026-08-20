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
        if (sceneName == "HomeScene") EnsureHomeMenuButton();
        if (sceneName == "CharacterScene" && Object.FindFirstObjectByType<CharacterSceneController>() == null)
            new GameObject(nameof(CharacterSceneController)).AddComponent<CharacterSceneController>();
    }

    private static void EnsureHomeMenuButton()
    {
        if (GameObject.Find("CharacterMenuButton") != null) return;
        GameObject uiCanvasObject = GameObject.Find("UICanvas");
        Canvas canvas = uiCanvasObject != null
            ? uiCanvasObject.GetComponent<Canvas>()
            : Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject obj = new GameObject("CharacterMenuButton", typeof(RectTransform), typeof(Image), typeof(Button));
        obj.transform.SetParent(canvas.transform, false);
        RectTransform rect = obj.GetComponent<RectTransform>();
        CharacterMenuLayout layout = canvas.GetComponent<CharacterMenuLayout>();
        Vector2 anchor = layout != null ? layout.anchor : new Vector2(1f, 1f);
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = layout != null ? layout.anchoredPosition : new Vector2(-24f, -24f);
        rect.sizeDelta = layout != null ? layout.size : new Vector2(190f, 72f);
        obj.GetComponent<Image>().color = new Color(0.12f, 0.18f, 0.28f, 0.94f);
        obj.GetComponent<Button>().onClick.AddListener(OpenCharacterMenu);

        GameObject labelObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
        labelObject.transform.SetParent(obj.transform, false);
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
        Text label = labelObject.GetComponent<Text>();
        JapaneseFontProvider.Apply(label);
        label.text = "メニュー";
        label.fontSize = 28;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
    }

    private static void OpenCharacterMenu()
    {
        HomeScenePanelState.Clear();
        PlayerPrefs.DeleteKey("CurrentSelectingSlot");
        PlayerPrefs.DeleteKey("ReturnSceneName");
        PlayerPrefs.Save();
        SceneManager.LoadScene("CharacterScene");
    }
}
