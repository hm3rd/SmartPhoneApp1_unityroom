using UnityEngine;
using UnityEngine.UI;

/// <summary>WebGLと端末解像度に対応した画面座標の移動スティック。</summary>
public class MoveUI : MonoBehaviour
{
    [Header("見た目（Prefabは任意）")]
    public GameObject ringPrefab;
    public GameObject knobPrefab;
    [Min(20f)] public float fixedScale = 120f;
    [Min(10f)] public float knobRange = 55f;
    [SerializeField] private Color ringColor = new Color(1f, 1f, 1f, 0.3f);
    [SerializeField] private Color knobColor = new Color(1f, 1f, 1f, 0.65f);
    [Range(0.1f, 0.9f)] [SerializeField] private float activeScreenWidthRatio = 0.5f;
    public Vector2 inputDir;

    private Canvas overlayCanvas;
    private RectTransform ringRect;
    private RectTransform knobRect;
    private Vector2 startScreenPosition;
    private int trackedFingerId = -1;

    private void Awake()
    {
        // 旧ワールド座標版の値（Scale 1～2 / Range 1前後）を自動移行
        if (fixedScale < 20f) fixedScale = 120f;
        if (knobRange < 10f) knobRange = 55f;
        CreateOverlayCanvas();
    }

    private void Update()
    {
        if (Input.touchCount > 0) UpdateTouchInput();
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
        else UpdateMouseInput();
#else
        else HideStick();
#endif
    }

    private void UpdateTouchInput()
    {
        Touch? selected = null;
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            if (trackedFingerId == touch.fingerId ||
                (trackedFingerId < 0 && touch.phase == TouchPhase.Began && IsInActiveArea(touch.position)))
            {
                selected = touch;
                break;
            }
        }
        if (!selected.HasValue) return;

        Touch current = selected.Value;
        if (current.phase == TouchPhase.Began)
        {
            trackedFingerId = current.fingerId;
            ShowStick(current.position);
        }
        else if (current.phase == TouchPhase.Moved || current.phase == TouchPhase.Stationary)
            UpdateStick(current.position);
        else if (current.phase == TouchPhase.Ended || current.phase == TouchPhase.Canceled)
            HideStick();
    }

    private void UpdateMouseInput()
    {
        Vector2 position = Input.mousePosition;
        if (Input.GetMouseButtonDown(0) && IsInActiveArea(position)) ShowStick(position);
        else if (Input.GetMouseButton(0) && ringRect != null) UpdateStick(position);
        else if (Input.GetMouseButtonUp(0)) HideStick();
    }

    private bool IsInActiveArea(Vector2 position) =>
        position.x < Screen.width * activeScreenWidthRatio;

    private void ShowStick(Vector2 position)
    {
        HideStick();
        startScreenPosition = position;
        ringRect = CreatePart(
            "Move Ring",
            ringPrefab != null ? ringPrefab : knobPrefab,
            ringColor,
            fixedScale);
        knobRect = CreatePart("Move Knob", knobPrefab, knobColor, fixedScale * 0.45f);
        ringRect.position = position;
        knobRect.position = position;
    }

    private void UpdateStick(Vector2 position)
    {
        if (ringRect == null || knobRect == null) return;
        Vector2 offset = Vector2.ClampMagnitude(position - startScreenPosition, knobRange);
        knobRect.position = startScreenPosition + offset;
        inputDir = knobRange > 0f ? offset / knobRange : Vector2.zero;
    }

    private void HideStick()
    {
        if (ringRect != null) Destroy(ringRect.gameObject);
        if (knobRect != null) Destroy(knobRect.gameObject);
        ringRect = null;
        knobRect = null;
        trackedFingerId = -1;
        inputDir = Vector2.zero;
    }

    private void CreateOverlayCanvas()
    {
        GameObject canvasObject = new GameObject("Move UI Overlay", typeof(RectTransform),
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        overlayCanvas = canvasObject.GetComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.sortingOrder = 1000;
        canvasObject.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
    }

    private RectTransform CreatePart(string partName, GameObject prefab, Color fallbackColor, float size)
    {
        GameObject part = new GameObject(partName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        part.transform.SetParent(overlayCanvas.transform, false);
        RectTransform rect = part.GetComponent<RectTransform>();
        rect.sizeDelta = Vector2.one * size;
        Image image = part.GetComponent<Image>();
        image.raycastTarget = false;
        SpriteRenderer spriteRenderer = prefab != null ? prefab.GetComponent<SpriteRenderer>() : null;
        Image sourceImage = prefab != null ? prefab.GetComponent<Image>() : null;
        image.sprite = sourceImage != null ? sourceImage.sprite : spriteRenderer != null ? spriteRenderer.sprite : null;
        image.color = sourceImage != null ? sourceImage.color : spriteRenderer != null ? spriteRenderer.color : fallbackColor;
        return rect;
    }

    private void OnDisable() { HideStick(); }
}
