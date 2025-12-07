using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ゲーム内で使用するキャラクターデータを管理します
/// Inspector からキャラクターを作成・編集できます
/// </summary>
public class CharacterDatabase : MonoBehaviour
{
    public static CharacterDatabase Instance { get; private set; }
    
    [Header("作成済みキャラクター")]
    [Tooltip("ゲーム内で使用するキャラクターのリスト")]
    public List<CharacterData> characters = new List<CharacterData>();
    
    void Awake()
    {
        // シングルトン設定
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 指定番号のキャラクターを取得
    /// </summary>
    public CharacterData GetCharacter(int index)
    {
        if (index < 0 || index >= characters.Count)
        {
            Debug.LogWarning($"キャラクターインデックス {index} は無効です");
            return null;
        }
        return characters[index];
    }
    
    /// <summary>
    /// キャラクター名で検索
    /// </summary>
    public CharacterData GetCharacterByName(string name)
    {
        return characters.Find(c => c.characterName == name);
    }
    
    /// <summary>
    /// 全キャラクター数を取得
    /// </summary>
    public int GetCharacterCount()
    {
        return characters.Count;
    }
}
