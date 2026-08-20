using System;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// GameScene の開始・クリア時に、文章を一文字ずつズーム表示する。
/// 演出中はゲーム時間を止め、全面パネルでプレイヤー入力を遮断する。
/// </summary>
public sealed class StageAnnouncementController : MonoBehaviour
{
    [Header("表示テキスト")]
    [SerializeField] private string startText = "任務開始";
    [SerializeField] private string clearText = "任務完了";

    [Header("文字の見た目")]
    [Min(20)] [SerializeField] private int fontSize = 144;
    [Min(0f)] [SerializeField] private float characterSpacing = 12f;
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.35f);

    [Header("ズーム演出")]
    [Min(0.01f)] [SerializeField] private float zoomDuration = 0.22f;
    [Min(0f)] [SerializeField] private float characterInterval = 0.08f;
    [Min(0f)] [SerializeField] private float displayHoldDuration = 0.55f;
    [Min(0.01f)] [SerializeField] private float startScale = 2.5f;

    private Canvas overlayCanvas;
    private GameObject blocker;
    private RectTransform characterContainer;
    private Coroutine playingRoutine;
    private float previousTimeScale = 1f;
    private readonly List<BehaviourState> disabledInputBehaviours = new List<BehaviourState>();

    private struct BehaviourState
    {
        public Behaviour behaviour;
        public bool wasEnabled;
    }

    public void PlayStart(Action onComplete)
    {
        Play(startText, onComplete);
    }

    public void PlayClear(Action onComplete)
    {
        Play(clearText, onComplete);
    }

    private void Play(string message, Action onComplete)
    {
        EnsureUI();
        if (playingRoutine != null) StopCoroutine(playingRoutine);
        LockGame();
        playingRoutine = StartCoroutine(PlayRoutine(message, onComplete));
    }

    private IEnumerator PlayRoutine(string message, Action onComplete)
    {
        ClearCharacters();
        blocker.SetActive(true);

        TextElementEnumerator enumerator = StringInfo.GetTextElementEnumerator(message ?? string.Empty);
        while (enumerator.MoveNext())
        {
            Text character = CreateCharacter(enumerator.GetTextElement());
            yield return ZoomCharacter(character.rectTransform);

            if (characterInterval > 0f)
                yield return new WaitForSecondsRealtime(characterInterval);
        }

        if (displayHoldDuration > 0f)
            yield return new WaitForSecondsRealtime(displayHoldDuration);

        blocker.SetActive(false);
        UnlockGame();
        playingRoutine = null;
        onComplete?.Invoke();
    }

    private IEnumerator ZoomCharacter(RectTransform target)
    {
        float elapsed = 0f;
        target.localScale = Vector3.one * startScale;
        Color originalColor = target.GetComponent<Text>().color;

        while (elapsed < zoomDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float normalized = Mathf.Clamp01(elapsed / zoomDuration);
            float eased = 1f - Mathf.Pow(1f - normalized, 3f);
            target.localScale = Vector3.one * Mathf.Lerp(startScale, 1f, eased);
            Color color = originalColor;
            color.a *= normalized;
            target.GetComponent<Text>().color = color;
            yield return null;
        }

        target.localScale = Vector3.one;
        target.GetComponent<Text>().color = originalColor;
    }

    private void LockGame()
    {
        previousTimeScale = Time.timeScale;
        DisableInputBehaviours();
        Time.timeScale = 0f;
    }

    private void UnlockGame()
    {
        Time.timeScale = previousTimeScale;
        foreach (BehaviourState state in disabledInputBehaviours)
        {
            if (state.behaviour != null) state.behaviour.enabled = state.wasEnabled;
        }
        disabledInputBehaviours.Clear();
    }

    private void DisableInputBehaviours()
    {
        disabledInputBehaviours.Clear();
        DisableAll(FindObjectsByType<TouchMove2>(FindObjectsInactive.Include));
        DisableAll(FindObjectsByType<WASDMoveDebug>(FindObjectsInactive.Include));
        DisableAll(FindObjectsByType<SafeWASDDebug>(FindObjectsInactive.Include));
        DisableAll(FindObjectsByType<MoveUI>(FindObjectsInactive.Include));
    }

    private void DisableAll<T>(T[] behaviours) where T : Behaviour
    {
        foreach (T behaviour in behaviours)
        {
            if (behaviour == null) continue;
            TouchMove2 touchMove = behaviour as TouchMove2;
            if (touchMove != null) touchMove.ClearPendingMovement();
            disabledInputBehaviours.Add(new BehaviourState
            {
                behaviour = behaviour,
                wasEnabled = behaviour.enabled
            });
            behaviour.enabled = false;
        }
    }

    private void EnsureUI()
    {
        if (overlayCanvas != null) return;

        GameObject canvasObject = new GameObject(
            "StageAnnouncementCanvas",
            typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        overlayCanvas = canvasObject.GetComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.sortingOrder = 1000;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        blocker = new GameObject("InputBlocker", typeof(RectTransform), typeof(Image));
        blocker.transform.SetParent(canvasObject.transform, false);
        RectTransform blockerRect = blocker.GetComponent<RectTransform>();
        blockerRect.anchorMin = Vector2.zero;
        blockerRect.anchorMax = Vector2.one;
        blockerRect.offsetMin = blockerRect.offsetMax = Vector2.zero;
        Image blockerImage = blocker.GetComponent<Image>();
        blockerImage.color = backgroundColor;
        blockerImage.raycastTarget = true;

        GameObject container = new GameObject(
            "Characters", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        container.transform.SetParent(blocker.transform, false);
        characterContainer = container.GetComponent<RectTransform>();
        characterContainer.anchorMin = new Vector2(0.05f, 0.38f);
        characterContainer.anchorMax = new Vector2(0.95f, 0.62f);
        characterContainer.offsetMin = characterContainer.offsetMax = Vector2.zero;
        HorizontalLayoutGroup layout = container.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = characterSpacing;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = true;

        blocker.SetActive(false);
    }

    private Text CreateCharacter(string value)
    {
        GameObject characterObject = new GameObject(
            "Character_" + value, typeof(RectTransform), typeof(LayoutElement), typeof(Text));
        characterObject.transform.SetParent(characterContainer, false);
        LayoutElement element = characterObject.GetComponent<LayoutElement>();
        element.preferredWidth = fontSize * 1.15f;

        Text text = characterObject.GetComponent<Text>();
        JapaneseFontProvider.Apply(text);
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = textColor;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = Mathf.Max(20, fontSize / 2);
        text.resizeTextMaxSize = fontSize;
        text.raycastTarget = false;
        return text;
    }

    private void ClearCharacters()
    {
        if (characterContainer == null) return;
        for (int i = characterContainer.childCount - 1; i >= 0; i--)
            Destroy(characterContainer.GetChild(i).gameObject);
    }

    private void OnDisable()
    {
        if (playingRoutine == null) return;
        UnlockGame();
        playingRoutine = null;
    }
}
