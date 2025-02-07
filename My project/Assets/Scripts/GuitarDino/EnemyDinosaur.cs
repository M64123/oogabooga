using UnityEngine;

public class EnemyDinosaur : MonoBehaviour
{
    [Header("Estadísticas del Enemigo")]
    public DinoStats stats; // Asigna el ScriptableObject en el Inspector

    private int currentHealth;
    private Animator animator;

    void Awake()
    {
        currentHealth = stats.vidaBase;
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Propiedad para obtener la salud actual del enemigo.
    /// </summary>
    public int CurrentHealth
    {
        get { return currentHealth; }
    }

    /// <summary>
    /// Propiedad para obtener la salud máxima del enemigo.
    /// </summary>
    public int MaxHealth
    {
        get { return stats.vidaBase; }
    }

    /// <summary>
    /// Propiedad para obtener el daño del enemigo. (Ajusta el cálculo según tus necesidades)
    /// </summary>
    public int DamageValue
    {
        get { return stats.ataqueBase; }
    }

    /// <summary>
    /// Aplica daño al enemigo y activa la animación de ataque (trigger "Attack").
    /// </summary>
    public void ReceiveDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("Enemy received " + damage + " damage. Current Health: " + currentHealth);
        
    }
}