using UnityEngine;

public sealed class RepairObjectiveTracker : MonoBehaviour
{
    [SerializeField] private MalfunctioningEnemy[] enemies;
    [SerializeField] private DialogueActor completionNpc;

    private int repairedCount;

    private void OnEnable()
    {
        if (enemies == null || enemies.Length == 0)
        {
            enemies = FindObjectsOfType<MalfunctioningEnemy>();
        }

        repairedCount = 0;
        foreach (MalfunctioningEnemy enemy in enemies)
        {
            if (enemy == null)
            {
                continue;
            }

            if (enemy.IsRepaired)
            {
                repairedCount++;
            }
            else
            {
                enemy.Repaired += OnEnemyRepaired;
            }
        }
    }

    private void Start()
    {
        UpdateObjective();
    }

    private void OnDisable()
    {
        if (enemies == null)
        {
            return;
        }

        foreach (MalfunctioningEnemy enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.Repaired -= OnEnemyRepaired;
            }
        }
    }

    private void OnEnemyRepaired()
    {
        repairedCount++;
        UpdateObjective();
    }

    private void UpdateObjective()
    {
        if (HudDocumentController.Instance == null || enemies == null)
        {
            return;
        }

        if (repairedCount >= enemies.Length)
        {
            HudDocumentController.Instance.SetObjectiveText("All machines repaired. Talk to the engineer with E.");
        }
        else
        {
            HudDocumentController.Instance.SetObjectiveText($"Repair machines: {repairedCount} / {enemies.Length}");
        }
    }
}
