// UnitIdleMovement.cs

using UnityEngine;

public class UnitIdleMovement : MonoBehaviour
{
    public float moveSpeed = 1f;

    public float minIdleTime = 1f; // Tiempo m�nimo de espera
    public float maxIdleTime = 3f; // Tiempo m�ximo de espera

    public float minMoveTime = 1f; // Tiempo m�nimo de movimiento
    public float maxMoveTime = 2f; // Tiempo m�ximo de movimiento

    private float idleTimer = 0f;
    private float moveTimer = 0f;

    private bool isMoving = false;

    private Vector3 targetPosition;

    private Animator animator;

    // Referencia al Box Area para obtener sus l�mites
    public Transform boxAreaTransform;
    private Bounds boxAreaBounds;

    void Start()
    {
        animator = GetComponent<Animator>();

        // Obtener los l�mites del Box Area
        if (boxAreaTransform != null)
        {
            Collider boxCollider = boxAreaTransform.GetComponent<Collider>();
            if (boxCollider != null)
            {
                boxAreaBounds = boxCollider.bounds;
            }
            else
            {
                Debug.LogError("El Box Area no tiene un Collider para obtener sus l�mites.");
            }
        }
        else
        {
            Debug.LogError("No se ha asignado el Box Area en UnitIdleMovement.");
        }

        // Iniciar en estado de espera
        idleTimer = Random.Range(minIdleTime, maxIdleTime);
    }

    void Update()
    {
        if (isMoving)
        {
            MoveTowardsTarget();
        }
        else
        {
            idleTimer -= Time.deltaTime;
            if (idleTimer <= 0f)
            {
                StartMoving();
            }
        }
    }

    void StartMoving()
    {
        isMoving = true;

        // Generar una duraci�n aleatoria para el movimiento
        moveTimer = Random.Range(minMoveTime, maxMoveTime);

        // Generar una posici�n aleatoria dentro de los l�mites del Box Area
        float xPos = Random.Range(boxAreaBounds.min.x, boxAreaBounds.max.x);
        float zPos = Random.Range(boxAreaBounds.min.z, boxAreaBounds.max.z);
        float yPos = transform.position.y; // Mantener la posici�n en Y constante

        targetPosition = new Vector3(xPos, yPos, zPos);

        // Iniciar animaci�n de movimiento si es necesario
        if (animator != null)
        {
            animator.SetBool("isMoving", true);
        }
    }

    void MoveTowardsTarget()
    {
        moveTimer -= Time.deltaTime;

        // Moverse hacia la posici�n objetivo
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (moveTimer <= 0f || Vector3.Distance(transform.position, targetPosition) <= 0.1f)
        {
            // Detener el movimiento
            isMoving = false;

            // Iniciar un nuevo tiempo de espera aleatorio
            idleTimer = Random.Range(minIdleTime, maxIdleTime);

            // Detener animaci�n de movimiento si es necesario
            if (animator != null)
            {
                animator.SetBool("isMoving", false);
            }
        }
    }
}
