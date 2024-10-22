using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float transitionDuration = 1f; // Duración de las transiciones

    // Posiciones y Rotaciones
    [Header("Camera Positions and Rotations")]
    public Vector3 position1 = new Vector3(0f, 5f, -10f);
    public Vector3 rotation1 = new Vector3(20f, 0f, 0f);

    public Vector3 position2 = new Vector3(0f, 5f, 0f);
    public Vector3 rotation2 = new Vector3(20f, 0f, 0f);

    // Variables de transición
    private Vector3 targetPosition;
    private Vector3 targetRotation;

    private Vector3 startPosition;
    private Vector3 startRotation;

    private float transitionProgress = 0f;
    private bool isTransitioning = false;

    private bool isInPosition1 = true;

    // Parámetros para detección de zonas
    [Range(0f, 1f)]
    public float upperZoneThreshold = 0.6f; // Porcentaje de altura para la zona superior
    [Range(0f, 1f)]
    public float lowerZoneThreshold = 0.4f; // Porcentaje de altura para la zona inferior

    void Start()
    {
        // Establecer la posición inicial
        transform.position = position1;
        transform.eulerAngles = rotation1;
        isInPosition1 = true;
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
                isInPosition1 = !isInPosition1; // Actualizar el estado de la posición
            }

            // Interpolar posición y rotación
            transform.position = Vector3.Lerp(startPosition, targetPosition, transitionProgress);
            Vector3 currentRotation = Vector3.Lerp(startRotation, targetRotation, transitionProgress);
            transform.eulerAngles = currentRotation;
        }
        else
        {
            // Detectar si el cursor está en la zona superior o inferior
            DetectScreenZones();
        }
    }

    void DetectScreenZones()
    {
        Vector3 mousePos = Input.mousePosition;

        // Zona superior
        if (mousePos.y >= Screen.height * upperZoneThreshold && isInPosition1)
        {
            TransitionToPosition2();
        }
        // Zona inferior
        else if (mousePos.y <= Screen.height * lowerZoneThreshold && !isInPosition1)
        {
            TransitionToPosition1();
        }
    }

    public void TransitionToPosition1()
    {
        StartTransition(position1, rotation1);
        // isInPosition1 = true; // Eliminar esta línea
    }

    public void TransitionToPosition2()
    {
        StartTransition(position2, rotation2);
        // isInPosition1 = false; // Eliminar esta línea
    }

    void StartTransition(Vector3 newPosition, Vector3 newRotation)
    {
        startPosition = transform.position;
        startRotation = transform.eulerAngles;

        targetPosition = newPosition;
        targetRotation = newRotation;

        transitionProgress = 0f;
        isTransitioning = true;
    }
}
