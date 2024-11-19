using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerCombat : MonoBehaviour
{
    [Header("Combat Settings")]
    public float movementSpeed = 5f; // Velocidad de movimiento hacia el enemigo
    public float attackDamage = 10f; // Daño que el jugador hace por ataque
    public float distanceToStop = 1f; // Distancia al enemigo en la que se detiene

    private Transform enemyTarget;
    private bool isMoving = true;

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
    }

    void Update()
    {
        if (enemyTarget != null && isMoving)
        {
            MoveTowardsEnemy();
        }
    }

    private void MoveTowardsEnemy()
    {
        float distance = Vector2.Distance(transform.position, enemyTarget.position);

        if (distance > distanceToStop)
        {
            // Mover al jugador hacia el enemigo con la velocidad especificada
            transform.position = Vector2.MoveTowards(transform.position, enemyTarget.position, movementSpeed * Time.deltaTime);
        }
        else
        {
            // Detener movimiento y comenzar la fase de combate
            isMoving = false;
            StartCombat();
        }
    }

    private void StartCombat()
    {
        if (BeatManager.Instance != null)
        {
            BeatManager.Instance.StartCombatPhase(); // Cambiado de StartCombatBeat a StartCombatPhase
        }
    }

    public void Attack()
    {
        if (enemyTarget != null)
        {
            Health enemyHealth = enemyTarget.GetComponent<Health>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(attackDamage);
                Debug.Log("El enemigo recibió " + attackDamage + " de daño.");
            }
            else
            {
                Debug.LogError("El enemigo no tiene un componente Health");
            }
        }
    }
}
