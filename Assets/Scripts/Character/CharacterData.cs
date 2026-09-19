using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// キャラクターの基本情報を定義するScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "NewCharacter", menuName = "Character/CharacterData")]
public class CharacterData : ScriptableObject
{
    [Serializable]
    public class AnimationFrame
    {
        [Tooltip("このフレームで表示する画像")]
        public Sprite sprite;

        [Min(0.01f)]
        [Tooltip("この画像を表示する秒数")]
        public float displaySeconds = 0.1f;
    }

    [Serializable]
    public class SpriteAnimation
    {
        [Tooltip("上から順に再生します。画像の枚数はアクションごとに自由です")]
        public List<AnimationFrame> frames = new List<AnimationFrame>();

        public bool HasFrames => frames != null && frames.Exists(frame => frame != null && frame.sprite != null);
    }

    [Header("基本情報")]
    [Tooltip("キャラクターID（ユニークな識別子）")]
    public int characterId = 0;
    
    [Tooltip("キャラクター名")]
    public string characterName = "新しいキャラクター";
    
    [Tooltip("キャラクターの説明")]
    [TextArea(2, 4)]
    public string description = "";
    
    [Header("HomeScene・CharacterScene用の画像")]
    [Tooltip("HomeScene、CharacterScene、編成アイコンなどで表示するメイン画像1枚")]
    public Sprite characterSprite;
    
    // characterIconとして使用するプロパティ
    public Sprite characterIcon => characterSprite;

    [Header("GameScene用スプライトアニメーション")]
    [Tooltip("有効にすると、GameScene用の全画像を基準状態から左右反転します。左向きで作成された素材などに使用します")]
    public bool flipGameSpritesByDefault;

    [Tooltip("何もしていない時。複数設定すると繰り返し再生します")]
    public SpriteAnimation idleAnimation = new SpriteAnimation();

    [Tooltip("移動中。複数設定すると繰り返し再生します")]
    public SpriteAnimation moveAnimation = new SpriteAnimation();

    [Tooltip("攻撃した時。設定した画像を1回再生します")]
    public SpriteAnimation attackAnimation = new SpriteAnimation();

    [Tooltip("ダメージを受けた時。設定した画像を1回再生します")]
    public SpriteAnimation damageAnimation = new SpriteAnimation();

    [Header("ガチャ設定")]
    [Tooltip("ガチャの排出対象に含める")]
    public bool canBeObtainedFromGacha = true;

    [Min(1)]
    [Tooltip("排出の重み。値が大きいほど出やすくなります")]
    public int gachaWeight = 100;

    [Header("ホーム画面のセリフ")]
    [Tooltip("ホーム画面でキャラクターをタッチした時のセリフ候補")]
    public string[] homeTouchPhrases =
    {
        "やめてってば！",
        "くすぐったいよ",
        "びっくりした！"
    };
    
    [Header("ステータス")]
    [Tooltip("最大HP")]
    public int maxHP = 100;
    
    [Tooltip("移動速度")]
    public float moveSpeed = 5f;
    
    [Header("攻撃設定")]
    [Tooltip("このキャラクターが使える攻撃のリスト（PlayerATK2フォルダのAttackDataを設定）")]
    public List<AttackData> availableAttacks = new List<AttackData>();
    
    [Header("見た目")]
    [Tooltip("ゲーム内で使用するキャラクタープレハブ（任意）")]
    public GameObject characterPrefab;
    
    [Tooltip("UI表示用の色")]
    public Color themeColor = Color.white;

    private void OnValidate()
    {
        gachaWeight = Mathf.Max(1, gachaWeight);
        ValidateAnimation(idleAnimation);
        ValidateAnimation(moveAnimation);
        ValidateAnimation(attackAnimation);
        ValidateAnimation(damageAnimation);
    }

    private static void ValidateAnimation(SpriteAnimation animation)
    {
        if (animation == null || animation.frames == null) return;
        foreach (AnimationFrame frame in animation.frames)
        {
            if (frame != null)
                frame.displaySeconds = Mathf.Max(0.01f, frame.displaySeconds);
        }
    }
}
