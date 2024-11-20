using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerCombat : MonoBehaviour
{
    [Header("Combat Settings")]
    public float attackDamage = 10f; // Da�o que el jugador hace por ataque

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
            Debug.LogError("No se encontr� ning�n objeto con el tag 'enemy'");
        }

        // Subscribirse al evento de �xito del QTE
        if (QTEManager.Instance != null)
        {
            QTEManager.Instance.onQTESuccess.AddListener(HandleQTESuccess);
        }
        else
        {
            Debug.LogError("No se encontr� QTEManager en la escena");
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
            Debug.Log(target.tag + " recibi� " + attackDamage + " de da�o del jugador");
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
