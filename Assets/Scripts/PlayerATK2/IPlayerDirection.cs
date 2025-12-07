using UnityEngine;

/// <summary>
/// プレイヤーの向き情報を提供するインターフェース
/// </summary>
public interface IPlayerDirection
{
    /// <summary>
    /// プレイヤーが右を向いているか（true=右、false=左）
    /// </summary>
    bool IsFacingRight { get; }
}
