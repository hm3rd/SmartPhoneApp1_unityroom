using UnityEngine;

/// <summary>
/// 攻撃方法を1アセットで設定するマスターデータ。
/// 通常・多段・チャージを Attack Type で切り替える。
/// </summary>
[CreateAssetMenu(fileName = "NewAttackData", menuName = "PlayerATK2/AttackData")]
public class AttackData : ScriptableObject
{
    public enum AttackType
    {
        Normal,
        MultiHit,
        Charge
    }

    [Header("基本情報")]
    [Tooltip("Inspectorやログに表示する攻撃名")]
    public string attackName = "通常攻撃";

    [Tooltip("攻撃の実行方法")]
    public AttackType attackType = AttackType.Normal;

    [Header("ダメージ・クールタイム")]
    [Min(0)] public int damage = 10;
    [Min(0f)] public float cooldownTime = 1f;

    [Header("攻撃判定")]
    [Tooltip("HitBoxコンポーネントを付けたPrefab")]
    public GameObject hitBoxPrefab;

    [Min(0f)]
    [Tooltip("生成した攻撃判定が消えるまでの秒数")]
    public float duration = 0.2f;

    [Header("生成位置")]
    [Min(0f)]
    [Tooltip("プレイヤー前方へ離す距離")]
    public float spawnDistance = 1f;

    [Tooltip("前後・上下の追加オフセット。Xは向きに応じて反転します")]
    public Vector2 spawnOffset = Vector2.zero;

    [Tooltip("OFFの場合は常に右向きを基準に生成します")]
    public bool followPlayerDirection = true;

    [Header("見た目")]
    [Tooltip("全体の大きさ")]
    [Min(0f)] public float scale = 1f;

    [Tooltip("X・Yを個別に拡大縮小する倍率")]
    public Vector2 scaleAxes = Vector2.one;

    [Tooltip("生成時のZ回転角度")]
    public float rotationZ;

    [Tooltip("左向きのときPrefabのX軸を反転する")]
    public bool flipOnDirection = true;

    [Header("多段攻撃")]
    [Min(1)] public int hitCount = 3;
    [Min(0f)] public float hitInterval = 0.15f;

    [Tooltip("ONなら各ヒットの生成時にプレイヤーの向きを取り直します")]
    public bool updateDirectionEachHit = true;

    [Header("チャージ攻撃")]
    [Min(0.01f)] public float maxChargeTime = 2f;
    [Min(0)] public int minDamage = 10;
    [Min(0)] public int maxDamage = 50;
    [Min(0f)] public float minScale = 0.5f;
    [Min(0f)] public float maxScale = 2f;

    private void OnValidate()
    {
        damage = Mathf.Max(0, damage);
        cooldownTime = Mathf.Max(0f, cooldownTime);
        duration = Mathf.Max(0f, duration);
        spawnDistance = Mathf.Max(0f, spawnDistance);
        scale = Mathf.Max(0f, scale);
        scaleAxes.x = Mathf.Max(0f, scaleAxes.x);
        scaleAxes.y = Mathf.Max(0f, scaleAxes.y);

        hitCount = Mathf.Max(1, hitCount);
        hitInterval = Mathf.Max(0f, hitInterval);

        maxChargeTime = Mathf.Max(0.01f, maxChargeTime);
        minDamage = Mathf.Max(0, minDamage);
        maxDamage = Mathf.Max(minDamage, maxDamage);
        minScale = Mathf.Max(0f, minScale);
        maxScale = Mathf.Max(minScale, maxScale);
    }
}
