using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerCombat : MonoBehaviour
{
    [Header("Combat Settings")]
    public float attackDamage = 10f; // Daño que el jugador hace por ataque

    private Transform enemyTarget;

    void Start()
    {
        // Buscar el enemigo con el tag "enemy"
        GameObject enemy = GameObject.FindGameObjectWithTag("enemy");
        if (enemy != null)
        {
            enemyTarget = enemy.transform;
        }
        else
        {
            Debug.LogError("No se encontró ningún objeto con el tag 'enemy'");
        }

        // Subscribirse al evento de éxito del QTE
        if (QTEManager.Instance != null)
        {
            QTEManager.Instance.onQTESuccess.AddListener(HandleQTESuccess);
        }
        else
        {
            Debug.LogError("No se encontró QTEManager en la escena");
        }
    }

    void HandleQTESuccess()
    {
        if (enemyTarget == null)
            return;

        // Realizar el ataque al enemigo
        Attack(enemyTarget.gameObject);
    }

    void Attack(GameObject target)
    {
        // Intentar obtener el componente Health del objetivo
        Health targetHealth = target.GetComponent<Health>();
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(attackDamage);
            Debug.Log(target.tag + " recibió " + attackDamage + " de daño del jugador");
        }
        else
        {
            Debug.LogError("El objeto " + target.name + " no tiene un componente Health");
        }
    }

    private void OnDestroy()
    {
        // Desuscribirse del evento de QTEManager si el objeto se destruye
        if (QTEManager.Instance != null)
        {
            QTEManager.Instance.onQTESuccess.RemoveListener(HandleQTESuccess);
        }
    }
}
