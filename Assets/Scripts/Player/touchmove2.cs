using UnityEngine;

public class TouchMove2 : MonoBehaviour, IPlayerAttack
{
    private Vector2 firstPos;
    private Vector2 currentPos;
    private Vector2 moveVector;
    public float speed = 5f; // 通常移動速度

    // フリック用
    private Vector2 flickStartPos;
    private float flickStartTime;
    private Vector3 flickVelocity = Vector3.zero;
    public float flickPower = 15f; // フリック時の速度
    public float flickDuration = 0.2f; // フリックで進む時間
    private float flickTimer = 0f;
    public float flickCooldown = 1.0f; // フリッククールタイム（秒）
    private float flickCooldownTimer = 0f;

    [Header("向き/表示")]
    [Tooltip("プレイヤーのSpriteRenderer。向きに応じてflipXを切り替えます。")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    public bool isRight { get; set; } = true; // IPlayerAttack 実装
    private bool lastFacingRight = true;

    [Header("敵との重なり防止")]
    [Min(0.01f)]
    [Tooltip("重なり防止用CircleCollider2Dの半径。接触ダメージ用Triggerとは別に自動生成します")]
    [SerializeField] private float collisionRadius = 1.7f;

    private Rigidbody2D rb;
    private Vector2 pendingMovement;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        EnsureSolidCollider();
    }

    void Start()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
    }

    void Update()
    {
        if (flickCooldownTimer > 0f)
        {
            flickCooldownTimer -= Time.deltaTime;
        }
        Move();
        FlickMove(); // フリック移動を毎フレーム処理
    }

    void FixedUpdate()
    {
        if (pendingMovement.sqrMagnitude <= 0f)
        {
            return;
        }

        if (rb != null)
        {
            rb.MovePosition(rb.position + pendingMovement);
        }
        else
        {
            transform.position += (Vector3)pendingMovement;
        }
        pendingMovement = Vector2.zero;
    }

    private void EnsureSolidCollider()
    {
        CircleCollider2D solidCollider = null;
        CircleCollider2D[] colliders = GetComponents<CircleCollider2D>();
        foreach (CircleCollider2D collider in colliders)
        {
            if (!collider.isTrigger)
            {
                solidCollider = collider;
                break;
            }
        }

        if (solidCollider == null)
        {
            solidCollider = gameObject.AddComponent<CircleCollider2D>();
            solidCollider.isTrigger = false;
        }

        solidCollider.radius = Mathf.Max(0.01f, collisionRadius);
    }

    void Move()
    {
        if (Input.touchCount > 0)
        {
            Vector2 touchPos = Input.touches[0].position;
            if (touchPos.x < Screen.width / 2)
            {
                if (Input.touches[0].phase == TouchPhase.Moved)
                {
                    Vector2 move = Input.touches[0].deltaPosition;
                    // 右に動いたら右向き、左に動いたら左向き
                    if (move.x > 0.1f) UpdateFacing(true);
                    else if (move.x < -0.1f) UpdateFacing(false);
                }
                if (Input.touches[0].phase == TouchPhase.Began)
                {
                    firstPos = touchPos;
                    // フリック用
                    flickStartPos = touchPos;
                    flickStartTime = Time.time;
                }
                if (Input.touches[0].phase == TouchPhase.Moved || Input.touches[0].phase == TouchPhase.Stationary)
                {
                    currentPos = touchPos;
                    moveVector = currentPos - firstPos; // 中心からの差分
                    Vector3 moveDir = new Vector3(moveVector.x, moveVector.y, 0).normalized;
                    pendingMovement +=
                        (Vector2)moveDir * speed * Time.deltaTime;
                }
                if (Input.touches[0].phase == TouchPhase.Ended || Input.touches[0].phase == TouchPhase.Canceled)
                {
                    moveVector = Vector2.zero;
                    FlickJudge(touchPos); // フリック判定
                }
            }
        }
    }

    private void UpdateFacing(bool facingRight)
    {
        isRight = facingRight;
        if (lastFacingRight != facingRight)
        {
            if (spriteRenderer != null)
            {
                // 右向きのとき flipX=false, 左でtrue （必要に応じて逆転）
                spriteRenderer.flipX = !facingRight;
            }
            lastFacingRight = facingRight;
        }
    }

    // フリック判定関数
    void FlickJudge(Vector2 endPos)
    {
        if (flickCooldownTimer > 0f) return; // クールタイム中はフリック不可

        float flickTime = Time.time - flickStartTime;
        Vector2 flickVector = endPos - flickStartPos;

        // フリック判定（距離と時間で判定、値は調整してください）
        if (flickVector.magnitude > 50f && flickTime < 0.3f)
        {
            Vector3 dir = new Vector3(flickVector.x, flickVector.y, 0).normalized;
            flickVelocity = dir * flickPower;
            flickTimer = flickDuration;
            flickCooldownTimer = flickCooldown; // クールタイム開始
        }
    }

    // フリック移動処理
    void FlickMove()
    {
        if (flickTimer > 0f)
        {
            pendingMovement +=
                (Vector2)flickVelocity * Time.deltaTime;
            flickTimer -= Time.deltaTime;
            if (flickTimer <= 0f)
            {
                flickVelocity = Vector3.zero;
            }
        }
    }
}

public interface IPlayerAttack
{
    bool isRight { get; set; }
    // 必要なら攻撃メソッドなども定義
}
