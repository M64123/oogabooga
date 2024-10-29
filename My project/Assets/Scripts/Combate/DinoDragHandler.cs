// DinoDragHandler.cs

using UnityEngine;
using System.Collections;

public class DinoDragHandler : MonoBehaviour
{
    private bool isDragging = false;
    private bool isTransitioning = false;
    private bool isMovingToCursor = false;

    private Camera mainCamera;
    public CameraController cameraController;

    // Referencias
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

    // Alturas de las áreas
    public float boxAreaY = 0f;
    public float arenaY = 10f;

    // Offset en Y del dinosaurio sobre el cursor
    public float dinoYOffset = 1f;

    // Variables para el smooth transition entre planos
    private float currentPlaneY;
    private float targetPlaneY;
    public float planeYTransitionSpeed = 5f;

    // Bounds de las áreas
    public Collider boxAreaCollider;
    public Collider arenaCollider;

    // Offset respecto a la cámara durante la transición
    public Vector3 cameraDinoOffset = new Vector3(0f, 0f, 2f);

    // Duración de la transición del dinosaurio hacia el offset de la cámara
    public float dinoTransitionDuration = 1f;

    // Duración de la transición suave hacia el cursor
    public float dinoToCursorTransitionDuration = 0.5f;

    // Variables internas
    private Vector3 transitionStartPosition;
    private float transitionElapsedTime = 0f;

    void Start()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        idleMovementScript = GetComponent<UnitIdleMovement>();

        // Guardar la posición original
        originalPosition = transform.position;

        // Obtener referencia al controlador de cámara si no está asignado
        if (cameraController == null)
        {
            cameraController = mainCamera.GetComponent<CameraController>();
        }

        // Suscribirse a los eventos de transición de la cámara
        cameraController.OnTransitionStart += OnCameraTransitionStart;
        cameraController.OnTransitionEnd += OnCameraTransitionEnd;

        // Inicializar currentPlaneY
        currentPlaneY = transform.position.y - dinoYOffset;
        targetPlaneY = currentPlaneY;
    }

    void Update()
    {
        if (isDragging)
        {
            if (isTransitioning)
            {
                // Durante la transición, mover el dinosaurio hacia el offset de la cámara suavemente
                transitionElapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(transitionElapsedTime / dinoTransitionDuration);
                Vector3 targetPosition = mainCamera.transform.position + mainCamera.transform.rotation * cameraDinoOffset;
                transform.position = Vector3.Lerp(transitionStartPosition, targetPosition, t);

                // Sincronizar la rotación con la cámara en el eje X
                SyncRotationWithCamera();
            }
            else if (isMovingToCursor)
            {
                // Transición suave hacia la posición bajo el cursor
                transitionElapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(transitionElapsedTime / dinoToCursorTransitionDuration);

                // Obtener la posición objetivo bajo el cursor
                Vector3 targetPosition = GetCursorPositionOnPlane() + new Vector3(0f, dinoYOffset, 0f);

                // Limitar la posición del dinosaurio dentro de los límites del área actual
                targetPosition = ClampPositionToCurrentArea(targetPosition);

                // Mover el dinosaurio suavemente hacia la posición objetivo
                transform.position = Vector3.Lerp(transitionStartPosition, targetPosition, t);

                // Sincronizar la rotación con la cámara en el eje X
                SyncRotationWithCamera();

                if (t >= 1f)
                {
                    isMovingToCursor = false;
                }
            }
            else
            {
                UpdatePlaneY();

                FollowCursor();

                // Sincronizar la rotación con la cámara en el eje X
                SyncRotationWithCamera();

                // Detectar clic para intentar soltar el dinosaurio
                if (Input.GetMouseButtonDown(0))
                {
                    TryDropDino();
                }
            }
        }
    }

    void OnDestroy()
    {
        // Desuscribirse de los eventos
        if (cameraController != null)
        {
            cameraController.OnTransitionStart -= OnCameraTransitionStart;
            cameraController.OnTransitionEnd -= OnCameraTransitionEnd;
        }
    }

    void OnMouseDown()
    {
        // Verificar si el dinosaurio es un Ally
        if (!CompareTag(allyTag))
            return;

        if (!isDragging && !isTransitioning && !isMovingToCursor)
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
    }

    void UpdatePlaneY()
    {
        float detectedPlaneY = GetCurrentAreaY();

        if (Mathf.Abs(detectedPlaneY - targetPlaneY) > 0.01f)
        {
            // Cambiar el targetPlaneY
            targetPlaneY = detectedPlaneY;
        }

        // Suavizar la transición de currentPlaneY hacia targetPlaneY
        currentPlaneY = Mathf.MoveTowards(currentPlaneY, targetPlaneY, planeYTransitionSpeed * Time.deltaTime);
    }

    void FollowCursor()
    {
        Vector3 cursorPosition = GetCursorPositionOnPlane();

        // Posicionar el dinosaurio con un offset en Y sobre el cursor
        Vector3 dinoPosition = new Vector3(cursorPosition.x, currentPlaneY + dinoYOffset, cursorPosition.z);

        // Limitar la posición del dinosaurio dentro de los límites del área actual
        dinoPosition = ClampPositionToCurrentArea(dinoPosition);

        transform.position = dinoPosition;
    }

    Vector3 GetCursorPositionOnPlane()
    {
        // Crear un plano horizontal en la altura currentPlaneY
        Plane plane = new Plane(Vector3.up, new Vector3(0f, currentPlaneY, 0f));

        // Hacer un Raycast desde la cámara al cursor
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        // Si no se puede obtener la posición, devolver la posición actual
        return transform.position;
    }

    Vector3 ClampPositionToCurrentArea(Vector3 position)
    {
        Bounds currentBounds = GetCurrentAreaBounds();

        // En la Arena, limitar a X negativa
        if (IsInArena())
        {
            position.x = Mathf.Clamp(position.x, currentBounds.min.x, 0f);
        }
        else
        {
            position.x = Mathf.Clamp(position.x, currentBounds.min.x, currentBounds.max.x);
        }

        position.z = Mathf.Clamp(position.z, currentBounds.min.z, currentBounds.max.z);
        return position;
    }

    Bounds GetCurrentAreaBounds()
    {
        if (Mathf.Abs(currentPlaneY - boxAreaY) < Mathf.Abs(currentPlaneY - arenaY))
        {
            // Estamos en el BoxArea
            return boxAreaCollider.bounds;
        }
        else
        {
            // Estamos en la Arena
            return arenaCollider.bounds;
        }
    }

    float GetCurrentAreaY()
    {
        // Hacer un Raycast desde el cursor para detectar sobre qué área está
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, arenaLayerMask))
        {
            return arenaY;
        }
        else if (Physics.Raycast(ray, out hit, Mathf.Infinity, boxAreaLayerMask))
        {
            return boxAreaY;
        }
        else
        {
            // Si no está sobre ninguna área, mantener el targetPlaneY actual
            return targetPlaneY;
        }
    }

    void SyncRotationWithCamera()
    {
        // Obtener la rotación en X de la cámara
        float cameraRotationX = mainCamera.transform.eulerAngles.x;

        // Aplicar la rotación al dinosaurio
        transform.rotation = Quaternion.Euler(cameraRotationX, 0f, 0f);
    }

    void TryDropDino()
    {
        // Intentar soltar en el Box Area
        if (IsPointerOverLayer(boxAreaLayerMask))
        {
            StopDragging(true, boxAreaY);
            return;
        }

        // Intentar soltar en la Arena
        if (IsPointerOverLayer(arenaLayerMask))
        {
            // Verificar si la posición está en el lado del jugador (X negativa)
            if (transform.position.x <= 0)
            {
                StopDragging(false, arenaY);
                return;
            }
            else
            {
                // La posición está en el lado del enemigo, no permitido
                ReturnToOriginalPosition();
                isDragging = false;
                return;
            }
        }

        // Si no se soltó en una zona válida, continuar arrastrando
        // Opcionalmente, puedes mostrar feedback al usuario
    }

    void StopDragging(bool inBoxArea, float groundY)
    {
        isDragging = false;

        // Reactivar Rigidbody y colisiones
        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.detectCollisions = true;
        }

        // Ajustar la posición en Y para que los pies toquen el suelo
        AdjustPositionToGround(groundY);

        if (inBoxArea)
        {
            // Reactivar el movimiento aleatorio
            if (idleMovementScript != null)
            {
                idleMovementScript.enabled = true;
            }
        }
        else
        {
            // Desactivar el movimiento aleatorio
            if (idleMovementScript != null)
            {
                idleMovementScript.enabled = false;
            }
        }

        // Iniciar rotación suave a X=0
        StartCoroutine(SmoothRotateTo(0f));
    }

    bool IsPointerOverLayer(LayerMask layerMask)
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, Mathf.Infinity, layerMask);
    }

    void ReturnToOriginalPosition()
    {
        // Regresar el dinosaurio a su posición original
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

        // Iniciar rotación suave a X=0
        StartCoroutine(SmoothRotateTo(0f));
    }

    void AdjustPositionToGround(float groundY)
    {
        // Ajustar la posición en Y para que los pies toquen el suelo
        // Obtener el tamaño del dinosaurio en Y (altura)
        float dinoHeight = GetComponent<Renderer>().bounds.size.y;

        // Calcular la posición en Y donde los pies tocan el suelo
        float footPositionY = groundY + (dinoHeight / 2f);

        // Establecer la posición en Y del dinosaurio
        transform.position = new Vector3(transform.position.x, footPositionY, transform.position.z);
    }

    IEnumerator SmoothRotateTo(float targetXRotation)
    {
        float duration = 0.5f; // Duración de la rotación suave
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

    void OnCameraTransitionStart()
    {
        if (isDragging)
        {
            isTransitioning = true;
            transitionStartPosition = transform.position;
            transitionElapsedTime = 0f;
        }
    }

    void OnCameraTransitionEnd()
    {
        if (isDragging)
        {
            isTransitioning = false;
            // Actualizar la referencia de la cámara principal
            mainCamera = Camera.main;

            // Determinar el área actual basado en la posición de la cámara
            if (cameraController.IsInPosition1)
            {
                // Estamos en el BoxArea
                targetPlaneY = boxAreaY;
            }
            else
            {
                // Estamos en la Arena
                targetPlaneY = arenaY;
            }
            currentPlaneY = targetPlaneY;

            // Iniciar transición suave hacia la posición bajo el cursor
            isMovingToCursor = true;
            transitionStartPosition = transform.position;
            transitionElapsedTime = 0f;
        }
    }

    bool IsInArena()
    {
        return Mathf.Abs(currentPlaneY - arenaY) < Mathf.Abs(currentPlaneY - boxAreaY);
    }
}
