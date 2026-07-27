using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHP : MonoBehaviour
{
    [SerializeField] private GameCharacterManager gameCharacterManager;
    public float invincibleTime = 1.0f; // 無敵時間（秒）

    [Header("敵との接触ダメージ")]
    [Min(0)]
    [SerializeField] private int contactDamage = 10;

    [Header("大ダメージ時のノックバック")]
    [Min(0)]
    [Tooltip("この値以上のダメージを受けるとノックバックします")]
    [SerializeField] private int knockbackDamageThreshold = 20;

    [Min(0f)]
    [SerializeField] private float knockbackDistance = 1.5f;

    [Min(0.01f)]
    [SerializeField] private float knockbackDuration = 0.2f;

    private float lastDamageTime = -10f; // 最後にダメージを受けた時刻
    private float forcedInvincibleUntil = -1f;
    private Coroutine knockbackCoroutine;

    void Start()
    {
        // GameCharacterManager の自動検索
        if (gameCharacterManager == null)
        {
            gameCharacterManager = FindObjectOfType<GameCharacterManager>();
            if (gameCharacterManager == null)
            {
                enabled = false;
                return;
            }
        }
    }

    // HPが変化したときに呼ぶ
    public void TakeDamage(int damage)
    {
        ApplyDamage(damage, null);
    }

    public void TakeDamage(int damage, Vector2 damageSourcePosition)
    {
        ApplyDamage(damage, damageSourcePosition);
    }

    private void ApplyDamage(int damage, Vector2? damageSourcePosition)
    {
        if (Time.time < forcedInvincibleUntil)
        {
            return;
        }

        if (gameCharacterManager == null)
        {
            return;
        }

        // GameCharacterManager 経由でダメージを適用
        gameCharacterManager.ApplyDamageToCurrent(damage);

        if (damageSourcePosition.HasValue &&
            damage >= knockbackDamageThreshold &&
            knockbackDistance > 0f)
        {
            StartPlayerKnockback(damageSourcePosition.Value);
        }
    }

    public void SetTemporaryInvincibility(float duration)
    {
        forcedInvincibleUntil = Mathf.Max(
            forcedInvincibleUntil,
            Time.time + Mathf.Max(0f, duration));
    }

    public void Heal(int amount)
    {
        if (gameCharacterManager == null)
        {
            return;
        }

        // GameCharacterManager 経由で回復を適用
        gameCharacterManager.HealCurrent(amount);
    }

    private void StartPlayerKnockback(Vector2 sourcePosition)
    {
        if (knockbackCoroutine != null)
        {
            return;
        }
        knockbackCoroutine =
            StartCoroutine(PlayerKnockbackRoutine(sourcePosition));
    }

    private IEnumerator PlayerKnockbackRoutine(Vector2 sourcePosition)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        TouchMove2 touchMove = GetComponent<TouchMove2>();
        WASDMoveDebug debugMove = GetComponent<WASDMoveDebug>();
        bool restoreTouchMove = touchMove != null && touchMove.enabled;
        bool restoreDebugMove = debugMove != null && debugMove.enabled;

        if (touchMove != null)
        {
            touchMove.ClearPendingMovement();
            touchMove.enabled = false;
        }
        if (debugMove != null)
        {
            debugMove.enabled = false;
        }

        Vector2 startPosition = rb != null
            ? rb.position
            : (Vector2)transform.position;
        Vector2 direction = startPosition - sourcePosition;
        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = Vector2.left;
        }
        Vector2 endPosition =
            startPosition + direction.normalized * knockbackDistance;
        float duration = Mathf.Max(0.01f, knockbackDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.fixedDeltaTime;
            float ratio = Mathf.Clamp01(elapsed / duration);
            // 終端に向かって減速する
            float easedRatio = 1f - (1f - ratio) * (1f - ratio);
            Vector2 nextPosition =
                Vector2.Lerp(startPosition, endPosition, easedRatio);

            if (rb != null)
            {
                rb.MovePosition(nextPosition);
            }
            else
            {
                transform.position = new Vector3(
                    nextPosition.x,
                    nextPosition.y,
                    transform.position.z);
            }
            yield return new WaitForFixedUpdate();
        }

        if (touchMove != null)
        {
            touchMove.ClearPendingMovement();
            touchMove.enabled = restoreTouchMove;
        }
        if (debugMove != null)
        {
            debugMove.enabled = restoreDebugMove;
        }
        knockbackCoroutine = null;
    }

    // Enemyタグに当たったらダメージ（無敵時間考慮）
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyAttack enemyAttack =
                other.GetComponentInParent<EnemyAttack>();
            if (enemyAttack != null && enemyAttack.enabled)
            {
                return;
            }

            if (Time.time < forcedInvincibleUntil)
            {
                return;
            }

            if (Time.time - lastDamageTime >= invincibleTime)
            {
                TakeDamage(contactDamage, other.transform.position);
                lastDamageTime = Time.time; // ここでのみ更新
            }
        }
    }
}
