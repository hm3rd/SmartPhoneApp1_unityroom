using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public sealed class CharacterPlazaNpc : MonoBehaviour
{
    [Min(0)] [SerializeField] float moveSpeed = 1.5f;
    [Min(0)] [SerializeField] float minimumWaitSeconds = 1f;
    [Min(0)] [SerializeField] float maximumWaitSeconds = 3f;
    [SerializeField] SpriteRenderer spriteRenderer;
    public CharacterData Data { get; private set; }
    BoxCollider2D area; float padding, waitUntil; Vector2 destination; bool moving;

    void Awake()
    {
        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
        // NPCはプレイヤーと接触しても物理的な力を加えない。
        foreach (Collider2D collider in GetComponentsInChildren<Collider2D>(true))
            collider.isTrigger = true;
        Rigidbody2D body = GetComponent<Rigidbody2D>();
        if (body)
        {
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.linearVelocity = Vector2.zero;
        }
    }
    public void Configure(CharacterData data, BoxCollider2D bounds, float edgePadding, float targetWorldSize)
    {
        Data = data; area = bounds; padding = edgePadding;
        spriteRenderer.sprite = data.characterSprite; spriteRenderer.color = data.themeColor;
        NormalizeRenderer(targetWorldSize); ChooseDestination();
    }
    void Update()
    {
        if (!area || Time.time < waitUntil) return;
        if (!moving) { ChooseDestination(); return; }
        Vector2 difference = destination - (Vector2)transform.position;
        if (difference.sqrMagnitude < .01f)
        { moving = false; waitUntil = Time.time + Random.Range(minimumWaitSeconds, maximumWaitSeconds); return; }
        Vector2 step = difference.normalized * moveSpeed * Time.deltaTime;
        transform.position += (Vector3)step;
        if (Mathf.Abs(step.x) > .0001f) spriteRenderer.flipX = step.x < 0;
    }
    public void PauseForConversation() { moving = false; waitUntil = Time.time + Mathf.Max(2, maximumWaitSeconds); }
    void ChooseDestination()
    {
        Bounds b = area.bounds;
        destination = new Vector2(Random.Range(b.min.x + padding, b.max.x - padding), Random.Range(b.min.y + padding, b.max.y - padding));
        moving = true;
    }
    void NormalizeRenderer(float targetWorldSize)
    {
        if (!spriteRenderer || !spriteRenderer.sprite) return;
        Vector2 size = spriteRenderer.sprite.bounds.size;
        float longestSide = Mathf.Max(size.x, size.y);
        if (longestSide > .0001f)
            spriteRenderer.transform.localScale = Vector3.one * (Mathf.Max(.1f, targetWorldSize) / longestSide);
    }
}
