using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Gatya : MonoBehaviour
{
    [Header("Rewards")]
    [SerializeField] private List<GachaReward> rewardPool = new();

    [Header("Options")]
    [SerializeField] private bool spawnRewardPrefab = true;
    [SerializeField] private Transform rewardParent;
    [SerializeField, Range(0.05f, 1f)] private float rollInterval = 0.15f;
    [SerializeField, Range(0.05f, 1f)] private float introDelay = 0.25f;

    [Header("Effects")]
    [SerializeField] private ParticleSystem rollEffect;
    [SerializeField] private AnimationCurve pulseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Events")]
    [SerializeField] private UnityEvent onRollStarted;
    [SerializeField] private UnityEvent onRollFinished;
    [SerializeField] private GachaRewardEvent onRewardPulled;

    private bool _isRolling;
    private readonly List<GachaReward> _lastResults = new();

    public IReadOnlyList<GachaReward> LastResults => _lastResults;

    public void PullOnce()
    {
        StartCoroutine(RollRoutine(1));
    }

    public void PullTen()
    {
        StartCoroutine(RollRoutine(10));
    }

    private IEnumerator RollRoutine(int count)
    {
        if (_isRolling)
        {
            yield break;
        }

        if (rewardPool.Count == 0)
        {
            Debug.LogWarning("Gatya: reward pool is empty.", this);
            yield break;
        }

        _isRolling = true;
        _lastResults.Clear();

        onRollStarted?.Invoke();
        rollEffect?.Play();

        yield return PlayPulse(introDelay);

        var totalWeight = rewardPool.Sum(r => Mathf.Max(1, r.weight));

        for (var i = 0; i < count; i++)
        {
            var reward = PullReward(totalWeight);
            _lastResults.Add(reward);
            onRewardPulled?.Invoke(reward);

            if (spawnRewardPrefab && reward.prefab != null)
            {
                Instantiate(reward.prefab, rewardParent != null ? rewardParent : transform, false);
            }

            yield return new WaitForSeconds(rollInterval);
        }

        rollEffect?.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        onRollFinished?.Invoke();
        _isRolling = false;
    }

    private IEnumerator PlayPulse(float duration)
    {
        if (duration <= 0f)
        {
            yield break;
        }

        var original = transform.localScale;
        var elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / duration);
            var strength = 1f + 0.08f * (pulseCurve != null ? pulseCurve.Evaluate(t) : Mathf.Sin(t * Mathf.PI));
            transform.localScale = original * strength;
            yield return null;
        }

        transform.localScale = original;
    }

    private GachaReward PullReward(int totalWeight)
    {
        var roll = Random.Range(0, totalWeight);
        var accumulator = 0;
        for (var i = 0; i < rewardPool.Count; i++)
        {
            accumulator += Mathf.Max(1, rewardPool[i].weight);
            if (roll < accumulator)
            {
                return rewardPool[i];
            }
        }

        return rewardPool[rewardPool.Count - 1];
    }

    [System.Serializable]
    public class GachaReward
    {
        public string name;
        public Sprite icon;
        public GameObject prefab;
        public int weight = 1;
    }

    [System.Serializable]
    private class GachaRewardEvent : UnityEvent<GachaReward> { }
}
