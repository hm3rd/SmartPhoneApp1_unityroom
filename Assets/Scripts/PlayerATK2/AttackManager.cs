using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 攻撃システムのコアマネージャー
/// 全ての攻撃を統一的に管理し、クールタイムを制御する
/// </summary>
public class AttackManager : MonoBehaviour
{
    [Header("プレイヤー設定")]
    [Tooltip("プレイヤーのTransform（攻撃の生成位置）")]
    public Transform playerTransform;
    
    [Header("登録された攻撃")]
    [Tooltip("使用可能な攻撃のリスト")]
    public List<AttackData> availableAttacks = new List<AttackData>();
    
    // クールタイム管理（攻撃データごとに残り時間を記録）
    private Dictionary<AttackData, float> cooldownTimers = new Dictionary<AttackData, float>();

    // 多段攻撃は最後の一撃までクールタイムが始まらないため、実行中を別に管理する
    private readonly HashSet<AttackData> attacksInProgress = new HashSet<AttackData>();
    
    // プレイヤーの向き情報
    private IPlayerAttack playerAttack;
    
    private void Awake()
    {
        if (playerTransform == null)
        {
            playerTransform = transform;
        }
        
        // 既存の移動スクリプトが公開する向き情報を利用
        playerAttack = GetComponent<IPlayerAttack>();
        
        // 全ての攻撃のクールタイマーを初期化
        foreach (var attack in availableAttacks)
        {
            if (attack != null)
            {
                cooldownTimers[attack] = 0f;
            }
        }
    }

    /// <summary>
    /// 使用可能な攻撃リストを差し替え、クールタイムを再初期化する
    /// キャラクター交代時に呼び出す
    /// </summary>
    public void SetAvailableAttacks(List<AttackData> newAttacks)
    {
        // キャラクター交代前の多段攻撃を残さない
        StopAllCoroutines();
        attacksInProgress.Clear();

        availableAttacks = newAttacks ?? new List<AttackData>();
        cooldownTimers.Clear();

        foreach (var attack in availableAttacks)
        {
            if (attack != null)
            {
                cooldownTimers[attack] = 0f;
            }
        }
    }
    
    private void Update()
    {
        // クールタイムを減らす
        var keys = new List<AttackData>(cooldownTimers.Keys);
        foreach (var attack in keys)
        {
            if (cooldownTimers[attack] > 0f)
            {
                cooldownTimers[attack] = Mathf.Max(0f, cooldownTimers[attack] - Time.deltaTime);
            }
        }
    }
    
    /// <summary>
    /// 指定した攻撃を実行
    /// </summary>
    /// <param name="attackIndex">攻撃のインデックス（availableAttacksリストの番号）</param>
    public bool ExecuteAttack(int attackIndex)
    {
        if (attackIndex < 0 || attackIndex >= availableAttacks.Count)
        {
            Debug.LogWarning($"無効な攻撃インデックス: {attackIndex}");
            return false;
        }
        
        return ExecuteAttack(availableAttacks[attackIndex]);
    }
    
    /// <summary>
    /// 指定した攻撃データで攻撃を実行
    /// </summary>
    public bool ExecuteAttack(AttackData attackData)
    {
        if (attackData == null)
        {
            Debug.LogWarning("攻撃データがnullです");
            return false;
        }
        
        if (IsOnCooldown(attackData) || attacksInProgress.Contains(attackData))
        {
            Debug.Log($"{attackData.attackName} はクールタイム中です");
            return false;
        }

        if (attackData.attackType == AttackData.AttackType.Charge)
        {
            // チャージ攻撃はボタンを離した時に ExecuteChargedAttack から実行する
            return false;
        }

        if (attackData.attackType == AttackData.AttackType.MultiHit)
        {
            StartCoroutine(ExecuteMultiHit(attackData));
        }
        else
        {
            SpawnAttack(attackData, attackData.damage, attackData.scale);
            StartCooldown(attackData);
        }

        return true;
    }

    /// <summary>
    /// チャージ時間から威力を計算して攻撃する。
    /// </summary>
    public bool ExecuteChargedAttack(AttackData attackData, float chargeTime)
    {
        if (attackData == null || IsOnCooldown(attackData) || attacksInProgress.Contains(attackData))
        {
            return false;
        }

        float maxTime = Mathf.Max(0.01f, attackData.maxChargeTime);
        float ratio = Mathf.Clamp01(chargeTime / maxTime);
        int damage = Mathf.RoundToInt(Mathf.Lerp(attackData.minDamage, attackData.maxDamage, ratio));
        float scale = attackData.scale *
            Mathf.Lerp(attackData.minScale, attackData.maxScale, ratio);

        SpawnAttack(attackData, damage, scale);
        StartCooldown(attackData);
        return true;
    }

    private IEnumerator ExecuteMultiHit(AttackData attackData)
    {
        attacksInProgress.Add(attackData);
        bool initialFacingRight = IsFacingRight();

        int hitCount = Mathf.Max(1, attackData.hitCount);
        for (int i = 0; i < hitCount; i++)
        {
            bool? fixedDirection = attackData.updateDirectionEachHit
                ? null
                : initialFacingRight;
            SpawnAttack(attackData, attackData.damage, attackData.scale, fixedDirection);
            if (i < hitCount - 1)
            {
                yield return new WaitForSeconds(Mathf.Max(0f, attackData.hitInterval));
            }
        }

        attacksInProgress.Remove(attackData);
        StartCooldown(attackData);
    }
    
    /// <summary>
    /// 攻撃がクールタイム中か確認
    /// </summary>
    public bool IsOnCooldown(AttackData attackData)
    {
        if (attackData == null)
        {
            return false;
        }

        if (!cooldownTimers.ContainsKey(attackData))
        {
            cooldownTimers[attackData] = 0f;
        }
        return cooldownTimers[attackData] > 0f;
    }
    
    /// <summary>
    /// 残りクールタイムを取得（秒）
    /// </summary>
    public float GetRemainingCooldown(AttackData attackData)
    {
        if (!cooldownTimers.ContainsKey(attackData))
        {
            return 0f;
        }
        return Mathf.Max(0f, cooldownTimers[attackData]);
    }
    
    /// <summary>
    /// クールタイムの進行率を取得（0.0～1.0、1.0=完了）
    /// </summary>
    public float GetCooldownProgress(AttackData attackData)
    {
        if (attackData == null || attackData.cooldownTime <= 0f)
        {
            return 1f;
        }
        
        float remaining = GetRemainingCooldown(attackData);
        return 1f - (remaining / attackData.cooldownTime);
    }
    
    /// <summary>
    /// クールタイムを開始
    /// </summary>
    public void StartCooldown(AttackData attackData)
    {
        if (attackData != null)
        {
            cooldownTimers[attackData] = Mathf.Max(0f, attackData.cooldownTime);
        }
    }
    
    /// <summary>
    /// 攻撃を生成
    /// </summary>
    private void SpawnAttack(
        AttackData attackData,
        int damage,
        float scale,
        bool? facingRightOverride = null)
    {
        if (attackData.hitBoxPrefab == null)
        {
            Debug.LogWarning($"{attackData.attackName} のヒットボックスPrefabが設定されていません");
            return;
        }
        
        // プレイヤーの向きを取得
        bool isFacingRight = facingRightOverride ?? IsFacingRight();
        
        // 生成位置を計算
        float directionSign = attackData.followPlayerDirection && !isFacingRight ? -1f : 1f;
        Vector3 spawnPosition = playerTransform.position;
        spawnPosition.x += directionSign * (attackData.spawnDistance + attackData.spawnOffset.x);
        spawnPosition.y += attackData.spawnOffset.y;
        
        // ヒットボックスを生成
        GameObject hitBoxObj = Instantiate(
            attackData.hitBoxPrefab,
            spawnPosition,
            Quaternion.Euler(0f, 0f, attackData.rotationZ * directionSign));
        
        // スケール設定
        hitBoxObj.transform.localScale = new Vector3(
            Mathf.Max(0f, scale * attackData.scaleAxes.x),
            Mathf.Max(0f, scale * attackData.scaleAxes.y),
            1f);
        // 左向きならX反転（必要な場合）
        if (attackData.followPlayerDirection &&
            attackData.flipOnDirection &&
            !isFacingRight)
        {
            var ls = hitBoxObj.transform.localScale;
            ls.x = -Mathf.Abs(ls.x);
            hitBoxObj.transform.localScale = ls;
        }
        
        // ダメージ設定
        HitBox hitBox = hitBoxObj.GetComponent<HitBox>();
        if (hitBox != null)
        {
            hitBox.SetDamage(Mathf.Max(0, damage));
        }
        
        // 持続時間後に削除
        Destroy(hitBoxObj, Mathf.Max(0f, attackData.duration));
        
        Debug.Log($"{attackData.attackName} を実行 (ダメージ: {damage})");
    }
    
    private bool IsFacingRight()
    {
        return playerAttack == null || playerAttack.isRight;
    }
}
