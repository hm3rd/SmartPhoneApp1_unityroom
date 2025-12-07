using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 攻撃のクールタイムをUI上に表示する
/// </summary>
public class AttackCooldownUI : MonoBehaviour
{
    [Header("UI設定")]
    [Tooltip("クールタイム表示用のImage（fillAmountで進行率を表示）")]
    public Image cooldownImage;
    
    [Tooltip("クールタイム中に表示するオーバーレイ（任意）")]
    public GameObject cooldownOverlay;
    
    [Tooltip("オーバーレイのアルファで進捗を表現する（ゲージ不要）")]
    public bool overlayShowsProgress = true;
    
    [Tooltip("進捗表示方法")]
    public enum ProgressMode { Alpha, FillAmount, RadialRing }
    public ProgressMode progressMode = ProgressMode.FillAmount;
    
    [Range(0f, 1f)] public float overlayMinAlpha = 0f;
    [Range(0f, 1f)] public float overlayMaxAlpha = 0.7f;
    
    [Header("攻撃設定")]
    [Tooltip("表示する攻撃データ")]
    public AttackData attackData;
    
    [Tooltip("AttackManagerの参照")]
    public AttackManager attackManager;
    
    void Start()
    {
        if (attackManager == null)
        {
            attackManager = FindFirstObjectByType<AttackManager>();
        }
        
        // 初期状態でオーバーレイを非表示にする
        if (cooldownOverlay != null)
        {
            cooldownOverlay.SetActive(false);
            ApplyOverlayAlpha(overlayMaxAlpha); // 初期値（非表示だがαを最大に揃える）
        }
    }
    
    void Update()
    {
        if (attackManager == null || attackData == null)
        {
            return;
        }
        
        // クールタイムの進行率を取得（0.0～1.0）
        float progress = attackManager.GetCooldownProgress(attackData);
        
        // UI更新
        if (cooldownImage != null)
        {
            if (progressMode == ProgressMode.RadialRing)
            {
                // Radial360用: fillAmount=1→0でリングが消える
                cooldownImage.fillAmount = 1f - progress;
            }
            else
            {
                cooldownImage.fillAmount = 1f - progress; // 通常ゲージ
            }
        }
        
        // オーバーレイの表示/非表示 + 進捗表現
        if (cooldownOverlay != null)
        {
            bool isOnCooldown = attackManager.IsOnCooldown(attackData);
            cooldownOverlay.SetActive(isOnCooldown);
            
            if (isOnCooldown && overlayShowsProgress)
            {
                if (progressMode == ProgressMode.FillAmount)
                {
                    // FillAmount方式: 上から削れていく（1→0）
                    ApplyOverlayFillAmount(1f - progress);
                }
                else if (progressMode == ProgressMode.RadialRing)
                {
                    // RadialRing: fillAmountでリング状に表示
                    ApplyOverlayFillAmount(1f - progress);
                }
                else
                {
                    // Alpha方式: progress: 0→1 で α: max→min に遷移
                    float alpha = Mathf.Lerp(overlayMaxAlpha, overlayMinAlpha, Mathf.Clamp01(progress));
                    ApplyOverlayAlpha(alpha);
                }
            }
        }
    }

    private void ApplyOverlayFillAmount(float fillAmount)
    {
        if (cooldownOverlay == null) return;
        var img = cooldownOverlay.GetComponent<Image>();
        if (img != null)
        {
            img.fillAmount = fillAmount;
        }
    }

    private void ApplyOverlayAlpha(float alpha)
    {
        if (cooldownOverlay == null) return;
        var cg = cooldownOverlay.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = alpha;
            return;
        }
        var img = cooldownOverlay.GetComponent<Image>();
        if (img != null)
        {
            var c = img.color;
            c.a = alpha;
            img.color = c;
        }
    }
}
