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
            Debug.LogError("El componente Dinosaurio no est� asignado al GameObject.");
            return; // Salir del m�todo si falta el componente
        }
    }

    public void StartCombat()
    {
        Debug.Log($"{name} est� listo para combatir.");
        // Puedes agregar l�gica adicional para iniciar combate aqu� si es necesario
    }

    public void ExecuteAttack()
    {
        // Activar la animaci�n de ataque
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // Buscar al enemigo en la escena
        EnemyCombatController enemy = FindObjectOfType<EnemyCombatController>();
        if (enemy != null)
        {
            // Aplicar da�o al enemigo
            enemy.TakeDamage(ataqueFinal);
            Debug.Log($"{name} atac� al enemigo causando {ataqueFinal} de da�o.");
        }
        else
        {
            Debug.LogWarning("No se encontr� un enemigo en la escena.");
        }
    }

    public void TakeDamage(float damage)
    {
        vidaActual -= Mathf.RoundToInt(damage);
        Debug.Log($"{name} recibi� {damage} de da�o. Vida restante: {vidaActual}");

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