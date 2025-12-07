using UnityEngine;

/// <summary>
/// 攻撃判定のヒットボックス。各攻撃スクリプトがダメージを設定して使用する。
/// </summary>
public class AttackHitBox : MonoBehaviour
{
    private int damage = 10; // デフォルトダメージ
    
    /// <summary>
    /// ダメージを設定（攻撃スクリプトから呼び出す）
    /// </summary>
    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyHP enemyHP = other.GetComponent<EnemyHP>();
            if (enemyHP != null)
            {
                enemyHP.TakeDamage(damage);
            }
        }
    }
}
