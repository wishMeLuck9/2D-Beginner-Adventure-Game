using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public sealed class HudDocumentController : MonoBehaviour
{
    [SerializeField] private Health playerHealth;

    private UIDocument document;
    private Label healthLabel;
    private VisualElement healthFill;
    private Label dialogueLabel;
    private Label objectiveLabel;
    private Coroutine dialogueRoutine;

    public static HudDocumentController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        document = GetComponent<UIDocument>();
    }

    private void OnEnable()
    {
        VisualElement root = document.rootVisualElement;
        healthLabel = root.Q<Label>("health-label");
        healthFill = root.Q<VisualElement>("health-fill");
        dialogueLabel = root.Q<Label>("dialogue-label");
        objectiveLabel = root.Q<Label>("objective-label");

        if (playerHealth == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerHealth = player.GetComponent<Health>();
            }
        }

        if (playerHealth != null)
        {
            playerHealth.Changed += OnHealthChanged;
            OnHealthChanged(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.Changed -= OnHealthChanged;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void SetObjectiveText(string text)
    {
        if (objectiveLabel != null)
        {
            objectiveLabel.text = text;
        }
    }

    public void ShowDialogue(string message, float duration = 4f)
    {
        if (dialogueLabel == null)
        {
            return;
        }

        if (dialogueRoutine != null)
        {
            StopCoroutine(dialogueRoutine);
        }

        dialogueRoutine = StartCoroutine(ShowDialogueRoutine(message, duration));
    }

    private IEnumerator ShowDialogueRoutine(string message, float duration)
    {
        dialogueLabel.text = message;
        dialogueLabel.RemoveFromClassList("hidden");
        yield return new WaitForSeconds(duration);
        dialogueLabel.AddToClassList("hidden");
        dialogueRoutine = null;
    }

    private void OnHealthChanged(int current, int max)
    {
        if (healthLabel != null)
        {
            healthLabel.text = $"{current} / {max}";
        }

        if (healthFill != null)
        {
            float percent = max > 0 ? Mathf.Clamp01(current / (float)max) * 100f : 0f;
            healthFill.style.width = Length.Percent(percent);
        }
    }
}
