using UnityEngine;

/// <summary>
/// AttackDataから生成された飛び道具を直進させ、
/// 画面外またはAttackDataのDuration到達時に削除する。
/// </summary>
public class ProjectileMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 direction = Vector2.right;
    private float speed;
    private bool destroyOffScreen;
    private float viewportMargin;
    private Camera targetCamera;
    private bool initialized;

    public void Initialize(
        Vector2 moveDirection,
        float moveSpeed,
        bool shouldDestroyOffScreen,
        float offScreenMargin)
    {
        direction = moveDirection.sqrMagnitude > 0f
            ? moveDirection.normalized
            : Vector2.right;
        speed = Mathf.Max(0f, moveSpeed);
        destroyOffScreen = shouldDestroyOffScreen;
        viewportMargin = Mathf.Max(0f, offScreenMargin);
        targetCamera = Camera.main;

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        initialized = true;
    }

    private void FixedUpdate()
    {
        if (!initialized)
        {
            return;
        }

        Vector2 next = rb.position +
            direction * speed * Time.fixedDeltaTime;
        rb.MovePosition(next);
    }

    private void LateUpdate()
    {
        if (!initialized || !destroyOffScreen)
        {
            return;
        }

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null)
            {
                return;
            }
        }

        Vector3 viewport =
            targetCamera.WorldToViewportPoint(transform.position);
        if (viewport.z < 0f ||
            viewport.x < -viewportMargin ||
            viewport.x > 1f + viewportMargin ||
            viewport.y < -viewportMargin ||
            viewport.y > 1f + viewportMargin)
        {
            Destroy(gameObject);
        }
    }
}
