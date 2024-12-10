using UnityEngine;

public class UnitIdleMovement : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float minIdleTime = 1f;
    public float maxIdleTime = 3f;
    public float minMoveTime = 1f;
    public float maxMoveTime = 2f;
    public Transform boxAreaTransform;

    private float idleTimer = 0f;
    private float moveTimer = 0f;
    private bool isMoving = false;
    private Vector3 targetPosition;
    private Animator animator;
    private Bounds boxAreaBounds;

    void Start()
    {
        animator = GetComponent<Animator>();

        Collider boxCollider = boxAreaTransform.GetComponent<Collider>();
        if (boxCollider != null)
        {
            boxAreaBounds = boxCollider.bounds;
        }
        else
        {
            Debug.LogError("El boxArea no tiene un Collider.");
        }

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

        float xPos = Random.Range(boxAreaBounds.min.x, boxAreaBounds.max.x);
        float zPos = Random.Range(boxAreaBounds.min.z, boxAreaBounds.max.z);
        float yPos = transform.position.y;

        targetPosition = new Vector3(xPos, yPos, zPos);

        if (animator != null)
        {
            animator.SetBool("isMoving", true);
        }
    }

    void MoveTowardsTarget()
    {
        moveTimer -= Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (moveTimer <= 0f || Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            isMoving = false;
            idleTimer = Random.Range(minIdleTime, maxIdleTime);

            if (animator != null)
            {
                animator.SetBool("isMoving", false);
            }
        }
    }
}
