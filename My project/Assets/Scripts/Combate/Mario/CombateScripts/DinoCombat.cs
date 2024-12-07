using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DinoCombat : MonoBehaviour
{
    public Animator animator;          // Animator para las animaciones
    public int damage = 10;            // Da�o que este dinosaurio inflige
    public HealthSystem targetHealth;    // Salud del objetivo

    public void ExecuteAttack()
    {
        if (animator != null)
        {
            Debug.Log($"{gameObject.name}: Iniciando animaci�n de ataque.");
            animator.SetTrigger("Attack"); // Activar la animaci�n de ataque
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: No se asign� Animator.");
        }

        if (targetHealth != null)
        {
            targetHealth.TakeDamage(damage); // Infligir da�o al objetivo
            Debug.Log($"Infligido {damage} de da�o al objetivo.");
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: No se asign� un objetivo para el ataque.");
        }
    }
}
