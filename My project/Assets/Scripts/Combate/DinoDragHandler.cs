// DinoDragHandler.cs
using UnityEngine;

public class DinoDragHandler : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 offset;
    private Camera mainCamera;

    // Referencia al Rigidbody y Animator (si es necesario)
    private Rigidbody rb;
    private Animator animator;

    // Etiquetas y capas
    public string arenaTag = "Arena";
    public string boxAreaTag = "BoxArea";
    public string allyTag = "Ally";

    // LayerMasks para detección
    public LayerMask arenaLayerMask;
    public LayerMask boxAreaLayerMask;

    // Referencia al script de movimiento aleatorio
    private UnitIdleMovement idleMovementScript;

    // Posición original del dinosaurio
    private Vector3 originalPosition;

    // Altura del suelo (ajusta este valor según tu escena)
    public float groundY = 0f;

    void Start()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        idleMovementScript = GetComponent<UnitIdleMovement>();

        // Guardar la posición original
        originalPosition = transform.position;
    }

    void OnMouseDown()
    {
        // Verificar si el dinosaurio es un Ally
        if (!CompareTag(allyTag))
            return;

        if (!isDragging)
        {
            StartDragging();
        }
    }

    void OnMouseDrag()
    {
        if (isDragging)
        {
            FollowCursor();
        }
    }

    void OnMouseUp()
    {
        if (isDragging)
        {
            TryDropDino();
        }
    }

    void StartDragging()
    {
        isDragging = true;

        // Detener el movimiento del dinosaurio
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.detectCollisions = false; // Desactivar colisiones
        }

        // Desactivar el movimiento aleatorio si está activo
        if (idleMovementScript != null)
        {
            idleMovementScript.enabled = false;
        }

        // Si tiene animación de movimiento, detenerla
        if (animator != null)
        {
            animator.SetBool("isMoving", false);
        }

        // Calcular el offset entre el cursor y el dinosaurio
        Vector3 worldPosition = GetWorldPositionOnPlane(Input.mousePosition, groundY);
        offset = transform.position - worldPosition;
    }

    void FollowCursor()
    {
        Vector3 worldPosition = GetWorldPositionOnPlane(Input.mousePosition, groundY);
        transform.position = worldPosition + offset;
    }

    void TryDropDino()
    {
        isDragging = false;

        // Reactivar Rigidbody y colisiones
        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.detectCollisions = true;
        }

        // Intentar soltar en el Box Area
        if (IsPointerOverLayer(boxAreaLayerMask))
        {
            // Reactivar el movimiento aleatorio
            if (idleMovementScript != null)
            {
                idleMovementScript.enabled = true;
            }

            // Asegurar que los "pies" toquen el suelo
            AdjustPositionToGround();

            return;
        }

        // Intentar soltar en la Arena
        if (IsPointerOverLayer(arenaLayerMask))
        {
            // Obtener la posición actual del mouse en el plano
            Vector3 worldPosition = GetWorldPositionOnPlane(Input.mousePosition, groundY) + offset;

            // Verificar si la posición está en el lado del jugador (X negativa)
            if (worldPosition.x <= 0)
            {
                // Soltar el dinosaurio en la posición actual
                transform.position = new Vector3(worldPosition.x, groundY, worldPosition.z);

                // Desactivar el movimiento aleatorio
                if (idleMovementScript != null)
                {
                    idleMovementScript.enabled = false;
                }

                // Asegurar que los "pies" toquen el suelo
                AdjustPositionToGround();

                return;
            }
            else
            {
                // La posición está en el lado del enemigo, no permitido
                ReturnToOriginalPosition();
                return;
            }
        }

        // Si no se soltó en una zona válida, regresar a la posición original
        ReturnToOriginalPosition();
    }

    void ReturnToOriginalPosition()
    {
        // Regresar el dinosaurio a su posición original
        transform.position = new Vector3(originalPosition.x, groundY, originalPosition.z);

        // Desactivar el movimiento aleatorio
        if (idleMovementScript != null)
        {
            idleMovementScript.enabled = false;
        }

        // Si tiene animación de movimiento, ajustarla si es necesario
        if (animator != null)
        {
            animator.SetBool("isMoving", false);
        }
    }

    Vector3 GetWorldPositionOnPlane(Vector3 screenPosition, float y)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        Plane plane = new Plane(Vector3.up, y);
        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }

    bool IsPointerOverLayer(LayerMask layerMask)
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            return true;
        }
        return false;
    }

    void AdjustPositionToGround()
    {
        // Hacer un Raycast hacia abajo para ajustar la posición en Y
        Ray ray = new Ray(transform.position + Vector3.up * 1f, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 2f))
        {
            transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
        }
        else
        {
            // Si no se detecta el suelo, establecer Y a groundY
            transform.position = new Vector3(transform.position.x, groundY, transform.position.z);
        }
    }
}
