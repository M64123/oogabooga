// DinoDragHandler.cs

using UnityEngine;
using System.Collections;

public class DinoDragHandler : MonoBehaviour
{
    private bool isDragging = false;
    private bool isTransitioning = false;
    private float transitionProgress = 0f;

    private Camera mainCamera;
    public CameraController cameraController;

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

    // Altura de la arena
    public float arenaY = 10f;

    // Variables para el movimiento y rotación
    public float cameraDinoZOffset = 1f; // Offset en Z entre la cámara y el dinosaurio durante el arrastre
    public float transitionDuration = 1f; // Duración de la transición de elevación al iniciar el arrastre

    // Curva para el movimiento en Z (para lograr la trayectoria curva)
    public AnimationCurve movementCurve;

    // Variables internas para el movimiento
    private Vector3 startDragPosition;
    private Vector3 targetDragPosition;
    private Quaternion startRotation;

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
    }

    void Update()
    {
        if (isTransitioning)
        {
            // Actualizar el progreso de la transición
            transitionProgress += Time.deltaTime / transitionDuration;
            if (transitionProgress >= 1f)
            {
                transitionProgress = 1f;
                isTransitioning = false;
            }

            // Movimiento en curva usando la AnimationCurve
            float curveValue = movementCurve.Evaluate(transitionProgress);
            transform.position = Vector3.Lerp(startDragPosition, targetDragPosition, curveValue);

            // Sincronizar la rotación con la cámara en el eje X
            SyncRotationWithCamera();
        }
        else if (isDragging)
        {
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

    void OnMouseDown()
    {
        // Verificar si el dinosaurio es un Ally
        if (!CompareTag(allyTag))
            return;

        if (!isDragging && !isTransitioning)
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

        // Guardar la posición inicial
        startDragPosition = transform.position;
        startRotation = transform.rotation;

        // Calcular la posición objetivo (elevada)
        Vector3 cameraForward = mainCamera.transform.forward;
        targetDragPosition = mainCamera.transform.position + cameraForward * cameraDinoZOffset;
        targetDragPosition.y += cameraDinoZOffset; // Elevar en Y si es necesario

        // Iniciar la transición
        transitionProgress = 0f;
        isTransitioning = true;
    }

    void FollowCursor()
    {
        // Obtener la posición del cursor en el mundo
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.forward, mainCamera.transform.position + mainCamera.transform.forward * cameraDinoZOffset);
        if (plane.Raycast(ray, out float distance))
        {
            Vector3 worldPosition = ray.GetPoint(distance);

            // Aplicar offset en Z
            worldPosition.z = mainCamera.transform.position.z + cameraDinoZOffset;

            transform.position = worldPosition;
        }
    }

    void SyncRotationWithCamera()
    {
        if (cameraController != null)
        {
            // Obtener la rotación en X de la cámara
            float cameraRotationX = mainCamera.transform.eulerAngles.x;

            // Aplicar la rotación al dinosaurio
            transform.rotation = Quaternion.Euler(cameraRotationX, 0f, 0f);
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

            // Ajustar la posición en Y
            AdjustPositionToGround();

            // Iniciar rotación suave a X=0
            StartCoroutine(SmoothRotateTo(0f));

            return;
        }

        // Intentar soltar en la Arena
        if (IsPointerOverLayer(arenaLayerMask))
        {
            // Verificar si la posición está en el lado del jugador (X negativa)
            if (transform.position.x <= 0)
            {
                isDragging = false;

                // Ajustar la posición en Y
                transform.position = new Vector3(transform.position.x, arenaY, transform.position.z);

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
            // Si no se detecta el suelo, establecer Y a arenaY
            transform.position = new Vector3(transform.position.x, arenaY, transform.position.z);
        }
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
}
