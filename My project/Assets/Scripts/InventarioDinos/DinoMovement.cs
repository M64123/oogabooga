using UnityEngine;
using System.Collections;

public class DinoMovement : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float minIdleTime = 1f;
    public float maxIdleTime = 3f;
    public float minMoveTime = 1f;
    public float maxMoveTime = 2f;

    private float idleTimer = 0f;
    private float moveTimer = 0f;
    private bool isMoving = false;
    private Vector3 targetPosition;

    private Bounds movementBounds;

    public void SetMovementBounds(Bounds bounds)
    {
        movementBounds = bounds;
    }

    void Start()
    {
        // Empezar en estado de espera
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
        moveTimer = Random.Range(minMoveTime, maxMoveTime);

        // Generar posición aleatoria dentro de movementBounds
        float xPos = Random.Range(movementBounds.min.x, movementBounds.max.x);
        float zPos = Random.Range(movementBounds.min.z, movementBounds.max.z);
        float yPos = transform.position.y; // Mantener Y

        targetPosition = new Vector3(xPos, yPos, zPos);

        // Opcional: activar animación "isMoving"
    }

    void MoveTowardsTarget()
    {
        moveTimer -= Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (moveTimer <= 0f || Vector3.Distance(transform.position, targetPosition) <= 0.1f)
        {
            isMoving = false;
            idleTimer = Random.Range(minIdleTime, maxIdleTime);

            // Opcional: desactivar animación "isMoving"
        }
    }
}
