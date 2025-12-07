using UnityEngine;

/// <summary>
/// UIや他挙動に影響を与えにくい安全なWASDデバッグ移動。
/// - Transformのスケールは変更しない
/// - Rigidbody2Dの設定を勝手に変更しない
/// - 他スクリプトを無効化しない
/// - Editor/Development Build のみ動作(任意)
/// </summary>
public class SafeWASDDebug : MonoBehaviour
{
    [Header("基本設定")]
    [Tooltip("WASD/矢印キーでの移動速度")]
    public float speed = 5f;

    [Tooltip("Rigidbody2Dで移動するか(推奨) / falseでTransform移動")]
    public bool usePhysics = true;

    [Tooltip("Editor/Developmentビルドでのみ動作させる")]
    public bool onlyWhenDebugBuild = true;

    [Header("オプション")]
    [Tooltip("左右入力でIPlayerAttack.isRightだけ更新(向きの反転はしない)")]
    public bool affectDirection = true;

    [Tooltip("Z座標を0に固定する(2DでZがずれる問題対策)")]
    public bool clampZToZero = false;

    [Tooltip("Verboseログ出力")]
    public bool verboseLog = false;

    private Rigidbody2D rb;
    private IPlayerAttack playerAttack;
    private Vector2 input;

    void Awake()
    {
        if (onlyWhenDebugBuild)
        {
#if !(UNITY_EDITOR || DEVELOPMENT_BUILD)
            enabled = false; // 本番ビルドでは無効化
            return;
#endif
        }

        if (usePhysics)
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                Debug.LogWarning("SafeWASDDebug: Rigidbody2Dが見つからないためTransform移動に切替えます");
                usePhysics = false;
            }
        }

        playerAttack = GetComponent<IPlayerAttack>();
    }

    void Update()
    {
        // 入力取得 (WASD/矢印キー)
        float h = 0f;
        float v = 0f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) h = 1f;
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) h = -1f;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) v = 1f;
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) v = -1f;

        input = new Vector2(h, v).normalized;

        // 向きだけ更新(オプション)。Transformは反転しない。
        if (affectDirection && playerAttack != null)
        {
            if (h > 0.1f) playerAttack.isRight = true;
            else if (h < -0.1f) playerAttack.isRight = false;
        }
    }

    void FixedUpdate()
    {
        if (input.sqrMagnitude > 0f)
        {
            Vector2 delta = input * speed * Time.fixedDeltaTime;
            if (usePhysics && rb != null)
            {
                rb.MovePosition(rb.position + delta);
            }
            else
            {
                transform.position += (Vector3)delta;
            }
            if (verboseLog) Debug.Log($"SafeWASDDebug move: {delta}, pos:{transform.position}");
        }

        if (clampZToZero)
        {
            var p = transform.position;
            if (Mathf.Abs(p.z) > 0.0001f)
            {
                p.z = 0f;
                transform.position = p;
            }
        }
    }
}
