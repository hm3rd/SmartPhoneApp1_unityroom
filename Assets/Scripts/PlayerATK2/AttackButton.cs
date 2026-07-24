using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 特定の攻撃を実行するボタン用コンポーネント
/// UIボタンや入力イベントから攻撃を発動する
/// </summary>
public class AttackButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("攻撃設定")]
    [Tooltip("実行する攻撃データ（キャラ固有でない場合のみ使用）")]
    public AttackData attackData;
    
    [Tooltip("攻撃のインデックス（availableAttacks内の位置、-1なら使用しない）")]
    public int attackIndex = -1;
    
    [Tooltip("AttackManagerの参照（未設定の場合は自動検索）")]
    public AttackManager attackManager;
    
    // チャージ用内部状態
    private bool isCharging = false;
    private float currentChargeTime = 0f;
    
    private ChargeAttackData chargingAttack;

    private void Start()
    {
        // AttackManagerが未設定なら自動で探す
        if (attackManager == null)
        {
            attackManager = FindFirstObjectByType<AttackManager>();
            if (attackManager == null)
            {
                Debug.LogError("AttackManagerが見つかりません！");
            }
        }
    }
    
    /// <summary>
    /// 攻撃を実行（UIボタンから呼び出す）
    /// </summary>
    public void OnAttackButtonPressed()
    {
        if (attackManager == null)
        {
            Debug.LogWarning("AttackManagerが設定されていません");
            return;
        }

        // attackIndex が設定されている場合、そこから attackData を取得（キャラ交代で自動更新される）
        AttackData dataToUse = GetSelectedAttack();

        if (dataToUse == null)
        {
            Debug.LogWarning("攻撃データが設定されていません");
            return;
        }
        
        if (dataToUse is ChargeAttackData)
        {
            // チャージはボタン押し/離しで制御するため、OnClickでは何もしない
        }
        else
        {
            attackManager.ExecuteAttack(dataToUse);
        }
    }
    
    private void Update()
    {
        // チャージ中なら時間を加算（最大値で打ち止め）
        if (isCharging && chargingAttack != null)
        {
            currentChargeTime = Mathf.Min(
                currentChargeTime + Time.deltaTime,
                Mathf.Max(0f, chargingAttack.maxChargeTime));
        }
    }
    
    // ===== チャージ攻撃実装 =====
    public void OnPointerDown(PointerEventData eventData)
    {
        ChargeAttackData chargeData = GetSelectedAttack() as ChargeAttackData;
        if (chargeData != null)
        {
            // クールタイム中は開始しない
            if (attackManager != null && !attackManager.IsOnCooldown(chargeData))
            {
                isCharging = true;
                currentChargeTime = 0f;
                chargingAttack = chargeData;
            }
        }
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        if (isCharging)
        {
            ReleaseCharge();
        }
    }
    
    private void ReleaseCharge()
    {
        if (!isCharging || chargingAttack == null || attackManager == null)
        {
            ResetCharge();
            return;
        }

        ChargeAttackData chargeData = chargingAttack;
        isCharging = false;
        attackManager.ExecuteChargedAttack(chargeData, currentChargeTime);
        ResetCharge();
    }

    private void ResetCharge()
    {
        isCharging = false;
        currentChargeTime = 0f;
        chargingAttack = null;
    }

    /// <summary>
    /// index が有効ならキャラクター固有の攻撃、無効なら固定の attackData を返す。
    /// </summary>
    private AttackData GetSelectedAttack()
    {
        if (attackManager != null &&
            attackIndex >= 0 &&
            attackIndex < attackManager.availableAttacks.Count)
        {
            return attackManager.availableAttacks[attackIndex];
        }

        return attackData;
    }
}
