using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// CharacterScene の詳細表示を担当する。
/// UI は未配置でも自動生成され、後から Inspector 上の専用 UI に差し替えられる。
/// </summary>
public sealed class CharacterSceneController : MonoBehaviour
{
    [SerializeField] private RectTransform detailPanel;
    [SerializeField] private Image characterImage;
    [SerializeField] private Text nameText;
    [SerializeField] private Text statusText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private Text reactionText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private GameObject characterPlaceholder;
    [SerializeField] private float slideSeconds = 0.25f;

    private CharacterData currentCharacter;
    private Vector2 shownPosition;
    private Coroutine panelAnimation;
    private Character_behavior sharedTouchReaction;
    private CharacterPortraitLayout portraitLayout;
    private RectTransform reactionRoot;

    private bool IsFormationSelection => HomeScenePanelState.HasSavedState ||
        PlayerPrefs.HasKey("CurrentSelectingSlot");

    private void Awake()
    {
        if (!HasRequiredReferences()) BuildDefaultDetailPanel();
        if (portraitLayout == null)
        {
            Canvas ownerCanvas = characterImage.GetComponentInParent<Canvas>();
            if (ownerCanvas != null) portraitLayout = ownerCanvas.GetComponent<CharacterPortraitLayout>();
        }
        if (sharedTouchReaction == null)
        {
            sharedTouchReaction = characterImage.GetComponent<Character_behavior>() ??
                                  characterImage.gameObject.AddComponent<Character_behavior>();
        }
        if (characterPlaceholder == null)
        {
            characterPlaceholder = CreatePlaceholder(characterImage.transform.parent);
            Button placeholderButton = characterPlaceholder.AddComponent<Button>();
            placeholderButton.targetGraphic = characterPlaceholder.GetComponent<Image>();
            placeholderButton.transition = Selectable.Transition.None;
            placeholderButton.onClick.AddListener(ReactToCharacter);
        }
        shownPosition = detailPanel.anchoredPosition;
        detailPanel.gameObject.SetActive(false);
    }

    private bool HasRequiredReferences()
    {
        return detailPanel != null &&
               characterImage != null &&
               nameText != null &&
               statusText != null &&
               descriptionText != null &&
               reactionText != null &&
               confirmButton != null;
    }

    public void ShowCharacter(CharacterData data)
    {
        if (data == null) return;
        currentCharacter = data;
        characterImage.sprite = data.characterSprite;
        characterImage.preserveAspect = true;
        characterImage.enabled = data.characterSprite != null;
        if (characterPlaceholder != null)
            characterPlaceholder.SetActive(data.characterSprite == null);
        nameText.text = data.characterName;
        statusText.text = BuildStatusText(data);
        descriptionText.text = data.description;
        reactionText.text = "キャラクターをタッチ！";
        confirmButton.gameObject.SetActive(IsFormationSelection);
        float uiJumpHeight = portraitLayout != null ? portraitLayout.uiJumpHeight : 55f;
        sharedTouchReaction.ConfigureForCharacterUI(data, reactionText, uiJumpHeight);
        if (portraitLayout != null)
        {
            sharedTouchReaction.ApplyTouchReactionSettings(
                portraitLayout.animationDuration,
                portraitLayout.uiJumpHeight,
                portraitLayout.jumpRepeatCount,
                portraitLayout.swingAngle,
                portraitLayout.swingCount,
                portraitLayout.zoomScale,
                portraitLayout.zoomRepeatCount,
                portraitLayout.touchCooldown);
        }

        detailPanel.gameObject.SetActive(true);
        detailPanel.SetAsLastSibling();
        StartPanelAnimation(true);
    }

    public void ClosePanel()
    {
        if (detailPanel.gameObject.activeSelf) StartPanelAnimation(false);
    }

    public void ReactToCharacter()
    {
        if (currentCharacter != null && sharedTouchReaction != null)
            sharedTouchReaction.ReactToTouch();
    }

    public void ConfirmSelection()
    {
        if (currentCharacter == null || !IsFormationSelection) return;
        PlayerPrefs.SetInt("TempSelectedCharacterId", currentCharacter.characterId);
        PlayerPrefs.SetString("TempSelectedCharacterName", currentCharacter.characterName);
        PlayerPrefs.Save();
        string sceneName = HomeScenePanelState.HasSavedState
            ? HomeScenePanelState.ReturnSceneName
            : PlayerPrefs.GetString("ReturnSceneName", "HomeScene");
        SceneManager.LoadScene(string.IsNullOrEmpty(sceneName) ? "HomeScene" : sceneName);
    }

    private string BuildStatusText(CharacterData data)
    {
        // 新しいステータスを追加する場合は、この表示モデルだけを拡張する。
        return $"HP  {data.maxHP}\n移動速度  {data.moveSpeed:0.##}\n使用可能な攻撃  {data.availableAttacks.Count}";
    }

    private void StartPanelAnimation(bool show)
    {
        if (panelAnimation != null) StopCoroutine(panelAnimation);
        panelAnimation = StartCoroutine(SlidePanel(show));
    }

    private IEnumerator SlidePanel(bool show)
    {
        Vector2 hidden = shownPosition + Vector2.up * (detailPanel.rect.height + 80f);
        Vector2 from = show ? hidden : detailPanel.anchoredPosition;
        Vector2 to = show ? shownPosition : hidden;
        float elapsed = 0f;
        while (elapsed < slideSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / slideSeconds));
            detailPanel.anchoredPosition = Vector2.LerpUnclamped(from, to, t);
            yield return null;
        }
        detailPanel.anchoredPosition = to;
        if (!show) detailPanel.gameObject.SetActive(false);
        panelAnimation = null;
    }

    private void BuildDefaultDetailPanel()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("CharacterCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }
        portraitLayout = canvas.GetComponent<CharacterPortraitLayout>();

        detailPanel = CreateUI("CharacterDetailPanel", canvas.transform).GetComponent<RectTransform>();
        detailPanel.anchorMin = Vector2.zero;
        detailPanel.anchorMax = Vector2.one;
        detailPanel.offsetMin = detailPanel.offsetMax = Vector2.zero;
        detailPanel.gameObject.AddComponent<Image>().color = new Color(0.08f, 0.1f, 0.16f, 0.97f);

        GameObject portraitViewport = CreateUI("UpperBodyViewport", detailPanel);
        Vector2 viewportMin = portraitLayout != null
            ? portraitLayout.viewportAnchorMin
            : new Vector2(0.03f, 0.16f);
        Vector2 viewportMax = portraitLayout != null
            ? portraitLayout.viewportAnchorMax
            : new Vector2(0.55f, 0.84f);
        SetRect(portraitViewport.GetComponent<RectTransform>(), viewportMin, viewportMax);
        portraitViewport.AddComponent<RectMask2D>();

        // この親のPivotを中央に固定し、共通スクリプトによる回転の中心にする。
        GameObject reactionObject = CreateUI("CharacterReactionRoot", portraitViewport.transform);
        reactionRoot = reactionObject.GetComponent<RectTransform>();
        SetRect(reactionRoot, Vector2.zero, Vector2.one);

        characterImage = CreateUI("UpperBodyCharacter", reactionRoot).AddComponent<Image>();
        RectTransform characterRect = characterImage.rectTransform;
        characterRect.anchorMin = new Vector2(0.5f, 1f);
        characterRect.anchorMax = new Vector2(0.5f, 1f);
        characterRect.pivot = new Vector2(0.5f, 1f);
        characterRect.anchoredPosition = portraitLayout != null
            ? portraitLayout.imagePosition
            : Vector2.zero;
        characterRect.sizeDelta = portraitLayout != null
            ? portraitLayout.imageSize
            : new Vector2(680f, 980f);
        characterImage.color = Color.white;
        sharedTouchReaction = reactionObject.AddComponent<Character_behavior>();
        Button touchButton = characterImage.gameObject.AddComponent<Button>();
        touchButton.targetGraphic = characterImage;
        touchButton.transition = Selectable.Transition.None;
        touchButton.onClick.AddListener(ReactToCharacter);

        characterPlaceholder = CreatePlaceholder(reactionRoot);
        Button placeholderButton = characterPlaceholder.AddComponent<Button>();
        placeholderButton.targetGraphic = characterPlaceholder.GetComponent<Image>();
        placeholderButton.transition = Selectable.Transition.None;
        placeholderButton.onClick.AddListener(ReactToCharacter);

        nameText = CreateText("CharacterName", detailPanel, 34, TextAnchor.MiddleCenter);
        SetRect(nameText.rectTransform, new Vector2(0.05f, 0.85f), new Vector2(0.95f, 0.97f));
        statusText = CreateText("Status", detailPanel, 25, TextAnchor.UpperLeft);
        SetRect(statusText.rectTransform, new Vector2(0.56f, 0.55f), new Vector2(0.94f, 0.82f));
        descriptionText = CreateText("Description", detailPanel, 21, TextAnchor.UpperLeft);
        SetRect(descriptionText.rectTransform, new Vector2(0.56f, 0.24f), new Vector2(0.94f, 0.53f));
        reactionText = CreateText("Reaction", detailPanel, 22, TextAnchor.MiddleCenter);
        SetRect(reactionText.rectTransform, new Vector2(0.05f, 0.04f), new Vector2(0.55f, 0.14f));

        Button close = CreateButton("CloseButton", detailPanel, "閉じる", ClosePanel);
        SetRect(close.GetComponent<RectTransform>(), new Vector2(0.78f, 0.04f), new Vector2(0.94f, 0.14f));
        confirmButton = CreateButton("ConfirmButton", detailPanel, "選択", ConfirmSelection);
        SetRect(confirmButton.GetComponent<RectTransform>(), new Vector2(0.58f, 0.04f), new Vector2(0.75f, 0.14f));
    }

    private static GameObject CreateUI(string name, Transform parent)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        return obj;
    }

    private static Text CreateText(string name, Transform parent, int size, TextAnchor alignment)
    {
        Text text = CreateUI(name, parent).AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = size;
        text.color = Color.white;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    private static Button CreateButton(string name, Transform parent, string label, UnityEngine.Events.UnityAction action)
    {
        GameObject obj = CreateUI(name, parent);
        Image image = obj.AddComponent<Image>();
        image.color = new Color(0.2f, 0.55f, 0.85f, 1f);
        Button button = obj.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);
        Text text = CreateText("Text", obj.transform, 22, TextAnchor.MiddleCenter);
        text.text = label;
        SetRect(text.rectTransform, Vector2.zero, Vector2.one);
        return button;
    }

    private static GameObject CreatePlaceholder(Transform parent)
    {
        GameObject placeholder = CreateUI("CharacterPlaceholder", parent);
        RectTransform rect = placeholder.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(260f, 260f);
        rect.anchoredPosition = Vector2.zero;
        rect.localRotation = Quaternion.Euler(0f, 0f, 45f);
        Image shape = placeholder.AddComponent<Image>();
        shape.color = new Color(0.28f, 0.34f, 0.46f, 1f);

        Text question = CreateText("QuestionMark", placeholder.transform, 96, TextAnchor.MiddleCenter);
        question.text = "?";
        question.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -45f);
        SetRect(question.rectTransform, Vector2.zero, Vector2.one);
        placeholder.SetActive(false);
        return placeholder;
    }

    private static void SetRect(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

}
