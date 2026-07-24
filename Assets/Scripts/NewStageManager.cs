using UnityEngine;

[System.Serializable]
public class SubStageInfo
{
    public string subStageName;
    public GameObject panel;
    public GameObject enemyPrefab;
    public int targetDefeatCount = 3;
    public float spawnIntervalMin = 1.0f;
    public float spawnIntervalMax = 3.0f;
}

[System.Serializable]
public class StageInfo
{
    public string stageName;
    public SubStageInfo[] subStages;
}

public class NewStageManager : MonoBehaviour
{
    public StageInfo[] allStages;
    public GameObject player;
    public float rightEdgeX = 8.0f;
    public float leftEdgeX = -8.0f;
    public GameObject resultPanel;
    public bool debugLogs = false;

    // ✅ 他スクリプトからセットされる整数ステージ番号
    [HideInInspector] public int targetStageIndex = 0;

    private StageInfo targetStage;
    private int currentSubStage = 0;
    private bool isStageMoving = false;
    private int spawnedEnemyCount = 0;
    private int defeatedEnemyCount = 0;
    private float timer = 0f;
    private float nextSpawnTime = 1f;
    private bool isSubStageCleared = false;
    private bool isStageCleared = false;
    private bool clearRewardGranted = false;

    void Start()
    {
    // 他スクリプト（StageSelector）から送られたステージ番号を取得
    targetStageIndex = PlayerPrefs.GetInt("SelectedStageIndex", 0);

    if (targetStageIndex < 0 || targetStageIndex >= allStages.Length)
    {
        Debug.LogError("指定されたステージ番号が無効です: " + targetStageIndex);
        return;
    }

    targetStage = allStages[targetStageIndex];
    currentSubStage = 0;
    SetSubStage(currentSubStage);

    if (resultPanel != null)
        resultPanel.SetActive(false);
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
            if (spawnedEnemyCount < info.targetDefeatCount)
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
        GameObject enemyObj = Instantiate(info.enemyPrefab, GetRandomSpawnPosition(), Quaternion.identity);
        EnemyHP enemy = enemyObj.GetComponent<EnemyHP>();
        if (enemy != null)
        {
            enemy.manager = this;
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
        var info = targetStage.subStages[currentSubStage];
        defeatedEnemyCount++;
        if (debugLogs)
        {
            Debug.Log($"[Stage] Enemy destroyed. defeated:{defeatedEnemyCount}/{info.targetDefeatCount}");
        }
        if (defeatedEnemyCount >= info.targetDefeatCount)
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
                if (debugLogs)
                {
                    Debug.Log("[Stage] Sub-stage cleared. Move to edge to proceed.");
                }
            }
        }
    }

    private void CompleteStage()
    {
        isStageCleared = true;
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }

        if (clearRewardGranted) return;

        int reward = Mathf.Max(
            0,
            PlayerPrefs.GetInt(StageSelectManager.PendingStoneRewardKey, 0));
        PlayerStoneWallet.Add(reward);
        PlayerPrefs.DeleteKey(StageSelectManager.PendingStoneRewardKey);
        PlayerPrefs.Save();
        clearRewardGranted = true;

        if (debugLogs)
        {
            Debug.Log($"[Stage] クリア報酬として石を{reward}個獲得しました。");
        }
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
        timer = 0f;
        SetNextSpawnTime(info);

        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            Destroy(enemy);
        }
    }
}
