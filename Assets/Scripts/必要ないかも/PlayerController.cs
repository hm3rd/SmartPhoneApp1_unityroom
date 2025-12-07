using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public StageManager stageManager;
    public EnemySpawner spawner;
    public float rightEdgeX = 8.0f;
    public float leftEdgeX = -8.0f;
    private int currentStage = 1;
    private bool isStageMoving = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (stageManager == null) stageManager = FindObjectOfType<StageManager>();
        if (spawner == null) spawner = FindObjectOfType<EnemySpawner>();
    }

    // Update is called once per frame
    void Update()
    {
        // ステージクリア済みで右端に到達したら次のステージへ
        if (spawner.isStageCleared && transform.position.x >= rightEdgeX && !isStageMoving)
        {
            isStageMoving = true; // 多重実行防止
            currentStage++;
            stageManager.SetStage(currentStage);
            // プレイヤーを左端にワープ
            transform.position = new Vector3(leftEdgeX, transform.position.y, transform.position.z);
            // isStageClearedはSetStage→ResetSpawnerで自動リセットされる想定
        }

        // プレイヤーが左端より右に戻ったら再びステージ移動を許可
        if (isStageMoving && transform.position.x < rightEdgeX)
        {
            isStageMoving = false;
        }
    }
}
