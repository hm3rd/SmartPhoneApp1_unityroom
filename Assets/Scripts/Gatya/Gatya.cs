using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// キャラクターガチャの抽選・石消費・獲得登録・結果演出を管理する。
/// UI未設定時はGatyaSceneの既存ボタンを名前で取得し、結果パネルを自動生成する。
/// </summary>
public class Gatya : MonoBehaviour
{
    [System.Serializable]
    public class CharacterResultEvent : UnityEvent<CharacterData> { }

    public enum DuplicateMode
    {
        KeepAsOwned
    }

    [Header("データ")]
    [SerializeField] private CharacterCatalog catalog;
    [SerializeField] private DuplicateMode duplicateMode = DuplicateMode.KeepAsOwned;

    [Header("価格")]
    [Min(0)] [SerializeField] private int singleCost = 3;
    [Min(0)] [SerializeField] private int tenPullCost = 30;

    [Header("既存UI（未設定なら自動検索）")]
    [SerializeField] private Button singlePullButton;
    [SerializeField] private Button tenPullButton;
    [SerializeField] private Text stoneText;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private Transform resultContainer;
    [SerializeField] private Text messageText;

    [Header("演出")]
    [Min(0f)] [SerializeField] private float introDuration = 0.25f;
    [Min(0f)] [SerializeField] private float characterInterval = 0.25f;

    [Header("イベント")]
    [SerializeField] private UnityEvent onRollStarted;
    [SerializeField] private UnityEvent onRollFinished;
    [SerializeField] private CharacterResultEvent onCharacterRevealed;

    private readonly List<CharacterData> lastResults = new List<CharacterData>();
    private bool isRolling;
    private Transform overlayRoot;

    public IReadOnlyList<CharacterData> LastResults => lastResults;

    private void Awake()
    {
        if (catalog == null)
        {
            catalog = Resources.Load<CharacterCatalog>("CharacterCatalog");
        }

        FindExistingButtons();
        EnsureOverlayCanvas();
        EnsureStoneUI();
        EnsureResultUI();

        if (singlePullButton != null) singlePullButton.onClick.AddListener(PullOnce);
        if (tenPullButton != null) tenPullButton.onClick.AddListener(PullTen);
    }

    private void OnEnable()
    {
        PlayerStoneWallet.AmountChanged += RefreshStoneText;
        RefreshStoneText(PlayerStoneWallet.Amount);
    }

    private void OnDisable()
    {
        PlayerStoneWallet.AmountChanged -= RefreshStoneText;
    }

    public void PullOnce()
    {
        TryStartRoll(1, singleCost);
    }

    public void PullTen()
    {
        TryStartRoll(10, tenPullCost);
    }

    public void CloseResults()
    {
        if (!isRolling && resultPanel != null)
        {
            resultPanel.SetActive(false);
        }
    }

    private void TryStartRoll(int count, int cost)
    {
        if (isRolling) return;

        if (catalog == null || GetTotalWeight() <= 0)
        {
            ShowMessage("排出キャラクターが設定されていません");
            return;
        }

        if (!PlayerStoneWallet.TrySpend(cost))
        {
            ShowMessage($"石が足りません（必要: {cost}）");
            return;
        }

        StartCoroutine(RollRoutine(count));
    }

    private IEnumerator RollRoutine(int count)
    {
        isRolling = true;
        lastResults.Clear();
        SetButtonsInteractable(false);
        ClearResultItems();
        ConfigureResultGrid(count);
        resultPanel.SetActive(true);
        ShowMessage("召喚中...");
        onRollStarted?.Invoke();

        if (introDuration > 0f)
        {
            yield return new WaitForSeconds(introDuration);
        }

        messageText.text = string.Empty;
        int totalWeight = GetTotalWeight();
        CharacterDatabase ownedDatabase = CharacterDatabase.GetOrCreate();

        for (int i = 0; i < count; i++)
        {
            CharacterData character = DrawCharacter(totalWeight);
            if (character == null) continue;

            lastResults.Add(character);
            bool isNew = ownedDatabase.UnlockCharacter(character);
            CreateResultItem(character, isNew, count);
            onCharacterRevealed?.Invoke(character);

            if (characterInterval > 0f && i < count - 1)
            {
                yield return new WaitForSeconds(characterInterval);
            }
        }

        ShowMessage(count == 1 ? "獲得しました！" : "10回ガチャ結果");
        onRollFinished?.Invoke();
        isRolling = false;
        SetButtonsInteractable(true);
    }

    private CharacterData DrawCharacter(int totalWeight)
    {
        int roll = Random.Range(0, totalWeight);
        int accumulated = 0;
        foreach (CharacterData character in catalog.allCharacters)
        {
            if (character == null || !character.canBeObtainedFromGacha) continue;
            accumulated += Mathf.Max(1, character.gachaWeight);
            if (roll < accumulated) return character;
        }
        return null;
    }

    private int GetTotalWeight()
    {
        if (catalog == null) return 0;

        int total = 0;
        foreach (CharacterData character in catalog.allCharacters)
        {
            if (character != null && character.canBeObtainedFromGacha)
            {
                total += Mathf.Max(1, character.gachaWeight);
            }
        }
        return total;
    }

    private void FindExistingButtons()
    {
        if (singlePullButton == null)
        {
            GameObject target = GameObject.Find("Gatya1");
            if (target != null) singlePullButton = target.GetComponent<Button>();
        }
        if (tenPullButton == null)
        {
            GameObject target = GameObject.Find("Gatya10");
            if (target != null) tenPullButton = target.GetComponent<Button>();
        }
    }

    private void EnsureResultUI()
    {
        if (resultPanel != null && resultContainer != null && messageText != null) return;

        resultPanel = new GameObject("GachaResultPanel", typeof(RectTransform), typeof(Image), typeof(Button));
        resultPanel.transform.SetParent(overlayRoot, false);

        RectTransform panelRect = resultPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        resultPanel.GetComponent<Image>().color = new Color(0.05f, 0.04f, 0.12f, 0.94f);
        resultPanel.GetComponent<Button>().onClick.AddListener(CloseResults);

        GameObject title = CreateTextObject("Message", resultPanel.transform, 34);
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.1f, 0.82f);
        titleRect.anchorMax = new Vector2(0.9f, 0.96f);
        titleRect.offsetMin = titleRect.offsetMax = Vector2.zero;
        messageText = title.GetComponent<Text>();

        GameObject container = new GameObject("Results", typeof(RectTransform), typeof(GridLayoutGroup));
        container.transform.SetParent(resultPanel.transform, false);
        RectTransform containerRect = container.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.08f, 0.12f);
        containerRect.anchorMax = new Vector2(0.92f, 0.8f);
        containerRect.offsetMin = containerRect.offsetMax = Vector2.zero;
        GridLayoutGroup grid = container.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(120f, 150f);
        grid.spacing = new Vector2(12f, 12f);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 5;
        grid.childAlignment = TextAnchor.MiddleCenter;
        resultContainer = container.transform;

        resultPanel.SetActive(false);
    }

    private void ConfigureResultGrid(int resultCount)
    {
        if (resultContainer == null) return;

        GridLayoutGroup grid = resultContainer.GetComponent<GridLayoutGroup>();
        if (grid == null) return;

        if (resultCount == 1)
        {
            // 単発は画面中央に大きく表示
            grid.cellSize = new Vector2(520f, 680f);
            grid.spacing = Vector2.zero;
            grid.constraintCount = 1;
        }
        else
        {
            // 10連は5列×2段。1080幅に収まる範囲で大きく表示
            grid.cellSize = new Vector2(180f, 250f);
            grid.spacing = new Vector2(14f, 18f);
            grid.constraintCount = 5;
        }
    }

    private void EnsureStoneUI()
    {
        if (stoneText != null) return;

        GameObject stoneObject = CreateTextObject("StoneAmount", overlayRoot, 28);
        RectTransform rect = stoneObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.68f, 0.9f);
        rect.anchorMax = new Vector2(0.96f, 0.98f);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        stoneText = stoneObject.GetComponent<Text>();
        stoneText.alignment = TextAnchor.MiddleRight;
        stoneText.text = $"石: {PlayerStoneWallet.Amount}";
    }

    /// <summary>
    /// 既存のガチャCanvasはWorld Spaceなので、演出だけを画面座標のCanvasへ分離する。
    /// </summary>
    private void EnsureOverlayCanvas()
    {
        if (overlayRoot != null) return;

        GameObject canvasObject = new GameObject(
            "GachaOverlayCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));

        Canvas overlayCanvas = canvasObject.GetComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        overlayRoot = canvasObject.transform;
    }

    private void CreateResultItem(CharacterData character, bool isNew, int resultCount)
    {
        GameObject item = new GameObject(character.characterName, typeof(RectTransform), typeof(Image));
        item.transform.SetParent(resultContainer, false);
        item.GetComponent<Image>().color = character.themeColor * new Color(1f, 1f, 1f, 0.7f);

        GameObject iconObject = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconObject.transform.SetParent(item.transform, false);
        RectTransform iconRect = iconObject.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.08f, 0.25f);
        iconRect.anchorMax = new Vector2(0.92f, 0.95f);
        iconRect.offsetMin = iconRect.offsetMax = Vector2.zero;
        Image icon = iconObject.GetComponent<Image>();
        icon.sprite = character.characterIcon;
        icon.preserveAspect = true;

        GameObject label = CreateTextObject("Name", item.transform, resultCount == 1 ? 24 : 15);
        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.02f, 0.02f);
        labelRect.anchorMax = new Vector2(0.98f, 0.25f);
        labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
        label.GetComponent<Text>().text = isNew
            ? $"NEW! {character.characterName}"
            : character.characterName;

        StartCoroutine(RevealItem(item.transform));
    }

    private IEnumerator RevealItem(Transform item)
    {
        const float duration = 0.2f;
        float elapsed = 0f;
        item.localScale = Vector3.zero;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float value = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            item.localScale = Vector3.one * value;
            yield return null;
        }
        item.localScale = Vector3.one;
    }

    private static GameObject CreateTextObject(string name, Transform parent, int fontSize)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(parent, false);
        Text text = textObject.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return textObject;
    }

    private void ClearResultItems()
    {
        if (resultContainer == null) return;
        foreach (Transform child in resultContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private void ShowMessage(string message)
    {
        EnsureResultUI();
        resultPanel.SetActive(true);
        messageText.text = message;
    }

    private void RefreshStoneText(int amount)
    {
        if (stoneText != null) stoneText.text = $"石: {amount}";
    }

    private void SetButtonsInteractable(bool interactable)
    {
        if (singlePullButton != null) singlePullButton.interactable = interactable;
        if (tenPullButton != null) tenPullButton.interactable = interactable;
    }
}
