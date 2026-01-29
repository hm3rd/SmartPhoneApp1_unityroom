using UnityEngine;
using UnityEngine.UI;

public class PlayerHP : MonoBehaviour
{
    [SerializeField] private GameCharacterManager gameCharacterManager;
    public float invincibleTime = 1.0f; // 無敵時間（秒）
    private float lastDamageTime = -10f; // 最後にダメージを受けた時刻

    void Start()
    {
        // GameCharacterManager の自動検索
        if (gameCharacterManager == null)
        {
            gameCharacterManager = FindObjectOfType<GameCharacterManager>();
            if (gameCharacterManager == null)
            {
                enabled = false;
                return;
            }
        }
    }

    // HPが変化したときに呼ぶ
    public void TakeDamage(int damage)
    {
        if (gameCharacterManager == null)
        {
            return;
        }

        // GameCharacterManager 経由でダメージを適用
        gameCharacterManager.ApplyDamageToCurrent(damage);
    }

    public void Heal(int amount)
    {
        if (gameCharacterManager == null)
        {
            return;
        }

        // GameCharacterManager 経由で回復を適用
        gameCharacterManager.HealCurrent(amount);
    }

    // Enemyタグに当たったらダメージ（無敵時間考慮）
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (Time.time - lastDamageTime >= invincibleTime)
            {
                TakeDamage(10); // 例：10ダメージ
                lastDamageTime = Time.time; // ここでのみ更新
            }
        }
    }
}