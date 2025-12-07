using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// CharacterDatabaseのキャラクターを ScrollView に一覧表示します
/// </summary>
public class CharacterListDisplay : MonoBehaviour
{
    [Header("ScrollView設定")]
    [Tooltip("Scroll View の Content パネル（アイテムの親になる）")]
    public RectTransform contentPanel;
    
    [Header("キャラクターアイテムPrefab")]
    [Tooltip("キャラクター1体分のUI（Prefab）")]
    public GameObject characterItemPrefab;
    
    [Header("レイアウト")]
    [Tooltip("Grid Layout Group の設定用（任意）")]
    public GridLayoutGroup gridLayout;
    
    void Start()
    {
        DisplayCharacters();
    }
    
    /// <summary>
    /// CharacterDatabase のキャラクターをScroll View に表示
    /// </summary>
    void DisplayCharacters()
    {
        if (CharacterDatabase.Instance == null)
        {
            Debug.LogWarning("CharacterDatabase が見つかりません");
            return;
        }
        
        if (contentPanel == null)
        {
            Debug.LogWarning("Content Panel が設定されていません");
            return;
        }
        
        if (characterItemPrefab == null)
        {
            Debug.LogWarning("Character Item Prefab が設定されていません");
            return;
        }
        
        // 既存のアイテムをクリア
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }
        
        // キャラクター分だけUIアイテムを生成
        for (int i = 0; i < CharacterDatabase.Instance.GetCharacterCount(); i++)
        {
            CharacterData character = CharacterDatabase.Instance.GetCharacter(i);
            if (character == null) continue;
            
            // Prefab をインスタンス化
            GameObject itemObj = Instantiate(characterItemPrefab, contentPanel);
            
            // キャラクター情報をUI要素に反映
            CharacterListItem item = itemObj.GetComponent<CharacterListItem>();
            if (item != null)
            {
                item.Setup(character);
            }
            
            Debug.Log($"キャラクター '{character.characterName}' を表示リストに追加しました");
        }
    }
}
