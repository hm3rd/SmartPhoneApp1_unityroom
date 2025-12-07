using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// キャラクター選択UI（1キャラ分のカード）
/// </summary>
public class CharacterSelectCard : MonoBehaviour
{
    [Header("UI要素")]
    public Image characterImage;
    public TextMeshProUGUI characterNameText;
    public TextMeshProUGUI hpText;
    public GameObject selectedFrame; // 選択時に表示する枠
    public Button selectButton;
    
    private CharacterData characterData;
    
    void Start()
    {
        if (selectButton != null)
        {
            selectButton.onClick.AddListener(OnCardClicked);
        }
    }
    
    /// <summary>
    /// カードにキャラクターデータを設定
    /// </summary>
    public void Setup(CharacterData data)
    {
        characterData = data;
        
        if (characterImage != null && data.characterSprite != null)
        {
            characterImage.sprite = data.characterSprite;
        }
        
        if (characterNameText != null)
        {
            characterNameText.text = data.characterName;
        }
        
        if (hpText != null)
        {
            hpText.text = $"HP: {data.maxHP}";
        }
        
        UpdateSelectionVisual();
    }
    
    /// <summary>
    /// カードクリック時の処理
    /// </summary>
    void OnCardClicked()
    {
        if (characterData == null || CharacterSelectionManager.Instance == null) return;
        
        CharacterSelectionManager.Instance.ToggleCharacter(characterData);
        UpdateSelectionVisual();
    }
    
    /// <summary>
    /// 選択状態の見た目を更新
    /// </summary>
    void UpdateSelectionVisual()
    {
        if (selectedFrame == null || characterData == null) return;
        
        bool isSelected = CharacterSelectionManager.Instance != null && 
                         CharacterSelectionManager.Instance.IsSelected(characterData);
        
        selectedFrame.SetActive(isSelected);
    }
}
