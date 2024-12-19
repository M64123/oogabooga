using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class EnemyCombatController : MonoBehaviour
{
    [Header("Configuraciones de Ataque")]
    public float attackDamage = 10f;           // Daño por ataque
    [Range(0f, 1f)]
    public float attackProbability = 0.7f;     // Probabilidad de éxito del ataque

    [Header("Referencias")]
    public Animator animator;                  // Referencia al Animator del enemigo
    public Combatgrid combatGrid;              // Referencia al Combatgrid
    public BeatManagerCombat beatManagerCombat; // Referencia al BeatManagerCombat

    [Header("Configuraciones de Vida")]
    public float vidaMaxima = 100f;            // Vida inicial del enemigo
    private float vidaActual;                  // Vida actual del enemigo

    private bool combatStarted = false;        // Bandera para saber si el combate ha iniciado
    private bool hasAttemptedAttack = false;   // Bandera para evitar múltiples intentos por pulso

    void Start()
    {
        vidaActual = vidaMaxima; // Inicializar la vida

        // Verificar que las referencias estén asignadas
        if (combatGrid == null)
        {
            combatGrid = FindObjectOfType<Combatgrid>();
            if (combatGrid == null)
            {
                Debug.LogError("Combatgrid no asignado ni encontrado en la escena.");
            }
        }

        if (beatManagerCombat == null)
        {
            beatManagerCombat = FindObjectOfType<BeatManagerCombat>();
            if (beatManagerCombat == null)
            {
                Debug.LogError("BeatManagerCombat no asignado ni encontrado en la escena.");
            }
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError("Animator no asignado ni encontrado en el GameObject del enemigo.");
            }
        }
    }

    /// <summary>
    /// Inicia el combate y suscribe el método OnBeat a los eventos de intervalo.
    /// </summary>
    public void StartCombat()
    {
        if (combatStarted)
        {
            Debug.LogWarning("El combate ya ha sido iniciado.");
            return;
        }

        combatStarted = true;
        Debug.Log("El combate ha comenzado.");

        // Subscribirse al evento de intervalo del BeatManagerCombat
        if (beatManagerCombat != null)
        {
            foreach (var interval in beatManagerCombat.intervals)
            {
                interval.onIntervalTrigger.AddListener(OnBeat);
            }
        }
        else
        {
            Debug.LogError("BeatManagerCombat no está asignado. El enemigo no podrá atacar.");
        }
    }

    /// <summary>
    /// Método llamado en cada intervalo del BeatManagerCombat.
    /// </summary>
    private void OnBeat()
    {
        if (!combatStarted)
        {
            Debug.LogWarning("OnBeat llamado pero el combate no ha iniciado.");
            return;
        }

        if (hasAttemptedAttack)
        {
            Debug.Log("Ya se ha intentado un ataque en este intervalo.");
            return;
        }

        // Verificar si quedan dinosaurios en los slots
        if (combatGrid != null && !combatGrid.HasDinosaursInSlots())
        {
            Debug.Log("No hay dinosaurios en los slots. Regresando a la escena de título...");
            ResetGameAndLoadTitle();
            return;
        }

        // Obtener el dinosaurio en el primer slot
        DinoCombat firstSlotDino = combatGrid != null ? combatGrid.GetFirstSlotDino() : null;

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

    /// <summary>
    /// Reinicia la bandera que permite intentar un nuevo ataque.
    /// </summary>
    private void ResetAttackFlag()
    {
        hasAttemptedAttack = false;
    }

    /// <summary>
    /// Intenta realizar un ataque al dinosaurio objetivo.
    /// </summary>
    /// <param name="target">El dinosaurio objetivo del ataque.</param>
    private void AttemptAttack(DinoCombat target)
    {
        if (Random.value > attackProbability)
        {
            Debug.Log("El enemigo falló el ataque.");
            return;
        }

        // Activar animación de ataque
        if (animator != null)
        {
            animator.SetTrigger("Attack");
            Debug.Log("Animación de ataque activada.");
        }
        else
        {
            Debug.LogWarning("Animator no asignado. No se puede reproducir la animación de ataque.");
        }

        // Aplicar daño al dinosaurio aliado
        if (target != null)
        {
            Debug.Log($"El enemigo atacó a {target.name} causando {attackDamage} de daño.");
            target.TakeDamage(attackDamage);
        }
        else
        {
            Debug.LogWarning("El objetivo del ataque es nulo. No se puede aplicar daño.");
        }
    }

    /// <summary>
    /// Método para que el enemigo reciba daño.
    /// </summary>
    /// <param name="damage">Cantidad de daño recibido.</param>
    public void TakeDamage(float damage)
    {
        vidaActual -= damage;
        Debug.Log($"El enemigo recibió {damage} de daño. Vida restante: {vidaActual}");

        if (vidaActual <= 0)
        {
            HandleDeath();
        }
    }

    /// <summary>
    /// Resetea el juego y carga la escena de título.
    /// </summary>
    private void ResetGameAndLoadTitle()
    {
        // Eliminar cualquier objeto persistente
        DestroyPersistentObjects();

        // Cargar la escena de título
        SceneManager.LoadScene("Titulo");
    }

    /// <summary>
    /// Destruye cualquier objeto marcado como DontDestroyOnLoad.
    /// </summary>
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

    /// <summary>
    /// Maneja la muerte del enemigo.
    /// </summary>
    private void HandleDeath()
    {
        Debug.Log("El enemigo ha sido derrotado.");
        Destroy(gameObject); // Eliminar al enemigo
        ReturnToBoard();
    }

    /// <summary>
    /// Desuscribe de los eventos al destruirse el enemigo.
    /// </summary>
    private void OnDestroy()
    {
        if (beatManagerCombat != null)
        {
            foreach (var interval in beatManagerCombat.intervals)
            {
                interval.onIntervalTrigger.RemoveListener(OnBeat);
            }
        }
    }

    /// <summary>
    /// Retorna a la escena del tablero.
    /// </summary>
    void ReturnToBoard()
    {
        SceneManager.LoadScene("Tablero");
    }
}
