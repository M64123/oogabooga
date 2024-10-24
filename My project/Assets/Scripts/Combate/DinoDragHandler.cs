// DinoDragHandler.cs

using UnityEngine;
using System.Collections;

public class DinoDragHandler : MonoBehaviour
{
    private bool isDragging = false;

    private Camera mainCamera;

    // Referencia al Rigidbody y Animator (si es necesario)
    private Rigidbody rb;
    private Animator animator;

    // Etiquetas y capas
    public string arenaTag = "Arena";
    public string boxAreaTag = "BoxArea";
    public string allyTag = "Ally";

    // LayerMasks para detecci�n
    public LayerMask arenaLayerMask;
    public LayerMask boxAreaLayerMask;

    // Referencia al script de movimiento aleatorio
    private UnitIdleMovement idleMovementScript;

    // Posici�n original del dinosaurio
    private Vector3 originalPosition;

    // Variables para la rotaci�n
    private Coroutine rotationCoroutine;

    // Referencia al controlador de c�mara
    public CameraController cameraController;

    // Offset editable para el dinosaurio respecto a la c�mara
    public Vector3 dinoOffset = new Vector3(0f, 0f, 1f);

    // Altura del suelo (Y) para el Box Area y la Arena
    public float boxAreaY = 0f;
    public float arenaY = 10f;

    void Start()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        idleMovementScript = GetComponent<UnitIdleMovement>();

        // Guardar la posici�n original
        originalPosition = transform.position;

        // Obtener referencia al controlador de c�mara si no est� asignado
        if (cameraController == null)
        {
            cameraController = mainCamera.GetComponent<CameraController>();
        }
    }

    void Update()
    {
        if (isDragging)
        {
            FollowCursor();

            // Sincronizar la rotaci�n con la c�mara en el eje X
            float cameraRotationX = mainCamera.transform.eulerAngles.x;
            transform.rotation = Quaternion.Euler(cameraRotationX, 0f, 0f);

            // Detectar clic para intentar soltar el dinosaurio
            if (Input.GetMouseButtonDown(0))
            {
                TryDropDino();
            }
        }
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

    void StartDragging()
    {
        isDragging = true;

        // Desactivar Rigidbody y colisiones
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        // Desactivar el movimiento aleatorio si est� activo
        if (idleMovementScript != null)
        {
            idleMovementScript.enabled = false;
        }

        // Si tiene animaci�n de movimiento, detenerla
        if (animator != null)
        {
            animator.SetBool("isMoving", false);
        }

        // Iniciar rotaci�n suave para alinearse con la c�mara en el eje X
        float cameraRotationX = mainCamera.transform.eulerAngles.x;
        if (rotationCoroutine != null)
            StopCoroutine(rotationCoroutine);
        rotationCoroutine = StartCoroutine(SmoothRotateTo(cameraRotationX, 0.2f)); // Duraci�n de 0.2 segundos
    }

    void FollowCursor()
    {
        // Obtener la posici�n del cursor en el mundo
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // Utilizar un plano a la altura del dinosaurio
        Plane plane = new Plane(Vector3.up, transform.position);

        if (plane.Raycast(ray, out float distance))
        {
            Vector3 worldPosition = ray.GetPoint(distance);

            // Aplicar offset respecto a la c�mara
            Vector3 offset = mainCamera.transform.rotation * dinoOffset;
            worldPosition += offset;

            // Restringir la posici�n para que no atraviese los planos del Box Area y la Arena
            float minY = Mathf.Min(boxAreaY, arenaY);
            float maxY = Mathf.Max(boxAreaY, arenaY);
            worldPosition.y = Mathf.Clamp(worldPosition.y, minY, maxY);

            transform.position = worldPosition;
        }
    }

    void TryDropDino()
    {
        // Intentar soltar en el Box Area
        if (IsPointerOverLayer(boxAreaLayerMask))
        {
            isDragging = false;

            // Reactivar Rigidbody y colisiones
            if (rb != null)
            {
                rb.useGravity = true;
                rb.isKinematic = false;
                rb.detectCollisions = true;
            }

            // Reactivar el movimiento aleatorio
            if (idleMovementScript != null)
            {
                idleMovementScript.enabled = true;
            }

            // Ajustar la posici�n en Y para que los pies toquen el suelo
            AdjustPositionToGround(boxAreaY);

            // Iniciar rotaci�n suave a X=0
            if (rotationCoroutine != null)
                StopCoroutine(rotationCoroutine);
            rotationCoroutine = StartCoroutine(SmoothRotateTo(0f, 0.5f)); // Duraci�n de 0.5 segundos

            return;
        }

        // Intentar soltar en la Arena
        if (IsPointerOverLayer(arenaLayerMask))
        {
            // Verificar si la posici�n est� en el lado del jugador (X negativa)
            if (transform.position.x <= 0)
            {
                isDragging = false;

                // Ajustar la posici�n en Y para que los pies toquen el suelo
                AdjustPositionToGround(arenaY);

                // Reactivar Rigidbody y colisiones
                if (rb != null)
                {
                    rb.useGravity = true;
                    rb.isKinematic = false;
                    rb.detectCollisions = true;
                }

                // Desactivar el movimiento aleatorio
                if (idleMovementScript != null)
                {
                    idleMovementScript.enabled = false;
                }

                // Iniciar rotaci�n suave a X=0
                if (rotationCoroutine != null)
                    StopCoroutine(rotationCoroutine);
                rotationCoroutine = StartCoroutine(SmoothRotateTo(0f, 0.5f)); // Duraci�n de 0.5 segundos

                return;
            }
            else
            {
                // La posici�n est� en el lado del enemigo, no permitido
                ReturnToOriginalPosition();
                isDragging = false;
                return;
            }
        }

        // Si no se solt� en una zona v�lida, continuar arrastrando
        // Opcionalmente, puedes mostrar feedback al usuario
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

    void ReturnToOriginalPosition()
    {
        // Regresar el dinosaurio a su posici�n original
        transform.position = originalPosition;

        // Reactivar Rigidbody y colisiones
        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.detectCollisions = true;
        }

        // Desactivar el movimiento aleatorio
        if (idleMovementScript != null)
        {
            idleMovementScript.enabled = false;
        }

        // Iniciar rotaci�n suave a X=0
        if (rotationCoroutine != null)
            StopCoroutine(rotationCoroutine);
        rotationCoroutine = StartCoroutine(SmoothRotateTo(0f, 0.5f)); // Duraci�n de 0.5 segundos
    }

    void AdjustPositionToGround(float groundY)
    {
        // Ajustar la posici�n en Y para que los pies toquen el suelo
        // Obtener el tama�o del dinosaurio en Y (altura)
        float dinoHeight = GetComponent<Renderer>().bounds.size.y;

        // Calcular la posici�n en Y donde los pies tocan el suelo
        float footPositionY = groundY + (dinoHeight / 2f);

        // Establecer la posici�n en Y del dinosaurio
        transform.position = new Vector3(transform.position.x, footPositionY, transform.position.z);
    }

    IEnumerator SmoothRotateTo(float targetXRotation, float duration)
    {
        float elapsedTime = 0f;

        Quaternion startingRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(targetXRotation, 0f, 0f);

        while (elapsedTime < duration)
        {
            transform.rotation = Quaternion.Slerp(startingRotation, targetRotation, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation;
    }
}
