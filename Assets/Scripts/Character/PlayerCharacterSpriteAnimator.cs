using UnityEngine;

/// <summary>
/// CharacterDataに登録された画像だけで、GameSceneのプレイヤーをアニメーションする。
/// Animator ControllerやAnimation Clipは不要。
/// </summary>
public sealed class PlayerCharacterSpriteAnimator : MonoBehaviour
{
    private enum ActionState { Idle, Move, Attack, Damage }

    [SerializeField] private SpriteRenderer targetRenderer;
    [Tooltip("登録画像を右向きとして扱い、左へ移動した時に左右反転します")]
    [SerializeField] private bool flipImageWhenMovingLeft = true;
    [Min(0f)]
    [SerializeField] private float movementDetectionThreshold = 0.001f;
    [Min(0f)]
    [Tooltip("FixedUpdateの間でも移動アニメーションが途切れないための保持時間")]
    [SerializeField] private float movementHoldSeconds = 0.1f;

    private CharacterData characterData;
    private ActionState currentState;
    private Vector3 previousPosition;
    private int frameIndex;
    private float frameRemaining;
    private bool actionLocked;
    private float lastMovementTime = -10f;
    private bool facingRight = true;

    public void Configure(CharacterData data, SpriteRenderer renderer)
    {
        characterData = data;
        if (renderer != null) targetRenderer = renderer;
        previousPosition = transform.position;
        facingRight = true;
        actionLocked = false;
        ApplyImageFlip();
        ChangeState(ActionState.Idle, true);
    }

    public void PlayAttack()
    {
        if (characterData == null || characterData.attackAnimation == null ||
            !characterData.attackAnimation.HasFrames) return;
        if (actionLocked && currentState == ActionState.Damage) return;
        ChangeState(ActionState.Attack, true);
        actionLocked = true;
    }

    public void PlayDamage()
    {
        if (characterData == null || characterData.damageAnimation == null ||
            !characterData.damageAnimation.HasFrames) return;
        // 被ダメージ演出は攻撃演出より優先する。
        ChangeState(ActionState.Damage, true);
        actionLocked = true;
    }

    /// <summary>入力方向とCharacterDataの基準反転設定を画像へ反映する。</summary>
    public void SetFacingRight(bool isFacingRight)
    {
        facingRight = isFacingRight;
        ApplyImageFlip();
    }

    private void Update()
    {
        Vector3 currentPosition = transform.position;
        Vector3 movementDelta = currentPosition - previousPosition;
        bool movedThisFrame = movementDelta.sqrMagnitude >
            movementDetectionThreshold * movementDetectionThreshold;
        UpdateFacingDirection(movementDelta.x);
        if (movedThisFrame) lastMovementTime = Time.time;
        bool isMoving = movedThisFrame || Time.time - lastMovementTime <= movementHoldSeconds;
        previousPosition = currentPosition;

        if (characterData == null || targetRenderer == null) return;

        if (actionLocked)
        {
            if (AdvanceFrame(false))
            {
                actionLocked = false;
                ChangeState(isMoving ? ActionState.Move : ActionState.Idle, true);
            }
            return;
        }

        ActionState desiredState = isMoving ? ActionState.Move : ActionState.Idle;
        if (currentState != desiredState)
            ChangeState(desiredState, true);
        else
            AdvanceFrame(true);
    }

    private void ChangeState(ActionState state, bool restart)
    {
        if (!restart && currentState == state) return;
        currentState = state;
        frameIndex = 0;
        ShowCurrentFrame();
    }

    /// <returns>1回再生が終了した場合true。</returns>
    private bool AdvanceFrame(bool loop)
    {
        CharacterData.SpriteAnimation animation = GetCurrentAnimation();
        if (animation == null || !animation.HasFrames)
        {
            ShowFallbackSprite();
            return !loop;
        }

        frameRemaining -= Time.deltaTime;
        if (frameRemaining > 0f) return false;

        int nextIndex = FindNextValidFrame(animation, frameIndex + 1);
        if (nextIndex < 0)
        {
            if (!loop) return true;
            nextIndex = FindNextValidFrame(animation, 0);
        }
        frameIndex = Mathf.Max(0, nextIndex);
        ShowCurrentFrame();
        return false;
    }

    private void ShowCurrentFrame()
    {
        CharacterData.SpriteAnimation animation = GetCurrentAnimation();
        if (animation == null || !animation.HasFrames)
        {
            ShowFallbackSprite();
            return;
        }

        frameIndex = FindNextValidFrame(animation, frameIndex);
        if (frameIndex < 0)
        {
            ShowFallbackSprite();
            return;
        }

        CharacterData.AnimationFrame frame = animation.frames[frameIndex];
        targetRenderer.sprite = frame.sprite;
        frameRemaining = Mathf.Max(0.01f, frame.displaySeconds);
    }

    private void ShowFallbackSprite()
    {
        if (targetRenderer != null && characterData != null)
            targetRenderer.sprite = characterData.characterSprite;
        frameRemaining = 0.1f;
    }

    private void UpdateFacingDirection(float horizontalMovement)
    {
        if (targetRenderer == null) return;
        if (horizontalMovement > movementDetectionThreshold)
            SetFacingRight(true);
        else if (horizontalMovement < -movementDetectionThreshold)
            SetFacingRight(false);
    }

    private void ApplyImageFlip()
    {
        if (targetRenderer == null) return;

        // ONの場合は反転後の画像が右向き（正方向）の基準となる。
        bool baseFlip = characterData != null &&
            characterData.flipGameSpritesByDefault;
        bool directionFlip = flipImageWhenMovingLeft && !facingRight;
        targetRenderer.flipX = baseFlip ^ directionFlip;
    }

    private CharacterData.SpriteAnimation GetCurrentAnimation()
    {
        if (characterData == null) return null;
        switch (currentState)
        {
            case ActionState.Move: return characterData.moveAnimation;
            case ActionState.Attack: return characterData.attackAnimation;
            case ActionState.Damage: return characterData.damageAnimation;
            default: return characterData.idleAnimation;
        }
    }

    private static int FindNextValidFrame(CharacterData.SpriteAnimation animation, int startIndex)
    {
        if (animation == null || animation.frames == null) return -1;
        for (int i = Mathf.Max(0, startIndex); i < animation.frames.Count; i++)
        {
            CharacterData.AnimationFrame frame = animation.frames[i];
            if (frame != null && frame.sprite != null) return i;
        }
        return -1;
    }
}
