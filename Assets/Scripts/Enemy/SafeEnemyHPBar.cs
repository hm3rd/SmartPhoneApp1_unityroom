using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 敵のHPを安全に表示する独立したSlider管理（簡略版）
/// EnemyHP.csのhpBarは触らず、別のSliderで表示する。
/// 位置追従やオプションは持たず、値の反映のみ行う（追従が必要ならUIを敵の子に配置）。
/// </summary>
public class SafeEnemyHPBar : MonoBehaviour
{
    [Header("参照設定")]
    [Tooltip("監視するEnemyHPコンポーネント")]
    public EnemyHP enemyHP;
    
    [Tooltip("表示用のSlider（World Space Canvas推奨）")]
    public Slider displaySlider;
    
    void Start()
    {
        // 親から自動検索
        if (enemyHP == null)
        {
            enemyHP = GetComponentInParent<EnemyHP>();
            if (enemyHP == null)
            {
                Debug.LogError("SafeEnemyHPBar: EnemyHPが見つかりません");
                enabled = false;
                return;
            }
        }
        
        if (displaySlider == null)
        {
            Debug.LogError("SafeEnemyHPBar: displaySliderが設定されていません");
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
        if (enemyHP == null)
        {
            // 敵が倒されたらこのUIも削除
            Destroy(gameObject);
            return;
        }
        
        // HP値を更新（値の反映のみ）
        UpdateDisplay();
    }
    
    void UpdateDisplay()
    {
        if (enemyHP == null || displaySlider == null) return;
        
        // EnemyHPから現在値と最大値を読み取って、Sliderに反映
        float ratio = (float)enemyHP.currentHP / enemyHP.maxHP;
        displaySlider.value = ratio;
    }
}
