using UnityEngine;

public sealed class HealthCollectible : MonoBehaviour
{
    [SerializeField] private int healAmount = 1;
    [SerializeField] private ParticleSystem pickupEffectPrefab;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Health health = other.GetComponentInParent<Health>();
        if (health == null || health.IsFull)
        {
            return;
        }

        if (health.ChangeHealth(Mathf.Abs(healAmount)))
        {
            if (pickupEffectPrefab != null)
            {
                Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
            }

            ToneAudio.PlayTone(transform.position, 620f, 0.10f, 0.16f);
            Destroy(gameObject);
        }
    }
}
