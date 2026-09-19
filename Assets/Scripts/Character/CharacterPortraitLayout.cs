using UnityEngine;

/// <summary>
/// CharacterScene のキャラクター画像レイアウト設定。
/// CharacterCanvasに追加するとInspectorから実行前に調整できる。
/// </summary>
public sealed class CharacterPortraitLayout : MonoBehaviour
{
    [Header("画像を表示する範囲（画面に対する0～1の割合）")]
    [Tooltip("表示範囲の左下。X=横位置、Y=縦位置")]
    public Vector2 viewportAnchorMin = new Vector2(0.03f, 0.16f);

    [Tooltip("表示範囲の右上。範囲外の画像はマスクされます")]
    public Vector2 viewportAnchorMax = new Vector2(0.55f, 0.84f);

    [Header("キャラクター画像")]
    [Tooltip("表示範囲の上中央を基準にした画像位置")]
    public Vector2 imagePosition = Vector2.zero;

    [Tooltip("キャラクター画像の幅と高さ")]
    public Vector2 imageSize = new Vector2(680f, 980f);

    [Header("タッチ反応：共通")]
    [Tooltip("1回のアニメーションの基本時間")]
    [Min(0.01f)] public float animationDuration = 0.5f;

    [Tooltip("連続タッチを受け付けない時間")]
    [Min(0f)] public float touchCooldown = 0.2f;

    [Header("タッチ反応：ジャンプ")]
    [Tooltip("Character_behaviorのジャンプをUI座標へ換算した高さ")]
    [Min(0f)] public float uiJumpHeight = 55f;

    [Tooltip("ジャンプの繰り返し回数")]
    [Min(1)] public int jumpRepeatCount = 1;

    [Header("タッチ反応：横揺れ")]
    [Tooltip("左右に揺れる角度")]
    public float swingAngle = 30f;

    [Tooltip("横揺れの往復回数")]
    [Min(1)] public int swingCount = 2;

    [Header("タッチ反応：拡大")]
    [Tooltip("タッチ時の最大拡大倍率")]
    [Min(0.01f)] public float zoomScale = 1.3f;

    [Tooltip("拡大縮小の繰り返し回数")]
    [Min(1)] public int zoomRepeatCount = 1;

    private void OnValidate()
    {
        viewportAnchorMin.x = Mathf.Clamp01(viewportAnchorMin.x);
        viewportAnchorMin.y = Mathf.Clamp01(viewportAnchorMin.y);
        viewportAnchorMax.x = Mathf.Clamp(viewportAnchorMax.x, viewportAnchorMin.x, 1f);
        viewportAnchorMax.y = Mathf.Clamp(viewportAnchorMax.y, viewportAnchorMin.y, 1f);
        imageSize.x = Mathf.Max(1f, imageSize.x);
        imageSize.y = Mathf.Max(1f, imageSize.y);
        animationDuration = Mathf.Max(0.01f, animationDuration);
        touchCooldown = Mathf.Max(0f, touchCooldown);
        uiJumpHeight = Mathf.Max(0f, uiJumpHeight);
        jumpRepeatCount = Mathf.Max(1, jumpRepeatCount);
        swingCount = Mathf.Max(1, swingCount);
        zoomScale = Mathf.Max(0.01f, zoomScale);
        zoomRepeatCount = Mathf.Max(1, zoomRepeatCount);
    }
}
