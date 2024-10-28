using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HealthSystem : MonoBehaviour
{
    public float maxHealth;
    private float currentHealth;

    public UnityEvent onDeath;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            if (onDeath != null)
            {
                onDeath.Invoke();
            }

            Destroy(gameObject);
        }
    }

    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
}
