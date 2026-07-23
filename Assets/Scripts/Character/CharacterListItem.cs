using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// キャラクター一覧の1アイテム（1キャラクター分）
/// </summary>
public class CharacterListItem : MonoBehaviour
{
    [Header("UI要素")]
    [Tooltip("キャラクター画像")]
    public Image characterImage;
    
    [Tooltip("キャラクター名")]
    public TextMeshProUGUI characterNameText;
    
    [Tooltip("HP表示")]
    public TextMeshProUGUI hpText;
    
    [Tooltip("説明文")]
    public TextMeshProUGUI descriptionText;
    
    [Tooltip("選択ボタン")]
    public Button selectButton;

    [Header("遷移設定")]
    [Tooltip("選択後に戻るシーン名。未設定ならPlayerPrefsのReturnSceneName、なければHomeScene。")]
    public string returnSceneName = "HomeScene";
    
    private CharacterData characterData;
    
    void Start()
    {
        if (selectButton != null)
        {
            selectButton.onClick.AddListener(OnSelectButtonClicked);
        }
    }
    
    /// <summary>
    /// このアイテムにキャラクターデータを設定
    /// </summary>
    public void Setup(CharacterData data)
    {
        characterData = data;
        
        if (characterData == null)
        {
            Debug.LogWarning("CharacterData が null です");
            return;
        }
        
        // 画像設定
        if (characterImage != null && characterData.characterSprite != null)
        {
            characterImage.sprite = characterData.characterSprite;
        }
        
        // 名前設定
        if (characterNameText != null)
        {
            characterNameText.text = characterData.characterName;
        }
        
        // HP設定
        if (hpText != null)
        {
            hpText.text = $"HP: {characterData.maxHP}";
        }
        
        // 説明設定
        if (descriptionText != null)
        {
            descriptionText.text = characterData.description;
        }
        
        Debug.Log($"キャラクター '{characterData.characterName}' のアイテムをセットアップしました");
    }
    
    /// <summary>
    /// 選択ボタンが押された時の処理
    /// </summary>
    void OnSelectButtonClicked()
    {
        if (characterData == null) return;
        
        Debug.Log($"キャラクター '{characterData.characterName}' が選択されました");
        
        // PlayerPrefsに選択されたキャラクター情報を保存
        PlayerPrefs.SetInt("TempSelectedCharacterId", characterData.characterId);
        PlayerPrefs.SetString("TempSelectedCharacterName", characterData.characterName);
        PlayerPrefs.Save();
        
        // 戻り先シーンを決定
        string targetScene = HomeScenePanelState.HasSavedState
            ? HomeScenePanelState.ReturnSceneName
            : PlayerPrefs.GetString("ReturnSceneName", returnSceneName);
        if (string.IsNullOrEmpty(targetScene))
        {
            targetScene = returnSceneName;
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene(targetScene);
    }
}
