using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnIntervalMin = 1.0f;
    public float spawnIntervalMax = 3.0f;
    public int targetDefeatCount = 3; // この数だけ敵を倒したら次の場面へ

    private float timer = 0f;
    private float nextSpawnTime = 1f;
    private int spawnedEnemyCount = 0;
    private int defeatedEnemyCount = 0;

    public StageManager stageManager; // InspectorでStageManagerをアサイン
    public int currentStage = 1;      // 現在の場面番号

    public bool isStageCleared = false;

    void Start()
    {
        ResetSpawner();
    }

    void Update()
    {
        if (isStageCleared) return; // クリア後はスポーンしない

        // 合計でtargetDefeatCount体出したらスポーンしない
        if (spawnedEnemyCount >= targetDefeatCount) return;

        timer += Time.deltaTime;
        if (timer >= nextSpawnTime)
        {
            SpawnEnemy();
            SetNextSpawnTime();
            timer = 0f;
        }
    }

    void SetNextSpawnTime()
    {
        nextSpawnTime = Random.Range(spawnIntervalMin, spawnIntervalMax);
    }

    void SpawnEnemy()
    {
        Instantiate(enemyPrefab, GetRandomSpawnPosition(), Quaternion.identity);
        spawnedEnemyCount++;
    }

    Vector2 GetRandomSpawnPosition()
    {
        // 出現位置は適宜調整してください
        return new Vector2(Random.Range(-5f, 5f), Random.Range(-3f, 3f));
    }

    // スポナーの状態をリセット（ステージ移動時に呼ぶ）
    public void ResetSpawner()
    {
        // 既存の敵を全て削除
        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            Destroy(enemy);
        }
        timer = 0f;
        spawnedEnemyCount = 0;
        defeatedEnemyCount = 0;
        isStageCleared = false;
        SetNextSpawnTime();
    }

    // 敵が倒されたときに呼ぶ（Enemy側から呼び出し）
    public void OnEnemyDestroyed()
    {
        defeatedEnemyCount++;
        if (defeatedEnemyCount >= targetDefeatCount)
        {
            isStageCleared = true;
        }
    }
}