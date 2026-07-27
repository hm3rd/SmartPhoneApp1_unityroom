using UnityEngine;
using System.Collections;

/// <summary>
/// 敵ごとの近距離攻撃を管理する。
/// AnimatorがあればAttack Trigger、なければ簡易伸縮モーションを使用する。
/// </summary>
public class EnemyAttack : MonoBehaviour
{
    [Header("攻撃性能")]
    [Min(0)] public int damage = 10;
    [Min(0.01f)] public float attackRange = 1.6f;
    [Min(0.01f)] public float attackCooldown = 1.5f;
    [Min(0f)] public float attackWindup = 0.25f;

    [Header("Animator（任意）")]
    [SerializeField] private Animator animator;
    [SerializeField] private string attackTriggerName = "Attack";

    [Header("簡易モーション")]
    [Tooltip("Animator未設定時に溜めと伸縮を表示します")]
    [SerializeField] private bool useSimpleMotion = true;
    [Min(1f)]
    [SerializeField] private float simpleMotionScale = 1.15f;

    private Transform player;
    private EnemyMove enemyMove;
    private bool attacking;
    private float nextAttackTime;
    private Vector3 originalScale;

    private void Awake()
    {
        enemyMove = GetComponent<EnemyMove>();
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        originalScale = transform.localScale;
    }

    private void Start()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    private void Update()
    {
        if (attacking ||
            player == null ||
            Time.time < nextAttackTime)
        {
            return;
        }

        float sqrDistance =
            ((Vector2)player.position - (Vector2)transform.position).sqrMagnitude;
        if (sqrDistance <= attackRange * attackRange)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    private IEnumerator AttackRoutine()
    {
        attacking = true;
        if (enemyMove != null)
        {
            enemyMove.SetMovementLocked(true);
        }

        if (animator != null &&
            !string.IsNullOrWhiteSpace(attackTriggerName))
        {
            animator.SetTrigger(attackTriggerName);
        }

        float windup = Mathf.Max(0f, attackWindup);
        float elapsed = 0f;
        while (elapsed < windup)
        {
            elapsed += Time.deltaTime;
            if (animator == null && useSimpleMotion)
            {
                float ratio = windup <= 0f
                    ? 1f
                    : Mathf.Clamp01(elapsed / windup);
                float pulse = Mathf.Sin(ratio * Mathf.PI);
                transform.localScale =
                    originalScale * Mathf.Lerp(
                        1f,
                        simpleMotionScale,
                        pulse);
            }
            yield return null;
        }

        transform.localScale = originalScale;
        TryDamagePlayer();

        if (enemyMove != null)
        {
            enemyMove.SetMovementLocked(false);
        }
        nextAttackTime = Time.time + Mathf.Max(0.01f, attackCooldown);
        attacking = false;
    }

    private void TryDamagePlayer()
    {
        if (player == null)
        {
            return;
        }

        float sqrDistance =
            ((Vector2)player.position - (Vector2)transform.position).sqrMagnitude;
        if (sqrDistance > attackRange * attackRange)
        {
            return;
        }

        PlayerHP playerHP = player.GetComponent<PlayerHP>();
        if (playerHP != null)
        {
            playerHP.TakeDamage(damage, transform.position);
        }
    }

    private void OnDisable()
    {
        transform.localScale = originalScale;
        if (enemyMove != null)
        {
            enemyMove.SetMovementLocked(false);
        }
        attacking = false;
    }
}
