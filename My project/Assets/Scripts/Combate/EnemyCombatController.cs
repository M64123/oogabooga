using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class EnemyCombatController : MonoBehaviour
{
    [Header("Configuraciones de Ataque")]
    public float attackDamage = 10f;           // Da�o por ataque
    [Range(0f, 1f)]
    public float attackProbability = 0.7f;     // Probabilidad de �xito del ataque

    [Header("Referencias")]
    public Animator animator;                  // Referencia al Animator del enemigo
    public Combatgrid combatGrid;              // Referencia al Combatgrid
    public BeatManagerCombat beatManagerCombat; // Referencia al BeatManagerCombat

    [Header("Configuraciones de Vida")]
    public float vidaMaxima = 100f;            // Vida inicial del enemigo
    private float vidaActual;                  // Vida actual del enemigo

    private bool combatStarted = false;        // Bandera para saber si el combate ha iniciado
    private bool hasAttemptedAttack = false;   // Bandera para evitar m�ltiples intentos por pulso

    void Start()
    {
        vidaActual = vidaMaxima; // Inicializar la vida

        // Verificar que las referencias est�n asignadas
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
    /// Inicia el combate y suscribe el m�todo OnBeat a los eventos de intervalo.
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
            Debug.LogError("BeatManagerCombat no est� asignado. El enemigo no podr� atacar.");
        }
    }

    /// <summary>
    /// M�todo llamado en cada intervalo del BeatManagerCombat.
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
            Debug.Log("No hay dinosaurios en los slots. Regresando a la escena de t�tulo...");
            ResetGameAndLoadTitle();
            return;
        }

        // Obtener el dinosaurio en el primer slot
        DinoCombat firstSlotDino = combatGrid != null ? combatGrid.GetFirstSlotDino() : null;

        if (firstSlotDino != null)
        {
            hasAttemptedAttack = true; // Marcar que ya se intent� atacar
            AttemptAttack(firstSlotDino);
        }
        else
        {
            Debug.LogWarning("No hay dinosaurio en el primer slot.");
        }

        // Reiniciar la bandera despu�s de un breve intervalo
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
            Debug.Log("El enemigo fall� el ataque.");
            return;
        }

        // Activar animaci�n de ataque
        if (animator != null)
        {
            animator.SetTrigger("Attack");
            Debug.Log("Animaci�n de ataque activada.");
        }
        else
        {
            Debug.LogWarning("Animator no asignado. No se puede reproducir la animaci�n de ataque.");
        }

        // Aplicar da�o al dinosaurio aliado
        if (target != null)
        {
            Debug.Log($"El enemigo atac� a {target.name} causando {attackDamage} de da�o.");
            target.TakeDamage(attackDamage);
        }
        else
        {
            Debug.LogWarning("El objetivo del ataque es nulo. No se puede aplicar da�o.");
        }
    }

    /// <summary>
    /// M�todo para que el enemigo reciba da�o.
    /// </summary>
    /// <param name="damage">Cantidad de da�o recibido.</param>
    public void TakeDamage(float damage)
    {
        vidaActual -= damage;
        Debug.Log($"El enemigo recibi� {damage} de da�o. Vida restante: {vidaActual}");

        if (vidaActual <= 0)
        {
            HandleDeath();
        }
    }

    /// <summary>
    /// Resetea el juego y carga la escena de t�tulo.
    /// </summary>
    private void ResetGameAndLoadTitle()
    {
        // Eliminar cualquier objeto persistente
        DestroyPersistentObjects();

        // Cargar la escena de t�tulo
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
