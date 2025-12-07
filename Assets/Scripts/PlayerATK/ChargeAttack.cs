using UnityEngine;

public class ChargeAttack : MonoBehaviour, IAttackBehavior
{
    [Header("基本設定")]
    public GameObject chargePrefab; // チャージ攻撃のPrefab
    public float maxChargeTime = 2f; // 最大チャージ時間
    public float attackDuration = 0.5f; // 攻撃判定の持続時間

    [Header("スケール設定")]
    public float minScale = 0.5f; // 最小スケール
    public float maxScale = 2f; // 最大スケール

    [Header("ダメージ設定")]
    public int minDamage = 10; // 最小ダメージ
    public int maxDamage = 50; // 最大ダメージ

    private bool isCharging = false;
    private float currentCharge = 0f;
    private AttackCooldownManager cooldownManager;
    private IPlayerAttack playerAttack; // プレイヤーの向き情報

    void Awake()
    {
        cooldownManager = GetComponent<AttackCooldownManager>();
        playerAttack = GetComponent<IPlayerAttack>();
    }

    void Update()
    {
        if (isCharging)
        {
            currentCharge += Time.deltaTime;
            
            // 最大チャージ時間に達したら自動発動
            if (currentCharge >= maxChargeTime)
            {
                ReleaseCharge();
            }
        }
    }

    // ボタン押下開始
    public void StartCharge()
    {
        if (cooldownManager != null && cooldownManager.IsOnCooldown(this)) return;
        
        isCharging = true;
        currentCharge = 0f;
    }

    // ボタン離した時
    public void ReleaseCharge()
    {
        if (!isCharging) return;

        isCharging = false;

        // チャージ量に応じてスケールとダメージを計算
        float chargeRatio = Mathf.Clamp01(currentCharge / maxChargeTime);
        float scale = Mathf.Lerp(minScale, maxScale, chargeRatio);
        int damage = Mathf.RoundToInt(Mathf.Lerp(minDamage, maxDamage, chargeRatio));

        Debug.Log($"チャージ: {currentCharge:F2}秒 / {maxChargeTime}秒, 割合: {chargeRatio:F2}, ダメージ: {damage}");

        // プレイヤーの向きを取得
        bool isRight = playerAttack != null ? playerAttack.isRight : true;
        
        // チャージ攻撃生成（向きに応じて位置調整可能）
        GameObject obj = Instantiate(chargePrefab, transform.position, Quaternion.identity);
        obj.transform.localScale = Vector3.one * scale;

        // ダメージ設定
        AttackHitBox hitBox = obj.GetComponent<AttackHitBox>();
        if (hitBox != null)
        {
            hitBox.SetDamage(damage);
        }

        Destroy(obj, attackDuration);

        // クールダウン開始
        if (cooldownManager != null)
        {
            cooldownManager.StartCooldown(this);
        }

        currentCharge = 0f;
    }

    // IAttackBehavior 実装
    public void Attack()
    {
        StartCharge();
        currentCharge = maxChargeTime;
        ReleaseCharge();
    }
}
