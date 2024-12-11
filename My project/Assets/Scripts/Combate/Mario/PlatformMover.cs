using UnityEngine;
using UnityEngine.UI;

public class PlatformMover : MonoBehaviour
{
    public Transform platform;            // La plataforma principal (padre)
    public Transform enemyDino;           // El dinosaurio enemigo
    public float platformSpeed = 2.0f;    // Velocidad de la plataforma
    public float backgroundSpeedFactor = 0.5f; // Factor de velocidad para el fondo
    public Transform[] backgrounds;       // Elementos del fondo
    public Button startButton;            // Botón que inicia el movimiento
    public Combatgrid combatGrid;         // Referencia a la grid de combate
    public Canvas combatCanvas;           // Canvas que aparece tras la colisión
    public float stopDistance = 1.0f;     // Distancia mínima para detenerse

    private bool isMoving = false;        // Indica si la plataforma está en movimiento

    private void Start()
    {
        // Configurar el botón para iniciar el movimiento
        startButton.onClick.AddListener(OnStartButtonClick);
        startButton.gameObject.SetActive(false); // Ocultar el botón al inicio
    }

    private void Update()
    {
        // Verificar si hay al menos un slot cubierto para mostrar el botón
        if (combatGrid.NumberOfOccupiedSlots() > 0 && !isMoving)
        {
            startButton.gameObject.SetActive(true);
        }
        else
        {
            startButton.gameObject.SetActive(false);
        }

        // Mover la plataforma y el fondo si está en movimiento
        if (isMoving)
        {
            MovePlatformAndBackgrounds();
        }
    }

    private void OnStartButtonClick()
    {
        // Reorganizar los dinosaurios en los primeros slots
        combatGrid.ReorganizeSlots();

        // Iniciar el movimiento
        isMoving = true;

        // Notificar a los aliados y enemigos que el combate ha iniciado
        NotifyCombatStart();

        // Ocultar el botón de inicio
        startButton.gameObject.SetActive(false);
    }

    private void MovePlatformAndBackgrounds()
    {
        // Calcular el movimiento de la plataforma
        Vector3 platformMovement = Vector3.left * platformSpeed * Time.deltaTime;

        // Mover la plataforma junto con sus hijos
        platform.position += platformMovement;

        // Mover los fondos a menor velocidad
        foreach (Transform background in backgrounds)
        {
            background.position += platformMovement * backgroundSpeedFactor;
        }

        // Detener el movimiento cuando el primer dino alcance al enemigo
        Transform firstDino = GetFirstDinoOnPlatform();
        if (firstDino != null && Vector3.Distance(firstDino.position, enemyDino.position) <= stopDistance)
        {
            StopMovement();
        }
    }

    private Transform GetFirstDinoOnPlatform()
    {
        // Obtener el primer dinosaurio en los slots de la grid
        foreach (Transform slot in combatGrid.GetGridSlots())
        {
            if (slot.childCount > 0) // Si el slot tiene un dino
            {
                return slot.GetChild(0);
            }
        }
        return null; // No hay dinos en la plataforma
    }

    private void StopMovement()
    {
        isMoving = false;

        // Mostrar el canvas de combate
        combatCanvas.gameObject.SetActive(true);

        Debug.Log("Plataforma detenida: el primer dinosaurio alcanzó al enemigo.");
    }

    private void NotifyCombatStart()
    {
        // Notificar a todos los aliados
        GameObject[] allies = GameObject.FindGameObjectsWithTag("ally");
        foreach (GameObject ally in allies)
        {
            DinoCombat dinoCombat = ally.GetComponent<DinoCombat>();
            if (dinoCombat != null)
            {
                dinoCombat.StartCombat();
            }
        }

        // Notificar al enemigo
        GameObject enemy = GameObject.FindGameObjectWithTag("enemy");
        if (enemy != null)
        {
            EnemyCombatController enemyCombat = enemy.GetComponent<EnemyCombatController>();
            if (enemyCombat != null)
            {
                enemyCombat.StartCombat();
            }
        }
    }
}