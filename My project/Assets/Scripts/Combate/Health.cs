using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f; // Salud máxima
    private float currentHealth;

    void Start()
    {
        currentHealth = maxHealth; // Inicializar la salud al máximo al inicio
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log(gameObject.tag + " tiene " + currentHealth + " de salud restante");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log(gameObject.tag + " ha muerto");
        Destroy(gameObject);
    }
}
