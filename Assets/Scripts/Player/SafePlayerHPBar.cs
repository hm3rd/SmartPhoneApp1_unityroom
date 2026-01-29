using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// プレイヤーのHPを安全に表示する独立したSlider管理
/// GameCharacterManager から3体全員のHP を取得して常に表示する
/// キャラクター交代時にHPが正しく反映される
/// HP 0 で自動交代、全員HP 0 でゲームオーバー処理
/// </summary>
public class SafePlayerHPBar : MonoBehaviour
{
    private const int CHARACTER_SLOT_COUNT = 3;

    [Header("参照設定")]
    [Tooltip("GameCharacterManager（キャラクター管理）")]
    public GameCharacterManager gameCharacterManager;
    
    [Tooltip("3体分のHPスライダー")]
    public Slider[] displaySliders = new Slider[CHARACTER_SLOT_COUNT];
    
    [Tooltip("3体分のHP数値表示用テキスト")]
    public Text[] hpTexts = new Text[CHARACTER_SLOT_COUNT];
    
    [Tooltip("ゲームメニュー（ゲームオーバー処理用）")]
    public GameMenu gameMenu;
    
    void Start()
    {
        // 自動検索
        if (gameCharacterManager == null)
        {
            gameCharacterManager = FindObjectOfType<GameCharacterManager>();
            if (gameCharacterManager == null)
            {
                enabled = false;
                return;
            }
        }
        
        // Sliders の確認
        bool slidersValid = true;
        for (int i = 0; i < CHARACTER_SLOT_COUNT; i++)
        {
            if (displaySliders[i] == null)
            {
                slidersValid = false;
            }
            else
            {
                // Slider の初期化
                displaySliders[i].minValue = 0f;
                displaySliders[i].maxValue = 1f;
                displaySliders[i].wholeNumbers = false;
            }
        }
        
        if (!slidersValid)
        {
        }
        
        if (gameMenu == null)
        {
            gameMenu = FindObjectOfType<GameMenu>();
            if (gameMenu == null)
            {
            }
        }
        
        UpdateDisplay();
    }
    
    void LateUpdate()
    {
        // 全キャラクターのHP値を更新
        UpdateDisplay();
        CheckForCharacterDeath();
    }
    
    /// <summary>
    /// 3体全員のHP表示を更新
    /// </summary>
    void UpdateDisplay()
    {
        if (gameCharacterManager == null)
        {
            return;
        }
        
        // displaySliders の配列サイズをチェック
        if (displaySliders == null || displaySliders.Length == 0)
        {
            return;
        }
        
        // 3体分のHP表示を更新
        for (int i = 0; i < CHARACTER_SLOT_COUNT; i++)
        {
            int currentHp = gameCharacterManager.GetCharacterCurrentHp(i);
            int maxHp = gameCharacterManager.GetCharacterMaxHp(i);
            
            if (maxHp <= 0)
            {
                continue;
            }
            
            // Slider に反映
            if (i < displaySliders.Length && displaySliders[i] != null)
            {
                float ratio = (float)currentHp / maxHp;
                displaySliders[i].value = ratio;
            }
            
            // テキスト表示（設定されている場合）
            if (hpTexts != null && i < hpTexts.Length && hpTexts[i] != null)
            {
                hpTexts[i].text = $"{currentHp}/{maxHp}";
            }
        }
    }
    
    /// <summary>
    /// キャラクターの死亡判定と自動交代処理
    /// </summary>
    void CheckForCharacterDeath()
    {
        if (gameCharacterManager == null) return;
        
        // アクティブキャラクター（常にスロット0）のHPをチェック
        int currentHp = gameCharacterManager.GetCharacterCurrentHp(0);
        
        // 現在のアクティブキャラクター（スロット0）のHPが0になったか判定
        if (currentHp <= 0)
        {
            // 次のHP が残っているキャラクターに交代
            if (!gameCharacterManager.SwapToNextAliveCharacter())
            {
                // 全員のHPが0 の場合
                OnAllCharactersDead();
            }
            else
            {
                UpdateDisplay();
            }
        }
    }
    
    /// <summary>
    /// 全キャラクター死亡時の処理
    /// </summary>
    void OnAllCharactersDead()
    {
        if (gameMenu != null)
        {
            gameMenu.ShowGameOver();
        }
    }
}

