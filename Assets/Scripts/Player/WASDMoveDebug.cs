using UnityEngine;

public class WASDMoveDebug : MonoBehaviour
{
    [Header("移動設定")]
    public float speed = 5f; // 通常移動速度
    public float dashSpeed = 15f; // ダッシュ速度（Shift押下時）
    public float dashDuration = 0.2f; // ダッシュ持続時間

    [Header("クールダウン")]
    public float dashCooldown = 1.0f; // ダッシュクールタイム（秒）

    [Header("デバッグ")]
    public bool useTransformMove = false; // Rigidbody2Dの代わりにTransformで移動（デバッグ用）
    public bool disableOtherMoveScripts = true; // 他の移動スクリプトを自動無効化

    private Rigidbody2D rb;
    private IPlayerAttack playerAttack;
    
    // ダッシュ制御
    private Vector3 dashVelocity = Vector3.zero;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogWarning("WASDMoveDebug: Rigidbody2D が見つかりません。Transform移動に切り替えます。");
            useTransformMove = true;
        }
        else
        {
            Debug.Log($"WASDMoveDebug: 初期化完了 (BodyType: {rb.bodyType}, Constraints: {rb.constraints})");
            
            // Rigidbody2Dの設定を確認・修正
            if (rb.bodyType != RigidbodyType2D.Dynamic)
            {
                Debug.LogWarning("WASDMoveDebug: BodyType を Dynamic に変更しました");
                rb.bodyType = RigidbodyType2D.Dynamic;
            }
            
            // 重力を無効化（2Dトップダウンの場合）
            rb.gravityScale = 0f;
        }
        
        playerAttack = GetComponent<IPlayerAttack>();
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);

        // 他の移動スクリプトを無効化（競合防止）
        if (disableOtherMoveScripts)
        {
            var touchMove = GetComponent<TouchMove2>();
            if (touchMove != null)
            {
                touchMove.enabled = false;
                Debug.Log("WASDMoveDebug: TouchMove2 を無効化しました");
            }
        }
    }

    void Update()
    {
        // クールダウン処理
        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        // ダッシュ入力判定（Shift + 方向キー同時押し）
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            TryDash();
        }

        // 向き更新
        UpdateDirection();
    }

    void FixedUpdate()
    {
        // 通常移動
        float h = Input.GetKey(KeyCode.D) ? 1 : Input.GetKey(KeyCode.A) ? -1 : 0;
        float v = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0;
        
        if (h != 0 || v != 0)
        {
            Vector2 direction = new Vector2(h, v).normalized;
            
            // ダッシュ中の移動
            if (dashTimer > 0f)
            {
                Vector2 dashMove = (Vector2)dashVelocity * Time.fixedDeltaTime;
                if (useTransformMove || rb == null)
                {
                    transform.position += (Vector3)dashMove;
                }
                else
                {
                    rb.MovePosition(rb.position + dashMove);
                }
                dashTimer -= Time.fixedDeltaTime;
                if (dashTimer <= 0f)
                {
                    dashVelocity = Vector3.zero;
                }
                Debug.Log($"WASDDash: move={dashMove}, pos={transform.position}");
                return;
            }
            
            // 通常移動（速度を大幅に増加）
            Vector2 move = direction * speed * Time.fixedDeltaTime;
            
            if (useTransformMove || rb == null)
            {
                // Transform直接移動（より確実）
                transform.position += (Vector3)move;
                Debug.Log($"WASDMove(Transform): h={h}, v={v}, move={move}, pos={transform.position}");
            }
            else
            {
                // Rigidbody2D移動
                Vector2 newPosition = rb.position + move;
                rb.MovePosition(newPosition);
                Debug.Log($"WASDMove(RB): h={h}, v={v}, move={move}, rbPos={rb.position}, newPos={newPosition}");
            }
        }
    }

    void TryDash()
    {
        if (dashCooldownTimer > 0f) return; // クールタイム中

        // 現在の入力方向を取得
        float h = Input.GetKey(KeyCode.D) ? 1 : Input.GetKey(KeyCode.A) ? -1 : 0;
        float v = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0;

        Vector3 dashDir = new Vector3(h, v, 0).normalized;
        if (dashDir.magnitude > 0.1f) // 方向入力がある場合のみダッシュ
        {
            dashVelocity = dashDir * dashSpeed;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;
        }
    }

    void UpdateDirection()
    {
        if (playerAttack == null) return;

        // 左右の入力で向きを更新
        if (Input.GetKey(KeyCode.D))
        {
            playerAttack.isRight = true;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            playerAttack.isRight = false;
        }
    }
}
