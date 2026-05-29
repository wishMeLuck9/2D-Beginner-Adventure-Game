using System;
using UnityEngine;

public sealed class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private int currentHealth;
    [SerializeField] private float invincibleDuration = 1.2f;

    private float invincibleTimer;

    public event Action<int, int> Changed;
    public event Action Damaged;
    public event Action Healed;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public bool IsFull => currentHealth >= maxHealth;
    public bool IsInvincible => invincibleTimer > 0f;

    private void Awake()
    {
        if (currentHealth <= 0)
        {
            currentHealth = maxHealth;
        }

        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    private void Start()
    {
        Changed?.Invoke(currentHealth, maxHealth);
    }

    private void Update()
    {
        if (invincibleTimer > 0f)
        {
            invincibleTimer -= Time.deltaTime;
        }
    }

    public bool ChangeHealth(int amount)
    {
        if (amount == 0)
        {
            return false;
        }

        if (amount < 0 && IsInvincible)
        {
            return false;
        }

        int previous = currentHealth;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);

        if (currentHealth == previous)
        {
            return false;
        }

        if (amount < 0)
        {
            invincibleTimer = invincibleDuration;
            Damaged?.Invoke();
        }
        else
        {
            Healed?.Invoke();
        }

        Changed?.Invoke(currentHealth, maxHealth);
        return true;
    }

    public void ResetToFull()
    {
        currentHealth = maxHealth;
        invincibleTimer = 0f;
        Changed?.Invoke(currentHealth, maxHealth);
    }

    private void OnValidate()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        invincibleDuration = Mathf.Max(0f, invincibleDuration);
    }
}
