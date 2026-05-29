using UnityEngine;

public sealed class DamageZone : MonoBehaviour
{
    [SerializeField] private int damageAmount = 1;

    private void OnTriggerStay2D(Collider2D other)
    {
        ApplyDamage(other);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        ApplyDamage(collision.collider);
    }

    private void ApplyDamage(Collider2D other)
    {
        Health health = other.GetComponentInParent<Health>();
        if (health != null)
        {
            health.ChangeHealth(-Mathf.Abs(damageAmount));
        }
    }
}
