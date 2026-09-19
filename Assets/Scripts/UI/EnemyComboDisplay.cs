using UnityEngine;
using UnityEngine.UI;

/// <summary>指定時間内の連続撃破数を「〇斬」として表示する。</summary>
public class EnemyComboDisplay : MonoBehaviour
{
    [Header("表示先")]
    [Tooltip("表示位置はこのTextのRectTransformで設定します")]
    [SerializeField] private Text comboText;
    [Tooltip("Text未設定時に自動生成する画面上の位置")]
    [SerializeField] private Vector2 autoDisplayPosition = new Vector2(0f, 160f);
    [SerializeField] private Vector2 autoDisplaySize = new Vector2(320f, 100f);
    [Min(8)] [SerializeField] private int autoFontSize = 48;
    [Header("コンボ設定")]
    [Min(0.1f)] [SerializeField] private float comboTimeLimit = 2f;
    [Min(1)] [SerializeField] private int minimumDisplayCombo = 2;
    [Tooltip("{0}がコンボ数に置き換わります")]
    [SerializeField] private string displayFormat = "{0}斬";
    [Min(0f)] [SerializeField] private float displayDuration = 1.5f;
    [Header("演出")]
    [SerializeField] private Color textColor = Color.white;
    [Min(1f)] [SerializeField] private float popScale = 1.25f;
    [Min(0.01f)] [SerializeField] private float popDuration = 0.15f;

    private int comboCount;
    private float lastKillTime = float.NegativeInfinity;
    private float hideTime;
    private Vector3 baseScale = Vector3.one;

    private void Awake()
    {
        if (comboText == null) CreateComboText();
        if (comboText == null) return;
        JapaneseFontProvider.Apply(comboText);
        comboText.color = textColor;
        baseScale = comboText.rectTransform.localScale;
        comboText.gameObject.SetActive(false);
    }

    private void CreateComboText()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;
        GameObject textObject = new GameObject(
            "Combo Text",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Text));
        textObject.transform.SetParent(canvas.transform, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = autoDisplayPosition;
        rect.sizeDelta = autoDisplaySize;
        comboText = textObject.GetComponent<Text>();
        comboText.alignment = TextAnchor.MiddleCenter;
        comboText.fontSize = autoFontSize;
        comboText.fontStyle = FontStyle.Bold;
        comboText.raycastTarget = false;
    }

    private void Update()
    {
        if (comboCount > 0 && Time.time - lastKillTime > comboTimeLimit) comboCount = 0;
        if (comboText == null || !comboText.gameObject.activeSelf) return;
        if (Time.time >= hideTime)
        {
            comboText.gameObject.SetActive(false);
            return;
        }
        float ratio = Mathf.Clamp01((Time.time - lastKillTime) / Mathf.Max(0.01f, popDuration));
        comboText.rectTransform.localScale = Vector3.Lerp(baseScale * popScale, baseScale, ratio);
    }

    public void RegisterKill()
    {
        comboCount = Time.time - lastKillTime <= comboTimeLimit ? comboCount + 1 : 1;
        lastKillTime = Time.time;
        if (comboText == null || comboCount < minimumDisplayCombo) return;
        comboText.text = string.Format(displayFormat, comboCount);
        comboText.color = textColor;
        comboText.rectTransform.localScale = baseScale * popScale;
        comboText.gameObject.SetActive(true);
        hideTime = Time.time + displayDuration;
    }

    public void ResetCombo()
    {
        comboCount = 0;
        lastKillTime = float.NegativeInfinity;
        if (comboText != null) comboText.gameObject.SetActive(false);
    }
}
