using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCombatController : MonoBehaviour
{
    [Header("Combat Settings")]
    public float attackDamage = 5f; // Da�o que el enemigo hace por ataque
    public float attackProbability = 0.7f; // Probabilidad de �xito del ataque (ajustable desde el inspector)

    private Transform playerTarget;

    void Start()
    {
        // Buscar el jugador con el tag "ally"
        GameObject player = GameObject.FindGameObjectWithTag("ally");
        if (player != null)
        {
            playerTarget = player.transform;
        }
        else
        {
            Debug.LogError("No se encontr� ning�n objeto con el tag 'ally'");
        }

        // Subscribirse al evento del Interval del BeatManager
        if (BeatManager.Instance != null)
        {
            foreach (Intervals interval in BeatManager.Instance.Intervals) // Cambiado para usar la propiedad p�blica
            {
                interval.Trigger.AddListener(HandleAttack); // Cambiado para usar la propiedad p�blica
            }
        }
        else
        {
            Debug.LogError("No se encontr� BeatManager en la escena");
        }
    }

    void HandleAttack()
    {
        if (playerTarget == null)
            return;

        // Atacar con una probabilidad del 30% de fallar (ajustable)
        if (Random.value > attackProbability)
        {
            Debug.Log("El enemigo fall� el ataque");
        }
        else
        {
            // Realizar el ataque al jugador
            Attack(playerTarget.gameObject);
        }
    }

    void Attack(GameObject target)
    {
        // Intentar obtener el componente Health del objetivo
        Health targetHealth = target.GetComponent<Health>();
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(attackDamage);
            Debug.Log(target.tag + " recibi� " + attackDamage + " de da�o del enemigo");
        }
        else
        {
            Debug.LogError("El objeto " + target.name + " no tiene un componente Health");
        }
    }

    private void OnDestroy()
    {
        // Desuscribirse de los eventos del BeatManager si el objeto se destruye
        if (BeatManager.Instance != null)
        {
            foreach (Intervals interval in BeatManager.Instance.Intervals) // Cambiado para usar la propiedad p�blica
            {
                interval.Trigger.RemoveListener(HandleAttack); // Cambiado para usar la propiedad p�blica
            }
        }
    }
}
