using UnityEngine;

/// <summary>
/// 攻撃の基本データを定義するScriptableObject
/// Inspector上で攻撃の種類ごとに設定を作成できる
/// </summary>
[CreateAssetMenu(fileName = "NewAttackData", menuName = "PlayerATK2/AttackData")]
public class AttackData : ScriptableObject
{
    [Header("基本情報")]
    [Tooltip("攻撃の名前")]
    public string attackName = "通常攻撃";
    
    [Header("ダメージ設定")]
    [Tooltip("与えるダメージ量")]
    public int damage = 10;
    
    [Header("クールタイム設定")]
    [Tooltip("攻撃後のクールタイム（秒）")]
    public float cooldownTime = 1.0f;
    
    [Header("攻撃判定設定")]
    [Tooltip("攻撃判定のPrefab")]
    public GameObject hitBoxPrefab;
    
    [Tooltip("攻撃判定の持続時間（秒）")]
    public float duration = 0.2f;
    
    [Tooltip("攻撃判定の生成位置オフセット（プレイヤーからの距離）")]
    public float spawnDistance = 1.0f;
    
    [Tooltip("攻撃がプレイヤーの向きに従うか")]
    public bool followPlayerDirection = true;
    
    [Tooltip("左向きの際に攻撃PrefabのX軸を反転するか")]
    public bool flipOnDirection = true;
    
    [Header("特殊設定")]
    [Tooltip("攻撃のスケール倍率")]
    public float scale = 1.0f;
}
