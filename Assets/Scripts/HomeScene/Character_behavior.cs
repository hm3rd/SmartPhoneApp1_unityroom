using UnityEngine;
using UnityEngine.UI; // Text 用（TextMeshProを使う場合は適宜差し替え）

public class Character_behavior : MonoBehaviour
{
    private const string HomeCharacterIdKey = "SelectedCharacterId_0";

    [Header("ホーム表示キャラクター")]
    [Tooltip("ホーム画面に表示するSpriteRenderer。未設定なら同じオブジェクトから取得")]
    [SerializeField] private SpriteRenderer homeCharacterRenderer;

    [Header("おさわり反応設定")]
    [Tooltip("タッチ時にランダム表示する台詞候補")]
    public string[] touchPhrases =
    {
        "やめてってば！",
        "くすぐったいよ",
        "そこはダメ！",
        "ふーん…もっと？",
        "びっくりした！"
    };

    [Tooltip("台詞を表示する Text (任意)")]
    public Text phraseText; // 未設定なら Debug.Log で出力

    [Header("セリフ表示位置")]
    [Tooltip("テキストの土台に使用する吹き出し画像")]
    [SerializeField] private Sprite phraseBubbleSprite;

    [Tooltip("吹き出し内のテキスト位置。Yをマイナスにすると下へ移動します")]
    [SerializeField] private Vector2 phraseTextPositionOffset = new Vector2(0f, -15f);

    private static readonly Vector2 PhraseBubblePadding = new Vector2(50f, 30f);
    private const float PhraseBubbleMinWidth = 180f;
    private const float PhraseBubbleMaxWidth = 520f;
    private const float PhraseBubbleMinHeight = 80f;
    private Image phraseBubbleImage;
    private int displayedCharacterId = -1;

    [Header("アニメーション設定")]
    [Tooltip("アニメーションの継続秒数")]
    public float animationDuration = 0.5f;
    
    [Header("ジャンプ演出")]
    [Tooltip("ジャンプの高さ")]
    public float jumpHeight = 0.5f;
    [Tooltip("ジャンプの繰り返し回数")]
    public int jumpRepeatCount = 1;
    
    [Header("横揺れ演出")]
    [Tooltip("揺れの角度（度数法）")]
    public float swingAngle = 30f;
    [Tooltip("横揺れの往復回数")]
    public int swingCount = 2;
    
    [Header("クローズアップ演出")]
    [Tooltip("拡大倍率")]
    public float zoomScale = 1.3f;
    [Tooltip("拡大縮小の繰り返し回数")]
    public int zoomRepeatCount = 1;

    [Header("連続タッチ制御")]
    [Tooltip("連続タッチの間隔 (秒) これ未満だと無視")]
    public float touchCooldown = 0.2f;

    [Header("ボイス (任意)")]
    public AudioClip[] voiceClips; // セリフと同数でなくてもOK
    public AudioSource audioSource; // 未設定なら自動取得/追加

    private Vector3 _originalPos;
    private Vector3 _originalScale;
    private Quaternion _originalRotation;
    private bool _isReacting = false;
    private float _lastTouchTime = -10f;
    private int _lastPhraseIndex = -1;

    private enum AnimationType
    {
        Jump,           // ジャンプ
        HorizontalShake, // 横揺れ
        Zoom            // クローズアップ
    }

    void Awake()
    {
        ApplySelectedHomeCharacter();
        EnsurePhraseBubble();

        _originalPos = transform.localPosition;
        _originalScale = transform.localScale;
        _originalRotation = transform.localRotation;
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }
    }

    /// <summary>
    /// StagePreparePanelの一番左（スロット0）で選択したキャラクターをホームへ反映。
    /// </summary>
    private void ApplySelectedHomeCharacter()
    {
        if (homeCharacterRenderer == null)
        {
            homeCharacterRenderer = GetComponent<SpriteRenderer>();
        }

        if (homeCharacterRenderer == null ||
            !PlayerPrefs.HasKey(HomeCharacterIdKey))
        {
            return;
        }

        int characterId = PlayerPrefs.GetInt(HomeCharacterIdKey, -1);
        displayedCharacterId = characterId;
        CharacterDatabase database = CharacterDatabase.GetOrCreate();
        CharacterData selectedCharacter = database.GetCharacterById(characterId);
        if (selectedCharacter == null || selectedCharacter.characterSprite == null)
        {
            Debug.LogWarning($"ホーム表示用キャラクターID {characterId} が見つかりません。", this);
            return;
        }

        homeCharacterRenderer.sprite = selectedCharacter.characterSprite;
        gameObject.name = selectedCharacter.characterName;

        if (selectedCharacter.homeTouchPhrases != null &&
            selectedCharacter.homeTouchPhrases.Length > 0)
        {
            touchPhrases = selectedCharacter.homeTouchPhrases;
        }
    }

    private void RefreshSelectedHomeCharacterIfChanged()
    {
        int selectedId = PlayerPrefs.GetInt(HomeCharacterIdKey, -1);
        if (selectedId != displayedCharacterId)
        {
            ApplySelectedHomeCharacter();
        }
    }

    /// <summary>
    /// PhraseTextの背面に吹き出しImageを作り、Textをその子として前面表示する。
    /// </summary>
    private void EnsurePhraseBubble()
    {
        if (phraseText == null) return;

        RectTransform textRect = phraseText.rectTransform;
        if (phraseBubbleImage == null)
        {
            Transform originalParent = textRect.parent;
            int originalSiblingIndex = textRect.GetSiblingIndex();

            GameObject bubbleObject = new GameObject(
                "PhraseBubble",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            bubbleObject.layer = phraseText.gameObject.layer;
            bubbleObject.transform.SetParent(originalParent, false);
            bubbleObject.transform.SetSiblingIndex(originalSiblingIndex);

            RectTransform bubbleRect = bubbleObject.GetComponent<RectTransform>();
            bubbleRect.anchorMin = textRect.anchorMin;
            bubbleRect.anchorMax = textRect.anchorMax;
            bubbleRect.pivot = textRect.pivot;
            bubbleRect.anchoredPosition = textRect.anchoredPosition;
            bubbleRect.sizeDelta = textRect.sizeDelta + PhraseBubblePadding;

            phraseBubbleImage = bubbleObject.GetComponent<Image>();

            textRect.SetParent(bubbleRect, false);
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = -PhraseBubblePadding;
        }

        phraseBubbleImage.sprite = phraseBubbleSprite;
        phraseBubbleImage.color = Color.white;
        phraseBubbleImage.type = phraseBubbleSprite != null
            ? Image.Type.Sliced
            : Image.Type.Simple;
        phraseBubbleImage.raycastTarget = false;

        phraseText.color = Color.black;
        phraseText.raycastTarget = false;
        phraseText.horizontalOverflow = HorizontalWrapMode.Wrap;
        phraseText.verticalOverflow = VerticalWrapMode.Overflow;
        phraseText.transform.SetAsLastSibling();
        phraseBubbleImage.gameObject.SetActive(!string.IsNullOrEmpty(phraseText.text));

        if (!string.IsNullOrEmpty(phraseText.text))
        {
            ResizePhraseBubble();
        }
    }

    /// <summary>
    /// セリフの推奨サイズを測り、吹き出しを文字量に合わせて伸縮する。
    /// 最大幅を超えた文章は折り返し、高さを広げる。
    /// </summary>
    private void ResizePhraseBubble()
    {
        if (phraseText == null || phraseBubbleImage == null) return;

        RectTransform bubbleRect = phraseBubbleImage.rectTransform;
        RectTransform textRect = phraseText.rectTransform;

        phraseBubbleImage.gameObject.SetActive(true);

        float contentMaxWidth = Mathf.Max(
            PhraseBubbleMinWidth,
            PhraseBubbleMaxWidth - PhraseBubblePadding.x);
        float preferredWidth = phraseText.preferredWidth;
        float bubbleWidth = Mathf.Clamp(
            preferredWidth + PhraseBubblePadding.x,
            PhraseBubbleMinWidth,
            PhraseBubbleMaxWidth);

        // 幅を先に確定させてから、折り返し後の必要な高さを取得する
        bubbleRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, bubbleWidth);
        textRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            Mathf.Min(preferredWidth, contentMaxWidth));
        Canvas.ForceUpdateCanvases();

        float bubbleHeight = Mathf.Max(
            PhraseBubbleMinHeight,
            phraseText.preferredHeight + PhraseBubblePadding.y);
        bubbleRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, bubbleHeight);

        // Textは吹き出しの内側いっぱいに配置する
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.anchoredPosition = phraseTextPositionOffset;
        textRect.sizeDelta = -PhraseBubblePadding;
    }

    void Update()
    {
        RefreshSelectedHomeCharacterIfChanged();

        // スマホタッチ
        if (Input.touchCount > 0)
        {
            foreach (var t in Input.touches)
            {
                if (t.phase == TouchPhase.Began)
                {
                    TryTouchAtScreenPos(t.position);
                }
            }
        }

        // エディタ / PC 用マウスクリック
        if (Input.GetMouseButtonDown(0))
        {
            TryTouchAtScreenPos(Input.mousePosition);
        }
    }

    private void TryTouchAtScreenPos(Vector2 screenPos)
    {
        if (Time.time - _lastTouchTime < touchCooldown) return; // 連打制限

        // 当たり判定: 2Dコライダーがある前提。無い場合は SpriteRenderer の Bounds で簡易判定
        bool hit = false;
        var worldPoint = Camera.main.ScreenToWorldPoint(screenPos);
        var hit2d = Physics2D.OverlapPoint(worldPoint);
        if (hit2d != null && hit2d.transform == transform)
        {
            hit = true;
        }
        else
        {
            // フォールバック: Sprite の Bounds で判定
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                if (sr.bounds.Contains(worldPoint)) hit = true;
            }
        }

        if (!hit) return;

        _lastTouchTime = Time.time;
        ReactToTouch();
    }

    private void ReactToTouch()
    {
        if (!_isReacting)
        {
            // ランダムで3種類のアニメーションから選択
            AnimationType randomAnim = (AnimationType)Random.Range(0, 3);
            StartCoroutine(PlayAnimation(randomAnim));
        }
        ShowRandomPhrase();
        PlayRandomVoice();
    }

    private System.Collections.IEnumerator PlayAnimation(AnimationType animType)
    {
        _isReacting = true;

        switch (animType)
        {
            case AnimationType.Jump:
                yield return JumpAnimation();
                break;
            case AnimationType.HorizontalShake:
                yield return HorizontalShakeAnimation();
                break;
            case AnimationType.Zoom:
                yield return ZoomAnimation();
                break;
        }

        // 終了時に位置とスケールと回転を戻す
        transform.localPosition = _originalPos;
        transform.localScale = _originalScale;
        transform.localRotation = _originalRotation;
        _isReacting = false;
    }

    // ジャンプアニメーション
    private System.Collections.IEnumerator JumpAnimation()
    {
        float elapsed = 0f;
        float totalDuration = animationDuration * jumpRepeatCount;
        Vector3 startPos = _originalPos;
        Vector3 peakPos = _originalPos + Vector3.up * jumpHeight;

        while (elapsed < totalDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / totalDuration;
            
            // 放物線的な動き（Sin カーブ）を繰り返し回数分
            float jumpProgress = Mathf.Sin(t * Mathf.PI * jumpRepeatCount);
            transform.localPosition = Vector3.Lerp(startPos, peakPos, jumpProgress);
            
            yield return null;
        }
    }

    // 横揺れアニメーション（弧を描く回転）
    private System.Collections.IEnumerator HorizontalShakeAnimation()
    {
        float elapsed = 0f;
        float totalDuration = animationDuration * swingCount;
        
        while (elapsed < totalDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / totalDuration;
            
            // Sin 波で左右に揺れる（往復回数を考慮）
            float angle = Mathf.Sin(t * Mathf.PI * 2f * swingCount) * swingAngle;
            
            // Z軸周りに回転（2Dの場合）
            transform.localRotation = _originalRotation * Quaternion.Euler(0, 0, angle);
            
            yield return null;
        }
    }

    // クローズアップアニメーション
    private System.Collections.IEnumerator ZoomAnimation()
    {
        float elapsed = 0f;
        float totalDuration = animationDuration * zoomRepeatCount;
        Vector3 targetScale = _originalScale * zoomScale;

        while (elapsed < totalDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / totalDuration;
            
            // 前半で拡大、後半で縮小を繰り返し回数分
            float zoomProgress = Mathf.Sin(t * Mathf.PI * zoomRepeatCount);
            transform.localScale = Vector3.Lerp(_originalScale, targetScale, zoomProgress);
            
            yield return null;
        }
    }

    private void ShowRandomPhrase()
    {
        if (touchPhrases == null || touchPhrases.Length == 0)
        {
            Debug.Log("タッチ反応: セリフ未設定");
            return;
        }
        int idx = Random.Range(0, touchPhrases.Length);
        // 前回と同じなら別を再抽選（回数制限）
        if (idx == _lastPhraseIndex && touchPhrases.Length > 1)
        {
            idx = (idx + 1) % touchPhrases.Length;
        }
        _lastPhraseIndex = idx;
        string phrase = touchPhrases[idx];
        if (phraseText != null)
        {
            phraseText.text = phrase;
            if (phraseBubbleImage != null)
            {
                ResizePhraseBubble();
            }
        }
        else
        {
            Debug.Log($"キャラ発言: {phrase}");
        }
    }

    private void PlayRandomVoice()
    {
        if (audioSource == null || voiceClips == null || voiceClips.Length == 0) return;
        var clip = voiceClips[Random.Range(0, voiceClips.Length)];
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
