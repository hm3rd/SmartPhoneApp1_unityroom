using System.Collections;
using UnityEngine;

public class ComboChainAttack : MonoBehaviour
{
    [System.Serializable]
    public class ComboStep
    {
        [Tooltip("この段階で生成する攻撃判定のPrefab")]
        public GameObject atkPointPrefab;
        
        [Tooltip("この攻撃のダメージ量")]
        public int damage = 10;
        
        [Tooltip("前方に出す距離")]
        public float attackDistance = 1.0f;
        
        [Tooltip("判定の持続時間")]
        public float attackDuration = 0.2f;
        
        [Tooltip("次の段階までの待機時間（秒）")]
        public float intervalToNext = 0.2f;
    }

    [Header("コンボ設定")]
    [Tooltip("各段階の攻撃設定（順番に実行されます）")]
    public ComboStep[] comboSteps = new ComboStep[3];

    [Header("コンボリセット設定")]
    [Tooltip("最後の攻撃からこの秒数経過でコンボリセット")]
    public float comboResetTime = 1.0f;

    private AttackCooldownManager cooldownManager;
    private IPlayerAttack playerAttack; // プレイヤーの向き情報
    private bool isAttacking = false; // 攻撃実行中フラグ
    private int currentComboIndex = 0; // 現在のコンボ段階
    private float lastAttackTime = -999f; // 最後に攻撃した時刻

    void Awake()
    {
        cooldownManager = GetComponent<AttackCooldownManager>();
        playerAttack = GetComponent<IPlayerAttack>();
    }

    void Update()
    {
        // コンボリセット判定
        if (currentComboIndex > 0 && Time.time - lastAttackTime > comboResetTime)
        {
            currentComboIndex = 0;
        }
    }

    // ボタンから呼び出す
    public void Attack()
    {
        if (cooldownManager != null && cooldownManager.IsOnCooldown(this)) return; // クールタイム中は攻撃不可
        if (isAttacking) return; // 既に攻撃中なら新規攻撃しない
        if (comboSteps == null || comboSteps.Length == 0) return; // コンボ未設定

        StartCoroutine(ExecuteComboStep());
    }

    private IEnumerator ExecuteComboStep()
    {
        isAttacking = true;

        // 現在のコンボ段階を取得（ループさせる）
        int stepIndex = currentComboIndex % comboSteps.Length;
        ComboStep step = comboSteps[stepIndex];

        if (step.atkPointPrefab != null)
        {
            // プレイヤーの向きを取得
            bool isRight = playerAttack != null ? playerAttack.isRight : true;
            
            // 向きによって攻撃判定の方向を変える
            Vector3 atkDir = isRight ? transform.right : -transform.right;
            Vector3 atkPos = transform.position + atkDir * step.attackDistance;

            // Prefabを生成
            GameObject atkObj = Instantiate(step.atkPointPrefab, atkPos, Quaternion.identity);

            // AttackHitBoxコンポーネントがあればダメージを設定
            AttackHitBox hitBox = atkObj.GetComponent<AttackHitBox>();
            if (hitBox != null)
            {
                hitBox.SetDamage(step.damage);
            }

            Destroy(atkObj, step.attackDuration);
        }

        // 次のコンボ段階へ進める
        currentComboIndex++;
        lastAttackTime = Time.time;

        // 待機時間
        yield return new WaitForSeconds(step.intervalToNext);

        isAttacking = false;

        // クールタイム開始（各攻撃ごと、または最終段のみにしたい場合は条件追加）
        if (cooldownManager != null)
        {
            cooldownManager.StartCooldown(this);
        }
    }

    // コンボリセット（外部から呼ぶ場合）
    public void ResetCombo()
    {
        currentComboIndex = 0;
        StopAllCoroutines();
        isAttacking = false;
    }

    // デバッグ用：現在のコンボ段階を取得
    public int GetCurrentComboIndex()
    {
        return currentComboIndex;
    }
}
