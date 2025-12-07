using UnityEngine;
using UnityEngine.UI;

public class PlayerHP : MonoBehaviour
{
    public int maxHP = 100;
    public int currentHP = 100;
    public Slider hpBar;

    public float invincibleTime = 1.0f; // 無敵時間（秒）
    private float lastDamageTime = -10f; // 最後にダメージを受けた時刻

    void Start()
    {
        UpdateHPBar();
    }

    // HPが変化したときに呼ぶ
    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;
        UpdateHPBar();

        if (currentHP == 0)
        {
            GameOver();
        }
    }

    public void Heal(int amount)
    {
        currentHP += amount;
        if (currentHP > maxHP) currentHP = maxHP;
        UpdateHPBar();
    }

    void UpdateHPBar()
    {
        if (hpBar != null)
        {
            hpBar.value = (float)currentHP / maxHP;
        }
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

    // Menu.csのShowGameOverを呼び出す
    void GameOver()
    {
        GameMenu gamemenu = FindObjectOfType<GameMenu>();
        if (gamemenu != null)
        {
            gamemenu.ShowGameOver();
        }
    }
}
