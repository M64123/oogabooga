using UnityEngine;
using System.Collections;

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
    /// Propiedad para obtener el daño del enemigo.
    /// </summary>
    public int DamageValue
    {
        get { return stats.ataqueBase; }
    }

    /// <summary>
    /// Aplica daño al enemigo, actualiza la salud y, si ésta llega a 0,
    /// espera un breve período para que el HUD muestre la salud en 0 y se reproduzca la animación de muerte, antes de destruir el objeto.
    /// </summary>
    public void ReceiveDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("Enemy received " + damage + " damage. Current Health: " + currentHealth);

        // Aquí se asume que el HUD se actualiza automáticamente leyendo la propiedad CurrentHealth.

        if (currentHealth <= 0)
        {
            // Opcional: activar animación de muerte
            if (animator != null)
            {
                animator.SetTrigger("Die");
            }
            // Inicia una coroutine para esperar y luego destruir el objeto.
            StartCoroutine(DieAfterDelay());
        }
    }

    private IEnumerator DieAfterDelay()
    {
        // Espera un segundo (o el tiempo que desees) para que el HUD se actualice y se muestre la animación.
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
}