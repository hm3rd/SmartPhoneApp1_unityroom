using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// CharacterSceneでキャラクター一覧をScroll Viewに表示
/// </summary>
public class CharacterListDisplay : MonoBehaviour
{
    [SerializeField] private RectTransform contentPanel; // Scroll View > Viewport > Content
    [SerializeField] private GameObject itemPrefab; // 使用しない（自動生成）
    [SerializeField] private bool autoAddGridLayout = true; // グリッドレイアウト自動付与
    [SerializeField] private int columnCount = 6; // 6列固定
    [SerializeField] private float cellSize = 150f; // 正方形セルサイズ
    [Header("Text Size")]
    [Min(12f)] [SerializeField] private float characterNameFontSize = 24f;
    [Min(10f)] [SerializeField] private float characterNameMinimumFontSize = 16f;
    
    void Start()
    {
        Debug.Log("CharacterListDisplay Start()");
        
        // 設定確認
        if (contentPanel == null)
        {
            Debug.LogError("CharacterListDisplay: Content Panel が設定されていません！");
            Debug.LogError("手順: CharacterScene > CharacterListDisplay の Inspector > Content Panel に Scroll View > Viewport > Content をドラッグしてください");
            return;
        }
        
        Debug.Log($"Content Panel 設定OK: {contentPanel.name}");
        DisplayCharacters();
    }
    
    void DisplayCharacters()
    {
        Debug.Log("DisplayCharacters() 開始");
        
        // CharacterDatabase確認（複数の方法で検索）
        CharacterDatabase db = CharacterDatabase.GetOrCreate();
        
        if (db == null)
        {
            Debug.LogError("CharacterDatabase が見つかりません！");
            Debug.LogError("対処法: HomeSceneにCharacterDatabaseオブジェクトを配置し、HomeSceneから開始してください。");
            Debug.LogError("または、CharacterSceneにも一時的にCharacterDatabaseを配置してテストしてください。");
            return;
        }
        
        Debug.Log($"CharacterDatabase 見つかりました。キャラクター数: {db.GetCharacterCount()}");
        
        if (contentPanel == null)
        {
            Debug.LogError("Content Panel が設定されていません！Inspectorで設定してください。");
            return;
        }
        
        Debug.Log($"Content Panel: {contentPanel.name}");

        // グリッドレイアウト自動付与
        if (autoAddGridLayout)
        {
            EnsureGridLayout();
        }
        
        // 既存のアイテムをクリア
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }
        
        // キャラクター分だけUIアイテムを生成
        CharacterSceneController sceneController = FindFirstObjectByType<CharacterSceneController>();
        if (sceneController != null && sceneController.IsFormationSelectionMode)
        {
            CreateClearSelectionItem(sceneController);
        }

        int count = db.GetCharacterCount();
        if (count == 0)
        {
            Debug.LogWarning("CharacterDatabaseにキャラクターが登録されていません！");
            return;
        }
        
        for (int i = 0; i < count; i++)
        {
            CharacterData character = db.GetCharacter(i);
            if (character == null)
            {
                Debug.LogWarning($"CharacterData[{i}] が null です");
                continue;
            }
            
            Debug.Log($"キャラクター生成: {character.characterName}");
            
            // シンプルなアイテムを生成
            GameObject itemObj = CreateCharacterItem(character);
            
            // CharacterListItemコンポーネントをセットアップ
            CharacterListItem item = itemObj.GetComponent<CharacterListItem>();
            if (item != null)
            {
                item.Setup(character);
                if (sceneController != null)
                {
                    item.SetSelectionHandler(sceneController.ShowCharacter);
                }
            }
        }
        
        Debug.Log($"DisplayCharacters() 完了。{count}体のキャラクターを表示しました。");
    }
    
    void EnsureGridLayout()
    {
        if (contentPanel == null)
        {
            Debug.LogError("EnsureGridLayout: contentPanel が null です！");
            return;
        }
        
        Debug.Log($"EnsureGridLayout 開始: contentPanel={contentPanel.name}, GameObject={contentPanel.gameObject.name}");

        // 既存のレイアウトを無効化
        var vlg = contentPanel.GetComponent<VerticalLayoutGroup>();
        if (vlg != null)
        {
            vlg.enabled = false;
            Debug.Log("既存の VerticalLayoutGroup を無効化");
        }

        // GridLayoutGroup 設置
        var grid = contentPanel.GetComponent<GridLayoutGroup>();
        if (grid == null)
        {
            Debug.Log("GridLayoutGroup を新規作成");
            try
            {
                grid = contentPanel.gameObject.AddComponent<GridLayoutGroup>();
                Debug.Log("GridLayoutGroup の作成成功");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"GridLayoutGroup の作成に失敗: {e.Message}");
                Debug.LogError("手動でContentにGridLayoutGroupを追加してください。");
                return;
            }
        }
        else
        {
            Debug.Log("既存の GridLayoutGroup を使用");
        }
        
        if (grid == null)
        {
            Debug.LogError("GridLayoutGroup が null です。手動でScroll View > Viewport > Content に GridLayoutGroup コンポーネントを追加してください。");
            return;
        }
        
        Debug.Log($"GridLayoutGroup 設定: cellSize={cellSize}, columnCount={columnCount}");
        grid.cellSize = new Vector2(cellSize, cellSize + 50); // 画像+名前スペース
        grid.spacing = new Vector2(10, 10);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columnCount; // 6列固定
        grid.padding = new RectOffset(10, 10, 10, 10);
        grid.childAlignment = TextAnchor.UpperLeft;

        // ContentSizeFitter 設置
        var fitter = contentPanel.GetComponent<ContentSizeFitter>();
        if (fitter == null)
        {
            fitter = contentPanel.gameObject.AddComponent<ContentSizeFitter>();
        }
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        // Content のアンカー/ピボットを縦並び向けに
        contentPanel.anchorMin = new Vector2(0, 1);
        contentPanel.anchorMax = new Vector2(1, 1);
        contentPanel.pivot = new Vector2(0.5f, 1);
    }
    
    GameObject CreateCharacterItem(CharacterData character)
    {
        // ベースオブジェクト（正方形枠）
        GameObject itemObj = new GameObject(character.characterName);
        itemObj.transform.SetParent(contentPanel, false);
        
        RectTransform rect = itemObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(cellSize, cellSize + 50); // 画像+名前スペース

        // LayoutElement（グリッドが高さを認識）
        var layout = itemObj.AddComponent<LayoutElement>();
        layout.preferredWidth = cellSize;
        layout.preferredHeight = cellSize + 50;

        // コンテナ用Image
        Image containerBg = itemObj.AddComponent<Image>();
        containerBg.color = new Color(0.95f, 0.95f, 0.95f);

        // ===== 子オブジェクト1: 画像枠（正方形）=====
        GameObject imageFrame = new GameObject("ImageFrame");
        imageFrame.transform.SetParent(itemObj.transform, false);
        RectTransform frameRect = imageFrame.AddComponent<RectTransform>();
        frameRect.anchorMin = new Vector2(0.5f, 0.5f);
        frameRect.anchorMax = new Vector2(0.5f, 0.5f);
        frameRect.anchoredPosition = new Vector2(0, cellSize * 0.1f); // 下に少しずらす
        frameRect.sizeDelta = new Vector2(cellSize - 10, cellSize - 10);

        Image frameBg = imageFrame.AddComponent<Image>();
        frameBg.color = new Color(0.8f, 0.8f, 0.8f);

        // アイコンImage（正方形内、全体を占める）
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(imageFrame.transform, false);
        RectTransform iconRect = iconObj.AddComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.sizeDelta = Vector2.zero;
        Image iconImage = iconObj.AddComponent<Image>();
        iconImage.preserveAspect = true;

        // ===== 子オブジェクト2: 名前テキスト（下部）=====
        GameObject nameObj = new GameObject("Name");
        nameObj.transform.SetParent(itemObj.transform, false);
        RectTransform nameRect = nameObj.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0.5f, 0);
        nameRect.anchorMax = new Vector2(0.5f, 0);
        nameRect.anchoredPosition = new Vector2(0, 20);
        nameRect.sizeDelta = new Vector2(cellSize - 10, 40);

        TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
        JapaneseFontProvider.Apply(nameText);
        nameText.text = character.characterName;
        nameText.fontSize = characterNameFontSize;
        nameText.enableAutoSizing = true;
        nameText.fontSizeMin = Mathf.Min(characterNameMinimumFontSize, characterNameFontSize);
        nameText.fontSizeMax = characterNameFontSize;
        nameText.color = Color.black;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.overflowMode = TextOverflowModes.Truncate;

        // ===== 子オブジェクト3: 選択ボタン（オーバーレイ）=====
        GameObject buttonObj = new GameObject("SelectButton");
        buttonObj.transform.SetParent(itemObj.transform, false);
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = Vector2.zero;
        buttonRect.anchorMax = Vector2.one;
        buttonRect.sizeDelta = Vector2.zero;

        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0, 0, 0, 0); // 透明（クリック領域のみ）

        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = buttonImage;

        // CharacterListItemコンポーネント追加
        CharacterListItem listItem = itemObj.AddComponent<CharacterListItem>();
        listItem.characterImage = iconImage;
        listItem.characterNameText = nameText;
        listItem.selectButton = button;

        return itemObj;
    }

    private void CreateClearSelectionItem(CharacterSceneController sceneController)
    {
        bool canClear = sceneController.CanClearCurrentFormationSlot;
        GameObject item = new GameObject("ClearSelection");
        item.transform.SetParent(contentPanel, false);

        RectTransform rect = item.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(cellSize, cellSize + 50f);
        LayoutElement layout = item.AddComponent<LayoutElement>();
        layout.preferredWidth = cellSize;
        layout.preferredHeight = cellSize + 50f;

        Image background = item.AddComponent<Image>();
        background.color = canClear
            ? new Color(0.88f, 0.32f, 0.32f, 1f)
            : new Color(0.45f, 0.45f, 0.45f, 1f);

        GameObject symbolObject = new GameObject("ClearSymbol");
        symbolObject.transform.SetParent(item.transform, false);
        RectTransform symbolRect = symbolObject.AddComponent<RectTransform>();
        symbolRect.anchorMin = new Vector2(0f, 0.2f);
        symbolRect.anchorMax = Vector2.one;
        symbolRect.offsetMin = symbolRect.offsetMax = Vector2.zero;
        TextMeshProUGUI symbol = symbolObject.AddComponent<TextMeshProUGUI>();
        JapaneseFontProvider.Apply(symbol);
        symbol.text = "×";
        symbol.fontSize = Mathf.Max(48f, characterNameFontSize * 2f);
        symbol.color = Color.white;
        symbol.alignment = TextAlignmentOptions.Center;
        symbol.raycastTarget = false;

        GameObject labelObject = new GameObject("Label");
        labelObject.transform.SetParent(item.transform, false);
        RectTransform labelRect = labelObject.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = new Vector2(1f, 0.25f);
        labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
        TextMeshProUGUI label = labelObject.AddComponent<TextMeshProUGUI>();
        JapaneseFontProvider.Apply(label);
        label.text = canClear ? "選択解除" : "解除不可";
        label.fontSize = characterNameFontSize;
        label.enableAutoSizing = true;
        label.fontSizeMin = characterNameMinimumFontSize;
        label.fontSizeMax = characterNameFontSize;
        label.color = Color.white;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;

        Button button = item.AddComponent<Button>();
        button.targetGraphic = background;
        button.interactable = canClear;
        if (canClear)
            button.onClick.AddListener(sceneController.ClearCurrentFormationSlot);
    }
}
