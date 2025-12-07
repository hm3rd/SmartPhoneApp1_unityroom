using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class AttackCooldownManager : MonoBehaviour
{
    [System.Serializable]
    public class AttackEntry
    {
        public MonoBehaviour attackScript; // 攻撃スクリプト(MonoBehaviour) 例: PlayerAttack1 / PlayerAttack2 / NormalAttack 等
        [Min(0f)] public float cooldownSeconds = 1f; // クールタイム長
        public Image cooldownGauge; // 任意: 個別ゲージ表示用Image
    }

    [Tooltip("管理したい攻撃スクリプトを登録してください")]
    public List<AttackEntry> attacks = new List<AttackEntry>();

    // 内部状態
    private readonly Dictionary<MonoBehaviour, float> durations = new Dictionary<MonoBehaviour, float>();
    private readonly Dictionary<MonoBehaviour, float> timers = new Dictionary<MonoBehaviour, float>();
    private readonly List<MonoBehaviour> _buffer = new List<MonoBehaviour>();

    void Awake()
    {
        // 重複を避けつつ初期化
        foreach (var e in attacks)
        {
            if (e.attackScript == null) continue;
            durations[e.attackScript] = Mathf.Max(0f, e.cooldownSeconds);
            // 初期ゲージを0に
            if (e.cooldownGauge != null)
            {
                e.cooldownGauge.fillAmount = 0f;
            }
        }
    }

    void Update()
    {
        if (timers.Count == 0) return;
        float dt = Time.deltaTime;
        _buffer.Clear();
        _buffer.AddRange(timers.Keys);
        foreach (var script in _buffer)
        {
            timers[script] -= dt;
            if (timers[script] <= 0f)
            {
                timers.Remove(script);
            }
        }

        // ゲージ更新
        foreach (var e in attacks)
        {
            if (e.attackScript == null || e.cooldownGauge == null) continue;
            float d = GetDuration(e.attackScript);
            if (d <= 0f)
            {
                e.cooldownGauge.fillAmount = 0f;
                continue;
            }
            float r = GetRemaining(e.attackScript);
            e.cooldownGauge.fillAmount = Mathf.Clamp01(r / d);
        }
    }

    public bool IsOnCooldown(MonoBehaviour attackScript)
    {
        if (attackScript == null) return false;
        return timers.TryGetValue(attackScript, out var t) && t > 0f;
    }

    public void StartCooldown(MonoBehaviour attackScript, float? overrideSeconds = null)
    {
        if (attackScript == null) return;
        float d = overrideSeconds ?? GetDuration(attackScript);
        if (d <= 0f) return;
        timers[attackScript] = d;
    }

    public float GetRemaining(MonoBehaviour attackScript)
    {
        if (attackScript == null) return 0f;
        return timers.TryGetValue(attackScript, out var t) ? Mathf.Max(0f, t) : 0f;
    }

    public float GetDuration(MonoBehaviour attackScript)
    {
        if (attackScript == null) return 0f;
        return durations.TryGetValue(attackScript, out var d) ? d : 0f;
    }

    public void SetDuration(MonoBehaviour attackScript, float seconds)
    {
        if (attackScript == null) return;
        durations[attackScript] = Mathf.Max(0f, seconds);
    }
}
