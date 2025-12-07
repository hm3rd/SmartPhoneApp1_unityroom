using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// キャラクター選択画面全体を管理
/// </summary>
public class CharacterSelectionUI : MonoBehaviour
{
    [Header("UI要素")]
    [Tooltip("キャラクターカードを生成する親オブジェクト")]
    public Transform cardContainer;
    
    [Tooltip("キャラクターカードのプレハブ")]
    public GameObject characterCardPrefab;
    
    [Tooltip("決定ボタン")]
    public Button confirmButton;
    
    [Tooltip("選択数表示テキスト")]
    public TextMeshProUGUI selectionCountText;
    
    [Header("シーン設定")]
    [Tooltip("決定後に遷移するシーン名")]
    public string nextSceneName = "GameScene";
    
    void Start()
    {
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        }
        
        GenerateCharacterCards();
        UpdateUI();
    }
    
    void Update()
    {
        UpdateUI();
    }
    
    /// <summary>
    /// キャラクターカードを生成
    /// </summary>
    void GenerateCharacterCards()
    {
        if (CharacterSelectionManager.Instance == null || cardContainer == null || characterCardPrefab == null)
        {
            Debug.LogWarning("CharacterSelectionUI: 必要な設定が不足しています");
            return;
        }
        
        // 既存のカードを削除
        foreach (Transform child in cardContainer)
        {
            Destroy(child.gameObject);
        }
        
        // 全キャラクターのカードを生成
        foreach (var character in CharacterSelectionManager.Instance.allCharacters)
        {
            GameObject cardObj = Instantiate(characterCardPrefab, cardContainer);
            CharacterSelectCard card = cardObj.GetComponent<CharacterSelectCard>();
            
            if (card != null)
            {
                card.Setup(character);
            }
        }
    }
    
    /// <summary>
    /// UIの状態を更新
    /// </summary>
    void UpdateUI()
    {
        if (CharacterSelectionManager.Instance == null) return;
        
        // 選択数表示を更新
        if (selectionCountText != null)
        {
            int current = CharacterSelectionManager.Instance.GetSelectedCount();
            int max = CharacterSelectionManager.Instance.maxSelectionCount;
            selectionCountText.text = $"選択中: {current} / {max}";
        }
        
        // 決定ボタンの有効/無効を切り替え
        if (confirmButton != null)
        {
            confirmButton.interactable = CharacterSelectionManager.Instance.IsSelectionValid();
        }
    }
    
    /// <summary>
    /// 決定ボタンクリック時の処理
    /// </summary>
    void OnConfirmButtonClicked()
    {
        if (CharacterSelectionManager.Instance == null || !CharacterSelectionManager.Instance.IsSelectionValid())
        {
            Debug.LogWarning("少なくとも1体のキャラクターを選択してください");
            return;
        }
        
        Debug.Log("キャラクター選択完了:");
        foreach (var character in CharacterSelectionManager.Instance.GetSelectedCharacters())
        {
            Debug.Log($"- {character.characterName}");
        }
        
        // 次のシーンへ遷移
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
