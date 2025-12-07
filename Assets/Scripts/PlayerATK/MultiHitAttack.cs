using System.Collections;
using UnityEngine;

public class MultiHitAttack : MonoBehaviour
{
    [Header("攻撃設定")]
    public GameObject atkPointPrefab; // 攻撃判定のPrefab
    public int hitCount = 3; // 攻撃回数
    public float hitInterval = 0.15f; // 各攻撃の間隔（秒）
    public float attackDistance = 1.0f; // 前方に出す距離
    public float attackDuration = 0.2f; // 各判定の持続時間
    public int damage = 8; // 1ヒット当たりのダメージ

    private AttackCooldownManager cooldownManager;
    private IPlayerAttack playerAttack; // プレイヤーの向き情報
    private bool isAttacking = false; // 多段攻撃実行中フラグ

    void Awake()
    {
        cooldownManager = GetComponent<AttackCooldownManager>();
        playerAttack = GetComponent<IPlayerAttack>();
    }

    // ボタンから呼び出す
    public void Attack()
    {
        if (cooldownManager != null && cooldownManager.IsOnCooldown(this)) return; // クールタイム中は攻撃不可
        if (isAttacking) return; // 既に多段攻撃中なら新規攻撃しない

        StartCoroutine(MultiHitRoutine());
    }

    private IEnumerator MultiHitRoutine()
    {
        isAttacking = true;
        
        // プレイヤーの向きを取得
        bool isRight = playerAttack != null ? playerAttack.isRight : true;

        for (int i = 0; i < hitCount; i++)
        {
            // 向きによって攻撃判定の方向を変える
            Vector3 atkDir = isRight ? transform.right : -transform.right;
            Vector3 atkPos = transform.position + atkDir * attackDistance;

            // Prefabを生成
            GameObject atkObj = Instantiate(atkPointPrefab, atkPos, Quaternion.identity);
            
            // ダメージ設定
            AttackHitBox hitBox = atkObj.GetComponent<AttackHitBox>();
            if (hitBox != null)
            {
                hitBox.SetDamage(damage);
            }
            
            Destroy(atkObj, attackDuration);

            // 最後の攻撃でなければ待機
            if (i < hitCount - 1)
            {
                yield return new WaitForSeconds(hitInterval);
            }
        }

        isAttacking = false;

        // 全ての攻撃完了後にクールタイム開始
        if (cooldownManager != null)
        {
            cooldownManager.StartCooldown(this);
        }
    }

    // コルーチンを外部から停止したい場合（オプション）
    public void CancelAttack()
    {
        StopAllCoroutines();
        isAttacking = false;
    }
}
