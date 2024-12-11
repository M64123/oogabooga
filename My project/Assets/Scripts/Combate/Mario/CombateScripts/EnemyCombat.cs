using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    public Animator animator;          // Animator para las animaciones
    public int damage = 15;            // Daño que hace este enemigo
    public float attackInterval = 2.0f; // Intervalo entre oportunidades de ataque
    public float attackChance = 0.05f; // Probabilidad de atacar (5%)
    public HealthSystem playerHealth;  // Sistema de salud del jugador

    private void Start()
    {
        // Iniciar el ciclo de ataques
        InvokeRepeating(nameof(TryAttack), attackInterval, attackInterval);
    }

    private void TryAttack()
    {
        if (Random.value <= attackChance)
        {
            ExecuteAttack();
        }
    }

    private void ExecuteAttack()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack"); // Ejecutar animación de ataque
        }

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage); // Reducir salud del jugador
            Debug.Log($"El jugador recibió {damage} puntos de daño.");
        }
    }
}
