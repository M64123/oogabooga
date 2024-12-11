using UnityEngine;
using System.Collections;

public class EnemyCombatController : MonoBehaviour
{
    public float attackDamage = 10f; // Daño por ataque
    public float attackProbability = 0.7f; // Probabilidad de éxito del ataque
    public Animator animator; // Referencia al Animator del enemigo
    public float vidaMaxima = 100f; // Vida inicial del enemigo
    private float vidaActual; // Vida actual del enemigo
    private bool combatStarted = false; // Bandera para saber si el combate ha iniciado
    private bool hasAttemptedAttack = false; // Bandera para evitar múltiples intentos por pulso

    void Start()
    {
        vidaActual = vidaMaxima; // Inicializar la vida al iniciar
    }

    public void StartCombat()
    {
        combatStarted = true;

        // Subscribirse al evento de intervalo del BeatManagerCombat
        if (BeatManagerCombat.Instance != null)
        {
            foreach (var interval in BeatManagerCombat.Instance.intervals)
            {
                interval.onIntervalTrigger.AddListener(OnBeat);
            }
        }
        else
        {
            Debug.LogWarning("No se encontró el BeatManagerCombat en la escena.");
        }
    }

    private void OnBeat()
    {
        if (!combatStarted || hasAttemptedAttack) return;

        DinoCombat nearestDino = FindNearestDino();
        if (nearestDino != null)
        {
            hasAttemptedAttack = true; // Marcar que ya se intentó atacar en este pulso
            AttemptAttack(nearestDino);
        }
        else
        {
            Debug.LogWarning("No se encontraron dinosaurios aliados cercanos.");
        }

        // Reiniciar la bandera después de un breve intervalo
        Invoke(nameof(ResetAttackFlag), 0.1f);
    }

    private void ResetAttackFlag()
    {
        hasAttemptedAttack = false;
    }

    private DinoCombat FindNearestDino()
    {
        GameObject[] dinos = GameObject.FindGameObjectsWithTag("ally");
        GameObject nearestDino = null;
        float shortestDistance = float.MaxValue;

        foreach (GameObject dino in dinos)
        {
            float distance = Vector3.Distance(transform.position, dino.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestDino = dino;
            }
        }

        return nearestDino != null ? nearestDino.GetComponent<DinoCombat>() : null;
    }

    private void AttemptAttack(DinoCombat target)
    {
        if (Random.value > attackProbability)
        {
            Debug.Log("El enemigo falló el ataque.");
        }
        else
        {
            // Activar animación de ataque
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }

            // Aplicar daño al dinosaurio aliado
            Debug.Log($"El enemigo atacó a {target.name} causando {attackDamage} de daño.");
            target.TakeDamage(attackDamage);
        }
    }

    public void TakeDamage(float damage)
    {
        vidaActual -= damage;
        Debug.Log($"El enemigo recibió {damage} de daño. Vida restante: {vidaActual}");

        if (vidaActual <= 0)
        {
            HandleDeath();
        }
    }

    private void HandleDeath()
    {
        Debug.Log("El enemigo ha sido derrotado.");
        Destroy(gameObject); // Eliminar al enemigo
    }

    private void OnDestroy()
    {
        // Desuscribirse de los eventos cuando el enemigo sea destruido
        if (BeatManagerCombat.Instance != null)
        {
            foreach (var interval in BeatManagerCombat.Instance.intervals)
            {
                interval.onIntervalTrigger.RemoveListener(OnBeat);
            }
        }
    }
}