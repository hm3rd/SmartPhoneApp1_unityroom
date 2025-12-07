using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UIリップルの拡大&フェードを行い、終了時に自壊します。
/// リップル用UI(Image推奨)にアタッチしてください。
/// </summary>
public class RippleEffect : MonoBehaviour
{
    [Header("時間(秒)")]
    public float duration = 0.6f;

    [Header("スケール(ローカル)")]
    public float startScale = 0.1f;
    public float endScale = 1.2f;

    [Header("色(アルファでフェード)")]
    public Color startColor = new Color(1f, 1f, 1f, 0.6f);
    public Color endColor = new Color(1f, 1f, 1f, 0f);

    [Header("カーブ(任意)")]
    public AnimationCurve scaleCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public AnimationCurve alphaCurve = AnimationCurve.Linear(0, 1, 1, 0);

    private float _t = 0f;
    private Image _img;           // UI用

    void Awake()
    {
        _img = GetComponent<Image>();
        // 初期化
        var s0 = Mathf.Max(0.0001f, startScale);
        transform.localScale = Vector3.one * s0;
        ApplyColor(startColor);
    }

    void Update()
    {
        if (duration <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        _t += Time.deltaTime / duration;
        float tClamped = Mathf.Clamp01(_t);

        // スケール
        float s = Mathf.Lerp(startScale, endScale, scaleCurve.Evaluate(tClamped));
        transform.localScale = Vector3.one * s;

        // カラー(特にアルファ)
        var col = Color.Lerp(startColor, endColor, 1f - alphaCurve.Evaluate(1f - tClamped));
        ApplyColor(col);

        if (_t >= 1f)
        {
            Destroy(gameObject);
        }
    }

    private void ApplyColor(Color c)
    {
        if (_img != null)
        {
            _img.color = c;
        }
    }
}
