using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DinoCombat : MonoBehaviour
{
    public Animator animator;          // Animator para las animaciones
    public int damage = 10;            // Daño que este dinosaurio inflige
    public HealthSystem targetHealth;    // Salud del objetivo

    public void ExecuteAttack()
    {
        if (animator != null)
        {
            Debug.Log($"{gameObject.name}: Iniciando animación de ataque.");
            animator.SetTrigger("Attack"); // Activar la animación de ataque
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: No se asignó Animator.");
        }

        if (targetHealth != null)
        {
            targetHealth.TakeDamage(damage); // Infligir daño al objetivo
            Debug.Log($"Infligido {damage} de daño al objetivo.");
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: No se asignó un objetivo para el ataque.");
        }
    }
}
