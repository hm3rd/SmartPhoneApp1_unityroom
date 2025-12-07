using UnityEngine;

public class StageManager : MonoBehaviour
{
    public EnemySpawner spawner;
    public GameObject[] mapObjects;
    public GameObject player; // プレイヤーをInspectorでアサイン
    public float rightEdgeX = 8.0f;
    public float leftEdgeX = -8.0f;
    private int currentStage = 1;
    private bool isStageMoving = false;

    public GameObject resultPanel; // リザルトパネルをInspectorでアサイン

    void Start()
    {
        if (spawner == null) spawner = FindObjectOfType<EnemySpawner>();
        if (player == null) player = GameObject.FindWithTag("Player");
        SetStage(currentStage);
    }

    void Update()
    {
        // 最後のステージをクリアして右端に到達したらリザルト
        if (currentStage >= mapObjects.Length && spawner.isStageCleared && player.transform.position.x >= rightEdgeX && !isStageMoving)
        {
            isStageMoving = true;
            if (resultPanel != null)
                resultPanel.SetActive(true); // リザルトパネル表示
            return;
        }

        // 通常のステージ進行
        if (spawner.isStageCleared && player.transform.position.x >= rightEdgeX && !isStageMoving && currentStage < mapObjects.Length)
        {
            isStageMoving = true;
            currentStage++;
            SetStage(currentStage);
            // プレイヤーを左端にワープ
            player.transform.position = new Vector3(leftEdgeX, player.transform.position.y, player.transform.position.z);
        }

        // プレイヤーが左端より右に戻ったら再びステージ移動を許可
        if (isStageMoving && player.transform.position.x < rightEdgeX)
        {
            isStageMoving = false;
        }
    }

    public void SetStage(int stageNumber)
    {
        // マップの表示切り替え
        for (int i = 0; i < mapObjects.Length; i++)
        {
            if (mapObjects[i] != null)
                mapObjects[i].SetActive(i == (stageNumber - 1));
        }

        // ステージ数を超えたらリザルト
        if (stageNumber > mapObjects.Length)
        {
            if (resultPanel != null)
                resultPanel.SetActive(true); // リザルトパネル表示
            return;
        }

        // 敵の出現数や間隔・討伐数を切り替え
        switch (stageNumber)
        {
            case 1:
                spawner.spawnIntervalMin = 2.0f;
                spawner.spawnIntervalMax = 3.0f;
                spawner.targetDefeatCount = 3;
                break;
            case 2:
                spawner.spawnIntervalMin = 1.0f;
                spawner.spawnIntervalMax = 2.0f;
                spawner.targetDefeatCount = 5;
                break;
            case 3:
                spawner.spawnIntervalMin = 0.5f;
                spawner.spawnIntervalMax = 1.5f;
                spawner.targetDefeatCount = 8;
                break;
            default:
                break;
        }
        spawner.ResetSpawner();
    }
}
