using UnityEngine;

/// <summary>
/// 攻撃のヒットボックス
/// 敵との衝突を検知してダメージを与える
/// </summary>
public class HitBox : MonoBehaviour
{
    private int damage = 10;
    private Vector2 sourcePosition;
    private Vector2 fallbackDirection = Vector2.right;
    private float knockbackDistance;
    private float knockbackDuration = 0.15f;
    private bool destroyOnHit;
    
    /// <summary>
    /// ダメージを設定
    /// </summary>
    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }

    public void Configure(
        int newDamage,
        Vector2 attackSourcePosition,
        Vector2 attackDirection,
        float newKnockbackDistance,
        float newKnockbackDuration,
        bool shouldDestroyOnHit)
    {
        damage = newDamage;
        sourcePosition = attackSourcePosition;
        fallbackDirection = attackDirection.sqrMagnitude > 0f
            ? attackDirection.normalized
            : Vector2.right;
        knockbackDistance = Mathf.Max(0f, newKnockbackDistance);
        knockbackDuration = Mathf.Max(0.01f, newKnockbackDuration);
        destroyOnHit = shouldDestroyOnHit;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyHP enemyHP = other.GetComponentInParent<EnemyHP>();
            if (enemyHP != null)
            {
                enemyHP.TakeDamage(damage);
            }

            EnemyMove enemyMove = other.GetComponentInParent<EnemyMove>();
            if (enemyMove != null && knockbackDistance > 0f)
            {
                Vector2 direction =
                    (Vector2)enemyMove.transform.position - sourcePosition;
                if (direction.sqrMagnitude < 0.0001f)
                {
                    direction = fallbackDirection;
                }
                enemyMove.ApplyKnockback(
                    direction.normalized,
                    knockbackDistance,
                    knockbackDuration);
            }

            if (destroyOnHit)
            {
                Destroy(gameObject);
            }
        }
    }
}
