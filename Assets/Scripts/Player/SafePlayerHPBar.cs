using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// プレイヤーのHPを安全に表示する独立したSlider管理（簡略版）
/// PlayerHP.csのhpBarは触らず、別のSliderで表示する。
/// 位置追従やオプションは持たず、値の反映のみ行う。
/// </summary>
public class SafePlayerHPBar : MonoBehaviour
{
    [Header("参照設定")]
    [Tooltip("監視するPlayerHPコンポーネント")]
    public PlayerHP playerHP;
    
    [Tooltip("表示用のSlider（Screen Space OverlayまたはWorld Space独立Canvas推奨）")]
    public Slider displaySlider;
    
    void Start()
    {
        // 自動検索
        if (playerHP == null)
        {
            playerHP = FindFirstObjectByType<PlayerHP>();
            if (playerHP == null)
            {
                Debug.LogError("SafePlayerHPBar: PlayerHPが見つかりません");
                enabled = false;
                return;
            }
        }
        
        if (displaySlider == null)
        {
            Debug.LogError("SafePlayerHPBar: displaySliderが設定されていません");
            enabled = false;
            return;
        }
        
        // Sliderの初期化
        displaySlider.minValue = 0f;
        displaySlider.maxValue = 1f;
        displaySlider.wholeNumbers = false;
        
        UpdateDisplay();
    }
    
    void LateUpdate()
    {
        // HP値を更新（値の反映のみ）
        UpdateDisplay();
    }
    
    void UpdateDisplay()
    {
        if (playerHP == null || displaySlider == null) return;
        
        // PlayerHPから現在値と最大値を読み取って、Sliderに反映
        float ratio = (float)playerHP.currentHP / playerHP.maxHP;
        displaySlider.value = ratio;
    }
}
