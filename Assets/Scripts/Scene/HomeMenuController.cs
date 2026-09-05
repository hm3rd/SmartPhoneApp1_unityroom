using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>HomeSceneの拡張可能なメニューと各画面への入口を管理する。</summary>
public sealed class HomeMenuController : MonoBehaviour
{
    public enum MenuActionType
    {
        LoadScene,
        OpenPanel,
        ReturnHome,
        CloseMenu,
        QuitGame
    }

    [Serializable]
    public class MenuEntry
    {
        [Tooltip("ボタンに表示する文字")]
        public string label = "メニュー項目";
        public MenuActionType actionType = MenuActionType.LoadScene;
        [Tooltip("Load Sceneの場合に使用するScene名")]
        public string sceneName;
        [Tooltip("Open Panelの場合に表示するPanel")]
        public GameObject targetPanel;
        [Tooltip("Target Panel未設定時にHierarchyから検索するオブジェクト名")]
        public string targetObjectName;
        [Tooltip("項目実行後にメニューを閉じます")]
        public bool closeMenuAfterAction = true;
        public bool interactable = true;
    }

    [Header("配置済みUI（未設定なら自動生成）")]
    [SerializeField] private Button menuButton;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private RectTransform buttonContainer;
    [SerializeField] private Button closeButton;
    [Tooltip("任意。未設定なら標準Buttonを生成します")]
    [SerializeField] private Button menuItemButtonPrefab;

    [Header("自動生成")]
    [SerializeField] private bool buildMissingUIAutomatically = true;
    [SerializeField] private string menuButtonLabel = "メニュー";

    [Header("自動生成メニューボタンの配置")]
    [Tooltip("メニューボタンの幅と高さ")]
    [SerializeField] private Vector2 menuButtonSize = new Vector2(190f, 72f);
    [Tooltip("画面内の基準位置。右上は(1, 1)、中央は(0.5, 0.5)、左下は(0, 0)")]
    [SerializeField] private Vector2 menuButtonAnchor = Vector2.one;
    [Tooltip("ボタン自身の基準点。通常はAnchorと同じ値にします")]
    [SerializeField] private Vector2 menuButtonPivot = Vector2.one;
    [Tooltip("Anchorからの位置。右上配置ではXとYをマイナスにすると画面内側へ移動します")]
    [SerializeField] private Vector2 menuButtonPosition = new Vector2(-24f, -24f);

    [Header("自動生成メニュー項目の配置")]
    [SerializeField] private Vector2 itemButtonSize = new Vector2(360f, 72f);
    [Min(0f)] [SerializeField] private float itemSpacing = 14f;
    [Min(12)] [SerializeField] private int fontSize = 28;

    [Header("メニュー項目（上から順に表示）")]
    [SerializeField] private List<MenuEntry> menuEntries = new List<MenuEntry>();

    private readonly List<Button> generatedButtons = new List<Button>();

    private void Awake()
    {
        EnsureDefaultEntries();
        if (buildMissingUIAutomatically &&
            (menuButton == null || menuPanel == null || buttonContainer == null))
        {
            BuildDefaultUI();
        }

        WireFixedButtons();
        RebuildMenuButtons();
        SetMenuVisible(false);
    }

    private void EnsureDefaultEntries()
    {
        if (menuEntries != null && menuEntries.Count > 0) return;
        menuEntries = new List<MenuEntry>
        {
            new MenuEntry { label = "ホーム", actionType = MenuActionType.ReturnHome },
            new MenuEntry { label = "出撃", actionType = MenuActionType.OpenPanel, targetObjectName = "StageSelectPanel" },
            new MenuEntry { label = "キャラクター", actionType = MenuActionType.LoadScene, sceneName = "CharacterScene" },
            new MenuEntry { label = "ガチャ", actionType = MenuActionType.LoadScene, sceneName = "GatyaScene" },
            new MenuEntry { label = "ショップ", actionType = MenuActionType.LoadScene, sceneName = "ShopScene" }
        };
    }

    private void WireFixedButtons()
    {
        if (menuButton != null)
        {
            menuButton.onClick.RemoveListener(OpenMenu);
            menuButton.onClick.AddListener(OpenMenu);
        }
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseMenu);
            closeButton.onClick.AddListener(CloseMenu);
        }
    }

    [ContextMenu("メニューボタンを再構築")]
    public void RebuildMenuButtons()
    {
        foreach (Button button in generatedButtons)
            if (button != null) Destroy(button.gameObject);
        generatedButtons.Clear();

        if (buttonContainer == null) return;
        for (int i = 0; i < menuEntries.Count; i++)
        {
            MenuEntry entry = menuEntries[i];
            if (entry == null) continue;
            int entryIndex = i;
            Button button = CreateEntryButton(entry.label);
            button.interactable = entry.interactable;
            button.onClick.AddListener(() => ExecuteEntry(entryIndex));
            generatedButtons.Add(button);
        }
    }

    public void OpenMenu() => SetMenuVisible(true);
    public void CloseMenu() => SetMenuVisible(false);

    /// <summary>Sceneを再読込せず、HomeSceneの初期表示（全パネル非表示）へ戻す。</summary>
    public void ReturnToHome()
    {
        CloseMenu();
        PanelManager[] panelManagers = FindObjectsOfType<PanelManager>();
        if (panelManagers.Length > 0)
            panelManagers[0].CloseAllPanels();
        ClearTemporaryPanelState();
    }

    public static void ClearTemporaryPanelState()
    {
        HomeScenePanelState.Clear();
        PlayerPrefs.DeleteKey("ShowPreparationPanel");
        PlayerPrefs.DeleteKey("ReturnSceneName");
        PlayerPrefs.DeleteKey("CurrentSelectingSlot");
        PlayerPrefs.DeleteKey("PreparingStageIndex");
        PlayerPrefs.Save();
    }

    public void ExecuteEntry(int index)
    {
        if (index < 0 || index >= menuEntries.Count) return;
        MenuEntry entry = menuEntries[index];
        if (entry == null || !entry.interactable) return;

        switch (entry.actionType)
        {
            case MenuActionType.LoadScene:
                LoadTargetScene(entry.sceneName);
                break;
            case MenuActionType.OpenPanel:
            {
                GameObject panel = entry.targetPanel;
                if (panel == null && !string.IsNullOrWhiteSpace(entry.targetObjectName))
                    panel = FindSceneObject(entry.targetObjectName);
                if (panel != null) panel.SetActive(true);
                else Debug.LogWarning($"{entry.label}: Target Panelが未設定です。", this);
                break;
            }
            case MenuActionType.ReturnHome:
                ReturnToHome();
                return;
            case MenuActionType.CloseMenu:
                CloseMenu();
                return;
            case MenuActionType.QuitGame:
                Application.Quit();
                break;
        }

        if (entry.closeMenuAfterAction) CloseMenu();
    }

    private void LoadTargetScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("遷移先Scene Nameが未設定です。", this);
            return;
        }

        if (sceneName == "CharacterScene")
        {
            ClearTemporaryPanelState();
        }
        SceneManager.LoadScene(sceneName);
    }

    private void SetMenuVisible(bool visible)
    {
        if (menuPanel != null) menuPanel.SetActive(visible);
    }

    private static GameObject FindSceneObject(string objectName)
    {
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            Transform found = FindChildRecursive(root.transform, objectName);
            if (found != null) return found.gameObject;
        }
        return null;
    }

    private static Transform FindChildRecursive(Transform current, string objectName)
    {
        if (current.name == objectName) return current;
        for (int i = 0; i < current.childCount; i++)
        {
            Transform found = FindChildRecursive(current.GetChild(i), objectName);
            if (found != null) return found;
        }
        return null;
    }

    private void BuildDefaultUI()
    {
        GameObject canvasObject = new GameObject(
            "Home Menu Canvas", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        // 既存UICanvasがWorld Spaceでも影響を受けない独立Overlay Canvasにする。
        canvasObject.transform.SetParent(null, false);
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 2000;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        menuButton = CreateButton("MenuButton", canvas.transform, menuButtonLabel, menuButtonSize);
        RectTransform menuRect = menuButton.GetComponent<RectTransform>();
        menuRect.anchorMin = menuRect.anchorMax = menuButtonAnchor;
        menuRect.pivot = menuButtonPivot;
        menuRect.anchoredPosition = menuButtonPosition;

        menuPanel = new GameObject("HomeMenuPanel", typeof(RectTransform), typeof(Image));
        menuPanel.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = menuPanel.GetComponent<RectTransform>();
        Stretch(panelRect);
        menuPanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.72f);

        GameObject window = new GameObject("MenuWindow", typeof(RectTransform), typeof(Image));
        window.transform.SetParent(menuPanel.transform, false);
        RectTransform windowRect = window.GetComponent<RectTransform>();
        windowRect.anchorMin = new Vector2(0.5f, 0.5f);
        windowRect.anchorMax = new Vector2(0.5f, 0.5f);
        windowRect.sizeDelta = new Vector2(460f, 760f);
        window.GetComponent<Image>().color = new Color(0.08f, 0.12f, 0.2f, 0.98f);

        GameObject container = new GameObject(
            "ButtonContainer", typeof(RectTransform), typeof(VerticalLayoutGroup),
            typeof(ContentSizeFitter));
        container.transform.SetParent(window.transform, false);
        buttonContainer = container.GetComponent<RectTransform>();
        buttonContainer.anchorMin = new Vector2(0.5f, 1f);
        buttonContainer.anchorMax = new Vector2(0.5f, 1f);
        buttonContainer.pivot = new Vector2(0.5f, 1f);
        buttonContainer.anchoredPosition = new Vector2(0f, -80f);
        buttonContainer.sizeDelta = new Vector2(itemButtonSize.x, 0f);
        VerticalLayoutGroup layout = container.GetComponent<VerticalLayoutGroup>();
        layout.spacing = itemSpacing;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        container.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        closeButton = CreateButton("CloseButton", window.transform, "閉じる", new Vector2(180f, 64f));
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = closeRect.anchorMax = new Vector2(0.5f, 0f);
        closeRect.pivot = new Vector2(0.5f, 0f);
        closeRect.anchoredPosition = new Vector2(0f, 30f);
    }

    private Button CreateEntryButton(string label)
    {
        Button button;
        if (menuItemButtonPrefab != null)
            button = Instantiate(menuItemButtonPrefab, buttonContainer);
        else
            button = CreateButton(label + "Button", buttonContainer, label, itemButtonSize);

        Text text = button.GetComponentInChildren<Text>(true);
        if (text != null)
        {
            JapaneseFontProvider.Apply(text);
            text.text = label;
            text.fontSize = fontSize;
        }
        return button;
    }

    private Button CreateButton(string name, Transform parent, string label, Vector2 size)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        obj.transform.SetParent(parent, false);
        obj.GetComponent<RectTransform>().sizeDelta = size;
        obj.GetComponent<Image>().color = new Color(0.16f, 0.3f, 0.5f, 1f);
        Button button = obj.GetComponent<Button>();
        button.targetGraphic = obj.GetComponent<Image>();
        LayoutElement element = obj.GetComponent<LayoutElement>();
        element.preferredWidth = size.x;
        element.preferredHeight = size.y;

        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(obj.transform, false);
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        Stretch(textRect);
        Text text = textObject.GetComponent<Text>();
        JapaneseFontProvider.Apply(text);
        text.text = label;
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        return button;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }
}
