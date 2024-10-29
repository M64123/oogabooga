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
    public string allyTag = "Ally";

    // LayerMasks para detección
    public LayerMask arenaLayerMask;   // Agregado
    public LayerMask boxAreaLayerMask; // Agregado
    public LayerMask dinoLayerMask;    // LayerMask para el dinosaurio
    public LayerMask ignoreLayers;     // Capas a ignorar en los raycasts

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

        // Configurar las capas a ignorar en el raycast (BoxArea y Arena)
        ignoreLayers = LayerMask.GetMask("BoxArea", "Arena");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!isDragging && !isTransitioning && !isMovingToCursor)
            {
                // Intentar seleccionar el dinosaurio
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                // Usar todas las capas excepto las ignoradas
                int layerMask = ~ignoreLayers;

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
                {
                    if (hit.collider.gameObject == gameObject && CompareTag(allyTag))
                    {
                        StartDragging();
                    }
                }
            }
            else if (isDragging)
            {
                // Intentar soltar el dinosaurio
                TryDropDino();
            }
        }

        if (isDragging)
        {
            if (isTransitioning)
            {
                // Durante la transición de la cámara
                transitionElapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(transitionElapsedTime / dinoTransitionDuration);
                Vector3 targetPosition = mainCamera.transform.position + mainCamera.transform.rotation * cameraDinoOffset;
                transform.position = Vector3.Lerp(transitionStartPosition, targetPosition, t);

                // Sincronizar la rotación con la cámara
                SyncRotationWithCamera();
            }
            else if (isMovingToCursor)
            {
                // Transición suave hacia la posición bajo el cursor
                transitionElapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(transitionElapsedTime / dinoToCursorTransitionDuration);

                Vector3 targetPosition = GetCursorPositionOnPlane() + new Vector3(0f, dinoYOffset, 0f);
                targetPosition = ClampPositionToCurrentArea(targetPosition);

                transform.position = Vector3.Lerp(transitionStartPosition, targetPosition, t);

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

                SyncRotationWithCamera();
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

        // Iniciar transición suave hacia la posición bajo el cursor
        isMovingToCursor = true;
        transitionStartPosition = transform.position;
        transitionElapsedTime = 0f;
    }

    void UpdatePlaneY()
    {
        float detectedPlaneY = GetCurrentAreaY();

        if (Mathf.Abs(detectedPlaneY - targetPlaneY) > 0.01f)
        {
            targetPlaneY = detectedPlaneY;
        }

        currentPlaneY = Mathf.MoveTowards(currentPlaneY, targetPlaneY, planeYTransitionSpeed * Time.deltaTime);
    }

    void FollowCursor()
    {
        Vector3 cursorPosition = GetCursorPositionOnPlane();
        Vector3 dinoPosition = new Vector3(cursorPosition.x, currentPlaneY + dinoYOffset, cursorPosition.z);
        dinoPosition = ClampPositionToCurrentArea(dinoPosition);
        transform.position = dinoPosition;
    }

    Vector3 GetCursorPositionOnPlane()
    {
        Plane plane = new Plane(Vector3.up, new Vector3(0f, currentPlaneY, 0f));
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
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
        return Mathf.Abs(currentPlaneY - boxAreaY) < Mathf.Abs(currentPlaneY - arenaY)
            ? boxAreaCollider.bounds
            : arenaCollider.bounds;
    }

    float GetCurrentAreaY()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        int layerMask = ~ignoreLayers; // Usar todas las capas excepto las ignoradas

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            if (((1 << hit.collider.gameObject.layer) & arenaLayerMask.value) != 0)
            {
                return arenaY;
            }
            else if (((1 << hit.collider.gameObject.layer) & boxAreaLayerMask.value) != 0)
            {
                return boxAreaY;
            }
        }
        return targetPlaneY;
    }

    void SyncRotationWithCamera()
    {
        float cameraRotationX = mainCamera.transform.eulerAngles.x;
        transform.rotation = Quaternion.Euler(cameraRotationX, 0f, 0f);
    }

    void TryDropDino()
    {
        if (IsPointerOverLayer(boxAreaLayerMask))
        {
            StopDragging(true, boxAreaY);
            return;
        }

        if (IsPointerOverLayer(arenaLayerMask))
        {
            if (transform.position.x <= 0)
            {
                StopDragging(false, arenaY);
                return;
            }
            else
            {
                ReturnToOriginalPosition();
                isDragging = false;
                return;
            }
        }
    }

    void StopDragging(bool inBoxArea, float groundY)
    {
        isDragging = false;

        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.detectCollisions = true;
        }

        AdjustPositionToGround(groundY);

        if (inBoxArea)
        {
            if (idleMovementScript != null)
            {
                idleMovementScript.enabled = true;
            }
        }
        else
        {
            if (idleMovementScript != null)
            {
                idleMovementScript.enabled = false;
            }
        }

        StartCoroutine(SmoothRotateTo(0f));
    }

    bool IsPointerOverLayer(LayerMask layerMask)
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, Mathf.Infinity, layerMask);
    }

    void ReturnToOriginalPosition()
    {
        transform.position = originalPosition;

        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.detectCollisions = true;
        }

        if (idleMovementScript != null)
        {
            idleMovementScript.enabled = false;
        }

        StartCoroutine(SmoothRotateTo(0f));
    }

    void AdjustPositionToGround(float groundY)
    {
        float dinoHeight = GetComponent<Renderer>().bounds.size.y;
        float footPositionY = groundY + (dinoHeight / 2f);
        transform.position = new Vector3(transform.position.x, footPositionY, transform.position.z);
    }

    IEnumerator SmoothRotateTo(float targetXRotation)
    {
        float duration = 0.5f;
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
            mainCamera = Camera.main;

            if (cameraController.IsInPosition1)
            {
                targetPlaneY = boxAreaY;
            }
            else
            {
                targetPlaneY = arenaY;
            }
            currentPlaneY = targetPlaneY;

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
