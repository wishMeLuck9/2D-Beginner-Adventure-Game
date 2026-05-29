using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Health))]
public sealed class PlayerController2D : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4.5f;
    [SerializeField] private float interactDistance = 1.2f;

    [Header("Projectile")]
    [SerializeField] private RepairProjectile projectilePrefab;
    [SerializeField] private Transform projectileSpawn;
    [SerializeField] private float projectileSpeed = 9f;

    [Header("Feedback")]
    [SerializeField] private PlayerAnimationController animationController;

    private Rigidbody2D body;
    private Health health;
    private Vector2 moveInput;
    private Vector2 lookDirection = Vector2.down;

#if ENABLE_INPUT_SYSTEM
    private InputAction moveAction;
    private InputAction fireAction;
    private InputAction interactAction;
#endif

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();

        if (animationController == null)
        {
            animationController = GetComponent<PlayerAnimationController>();
        }

#if ENABLE_INPUT_SYSTEM
        ConfigureInputActions();
#endif
    }

    private void OnEnable()
    {
        health.Damaged += OnDamaged;
        health.Healed += OnHealed;

#if ENABLE_INPUT_SYSTEM
        moveAction.Enable();
        fireAction.Enable();
        interactAction.Enable();
#endif
    }

    private void OnDisable()
    {
        health.Damaged -= OnDamaged;
        health.Healed -= OnHealed;

#if ENABLE_INPUT_SYSTEM
        moveAction.Disable();
        fireAction.Disable();
        interactAction.Disable();
#endif
    }

    private void Update()
    {
        ReadInput();

        if (moveInput.sqrMagnitude > 0.001f)
        {
            lookDirection = moveInput.normalized;
        }

        animationController?.SetMovement(moveInput, lookDirection);

        if (WasFirePressed())
        {
            LaunchProjectile();
        }

        if (WasInteractPressed())
        {
            TryInteract();
        }
    }

    private void FixedUpdate()
    {
        Vector2 nextPosition = body.position + moveInput * moveSpeed * Time.fixedDeltaTime;
        body.MovePosition(nextPosition);
    }

#if ENABLE_INPUT_SYSTEM
    private void ConfigureInputActions()
    {
        moveAction = new InputAction("Move", InputActionType.Value);
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");
        moveAction.AddBinding("<Gamepad>/leftStick");

        fireAction = new InputAction("Fire", InputActionType.Button, "<Keyboard>/space");
        fireAction.AddBinding("<Gamepad>/buttonSouth");

        interactAction = new InputAction("Interact", InputActionType.Button, "<Keyboard>/e");
        interactAction.AddBinding("<Gamepad>/buttonWest");
    }
#endif

    private void ReadInput()
    {
#if ENABLE_INPUT_SYSTEM
        moveInput = moveAction.ReadValue<Vector2>();
#else
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#endif
        moveInput = Vector2.ClampMagnitude(moveInput, 1f);
    }

    private bool WasFirePressed()
    {
#if ENABLE_INPUT_SYSTEM
        return fireAction.WasPressedThisFrame();
#else
        return Input.GetKeyDown(KeyCode.Space);
#endif
    }

    private bool WasInteractPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return interactAction.WasPressedThisFrame();
#else
        return Input.GetKeyDown(KeyCode.E);
#endif
    }

    private void LaunchProjectile()
    {
        if (projectilePrefab == null)
        {
            return;
        }

        Vector2 direction = lookDirection.sqrMagnitude > 0.001f ? lookDirection.normalized : Vector2.down;
        Vector3 spawnPosition = projectileSpawn != null
            ? projectileSpawn.position
            : transform.position + (Vector3)(direction * 0.65f);

        RepairProjectile projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        projectile.Launch(direction, projectileSpeed, gameObject);
        animationController?.PlayLaunch();
        ToneAudio.PlayTone(transform.position, 760f, 0.08f, 0.18f);
    }

    private void TryInteract()
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(body.position, lookDirection, interactDistance);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null || hit.collider.transform.IsChildOf(transform))
            {
                continue;
            }

            MonoBehaviour[] behaviours = hit.collider.GetComponentsInParent<MonoBehaviour>();
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is IInteractable interactable)
                {
                    interactable.Interact(this);
                    return;
                }
            }
        }
    }

    private void OnDamaged()
    {
        animationController?.PlayHit();
        ToneAudio.PlayTone(transform.position, 160f, 0.14f, 0.20f);
    }

    private void OnHealed()
    {
        ToneAudio.PlayTone(transform.position, 540f, 0.10f, 0.16f);
    }
}
