using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// キャラクターの基本情報を定義するScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "NewCharacter", menuName = "Character/CharacterData")]
public class CharacterData : ScriptableObject
{
    [Header("基本情報")]
    [Tooltip("キャラクターID（ユニークな識別子）")]
    public int characterId = 0;
    
    [Tooltip("キャラクター名")]
    public string characterName = "新しいキャラクター";
    
    [Tooltip("キャラクターの説明")]
    [TextArea(2, 4)]
    public string description = "";
    
    [Tooltip("キャラクターのアイコン/スプライト")]
    public Sprite characterSprite;
    
    // characterIconとして使用するプロパティ
    public Sprite characterIcon => characterSprite;

    [Header("ガチャ設定")]
    [Tooltip("ガチャの排出対象に含める")]
    public bool canBeObtainedFromGacha = true;

    [Min(1)]
    [Tooltip("排出の重み。値が大きいほど出やすくなります")]
    public int gachaWeight = 100;
    
    [Header("ステータス")]
    [Tooltip("最大HP")]
    public int maxHP = 100;
    
    [Tooltip("移動速度")]
    public float moveSpeed = 5f;
    
    [Header("攻撃設定")]
    [Tooltip("このキャラクターが使える攻撃のリスト（PlayerATK2フォルダのAttackDataを設定）")]
    public List<AttackData> availableAttacks = new List<AttackData>();
    
    [Header("見た目")]
    [Tooltip("ゲーム内で使用するキャラクタープレハブ（任意）")]
    public GameObject characterPrefab;
    
    [Tooltip("UI表示用の色")]
    public Color themeColor = Color.white;

    private void OnValidate()
    {
        gachaWeight = Mathf.Max(1, gachaWeight);
    }
}
