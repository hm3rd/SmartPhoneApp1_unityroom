using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(TextMeshProUGUI))]
public class DamagePopup : MonoBehaviour
{
    [Header("表示設定")]
    public float lifetime = 1.0f; // 表示時間
    public float moveSpeed = 1.5f; // 上昇速度
    public float fadeSpeed = 1.0f; // フェード速度

    [Header("アニメーション")]
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 0.5f, 0.3f, 1f);
    public float scaleMultiplier = 1.2f;

    private TextMeshProUGUI textMesh;
    private CanvasGroup canvasGroup;
    private float timer = 0f;
    private Vector3 moveDirection = Vector3.up;

    void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void Initialize(int damage, Vector3 worldPosition, Color? color = null)
    {
        textMesh.text = damage.ToString();
        textMesh.color = color ?? Color.red;
        
        // ワールド座標をスクリーン座標に変換してUIに配置
        transform.position = worldPosition;
        
        canvasGroup.alpha = 1f;
        timer = 0f;
        
        StartCoroutine(AnimatePopup());
    }

    private IEnumerator AnimatePopup()
    {
        Vector3 startPos = transform.position;
        Vector3 initialScale = transform.localScale;

        while (timer < lifetime)
        {
            timer += Time.deltaTime;
            float progress = timer / lifetime;

            // 上昇移動
            transform.position = startPos + moveDirection * (moveSpeed * timer);

            // スケールアニメーション
            float scaleValue = scaleCurve.Evaluate(progress) * scaleMultiplier;
            transform.localScale = initialScale * scaleValue;

            // フェードアウト
            canvasGroup.alpha = 1f - (progress * fadeSpeed);

            yield return null;
        }

        Destroy(gameObject);
    }
}
