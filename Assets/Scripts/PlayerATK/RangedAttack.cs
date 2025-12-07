using UnityEngine;

public class RangedAttack : MonoBehaviour, IAttackBehavior, IPlayerAttack
{
    public GameObject projectilePrefab;
    public float projectileSpeed = 5f;
    public float spawnOffset = 0.5f; // 発射位置のオフセット
    public int damage = 12; // この攻撃のダメージ
    public bool isRight { get; set; }

    public void Attack()
    {
        Debug.Log(isRight);
        // 向きに応じて発射位置を前方にずらす
        float dir = isRight ? 1f : -1f;
        Vector3 spawnPos = transform.position + Vector3.right * spawnOffset * dir;

        // 弾生成
        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        // ダメージ設定
        AttackHitBox hitBox = proj.GetComponent<AttackHitBox>();
        if (hitBox != null)
        {
            hitBox.SetDamage(damage);
        }

        // Rigidbody2D があれば速度を設定
        Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.right * projectileSpeed * dir;
        }

        // 弾を左右反転（Sprite用）
        proj.transform.localScale = new Vector3(dir, 1, 1);

        // 寿命で削除
        Destroy(proj, 3f);
    }
}
