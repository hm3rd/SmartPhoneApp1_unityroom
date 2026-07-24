using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    public float speed = 2f; // 追尾速度
    [Min(0f)]
    [Tooltip("Collider同士の間に確保する追加距離")]
    public float playerSeparationPadding = 0.05f;

    private Transform player;
    private Rigidbody2D rb;
    private Collider2D solidCollider;
    private Collider2D playerSolidCollider;
    private Vector2 knockbackDirection;
    private float knockbackSpeed;
    private float knockbackTimeRemaining;
    private float knockbackTotalTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        solidCollider = FindSolidCollider(gameObject);
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    void Start()
    {
        // "Player"タグのオブジェクトを探す
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerSolidCollider = FindSolidCollider(playerObj);
        }
    }

    void FixedUpdate()
    {
        Vector2 movement = Vector2.zero;

        if (knockbackTimeRemaining > 0f)
        {
            float ratio = knockbackTimeRemaining / knockbackTotalTime;
            movement = knockbackDirection * knockbackSpeed * ratio;
            knockbackTimeRemaining =
                Mathf.Max(0f, knockbackTimeRemaining - Time.fixedDeltaTime);
        }
        else
        if (player != null)
        {
            movement =
                ((Vector2)player.position - CurrentPosition).normalized * speed;
        }

        Vector2 nextPosition =
            CurrentPosition + movement * Time.fixedDeltaTime;
        nextPosition = KeepOutsidePlayer(nextPosition);
        if (rb != null)
        {
            rb.MovePosition(nextPosition);
        }
        else
        {
            transform.position =
                new Vector3(nextPosition.x, nextPosition.y, transform.position.z);
        }
    }

    private Vector2 CurrentPosition =>
        rb != null ? rb.position : (Vector2)transform.position;

    private Vector2 KeepOutsidePlayer(Vector2 nextPosition)
    {
        if (player == null ||
            solidCollider == null ||
            playerSolidCollider == null)
        {
            return nextPosition;
        }

        Vector2 playerPosition = player.position;
        Vector2 awayFromPlayer = nextPosition - playerPosition;
        if (awayFromPlayer.sqrMagnitude < 0.0001f)
        {
            awayFromPlayer = CurrentPosition - playerPosition;
            if (awayFromPlayer.sqrMagnitude < 0.0001f)
            {
                awayFromPlayer = Vector2.right;
            }
        }

        float enemyRadius = Mathf.Max(
            solidCollider.bounds.extents.x,
            solidCollider.bounds.extents.y);
        float playerRadius = Mathf.Max(
            playerSolidCollider.bounds.extents.x,
            playerSolidCollider.bounds.extents.y);
        float minimumDistance =
            enemyRadius + playerRadius + Mathf.Max(0f, playerSeparationPadding);

        if (awayFromPlayer.sqrMagnitude <
            minimumDistance * minimumDistance)
        {
            return playerPosition +
                awayFromPlayer.normalized * minimumDistance;
        }

        return nextPosition;
    }

    private static Collider2D FindSolidCollider(GameObject target)
    {
        Collider2D[] colliders = target.GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            if (collider.enabled && !collider.isTrigger)
            {
                return collider;
            }
        }
        return null;
    }

    public void ApplyKnockback(
        Vector2 direction,
        float distance,
        float duration)
    {
        if (distance <= 0f || direction.sqrMagnitude <= 0f)
        {
            return;
        }

        knockbackDirection = direction.normalized;
        knockbackTotalTime = Mathf.Max(0.01f, duration);
        knockbackTimeRemaining = knockbackTotalTime;

        // 速度を直線的に0まで落としたとき、指定距離だけ進む初速
        knockbackSpeed = distance * 2f / knockbackTotalTime;
    }
}
