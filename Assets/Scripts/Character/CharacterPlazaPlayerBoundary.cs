using UnityEngine;
public sealed class CharacterPlazaPlayerBoundary : MonoBehaviour
{
    [SerializeField] BoxCollider2D plazaBounds; [SerializeField] float padding = .5f;
    public void Configure(BoxCollider2D bounds, float edgePadding) { plazaBounds = bounds; padding = edgePadding; }
    void LateUpdate()
    {
        if (!plazaBounds) return; Bounds b = plazaBounds.bounds; Vector3 p = transform.position;
        p.x = Mathf.Clamp(p.x, b.min.x + padding, b.max.x - padding);
        p.y = Mathf.Clamp(p.y, b.min.y + padding, b.max.y - padding); transform.position = p;
    }
}
