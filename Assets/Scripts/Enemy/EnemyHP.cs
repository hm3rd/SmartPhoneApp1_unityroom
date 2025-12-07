using UnityEngine;
using UnityEngine.UI;

public class EnemyHP : MonoBehaviour
{
    public int maxHP = 100;
    public int currentHP = 100;
    public Slider hpBar; // 子オブジェクトのSliderをInspectorでアサイン
    //NewStageMangerに統合されたから必要ないかも
    //public EnemySpawner spawner; // EnemySpawnerへの参照
    public NewStageManager manager; // これをInspectorでなくSpawn時にセット

    void Start()
    {
        currentHP = maxHP;
        UpdateHPBar();
        //NewStageManagerに統合されたから必要ないかも
        //spawner = FindObjectOfType<EnemySpawner>(); // シーン内のEnemySpawnerを検索
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;
        
        // ダメージ表示
        DamagePopupManager.ShowDamage(damage, transform.position);
        
        UpdateHPBar();
        if (currentHP == 0)
        {
            ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
            if (scoreManager != null)
            {
                scoreManager.AddScore(1); // 1点加算
            }
            Destroy(gameObject); // HP0で消滅
        }
    }

    void UpdateHPBar()
    {
        if (hpBar != null)
        {
            hpBar.value = (float)currentHP / maxHP;
        }
    }

    void OnDestroy()
    {
        if (manager != null)
        {
            manager.OnEnemyDestroyed();
        }
    }
}
