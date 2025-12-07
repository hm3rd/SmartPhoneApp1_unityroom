using UnityEngine;

/// <summary>
/// チャージ攻撃用のデータ
/// </summary>
[CreateAssetMenu(fileName = "NewChargeAttack", menuName = "PlayerATK2/ChargeAttackData")]
public class ChargeAttackData : AttackData
{
    [Header("チャージ設定")]
    [Tooltip("最大チャージ時間（秒）")]
    public float maxChargeTime = 2.0f;
    
    [Tooltip("最小ダメージ（チャージなし）")]
    public int minDamage = 10;
    
    [Tooltip("最大ダメージ（フルチャージ）")]
    public int maxDamage = 50;
    
    [Tooltip("最小スケール")]
    public float minScale = 0.5f;
    
    [Tooltip("最大スケール")]
    public float maxScale = 2.0f;
}
