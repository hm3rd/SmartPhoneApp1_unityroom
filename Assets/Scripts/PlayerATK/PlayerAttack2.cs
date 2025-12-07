using UnityEngine;
using UnityEngine.UI;

public class PlayerAttack2 : MonoBehaviour
{
    public GameObject atkPointPrefab;      // 円形当たり判定のPrefab
    public float attackDuration = 0.2f;    // 判定の持続時間
    public int damage = 15; // この攻撃のダメージ

    private AttackCooldownManager cooldownManager;

    void Awake()
    {
        cooldownManager = GetComponent<AttackCooldownManager>();
    }

    void Update() {}

    // 攻撃ボタンから呼び出す
    public void Attack()
    {
        if (cooldownManager != null && cooldownManager.IsOnCooldown(this)) return;

        // プレイヤーの中心に生成
        GameObject atkObj = Instantiate(atkPointPrefab, transform.position, Quaternion.identity);
        
        // ダメージ設定
        AttackHitBox hitBox = atkObj.GetComponent<AttackHitBox>();
        if (hitBox != null)
        {
            hitBox.SetDamage(damage);
        }
        
        Destroy(atkObj, attackDuration);

        if (cooldownManager != null)
        {
            cooldownManager.StartCooldown(this);
        }
    }
}