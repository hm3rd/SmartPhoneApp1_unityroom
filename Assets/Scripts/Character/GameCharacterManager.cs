using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// GameScene でキャラクター管理と交代機能を提供
/// 準備画面で選択された3体のキャラクターを右上に縦並びで表示
/// 使用中のキャラクターは上で大きく、他をクリックで交代
/// </summary>
public class GameCharacterManager : MonoBehaviour
{
    private const int CHARACTER_SLOT_COUNT = 3;
    private const int INVALID_CHARACTER_ID = -1;

    [Header("キャラクターアイコン（右上に縦並び）")]
    [SerializeField] private Button[] characterButtons = new Button[CHARACTER_SLOT_COUNT];
    [SerializeField] private Image[] characterImages = new Image[CHARACTER_SLOT_COUNT];
    [SerializeField] private RectTransform[] characterRects = new RectTransform[CHARACTER_SLOT_COUNT];

    [Header("アイコンサイズ")]
    [SerializeField] private float activeIconSize = 100f;
    [SerializeField] private float inactiveIconSize = 70f;

    [Header("プレイヤーへの適用先")]
    [SerializeField] private AttackManager attackManager;
    [SerializeField] private SpriteRenderer playerSpriteRenderer;
    [SerializeField] private Transform playerVisualRoot; // 任意: プレハブを差し替える置き場所
    [SerializeField] private AttackButton[] attackButtons; // 攻撃ボタンの参照（キャラ交代時に更新）

    // 選択された3体のキャラクター情報
    private int[] characterIds = new int[CHARACTER_SLOT_COUNT];
    private string[] characterNames = new string[CHARACTER_SLOT_COUNT];
    private Sprite[] characterSprites = new Sprite[CHARACTER_SLOT_COUNT];
    private CharacterData[] characterDatas = new CharacterData[CHARACTER_SLOT_COUNT];
    private int[] characterMaxHp = new int[CHARACTER_SLOT_COUNT];
    private int[] characterCurrentHp = new int[CHARACTER_SLOT_COUNT];
    
    // 現在アクティブなキャラクター（配列内のインデックス）
    private int currentCharacterIndex = 0;
    private GameObject currentVisualInstance;
    
    private CharacterDatabase characterDatabase;

    void Awake()
    {
        characterDatabase = CharacterDatabase.Instance ?? FindObjectOfType<CharacterDatabase>();
        if (characterDatabase == null)
        {
        }
    }

    void Start()
    {
        LoadSelectedCharacters();
        InitializeUI();
        UpdateDisplay();
    }

    /// <summary>
    /// 準備画面で選択されたキャラクター情報をPlayerPrefsから読み込む
    /// </summary>
    private void LoadSelectedCharacters()
    {
        for (int i = 0; i < CHARACTER_SLOT_COUNT; i++)
        {
            characterIds[i] = PlayerPrefs.GetInt($"SelectedCharacterId_{i}", INVALID_CHARACTER_ID);
            characterNames[i] = PlayerPrefs.GetString($"SelectedCharacterName_{i}", "");

            if (characterDatabase != null && characterIds[i] != INVALID_CHARACTER_ID)
            {
                characterDatas[i] = characterDatabase.GetCharacterById(characterIds[i]);
                if (characterDatas[i] != null)
                {
                    // プレハブ・攻撃・スプライト等を保持
                    if (string.IsNullOrEmpty(characterNames[i]))
                    {
                        characterNames[i] = characterDatas[i].characterName;
                    }
                    if (characterDatas[i].characterSprite != null)
                    {
                        characterSprites[i] = characterDatas[i].characterSprite;
                    }
                    characterMaxHp[i] = characterDatas[i].maxHP;
                    characterCurrentHp[i] = characterDatas[i].maxHP; // 初期HPは最大値
                }
            }
        }
    }

    /// <summary>
    /// UI の初期化とボタン設定
    /// </summary>
    private void InitializeUI()
    {
        for (int i = 0; i < CHARACTER_SLOT_COUNT; i++)
        {
            int index = i; // クロージャ対策
            
            if (characterButtons[i] != null)
            {
                characterButtons[i].onClick.AddListener(() => OnCharacterButtonClicked(index));
            }

            if (characterImages[i] != null && characterSprites[i] != null)
            {
                characterImages[i].sprite = characterSprites[i];
            }
        }
    }

    /// <summary>
    /// キャラクターボタンがクリックされた時の処理
    /// </summary>
    private void OnCharacterButtonClicked(int clickedIndex)
    {
        if (clickedIndex < 0 || clickedIndex >= CHARACTER_SLOT_COUNT) return;
        if (characterIds[clickedIndex] == INVALID_CHARACTER_ID) return;
        if (clickedIndex == currentCharacterIndex) return; // 既にアクティブ

        // クリックされたキャラクターと現在のアクティブキャラクターを配列内で入れ替え
        SwapCharacters(currentCharacterIndex, clickedIndex);
        
        // アクティブキャラクターは常に index 0
        currentCharacterIndex = 0;
        UpdateDisplay();

        // TODO: プレイヤーのステータスや見た目を変更
    }

    /// <summary>
    /// 配列内の2つのキャラクターを入れ替え
    /// </summary>
    private void SwapCharacters(int index1, int index2)
    {
        // ID を入れ替え
        int tempId = characterIds[index1];
        characterIds[index1] = characterIds[index2];
        characterIds[index2] = tempId;

        // 名前を入れ替え
        string tempName = characterNames[index1];
        characterNames[index1] = characterNames[index2];
        characterNames[index2] = tempName;

        // スプライトを入れ替え
        Sprite tempSprite = characterSprites[index1];
        characterSprites[index1] = characterSprites[index2];
        characterSprites[index2] = tempSprite;

        // キャラクターデータを入れ替え
        CharacterData tempData = characterDatas[index1];
        characterDatas[index1] = characterDatas[index2];
        characterDatas[index2] = tempData;

        // HP情報を入れ替え
        int tempMaxHp = characterMaxHp[index1];
        characterMaxHp[index1] = characterMaxHp[index2];
        characterMaxHp[index2] = tempMaxHp;

        int tempCurrentHp = characterCurrentHp[index1];
        characterCurrentHp[index1] = characterCurrentHp[index2];
        characterCurrentHp[index2] = tempCurrentHp;

        // UI画像も入れ替え
        Sprite tempImageSprite = characterImages[index1].sprite;
        characterImages[index1].sprite = characterImages[index2].sprite;
        characterImages[index2].sprite = tempImageSprite;
    }

    /// <summary>
    /// 表示を更新（アクティブなキャラを一番上に、サイズを変更）
    /// </summary>
    private void UpdateDisplay()
    {
        // アクティブキャラクター（常に index 0）を一番上に大きく表示
        UpdateCharacterSlot(0, 0, activeIconSize);
        
        // 他のキャラを下に小さく表示
        for (int i = 1; i < CHARACTER_SLOT_COUNT; i++)
        {
            UpdateCharacterSlot(i, i, inactiveIconSize);
        }

        ApplyActiveCharacterToPlayer();
    }

    /// <summary>
    /// キャラクタースロットの表示を更新
    /// </summary>
    private void UpdateCharacterSlot(int characterIndex, int displayOrder, float size)
    {
        if (characterIndex < 0 || characterIndex >= CHARACTER_SLOT_COUNT) return;
        if (characterRects[characterIndex] == null) return;

        // サイズを変更
        characterRects[characterIndex].sizeDelta = new Vector2(size, size);
        
        // Hierarchy の順序を変更（上から順に表示）
        characterRects[characterIndex].SetSiblingIndex(displayOrder);
    }

    /// <summary>
    /// 現在アクティブなキャラクターのIDを取得
    /// </summary>
    public int GetCurrentCharacterId()
    {
        return characterIds[currentCharacterIndex];
    }

    /// <summary>
    /// 現在アクティブなキャラクターの名前を取得
    /// </summary>
    public string GetCurrentCharacterName()
    {
        return characterNames[currentCharacterIndex];
    }

    /// <summary>
    /// 指定スロットのキャラクターデータを取得
    /// </summary>
    public CharacterData GetCharacterData(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= CHARACTER_SLOT_COUNT) return null;
        if (characterIds[slotIndex] == INVALID_CHARACTER_ID) return null;
        if (characterDatabase == null) return null;

        return characterDatabase.GetCharacter(characterIds[slotIndex]);
    }

    /// <summary>
    /// 現在アクティブキャラクターのHPを取得
    /// </summary>
    public int GetCurrentHp() => characterCurrentHp[currentCharacterIndex];

    /// <summary>
    /// 現在アクティブキャラクターの最大HPを取得
    /// </summary>
    public int GetCurrentMaxHp() => characterMaxHp[currentCharacterIndex];

    /// <summary>
    /// 指定スロットのキャラクターの現在HPを取得
    /// </summary>
    public int GetCharacterCurrentHp(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= CHARACTER_SLOT_COUNT) return 0;
        return characterCurrentHp[slotIndex];
    }

    /// <summary>
    /// 指定スロットのキャラクターの最大HPを取得
    /// </summary>
    public int GetCharacterMaxHp(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= CHARACTER_SLOT_COUNT) return 0;
        return characterMaxHp[slotIndex];
    }

    /// <summary>
    /// 現在アクティブキャラクターにダメージを与える
    /// </summary>
    public void ApplyDamageToCurrent(int damage)
    {
        damage = Mathf.Max(0, damage);
        characterCurrentHp[currentCharacterIndex] = Mathf.Max(0, characterCurrentHp[currentCharacterIndex] - damage);
    }

    /// <summary>
    /// 現在アクティブキャラクターを回復
    /// </summary>
    public void HealCurrent(int amount)
    {
        amount = Mathf.Max(0, amount);
        characterCurrentHp[currentCharacterIndex] = Mathf.Min(characterMaxHp[currentCharacterIndex], characterCurrentHp[currentCharacterIndex] + amount);
    }

    /// <summary>
    /// HPが残っている次のキャラクターに自動交代
    /// </summary>
    /// <returns>交代成功時は true、全員HP0なら false</returns>
    public bool SwapToNextAliveCharacter()
    {
        // 現在のキャラクター以外で HP > 0 のキャラクターを探す
        for (int i = 1; i < CHARACTER_SLOT_COUNT; i++)
        {
            int checkIndex = i;
            if (characterCurrentHp[checkIndex] > 0)
            {
                // このキャラクターに交代
                SwapCharacters(currentCharacterIndex, checkIndex);
                currentCharacterIndex = 0;
                UpdateDisplay();
                return true;
            }
        }

        // HP > 0 のキャラクターが見つからない
        return false;
    }

    /// <summary>
    /// アクティブキャラクターのデータをプレイヤーへ適用
    /// </summary>
    private void ApplyActiveCharacterToPlayer()
    {
        if (characterDatas == null || characterDatas.Length == 0)
        {
            return;
        }
        var activeData = characterDatas[currentCharacterIndex];

        if (activeData == null)
        {
            return;
        }

        // 攻撃データをAttackManagerに反映
        if (attackManager != null)
        {
            attackManager.SetAvailableAttacks(activeData.availableAttacks ?? new List<AttackData>());
        }

        // 全ての攻撃ボタンの attackIndex を設定
        if (attackButtons != null && attackButtons.Length > 0)
        {
            for (int i = 0; i < attackButtons.Length; i++)
            {
                if (attackButtons[i] != null)
                {
                    attackButtons[i].attackIndex = i;
                    attackButtons[i].attackManager = attackManager;
                }
            }
        }
        // これにより、ボタン押下時に最新の攻撃リストから取得される
        if (attackButtons != null && attackButtons.Length > 0)
        {
            for (int i = 0; i < attackButtons.Length; i++)
            {
                if (attackButtons[i] != null)
                {
                    attackButtons[i].attackIndex = i;
                    attackButtons[i].attackManager = attackManager;
                }
            }
        }

        // 見た目（スプライト）を差し替え
        if (playerSpriteRenderer != null && activeData.characterSprite != null)
        {
            playerSpriteRenderer.sprite = activeData.characterSprite;
            playerSpriteRenderer.color = activeData.themeColor;
        }

        // プレハブを差し替える場合（任意設定）
        if (playerVisualRoot != null)
        {
            if (currentVisualInstance != null)
            {
                Destroy(currentVisualInstance);
            }

            if (activeData.characterPrefab != null)
            {
                currentVisualInstance = Instantiate(activeData.characterPrefab, playerVisualRoot);
                currentVisualInstance.transform.localPosition = Vector3.zero;
                currentVisualInstance.transform.localRotation = Quaternion.identity;
                currentVisualInstance.transform.localScale = Vector3.one;
            }
        }
    }
}
