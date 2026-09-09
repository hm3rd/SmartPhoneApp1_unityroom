using UnityEngine;
[RequireComponent(typeof(Camera))]
public sealed class CharacterPlazaCameraFollow : MonoBehaviour
{
    [SerializeField] Transform target; [SerializeField] BoxCollider2D plazaBounds;
    [SerializeField] float smoothTime = .15f; [SerializeField] Vector2 offset;
    Camera cameraComponent; Vector3 velocity;
    void Awake() { cameraComponent = GetComponent<Camera>(); }
    void LateUpdate()
    {
        if (!target) return;
        Vector3 desired = new Vector3(target.position.x + offset.x, target.position.y + offset.y, transform.position.z);
        if (plazaBounds && cameraComponent.orthographic)
        {
            Bounds b = plazaBounds.bounds; float hh = cameraComponent.orthographicSize, hw = hh * cameraComponent.aspect;
            desired.x = Clamp(desired.x, b.min.x + hw, b.max.x - hw);
            desired.y = Clamp(desired.y, b.min.y + hh, b.max.y - hh);
        }
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
    }
    static float Clamp(float value, float min, float max) { return min <= max ? Mathf.Clamp(value, min, max) : (min + max) * .5f; }
}
