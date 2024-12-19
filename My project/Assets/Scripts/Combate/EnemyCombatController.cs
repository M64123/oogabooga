using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class EnemyCombatController : MonoBehaviour
{
    public float attackDamage = 10f;           // Daño por ataque
    public float attackProbability = 0.7f;     // Probabilidad de éxito del ataque
    public Animator animator;                  // Referencia al Animator del enemigo
    public float vidaMaxima = 100f;            // Vida inicial del enemigo
    private float vidaActual;                  // Vida actual del enemigo
    private bool combatStarted = false;        // Bandera para saber si el combate ha iniciado
    private bool hasAttemptedAttack = false;   // Bandera para evitar múltiples intentos por pulso

    void Start()
    {
        vidaActual = vidaMaxima; // Inicializar la vida
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

        // Verificar si quedan dinosaurios en los slots
        if (!Combatgrid.Instance.HasDinosaursInSlots())
        {
            Debug.Log("No hay dinosaurios en los slots. Regresando a la escena de título...");
            ResetGameAndLoadTitle();
            return;
        }

        DinoCombat firstSlotDino = Combatgrid.Instance.GetFirstSlotDino();

        if (firstSlotDino != null)
        {
            hasAttemptedAttack = true; // Marcar que ya se intentó atacar
            AttemptAttack(firstSlotDino);
        }
        else
        {
            Debug.LogWarning("No hay dinosaurio en el primer slot.");
        }

        // Reiniciar la bandera después de un breve intervalo
        Invoke(nameof(ResetAttackFlag), 0.1f);
    }

    private void ResetAttackFlag()
    {
        hasAttemptedAttack = false;
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
    private void ResetGameAndLoadTitle()
    {
        // Eliminar cualquier objeto persistente
        DestroyPersistentObjects();

        // Cargar la escena de título
        SceneManager.LoadScene("Titulo");
    }

    private void DestroyPersistentObjects()
    {
        // Destruir cualquier objeto marcado como DontDestroyOnLoad
        GameObject[] persistentObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in persistentObjects)
        {
            if (obj.scene.rootCount == 0) // Si no pertenece a la escena actual
            {
                Destroy(obj);
            }
        }

        Debug.Log("Todos los objetos persistentes han sido eliminados.");
    }

    private void HandleDeath()
    {
        Debug.Log("El enemigo ha sido derrotado.");
        Destroy(gameObject); // Eliminar al enemigo
        ReturnToBoard();
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
    void ReturnToBoard()
    {
        SceneManager.LoadScene("Tablero");
    }
}