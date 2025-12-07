using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    public float speed = 2f; // 追尾速度
    private Transform player;

    void Start()
    {
        // "Player"タグのオブジェクトを探す
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        if (player != null)
        {
            // プレイヤーへの方向ベクトル
            Vector3 dir = (player.position - transform.position).normalized;
            // プレイヤーに向かって移動
            transform.position += dir * speed * Time.deltaTime;
        }
    }
}
