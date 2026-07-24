using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 実装予定を含む全キャラクターのマスターデータ。
/// 所持状態は持たず、CharacterDatabase が所持IDを保存する。
/// </summary>
[CreateAssetMenu(fileName = "CharacterCatalog", menuName = "Character/Character Catalog")]
public class CharacterCatalog : ScriptableObject
{
    [Tooltip("ゲームに実装する全キャラクター")]
    public List<CharacterData> allCharacters = new List<CharacterData>();

    [Tooltip("新規プレイヤーが最初から所持するキャラクター")]
    public List<CharacterData> initialCharacters = new List<CharacterData>();

    public CharacterData GetById(int characterId)
    {
        return allCharacters.Find(character =>
            character != null && character.characterId == characterId);
    }

    private void OnValidate()
    {
        HashSet<int> ids = new HashSet<int>();
        foreach (CharacterData character in allCharacters)
        {
            if (character != null && !ids.Add(character.characterId))
            {
                Debug.LogWarning($"CharacterCatalog: ID {character.characterId} が重複しています。", this);
            }
        }
    }
}
