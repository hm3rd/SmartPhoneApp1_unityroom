using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EnemySpawnSetting
{
    [Tooltip("Inspector上で判別するための名前")]
    public string enemyName;

    public GameObject enemyPrefab;

    [Min(0)]
    [Tooltip("このサブステージで出現させる数")]
    public int spawnCount = 1;
}

[System.Serializable]
public class SubStageInfo
{
    public string subStageName;
    public GameObject panel;

    [Header("出現する敵（新設定）")]
    [Tooltip("敵Prefabと出現数を種類ごとに登録します")]
    public EnemySpawnSetting[] enemySpawnSettings;

    [Header("旧設定（Enemy Spawn Settingsが空の場合のみ使用）")]
    public GameObject enemyPrefab;
    public int targetDefeatCount = 3;
    public float spawnIntervalMin = 1.0f;
    public float spawnIntervalMax = 3.0f;
}

[System.Serializable]
public class StageInfo
{
    public string stageName;

    [Min(0)]
    [Tooltip("このステージをクリアした際に獲得する石の数")]
    public int clearStoneReward = 10;

    public SubStageInfo[] subStages;
}

public class NewStageManager : MonoBehaviour
{
    [Header("ステージ設定")]
    public StageInfo[] allStages;

    [Header("プレイヤー・画面範囲")]
    public GameObject player;
    public float rightEdgeX = 8.0f;
    public float leftEdgeX = -8.0f;

    [Tooltip("未設定の場合はMain Cameraを使用します")]
    [SerializeField] private Camera stageCamera;

    [Tooltip("画面端とプレイヤーColliderの間に追加する余白")]
    [Min(0f)]
    [SerializeField] private float screenEdgePadding = 0.05f;

    [Header("クリア表示")]
    public GameObject resultPanel;

    [Header("クリア評価（3体の残りHP合計割合）")]
    [Range(0f, 1f)]
    [Tooltip("S評価に必要な残りHP割合。0.8なら80%以上")]
    [SerializeField] private float sRankHealthRatio = 0.8f;

    [Range(0f, 1f)]
    [Tooltip("A評価に必要な残りHP割合。これ未満はB評価")]
    [SerializeField] private float aRankHealthRatio = 0.5f;

    [Tooltip("サブステージクリア後に表示する「右へ移動」案内画像")]
    [SerializeField] private GameObject moveRightPrompt;

    public bool debugLogs = false;

    // ✅ 他スクリプトからセットされる整数ステージ番号
    [HideInInspector] public int targetStageIndex = 0;

    private StageInfo targetStage;
    private int currentSubStage = 0;
    private bool isStageMoving = false;
    private int spawnedEnemyCount = 0;
    private int defeatedEnemyCount = 0;
    private int totalDefeatedEnemyCount = 0;
    private long totalDamageDealt = 0;
    private float timer = 0f;
    private float nextSpawnTime = 1f;
    private bool isSubStageCleared = false;
    private bool isStageCleared = false;
    private bool clearRewardGranted = false;
    private readonly List<GameObject> currentSpawnQueue =
        new List<GameObject>();
    private Rigidbody2D playerBody;
    private Collider2D playerSolidCollider;
    private SpriteRenderer playerSpriteRenderer;
    private GameResultPanelController resultController;
    private float stageStartTime;

    void Start()
    {
    stageStartTime = Time.time;
    // 他スクリプト（StageSelector）から送られたステージ番号を取得
    targetStageIndex = PlayerPrefs.GetInt("SelectedStageIndex", 0);

    if (targetStageIndex < 0 || targetStageIndex >= allStages.Length)
    {
        Debug.LogError("指定されたステージ番号が無効です: " + targetStageIndex);
        return;
    }

    targetStage = allStages[targetStageIndex];
    if (stageCamera == null)
        stageCamera = Camera.main;
    if (player != null)
    {
        playerBody = player.GetComponent<Rigidbody2D>();
        playerSolidCollider = FindSolidPlayerCollider(player);
        playerSpriteRenderer = player.GetComponent<SpriteRenderer>();
    }

    currentSubStage = 0;
    SetSubStage(currentSubStage);

    if (resultPanel != null)
    {
        resultController = resultPanel.GetComponent<GameResultPanelController>() ??
                           resultPanel.AddComponent<GameResultPanelController>();
        resultPanel.SetActive(false);
    }
    SetMoveRightPrompt(false);
    }


    void Update()
    {
        if (isStageCleared)
        {
            if (resultPanel != null && !resultPanel.activeSelf)
                resultPanel.SetActive(true);
            return;
        }

        if (isSubStageCleared && player.transform.position.x >= rightEdgeX && !isStageMoving)
        {
            if (debugLogs)
            {
                Debug.Log($"[Stage] Clear detected. sub:{currentSubStage} -> {currentSubStage + 1}, playerX:{player.transform.position.x:F2} >= edge:{rightEdgeX:F2}");
            }
            isStageMoving = true;
            currentSubStage++;

            if (currentSubStage < targetStage.subStages.Length)
            {
                SetSubStage(currentSubStage);
                player.transform.position = new Vector3(leftEdgeX, player.transform.position.y, player.transform.position.z);
            }
            else
            {
                CompleteStage();
                if (debugLogs)
                {
                    Debug.Log("[Stage] All sub-stages cleared. Stage complete.");
                }
            }
        }

        if (isStageMoving && player.transform.position.x < rightEdgeX)
        {
            isStageMoving = false;
            if (debugLogs)
            {
                Debug.Log("[Stage] Stage moving finished.");
            }
        }

        if (!isSubStageCleared && currentSubStage < targetStage.subStages.Length)
        {
            var info = targetStage.subStages[currentSubStage];
            if (spawnedEnemyCount < currentSpawnQueue.Count)
            {
                timer += Time.deltaTime;
                if (timer >= nextSpawnTime)
                {
                    SpawnEnemy(info);
                    SetNextSpawnTime(info);
                    timer = 0f;
                }
            }
        }
    }

    void SetNextSpawnTime(SubStageInfo info)
    {
        nextSpawnTime = Random.Range(info.spawnIntervalMin, info.spawnIntervalMax);
    }

    void SpawnEnemy(SubStageInfo info)
    {
        if (spawnedEnemyCount < 0 ||
            spawnedEnemyCount >= currentSpawnQueue.Count)
        {
            return;
        }

        GameObject prefab = currentSpawnQueue[spawnedEnemyCount];
        if (prefab == null)
        {
            Debug.LogWarning(
                $"{info.subStageName}: 敵Prefabが未設定の出現枠をスキップしました。");
            spawnedEnemyCount++;
            return;
        }

        GameObject enemyObj = Instantiate(
            prefab,
            GetRandomSpawnPosition(),
            Quaternion.identity);
        EnemyHP enemy = enemyObj.GetComponent<EnemyHP>();
        if (enemy != null)
        {
            enemy.Initialize(this);
            if (debugLogs)
            {
                Debug.Log("[Stage] Enemy spawned and manager set.");
            }
        }
        spawnedEnemyCount++;
    }

    Vector2 GetRandomSpawnPosition()
    {
        return new Vector2(Random.Range(-5f, 5f), Random.Range(-3f, 3f));
    }

    public void OnEnemyDestroyed()
    {
        defeatedEnemyCount++;
        totalDefeatedEnemyCount++;
        if (debugLogs)
        {
            Debug.Log(
                $"[Stage] Enemy destroyed. defeated:{defeatedEnemyCount}/{currentSpawnQueue.Count}");
        }
        if (defeatedEnemyCount >= currentSpawnQueue.Count)
        {
            isSubStageCleared = true;
            // 最終サブステージをクリアしたら即ResultPanel表示
            bool isLastSubStage = (currentSubStage >= targetStage.subStages.Length - 1);
            if (isLastSubStage)
            {
                CompleteStage();
                if (debugLogs)
                {
                    Debug.Log("[Stage] Final sub-stage cleared. Result panel shown.");
                }
            }
            else
            {
                SetMoveRightPrompt(true);
                if (debugLogs)
                {
                    Debug.Log("[Stage] Sub-stage cleared. Move to edge to proceed.");
                }
            }
        }
    }

    public void OnDamageDealt(int damage)
    {
        if (!isStageCleared && damage > 0)
        {
            totalDamageDealt += damage;
        }
    }

    private void LateUpdate()
    {
        KeepPlayerInsideScreen();
    }

    private void CompleteStage()
    {
        isStageCleared = true;
        SetMoveRightPrompt(false);
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }

        if (clearRewardGranted) return;

        int reward = targetStage != null
            ? Mathf.Max(0, targetStage.clearStoneReward)
            : 0;
        PlayerStoneWallet.Add(reward);
        // 旧バージョンで保存された一時報酬値は今後使用しない
        PlayerPrefs.DeleteKey("PendingStageStoneReward");
        PlayerPrefs.Save();
        clearRewardGranted = true;

        if (resultController != null)
        {
            float clearTime = Mathf.Max(0f, Time.time - stageStartTime);
            float remainingHealthRatio = CalculatePartyHealthRatio();
            string evaluation = EvaluateClearRank(remainingHealthRatio);
            resultController.Show(
                totalDefeatedEnemyCount,
                totalDamageDealt,
                reward,
                clearTime,
                evaluation,
                remainingHealthRatio);
        }

        if (debugLogs)
        {
            Debug.Log($"[Stage] クリア報酬として石を{reward}個獲得しました。");
        }
    }

    private float CalculatePartyHealthRatio()
    {
        GameCharacterManager characterManager =
            FindFirstObjectByType<GameCharacterManager>();
        if (characterManager == null) return 0f;

        long currentHealth = 0;
        long maxHealth = 0;
        for (int i = 0; i < 3; i++)
        {
            int slotMaxHealth = Mathf.Max(0, characterManager.GetCharacterMaxHp(i));
            if (slotMaxHealth <= 0) continue;
            maxHealth += slotMaxHealth;
            currentHealth += Mathf.Clamp(
                characterManager.GetCharacterCurrentHp(i),
                0,
                slotMaxHealth);
        }
        return maxHealth > 0
            ? Mathf.Clamp01((float)currentHealth / maxHealth)
            : 0f;
    }

    private string EvaluateClearRank(float healthRatio)
    {
        float sThreshold = Mathf.Clamp01(sRankHealthRatio);
        float aThreshold = Mathf.Min(sThreshold, Mathf.Clamp01(aRankHealthRatio));
        if (healthRatio >= sThreshold) return "S";
        if (healthRatio >= aThreshold) return "A";
        return "B";
    }

    private void OnValidate()
    {
        sRankHealthRatio = Mathf.Clamp01(sRankHealthRatio);
        aRankHealthRatio = Mathf.Clamp(aRankHealthRatio, 0f, sRankHealthRatio);
    }

    public void SetSubStage(int subStageIdx)
    {
        foreach (var stage in allStages)
        {
            foreach (var sub in stage.subStages)
            {
                if (sub.panel != null) sub.panel.SetActive(false);
            }
        }

        if (targetStage == null || subStageIdx >= targetStage.subStages.Length)
            return;

        var info = targetStage.subStages[subStageIdx];
        if (info.panel != null)
            info.panel.SetActive(true);

        spawnedEnemyCount = 0;
        defeatedEnemyCount = 0;
        isSubStageCleared = false;
        SetMoveRightPrompt(false);
        timer = 0f;
        BuildSpawnQueue(info);
        SetNextSpawnTime(info);

        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            Destroy(enemy);
        }
    }

    private void KeepPlayerInsideScreen()
    {
        if (player == null || stageCamera == null || isStageCleared)
        {
            return;
        }

        Vector3 worldPosition = player.transform.position;
        Vector3 viewportPosition =
            stageCamera.WorldToViewportPoint(worldPosition);
        if (viewportPosition.z <= 0f)
        {
            return;
        }
        Vector3 originalViewportPosition = viewportPosition;

        float horizontalPadding = screenEdgePadding;
        float verticalPadding = screenEdgePadding;
        Bounds playerBounds = default;
        bool hasPlayerBounds = false;
        if (playerSolidCollider != null && playerSolidCollider.enabled)
        {
            playerBounds = playerSolidCollider.bounds;
            hasPlayerBounds = true;
        }
        else if (playerSpriteRenderer != null)
        {
            playerBounds = playerSpriteRenderer.bounds;
            hasPlayerBounds = true;
        }

        if (hasPlayerBounds)
        {
            Vector3 rightPoint = stageCamera.WorldToViewportPoint(
                worldPosition + Vector3.right * playerBounds.extents.x);
            Vector3 topPoint = stageCamera.WorldToViewportPoint(
                worldPosition + Vector3.up * playerBounds.extents.y);
            horizontalPadding +=
                Mathf.Abs(rightPoint.x - viewportPosition.x);
            verticalPadding +=
                Mathf.Abs(topPoint.y - viewportPosition.y);
        }

        viewportPosition.x = Mathf.Max(
            horizontalPadding,
            viewportPosition.x);

        // サブステージクリア後だけ右側の画面外へ移動可能にする
        if (!isSubStageCleared)
        {
            viewportPosition.x = Mathf.Min(
                1f - horizontalPadding,
                viewportPosition.x);
        }

        viewportPosition.y = Mathf.Clamp(
            viewportPosition.y,
            verticalPadding,
            1f - verticalPadding);

        // 範囲内ならRigidbody2Dへ触らない。
        // 毎フレーム書き戻すとDashのMovePositionを打ち消してしまう。
        if (Mathf.Approximately(
                viewportPosition.x,
                originalViewportPosition.x) &&
            Mathf.Approximately(
                viewportPosition.y,
                originalViewportPosition.y))
        {
            return;
        }

        Vector3 clampedWorldPosition =
            stageCamera.ViewportToWorldPoint(viewportPosition);
        clampedWorldPosition.z = worldPosition.z;

        if (playerBody != null)
        {
            playerBody.position = clampedWorldPosition;
        }
        else
        {
            player.transform.position = clampedWorldPosition;
        }
    }

    private void SetMoveRightPrompt(bool visible)
    {
        if (moveRightPrompt != null &&
            moveRightPrompt.activeSelf != visible)
        {
            moveRightPrompt.SetActive(visible);
        }
    }

    private static Collider2D FindSolidPlayerCollider(GameObject target)
    {
        foreach (Collider2D collider in target.GetComponents<Collider2D>())
        {
            if (collider.enabled && !collider.isTrigger)
            {
                return collider;
            }
        }
        return null;
    }

    private void BuildSpawnQueue(SubStageInfo info)
    {
        currentSpawnQueue.Clear();

        if (info.enemySpawnSettings != null &&
            info.enemySpawnSettings.Length > 0)
        {
            foreach (EnemySpawnSetting setting in info.enemySpawnSettings)
            {
                if (setting == null)
                {
                    continue;
                }

                if (setting.enemyPrefab == null)
                {
                    Debug.LogWarning(
                        $"{info.subStageName}: Enemy Spawn SettingsにPrefab未設定の項目があります。");
                    continue;
                }

                int count = Mathf.Max(0, setting.spawnCount);
                for (int i = 0; i < count; i++)
                {
                    currentSpawnQueue.Add(setting.enemyPrefab);
                }
            }
        }

        if (currentSpawnQueue.Count == 0 && info.enemyPrefab != null)
        {
            int legacyCount = Mathf.Max(1, info.targetDefeatCount);
            for (int i = 0; i < legacyCount; i++)
            {
                currentSpawnQueue.Add(info.enemyPrefab);
            }
        }

        if (currentSpawnQueue.Count == 0)
        {
            Debug.LogError(
                $"{info.subStageName}: 出現可能な敵Prefabが1つも設定されていません。");
        }

        // 登録順に偏らないよう、サブステージ開始時に出現順を混ぜる
        for (int i = currentSpawnQueue.Count - 1; i > 0; i--)
        {
            int swapIndex = Random.Range(0, i + 1);
            GameObject temp = currentSpawnQueue[i];
            currentSpawnQueue[i] = currentSpawnQueue[swapIndex];
            currentSpawnQueue[swapIndex] = temp;
        }
    }
}
