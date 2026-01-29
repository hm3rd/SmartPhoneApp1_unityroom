using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

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
    
    void Start()
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
        AttackData dataToUse = attackData;
        if (attackIndex >= 0 && attackIndex < attackManager.availableAttacks.Count)
        {
            dataToUse = attackManager.availableAttacks[attackIndex];
        }

        if (dataToUse == null)
        {
            Debug.LogWarning("攻撃データが設定されていません");
            return;
        }
        
        // データ型に応じて処理分岐
        if (dataToUse is MultiHitAttackData multi)
        {
            StartCoroutine(ExecuteMultiHit(multi));
        }
        else if (dataToUse is ChargeAttackData)
        {
            // チャージはボタン押し/離しで制御するため、OnClickでは何もしない
        }
        else
        {
            // 通常攻撃はAttackManagerに委譲
            attackManager.ExecuteAttack(dataToUse);
        }
    }
    
    void Update()
    {
        // チャージ中なら時間を加算（最大値で打ち止め）
        if (isCharging && attackData is ChargeAttackData chargeData)
        {
            currentChargeTime = Mathf.Min(currentChargeTime + Time.deltaTime, chargeData.maxChargeTime);
        }
    }
    
    // ===== 多段攻撃実装 =====
    private IEnumerator ExecuteMultiHit(MultiHitAttackData multi)
    {
        // クールタイム中は実行しない
        if (attackManager.IsOnCooldown(multi)) yield break;
        
        bool isFacingRight = GetFacingRight();
        
        for (int i = 0; i < multi.hitCount; i++)
        {
            Vector3 spawnPosition = attackManager.playerTransform.position;
            if (multi.followPlayerDirection && multi.spawnDistance > 0f)
            {
                spawnPosition += (isFacingRight ? Vector3.right : Vector3.left) * multi.spawnDistance;
            }
            
            if (multi.hitBoxPrefab != null)
            {
                GameObject obj = Instantiate(multi.hitBoxPrefab, spawnPosition, Quaternion.identity);
                Vector3 scale = Vector3.one * multi.scale;
                if (multi.flipOnDirection && !isFacingRight) scale.x = -Mathf.Abs(scale.x);
                obj.transform.localScale = scale;
                var hb = obj.GetComponent<HitBox>();
                if (hb != null) hb.SetDamage(multi.damage);
                Destroy(obj, multi.duration);
            }
            
            if (i < multi.hitCount - 1)
            {
                yield return new WaitForSeconds(multi.hitInterval);
            }
        }
        
        // 最後にクールタイムのみ開始
        attackManager.StartCooldown(multi);
    }
    
    // ===== チャージ攻撃実装 =====
    public void OnPointerDown(PointerEventData eventData)
    {
        if (attackData is ChargeAttackData chargeData)
        {
            // クールタイム中は開始しない
            if (attackManager != null && !attackManager.IsOnCooldown(chargeData))
            {
                isCharging = true;
                currentChargeTime = 0f;
            }
        }
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        if (attackData is ChargeAttackData)
        {
            ReleaseCharge();
        }
    }
    
    private void ReleaseCharge()
    {
        if (!(attackData is ChargeAttackData chargeData) || attackManager == null) { isCharging = false; return; }
        if (!isCharging) return;
        isCharging = false;
        
        float ratio = Mathf.Clamp01(currentChargeTime / chargeData.maxChargeTime);
        int finalDamage = Mathf.RoundToInt(Mathf.Lerp(chargeData.minDamage, chargeData.maxDamage, ratio));
        float finalScale = Mathf.Lerp(chargeData.minScale, chargeData.maxScale, ratio);
        
        bool isFacingRight = GetFacingRight();
        Vector3 spawnPosition = attackManager.playerTransform.position;
        if (chargeData.followPlayerDirection && chargeData.spawnDistance > 0f)
        {
            spawnPosition += (isFacingRight ? Vector3.right : Vector3.left) * chargeData.spawnDistance;
        }
        
        if (chargeData.hitBoxPrefab != null)
        {
            GameObject obj = Instantiate(chargeData.hitBoxPrefab, spawnPosition, Quaternion.identity);
            Vector3 scale = Vector3.one * finalScale;
            if (chargeData.flipOnDirection && !isFacingRight) scale.x = -Mathf.Abs(scale.x);
            obj.transform.localScale = scale;
            var hb = obj.GetComponent<HitBox>();
            if (hb != null) hb.SetDamage(finalDamage);
            Destroy(obj, chargeData.duration);
        }
        
        attackManager.StartCooldown(chargeData);
        currentChargeTime = 0f;
    }
    
    // ===== 向き取得ユーティリティ =====
    private bool GetFacingRight()
    {
        var dir = attackManager.GetComponent<IPlayerDirection>();
        if (dir != null) return dir.IsFacingRight;
        var atk = attackManager.GetComponent<IPlayerAttack>();
        return atk != null ? atk.isRight : true;
    }
}
