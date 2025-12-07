using UnityEngine;
using UnityEngine.UI;

public class PlayerAttack1 : MonoBehaviour
{
    public GameObject atkPointPrefab; // AttackHitBoxのPrefab
    public float attackDistance = 1.0f; // 前方に出す距離
    public float attackDuration = 0.2f; // 判定の持続時間
    public int damage = 10; // この攻撃のダメージ

    private AttackCooldownManager cooldownManager;
    private IPlayerAttack playerAttack; // プレイヤーの向き情報を取得

    void Awake()
    {
        // 同じGameObjectにアタッチされた中央管理を取得
        cooldownManager = GetComponent<AttackCooldownManager>();
        playerAttack = GetComponent<IPlayerAttack>();
    }

    void Update() {}

    // ボタンから呼び出す
    public void Attack()
    {
        if (cooldownManager != null && cooldownManager.IsOnCooldown(this)) return; // クールタイム中は攻撃不可

        // プレイヤーの向きを取得
        bool isRight = playerAttack != null ? playerAttack.isRight : true;
        
        // 向きによって攻撃判定の方向を変える
        Vector3 atkDir = isRight ? transform.right : -transform.right;
        Vector3 atkPos = transform.position + atkDir * attackDistance;

        // Prefabを前方に生成
        GameObject atkObj = Instantiate(atkPointPrefab, atkPos, Quaternion.identity);
        
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