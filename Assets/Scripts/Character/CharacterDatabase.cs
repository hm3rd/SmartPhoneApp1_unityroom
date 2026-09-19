using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 獲得済みキャラクターを管理・保存するランタイムDB。
/// 全キャラクターの定義は CharacterCatalog が担当する。
/// </summary>
public class CharacterDatabase : MonoBehaviour
{
    private const string OwnedIdsKey = "OwnedCharacterIds_v1";

    public static CharacterDatabase Instance { get; private set; }

    [Header("全キャラクターカタログ")]
    [SerializeField] private CharacterCatalog catalog;

    [Header("獲得済みキャラクター（実行時に構築）")]
    [Tooltip("既存Sceneとの互換用。初回起動時は、ここに登録済みのキャラも初期所持になります。")]
    public List<CharacterData> characters = new List<CharacterData>();

    private readonly HashSet<int> ownedCharacterIds = new HashSet<int>();
    private bool initialized;

    public CharacterCatalog Catalog => catalog;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureInitialized();
    }

    public static CharacterDatabase GetOrCreate()
    {
        if (Instance != null)
        {
            return Instance;
        }

        CharacterDatabase existing = FindFirstObjectByType<CharacterDatabase>();
        if (existing != null)
        {
            existing.EnsureInitialized();
            return existing;
        }

        return new GameObject(nameof(CharacterDatabase)).AddComponent<CharacterDatabase>();
    }

    private void EnsureInitialized()
    {
        if (initialized) return;
        initialized = true;

        if (catalog == null)
        {
            catalog = Resources.Load<CharacterCatalog>("CharacterCatalog");
        }

        List<CharacterData> legacyCharacters = characters
            .Where(character => character != null)
            .ToList();

        if (PlayerPrefs.HasKey(OwnedIdsKey))
        {
            LoadOwnedIds();
        }
        else
        {
            IEnumerable<CharacterData> starters =
                catalog != null && catalog.initialCharacters.Count > 0
                    ? catalog.initialCharacters
                    : legacyCharacters;

            foreach (CharacterData character in starters)
            {
                if (character != null)
                {
                    ownedCharacterIds.Add(character.characterId);
                }
            }
            SaveOwnedIds();
        }

        RebuildOwnedCharacters(legacyCharacters);
    }

    public bool UnlockCharacter(CharacterData character)
    {
        if (character == null || !ownedCharacterIds.Add(character.characterId))
        {
            return false;
        }

        SaveOwnedIds();
        RebuildOwnedCharacters();
        return true;
    }

    public bool IsOwned(int characterId)
    {
        return ownedCharacterIds.Contains(characterId);
    }

    public CharacterData GetCharacter(int index)
    {
        if (index < 0 || index >= characters.Count)
        {
            return null;
        }
        return characters[index];
    }

    public CharacterData GetCharacterByName(string name)
    {
        return characters.Find(character => character != null && character.characterName == name);
    }

    public CharacterData GetCharacterById(int id)
    {
        return characters.Find(character => character != null && character.characterId == id);
    }

    public Sprite GetCharacterIconById(int id)
    {
        return GetCharacterById(id)?.characterIcon;
    }

    public int GetCharacterCount()
    {
        return characters.Count;
    }

    private void RebuildOwnedCharacters(IEnumerable<CharacterData> fallback = null)
    {
        IEnumerable<CharacterData> source =
            catalog != null ? catalog.allCharacters : fallback ?? characters;

        characters = source
            .Where(character => character != null && ownedCharacterIds.Contains(character.characterId))
            .GroupBy(character => character.characterId)
            .Select(group => group.First())
            .ToList();
    }

    private void LoadOwnedIds()
    {
        ownedCharacterIds.Clear();
        string saved = PlayerPrefs.GetString(OwnedIdsKey, string.Empty);
        foreach (string value in saved.Split(','))
        {
            if (int.TryParse(value, out int id))
            {
                ownedCharacterIds.Add(id);
            }
        }
    }

    private void SaveOwnedIds()
    {
        PlayerPrefs.SetString(OwnedIdsKey, string.Join(",", ownedCharacterIds.OrderBy(id => id)));
        PlayerPrefs.Save();
    }
}
