using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// プレイヤーが選択したキャラクターを管理するシングルトン
/// </summary>
public class CharacterSelectionManager : MonoBehaviour
{
    public static CharacterSelectionManager Instance { get; private set; }
    
    [Header("利用可能なキャラクター")]
    [Tooltip("ゲーム内で選択可能な全キャラクター")]
    public List<CharacterData> allCharacters = new List<CharacterData>();
    
    [Header("選択設定")]
    [Tooltip("最大選択可能数")]
    public int maxSelectionCount = 3;
    
    // 現在選択されているキャラクター
    private List<CharacterData> selectedCharacters = new List<CharacterData>();
    
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
    /// キャラクターを選択/選択解除
    /// </summary>
    public bool ToggleCharacter(CharacterData character)
    {
        if (character == null) return false;
        
        if (selectedCharacters.Contains(character))
        {
            // 既に選択済みなら解除
            selectedCharacters.Remove(character);
            return false;
        }
        else
        {
            // 最大数に達していなければ追加
            if (selectedCharacters.Count < maxSelectionCount)
            {
                selectedCharacters.Add(character);
                return true;
            }
            else
            {
                Debug.Log($"最大{maxSelectionCount}体まで選択できます");
                return false;
            }
        }
    }
    
    /// <summary>
    /// キャラクターが選択されているか確認
    /// </summary>
    public bool IsSelected(CharacterData character)
    {
        return selectedCharacters.Contains(character);
    }
    
    /// <summary>
    /// 選択中のキャラクター数を取得
    /// </summary>
    public int GetSelectedCount()
    {
        return selectedCharacters.Count;
    }
    
    /// <summary>
    /// 選択中のキャラクターリストを取得
    /// </summary>
    public List<CharacterData> GetSelectedCharacters()
    {
        return new List<CharacterData>(selectedCharacters);
    }
    
    /// <summary>
    /// 全選択をクリア
    /// </summary>
    public void ClearSelection()
    {
        selectedCharacters.Clear();
    }
    
    /// <summary>
    /// 選択が完了しているか（最大数に達しているか）
    /// </summary>
    public bool IsSelectionComplete()
    {
        return selectedCharacters.Count == maxSelectionCount;
    }
    
    /// <summary>
    /// 選択が有効か（1体以上選択されているか）
    /// </summary>
    public bool IsSelectionValid()
    {
        return selectedCharacters.Count > 0;
    }
}
