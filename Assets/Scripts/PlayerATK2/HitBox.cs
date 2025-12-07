using UnityEngine;

/// <summary>
/// 攻撃のヒットボックス
/// 敵との衝突を検知してダメージを与える
/// </summary>
public class HitBox : MonoBehaviour
{
    private int damage = 10;
    
    /// <summary>
    /// ダメージを設定
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
