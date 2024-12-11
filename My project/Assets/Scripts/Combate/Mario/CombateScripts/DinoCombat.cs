using UnityEngine;

public class DinoCombat : MonoBehaviour
{
    private Animator animator;
    private int vidaActual;
    private int ataqueFinal;
    private Dinosaurio dinosaurio;

    void Start()
    {
        animator = GetComponent<Animator>();
        dinosaurio = GetComponent<Dinosaurio>();

        if (dinosaurio != null)
        {
            vidaActual = dinosaurio.statsBase.vidaBase;
            ataqueFinal = dinosaurio.statsBase.ataqueBase;
        }
        else
        {
            Debug.LogError("El componente Dinosaurio no está asignado al GameObject.");
            return; // Salir del método si falta el componente
        }
    }

    public void StartCombat()
    {
        Debug.Log($"{name} está listo para combatir.");
        // Puedes agregar lógica adicional para iniciar combate aquí si es necesario
    }

    public void ExecuteAttack()
    {
        // Activar la animación de ataque
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // Buscar al enemigo en la escena
        EnemyCombatController enemy = FindObjectOfType<EnemyCombatController>();
        if (enemy != null)
        {
            // Aplicar daño al enemigo
            enemy.TakeDamage(ataqueFinal);
            Debug.Log($"{name} atacó al enemigo causando {ataqueFinal} de daño.");
        }
        else
        {
            Debug.LogWarning("No se encontró un enemigo en la escena.");
        }
    }

    public void TakeDamage(float damage)
    {
        vidaActual -= Mathf.RoundToInt(damage);
        Debug.Log($"{name} recibió {damage} de daño. Vida restante: {vidaActual}");

        if (vidaActual <= 0)
        {
            HandleDeath();
        }
    }

    private void HandleDeath()
    {
        Debug.Log($"{name} ha sido derrotado.");
        Destroy(gameObject); // Elimina el GameObject del dinosaurio
    }
}