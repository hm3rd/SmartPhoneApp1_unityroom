using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// ゲーム開始前のキャラクター準備画面を管理するスクリプト
/// キャラクターを3体選択できる
/// </summary>
public class NewCharacterManager : MonoBehaviour
{
    private const int CHARACTER_SLOT_COUNT = 3;
    private const int INVALID_CHARACTER_ID = -1;
    
    [Header("キャラクター選択枠")]
    [SerializeField] private Button[] characterSlots = new Button[CHARACTER_SLOT_COUNT];
    [SerializeField] private Image[] characterDisplays = new Image[CHARACTER_SLOT_COUNT];
    [SerializeField] private Text[] characterNameTexts = new Text[CHARACTER_SLOT_COUNT];
    
    [Header("パネル設定")]
    [SerializeField] private GameObject preparationPanel;

    [Header("シーン設定")]
    [SerializeField] private string characterSelectSceneName = "CharacterScene";
    [SerializeField] private string gameSceneName = "GameScene";

    [Header("UI設定")]
    [SerializeField] private GameObject startGameButton;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color unselectedColor = Color.gray;

    // キャラクター選択状態の管理
    private int[] selectedCharacterIds = new int[CHARACTER_SLOT_COUNT] { INVALID_CHARACTER_ID, INVALID_CHARACTER_ID, INVALID_CHARACTER_ID };
    private string[] selectedCharacterNames = new string[CHARACTER_SLOT_COUNT] { "", "", "" };
    private Sprite[] selectedCharacterSprites = new Sprite[CHARACTER_SLOT_COUNT];
    private int currentSelectingSlot = 0;
    private bool isWaitingForSelection = false;
    private CharacterDatabase characterDatabase;
    private bool initialized;

    private void Awake()
    {
        characterDatabase = CharacterDatabase.Instance ?? FindObjectOfType<CharacterDatabase>();
        if (characterDatabase == null)
        {
            Debug.LogWarning("NewCharacterManager: CharacterDatabase が見つかりません。キャラ画像は表示されません。");
        }

        InitializeManager();
    }

    void Start()
    {
        if (!initialized)
        {
            InitializeManager();
        }
    }

    /// <summary>
    /// このオブジェクトが有効になった時に毎回準備パネルを表示
    /// Scene遷移後もパネルが見えるようにするため
    /// </summary>
    private void OnEnable()
    {
        if (initialized && preparationPanel != null)
        {
            ShowPreparationPanel();
            Debug.Log("OnEnable: 準備画面パネルを表示しました");
        }
    }

    /// <summary>
    /// 画面読み込み時の初期化
    /// </summary>
    private void InitializeManager()
    {
        if (initialized) return;
        
        initialized = true;
        LoadSelectionsFromPrefs();
        InitializeUI();
        
        // 準備パネルを最初に表示
        ShowPreparationPanel();
        
        CheckForReturnedCharacter();

        // CharacterSceneから戻ってきた場合、準備パネルを確実に表示
        if (PlayerPrefs.GetInt("ShowPreparationPanel", 0) == 1)
        {
            ShowPreparationPanel();
            PlayerPrefs.DeleteKey("ShowPreparationPanel");
            Debug.Log("CharacterScene から戻ってきました");
        }
    }

    /// <summary>
    /// PlayerPrefs から既存の選択状態をロード
    /// </summary>
    void LoadSelectionsFromPrefs()
    {
        for (int i = 0; i < CHARACTER_SLOT_COUNT; i++)
        {
            int id = PlayerPrefs.GetInt($"SelectedCharacterId_{i}", INVALID_CHARACTER_ID);
            selectedCharacterIds[i] = id;
            selectedCharacterNames[i] = PlayerPrefs.GetString($"SelectedCharacterName_{i}", "");

            if (characterDatabase != null && id != INVALID_CHARACTER_ID)
            {
                selectedCharacterSprites[i] = characterDatabase.GetCharacterIconById(id);
            }
        }
    }

    void InitializeUI()
    {
        for (int i = 0; i < CHARACTER_SLOT_COUNT; i++)
        {
            int slotIndex = i;
            characterSlots[i].onClick.AddListener(() => OnSlotClicked(slotIndex));
            UpdateSlotDisplay(i);
        }

        if (startGameButton != null)
        {
            startGameButton.SetActive(false);
        }
    }

    /// <summary>
    /// キャラクター選択シーンから戻ってきた時の処理
    /// </summary>
    void CheckForReturnedCharacter()
    {
        if (!PlayerPrefs.HasKey("TempSelectedCharacterId")) return;

        int characterId = PlayerPrefs.GetInt("TempSelectedCharacterId", INVALID_CHARACTER_ID);
        string characterName = PlayerPrefs.GetString("TempSelectedCharacterName", "");
        currentSelectingSlot = PlayerPrefs.GetInt("CurrentSelectingSlot", 0);

        Sprite characterSprite = null;
        if (characterDatabase != null)
        {
            characterSprite = characterDatabase.GetCharacterIconById(characterId);
        }

        OnCharacterSelected(characterId, characterName, characterSprite);

        PlayerPrefs.DeleteKey("TempSelectedCharacterId");
        PlayerPrefs.DeleteKey("TempSelectedCharacterName");
        PlayerPrefs.DeleteKey("TempSelectedCharacterIconId");
        
        ShowPreparationPanel();
    }

    /// <summary>
    /// キャラクター選択枠がクリックされた時の処理
    /// </summary>
    public void OnSlotClicked(int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex) || isWaitingForSelection) return;

        currentSelectingSlot = slotIndex;
        isWaitingForSelection = true;

        PlayerPrefs.SetInt("CurrentSelectingSlot", slotIndex);
        PlayerPrefs.SetString("ReturnSceneName", SceneManager.GetActiveScene().name);
        PlayerPrefs.SetInt("ShowPreparationPanel", 1);

        SceneManager.LoadScene(characterSelectSceneName);
    }
    
    /// <summary>
    /// 準備画面パネルを表示
    /// </summary>
    public void ShowPreparationPanel()
    {
        if (preparationPanel != null)
        {
            preparationPanel.SetActive(true);
            Debug.Log("準備画面パネルを表示しました");
        }
        else
        {
            Debug.LogWarning("preparationPanel が設定されていません");
        }
    }
    
    /// <summary>
    /// 準備画面パネルを非表示
    /// </summary>
    public void HidePreparationPanel()
    {
        if (preparationPanel != null)
        {
            preparationPanel.SetActive(false);
        }
    }

    /// <summary>
    /// CharacterSceneからキャラクターが選択された時に呼ばれる
    /// </summary>
    public void OnCharacterSelected(int characterId, string characterName, Sprite characterSprite)
    {
        if (!IsValidSlotIndex(currentSelectingSlot)) return;

        selectedCharacterIds[currentSelectingSlot] = characterId;
        selectedCharacterNames[currentSelectingSlot] = characterName;
        selectedCharacterSprites[currentSelectingSlot] = characterSprite;

        UpdateSlotDisplay(currentSelectingSlot);
        isWaitingForSelection = false;
        CheckAllCharactersSelected();
        SaveSelectionsToPrefs();
    }

    /// <summary>
    /// 選択状態を PlayerPrefs に保存
    /// </summary>
    void SaveSelectionsToPrefs()
    {
        for (int i = 0; i < CHARACTER_SLOT_COUNT; i++)
        {
            PlayerPrefs.SetInt($"SelectedCharacterId_{i}", selectedCharacterIds[i]);
            PlayerPrefs.SetString($"SelectedCharacterName_{i}", selectedCharacterNames[i]);
        }
    }

    /// <summary>
    /// スロットの表示を更新
    /// </summary>
    void UpdateSlotDisplay(int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex)) return;

        bool isSelected = selectedCharacterIds[slotIndex] != INVALID_CHARACTER_ID;

        if (characterDisplays[slotIndex] != null)
        {
            characterDisplays[slotIndex].sprite = (isSelected && selectedCharacterSprites[slotIndex] != null) 
                ? selectedCharacterSprites[slotIndex] : null;
            characterDisplays[slotIndex].color = (isSelected && selectedCharacterSprites[slotIndex] != null) 
                ? Color.white : unselectedColor;
        }

        if (characterNameTexts[slotIndex] != null)
        {
            characterNameTexts[slotIndex].text = isSelected ? selectedCharacterNames[slotIndex] : "未選択";
        }

        var colors = characterSlots[slotIndex].colors;
        colors.normalColor = isSelected ? selectedColor : unselectedColor;
        characterSlots[slotIndex].colors = colors;
    }

    /// <summary>
    /// 3体全て選択されたかチェック
    /// </summary>
    void CheckAllCharactersSelected()
    {
        bool allSelected = true;
        for (int i = 0; i < CHARACTER_SLOT_COUNT; i++)
        {
            if (selectedCharacterIds[i] == INVALID_CHARACTER_ID)
            {
                allSelected = false;
                break;
            }
        }

        if (startGameButton != null)
        {
            startGameButton.SetActive(allSelected);
        }
    }
    
    /// <summary>
    /// スロットインデックスが有効かチェック
    /// </summary>
    bool IsValidSlotIndex(int slotIndex)
    {
        return slotIndex >= 0 && slotIndex < CHARACTER_SLOT_COUNT;
    }

    /// <summary>
    /// ゲーム開始ボタンが押された時の処理
    /// </summary>
    public void OnStartGameButtonClicked()
    {
        SaveSelectionsToPrefs();
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    /// <summary>
    /// キャンセルボタンが押された時の処理
    /// </summary>
    public void OnCancelButtonClicked()
    {
        SceneManager.LoadScene("MainScene");
    }

    // デバッグ用：選択状態をログ出力
    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 200, 30), "表示選択状態"))
        {
            for (int i = 0; i < selectedCharacterIds.Length; i++)
            {
                Debug.Log($"Slot {i}: ID={selectedCharacterIds[i]}, Name={selectedCharacterNames[i]}");
            }
        }
    }
}
