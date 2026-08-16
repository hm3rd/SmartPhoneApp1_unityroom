using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>ステージクリア結果の表示と、帰還・再出撃操作を管理する。</summary>
public sealed class GameResultPanelController : MonoBehaviour, IPointerClickHandler
{
    private const int PartySize = 3;
    private Transform characterRow;
    private Text resultText;
    private Button retryButton;
    private bool isShown;

    private void Awake()
    {
        BuildUI();
    }

    public void Show(
        int defeatedEnemies,
        long totalDamage,
        int earnedStones,
        float clearTimeSeconds,
        string evaluation,
        float remainingHealthRatio)
    {
        BuildUI();
        int minutes = Mathf.FloorToInt(clearTimeSeconds / 60f);
        float seconds = clearTimeSeconds - minutes * 60f;
        resultText.text =
            $"クリアタイム　{minutes:00}:{seconds:00.00}\n" +
            $"評価　{evaluation}　（残りHP {remainingHealthRatio * 100f:0}%）\n" +
            $"倒した敵　{defeatedEnemies}体\n" +
            $"与えたダメージ　{totalDamage:N0}\n" +
            $"獲得した石　{earnedStones}個";
        PopulateCharacters();
        isShown = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isShown) return;
        if (eventData.pointerPress != null &&
            eventData.pointerPress.GetComponentInParent<Button>() == retryButton)
        {
            return;
        }
        GoHome();
    }

    private void RetryStage()
    {
        isShown = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void GoHome()
    {
        isShown = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene("HomeScene");
    }

    private void PopulateCharacters()
    {
        foreach (Transform child in characterRow)
            Destroy(child.gameObject);

        CharacterDatabase database = CharacterDatabase.GetOrCreate();
        for (int i = 0; i < PartySize; i++)
        {
            int characterId = PlayerPrefs.GetInt($"SelectedCharacterId_{i}", -1);
            CharacterData character = database.GetCharacterById(characterId);
            CreateCharacterSlot(character, i);
        }
    }

    private void CreateCharacterSlot(CharacterData character, int slotIndex)
    {
        GameObject slot = CreateUI($"Character{slotIndex + 1}", characterRow);
        LayoutElement layout = slot.AddComponent<LayoutElement>();
        layout.preferredWidth = 210f;
        layout.preferredHeight = 330f;
        layout.flexibleWidth = 1f;

        Image image = CreateUI("Image", slot.transform).AddComponent<Image>();
        SetRect(image.rectTransform, new Vector2(0f, 0.18f), Vector2.one);
        image.preserveAspect = true;
        image.raycastTarget = false;

        Text name = CreateText("Name", slot.transform, 22, TextAnchor.MiddleCenter);
        SetRect(name.rectTransform, Vector2.zero, new Vector2(1f, 0.18f));
        name.raycastTarget = false;

        if (character != null && character.characterSprite != null)
        {
            image.sprite = character.characterSprite;
            image.color = Color.white;
            name.text = character.characterName;
        }
        else
        {
            image.sprite = null;
            image.color = new Color(0.28f, 0.34f, 0.46f, 1f);
            name.text = character != null ? character.characterName : "未選択";
        }
    }

    private void BuildUI()
    {
        if (characterRow != null && resultText != null && retryButton != null) return;

        RectTransform panelRect = transform as RectTransform;
        if (panelRect != null)
        {
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = panelRect.offsetMax = Vector2.zero;
        }

        Image background = GetComponent<Image>() ?? gameObject.AddComponent<Image>();
        background.color = new Color(0.04f, 0.06f, 0.12f, 0.96f);
        background.raycastTarget = true;

        Text title = CreateText("ResultTitle", transform, 48, TextAnchor.MiddleCenter);
        title.text = "STAGE CLEAR";
        title.resizeTextForBestFit = true;
        title.resizeTextMinSize = 24;
        title.resizeTextMaxSize = 48;
        SetRect(title.rectTransform, new Vector2(0.1f, 0.82f), new Vector2(0.9f, 0.96f));
        title.raycastTarget = false;

        GameObject rowObject = CreateUI("SelectedCharacters", transform);
        RectTransform rowRect = rowObject.GetComponent<RectTransform>();
        SetRect(rowRect, new Vector2(0.08f, 0.38f), new Vector2(0.92f, 0.81f));
        HorizontalLayoutGroup row = rowObject.AddComponent<HorizontalLayoutGroup>();
        row.spacing = 12f;
        row.padding = new RectOffset(8, 8, 0, 0);
        row.childAlignment = TextAnchor.MiddleCenter;
        row.childControlWidth = true;
        row.childControlHeight = true;
        row.childForceExpandWidth = false;
        row.childForceExpandHeight = true;
        characterRow = rowObject.transform;

        resultText = CreateText("BattleResult", transform, 28, TextAnchor.MiddleCenter);
        resultText.horizontalOverflow = HorizontalWrapMode.Wrap;
        resultText.verticalOverflow = VerticalWrapMode.Truncate;
        resultText.resizeTextForBestFit = true;
        resultText.resizeTextMinSize = 16;
        resultText.resizeTextMaxSize = 28;
        SetRect(resultText.rectTransform, new Vector2(0.08f, 0.14f), new Vector2(0.92f, 0.37f));
        resultText.raycastTarget = false;

        retryButton = CreateButton("RetryButton", transform, "再出撃", RetryStage);
        SetRect(retryButton.GetComponent<RectTransform>(), new Vector2(0.39f, 0.04f), new Vector2(0.61f, 0.13f));

        Text guide = CreateText("ExitGuide", transform, 18, TextAnchor.LowerRight);
        guide.text = "画面をタップしてホームへ";
        guide.resizeTextForBestFit = true;
        guide.resizeTextMinSize = 12;
        guide.resizeTextMaxSize = 18;
        SetRect(guide.rectTransform, new Vector2(0.64f, 0.01f), new Vector2(0.97f, 0.1f));
        guide.raycastTarget = false;
    }

    private static GameObject CreateUI(string name, Transform parent)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        return obj;
    }

    private static Text CreateText(string name, Transform parent, int fontSize, TextAnchor alignment)
    {
        Text text = CreateUI(name, parent).AddComponent<Text>();
        JapaneseFontProvider.Apply(text);
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = alignment;
        return text;
    }

    private static Button CreateButton(string name, Transform parent, string label, UnityEngine.Events.UnityAction action)
    {
        GameObject obj = CreateUI(name, parent);
        Image image = obj.AddComponent<Image>();
        image.color = new Color(0.18f, 0.55f, 0.88f, 1f);
        Button button = obj.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);
        Text text = CreateText("Text", obj.transform, 26, TextAnchor.MiddleCenter);
        text.text = label;
        text.raycastTarget = false;
        SetRect(text.rectTransform, Vector2.zero, Vector2.one);
        return button;
    }

    private static void SetRect(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }
}
