using UnityEngine;

/// <summary>
/// HomeScene の MENU ボタン配置設定。
/// UICanvas に追加すると Inspector から位置と大きさを変更できる。
/// </summary>
public sealed class CharacterMenuLayout : MonoBehaviour
{
    [Tooltip("ボタンの基準位置。右上=(1,1)、左上=(0,1)、右下=(1,0)")]
    public Vector2 anchor = new Vector2(1f, 1f);

    [Tooltip("基準位置からの移動量")]
    public Vector2 anchoredPosition = new Vector2(-24f, -24f);

    [Tooltip("MENU ボタンの大きさ")]
    public Vector2 size = new Vector2(190f, 72f);
}
