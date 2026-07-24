using UnityEngine;

/// <summary>
/// 多段攻撃用のデータ
/// </summary>
[CreateAssetMenu(fileName = "NewMultiHitAttack", menuName = "PlayerATK2/MultiHitAttackData")]
public class MultiHitAttackData : AttackData
{
    [Header("多段攻撃設定")]
    [Tooltip("攻撃回数")]
    public int hitCount = 3;
    
    [Tooltip("各攻撃の間隔（秒）")]
    public float hitInterval = 0.15f;

    protected override void OnValidate()
    {
        base.OnValidate();
        hitCount = Mathf.Max(1, hitCount);
        hitInterval = Mathf.Max(0f, hitInterval);
    }
}
