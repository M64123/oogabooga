using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    public int maxHealth = 100; // Salud máxima del dinosaurio
    private int currentHealth;  // Salud actual del dinosaurio

    void Start()
    {
        currentHealth = maxHealth; // Inicializar la salud
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"{gameObject.name} recibió {damage} de daño. Salud actual: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die(); // Llama a la función de muerte si la salud llega a 0
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} ha sido destruido.");
        Destroy(gameObject); // Destruye el objeto
    }
}
