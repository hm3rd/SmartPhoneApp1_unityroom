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

    [Header("ダメージ量に応じたノックバック")]
    [Tooltip("横軸=受けたダメージ、縦軸=ノックバック距離。カーブ上を右クリックして点を追加できます")]
    [SerializeField] private AnimationCurve knockbackDistanceByDamage =
        new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(10f, 0.5f),
            new Keyframe(20f, 1.5f),
            new Keyframe(100f, 4f));

    [Min(0.01f)]
    [SerializeField] private float knockbackDuration = 0.2f;

    [Header("非常に大きなダメージ時の無敵")]
    [Min(0)]
    [Tooltip("この値以上のダメージを受けると無敵と点滅を開始します")]
    [SerializeField] private int heavyDamageThreshold = 50;

    [Min(0f)]
    [SerializeField] private float heavyDamageInvincibleTime = 1.5f;

    [Min(0.02f)]
    [Tooltip("点灯・消灯を切り替える間隔")]
    [SerializeField] private float blinkInterval = 0.1f;

    [Tooltip("点滅させるPlayer画像。未設定なら子オブジェクトを含めて自動検索します")]
    [SerializeField] private SpriteRenderer playerVisualRenderer;

    private float lastDamageTime = -10f; // 最後にダメージを受けた時刻
    private float forcedInvincibleUntil = -1f;
    private Coroutine knockbackCoroutine;
    private Coroutine blinkCoroutine;

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

        if (playerVisualRenderer == null)
        {
            playerVisualRenderer = GetComponentInChildren<SpriteRenderer>(true);
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

        damage = Mathf.Max(0, damage);
        if (damage == 0) return;

        // GameCharacterManager 経由でダメージを適用
        gameCharacterManager.ApplyDamageToCurrent(damage);

        float knockbackDistance = knockbackDistanceByDamage != null
            ? Mathf.Max(0f, knockbackDistanceByDamage.Evaluate(damage))
            : 0f;
        if (damageSourcePosition.HasValue && knockbackDistance > 0f)
        {
            StartPlayerKnockback(damageSourcePosition.Value, knockbackDistance);
        }

        if (damage >= heavyDamageThreshold && heavyDamageInvincibleTime > 0f)
        {
            SetTemporaryInvincibility(heavyDamageInvincibleTime);
            StartBlink(heavyDamageInvincibleTime);
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

    private void StartPlayerKnockback(Vector2 sourcePosition, float distance)
    {
        if (knockbackCoroutine != null)
        {
            return;
        }
        knockbackCoroutine =
            StartCoroutine(PlayerKnockbackRoutine(sourcePosition, distance));
    }

    private IEnumerator PlayerKnockbackRoutine(Vector2 sourcePosition, float distance)
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
            startPosition + direction.normalized * distance;
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

    private void StartBlink(float duration)
    {
        if (playerVisualRenderer == null)
        {
            playerVisualRenderer = GetComponentInChildren<SpriteRenderer>(true);
        }
        if (playerVisualRenderer == null) return;

        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            playerVisualRenderer.enabled = true;
        }
        blinkCoroutine = StartCoroutine(BlinkRoutine(duration));
    }

    private IEnumerator BlinkRoutine(float duration)
    {
        float endTime = Time.time + Mathf.Max(0f, duration);
        WaitForSeconds wait = new WaitForSeconds(Mathf.Max(0.02f, blinkInterval));
        while (Time.time < endTime)
        {
            playerVisualRenderer.enabled = !playerVisualRenderer.enabled;
            yield return wait;
        }
        playerVisualRenderer.enabled = true;
        blinkCoroutine = null;
    }

    private void OnDisable()
    {
        if (playerVisualRenderer != null)
        {
            playerVisualRenderer.enabled = true;
        }
    }

    private void OnValidate()
    {
        knockbackDuration = Mathf.Max(0.01f, knockbackDuration);
        heavyDamageThreshold = Mathf.Max(0, heavyDamageThreshold);
        heavyDamageInvincibleTime = Mathf.Max(0f, heavyDamageInvincibleTime);
        blinkInterval = Mathf.Max(0.02f, blinkInterval);
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
