using UnityEngine;

public sealed class CameraFollow2D : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime = 0.16f;
    [SerializeField] private Vector2 minBounds = new Vector2(-8f, -5f);
    [SerializeField] private Vector2 maxBounds = new Vector2(8f, 5f);

    private Vector3 velocity;

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desired = new Vector3(
            Mathf.Clamp(target.position.x, minBounds.x, maxBounds.x),
            Mathf.Clamp(target.position.y, minBounds.y, maxBounds.y),
            transform.position.z);

        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
