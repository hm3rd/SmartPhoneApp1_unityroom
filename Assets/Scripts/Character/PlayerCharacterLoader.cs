using UnityEngine;

/// <summary>
/// ゲーム画面でプレイヤーに選択されたキャラクターデータを適用
/// </summary>
public class PlayerCharacterLoader : MonoBehaviour
{
    [Header("プレイヤー設定")]
    [Tooltip("攻撃を管理するAttackManager")]
    public AttackManager attackManager;
    
    [Tooltip("HPを管理するPlayerHP")]
    public PlayerHP playerHP;
    
    [Tooltip("移動を管理するTouchMove2")]
    public TouchMove2 touchMove;
    
    [Tooltip("キャラクターのスプライトを表示するSpriteRenderer")]
    public SpriteRenderer spriteRenderer;
    
    [Header("複数キャラ対応")]
    [Tooltip("選択された複数キャラのうち、どれを使うか（0=1体目、1=2体目、2=3体目）")]
    public int characterIndex = 0;
    
    void Start()
    {
        LoadSelectedCharacter();
    }
    
    /// <summary>
    /// 選択されたキャラクターデータを読み込んで適用
    /// </summary>
    void LoadSelectedCharacter()
    {
        if (CharacterSelectionManager.Instance == null)
        {
            Debug.LogWarning("CharacterSelectionManagerが見つかりません。デフォルト設定を使用します。");
            return;
        }
        
        var selectedCharacters = CharacterSelectionManager.Instance.GetSelectedCharacters();
        
        if (selectedCharacters.Count == 0)
        {
            Debug.LogWarning("キャラクターが選択されていません。");
            return;
        }
        
        // インデックスを範囲内に収める
        int index = Mathf.Clamp(characterIndex, 0, selectedCharacters.Count - 1);
        CharacterData character = selectedCharacters[index];
        
        Debug.Log($"キャラクター '{character.characterName}' をロードしました");
        
        // 攻撃データを適用
        if (attackManager != null && character.availableAttacks != null)
        {
            attackManager.availableAttacks.Clear();
            attackManager.availableAttacks.AddRange(character.availableAttacks);
            Debug.Log($"攻撃を{character.availableAttacks.Count}個設定しました");
        }
        
        // 最大HPを適用
        if (playerHP != null)
        {
            playerHP.maxHP = character.maxHP;
            playerHP.currentHP = character.maxHP;
            Debug.Log($"最大HP: {character.maxHP}");
        }
        
        // 移動速度を適用
        if (touchMove != null)
        {
            touchMove.speed = character.moveSpeed;
        }
        
        // スプライトを適用
        if (spriteRenderer != null && character.characterSprite != null)
        {
            spriteRenderer.sprite = character.characterSprite;
        }
    }
}
