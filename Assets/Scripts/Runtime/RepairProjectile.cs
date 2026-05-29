using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class RepairProjectile : MonoBehaviour
{
    [SerializeField] private float lifeTime = 2.5f;
    [SerializeField] private ParticleSystem hitEffectPrefab;

    private Rigidbody2D body;
    private GameObject owner;
    private float lifeTimer;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifeTime)
        {
            Destroy(gameObject);
        }
    }

    public void Launch(Vector2 direction, float speed, GameObject projectileOwner)
    {
        owner = projectileOwner;
        Vector2 launchDirection = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector2.down;
        body.velocity = launchDirection * speed;
        transform.right = launchDirection;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (owner != null && other.transform.IsChildOf(owner.transform))
        {
            return;
        }

        MalfunctioningEnemy enemy = other.GetComponentInParent<MalfunctioningEnemy>();
        if (enemy != null)
        {
            enemy.Repair();
            SpawnHitEffect();
            Destroy(gameObject);
            return;
        }

        if (!other.isTrigger)
        {
            SpawnHitEffect();
            Destroy(gameObject);
        }
    }

    private void SpawnHitEffect()
    {
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }
    }
}
