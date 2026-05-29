using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public sealed class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int LookXHash = Animator.StringToHash("LookX");
    private static readonly int LookYHash = Animator.StringToHash("LookY");
    private static readonly int LaunchHash = Animator.StringToHash("Launch");
    private static readonly int HitHash = Animator.StringToHash("Hit");

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    public void SetMovement(Vector2 moveInput, Vector2 lookDirection)
    {
        if (animator != null)
        {
            animator.SetFloat(SpeedHash, moveInput.magnitude);
            animator.SetFloat(LookXHash, lookDirection.x);
            animator.SetFloat(LookYHash, lookDirection.y);
        }

        if (spriteRenderer != null && Mathf.Abs(lookDirection.x) > 0.1f)
        {
            spriteRenderer.flipX = lookDirection.x < 0f;
        }
    }

    public void PlayLaunch()
    {
        if (animator != null)
        {
            animator.SetTrigger(LaunchHash);
        }
    }

    public void PlayHit()
    {
        if (animator != null)
        {
            animator.SetTrigger(HitHash);
        }
    }
}
