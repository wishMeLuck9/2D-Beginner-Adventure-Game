using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class MalfunctioningEnemy : MonoBehaviour
{
    [SerializeField] private Vector2 patrolAxis = Vector2.right;
    [SerializeField] private float patrolDistance = 3f;
    [SerializeField] private float speed = 2f;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private ParticleSystem repairEffectPrefab;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int FixedHash = Animator.StringToHash("Fixed");

    private Rigidbody2D body;
    private Vector2 startPosition;
    private float direction = 1f;

    public event Action Repaired;

    public bool IsRepaired { get; private set; }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        startPosition = body.position;

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    private void FixedUpdate()
    {
        if (IsRepaired)
        {
            return;
        }

        Vector2 axis = patrolAxis.sqrMagnitude > 0.001f ? patrolAxis.normalized : Vector2.right;
        Vector2 nextPosition = body.position + axis * direction * speed * Time.fixedDeltaTime;
        body.MovePosition(nextPosition);

        float traveled = Vector2.Dot(nextPosition - startPosition, axis);
        if (Mathf.Abs(traveled) >= patrolDistance)
        {
            direction *= -1f;
        }

        if (spriteRenderer != null && Mathf.Abs(axis.x) > 0.1f)
        {
            spriteRenderer.flipX = direction < 0f;
        }

        if (animator != null)
        {
            animator.SetFloat(SpeedHash, speed);
        }
    }

    public void Repair()
    {
        if (IsRepaired)
        {
            return;
        }

        IsRepaired = true;
        body.velocity = Vector2.zero;

        foreach (DamageZone damageZone in GetComponentsInChildren<DamageZone>())
        {
            damageZone.enabled = false;
        }

        if (animator != null)
        {
            animator.SetFloat(SpeedHash, 0f);
            animator.SetBool(FixedHash, true);
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(0.62f, 0.92f, 0.82f, 1f);
        }

        if (repairEffectPrefab != null)
        {
            Instantiate(repairEffectPrefab, transform.position, Quaternion.identity);
        }

        ToneAudio.PlayTone(transform.position, 880f, 0.16f, 0.18f);
        Repaired?.Invoke();
    }
}
