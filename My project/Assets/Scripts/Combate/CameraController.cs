// CameraController.cs

using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float transitionDuration = 1f; // Duración de las transiciones

    // Posiciones y Rotaciones
    [Header("Camera Positions and Rotations")]
    public Vector3 position1 = new Vector3(0f, 3f, -12f);
    public Vector3 rotation1 = new Vector3(20f, 0f, 0f);

    public Vector3 position2 = new Vector3(0f, 16f, 8f);
    public Vector3 rotation2 = new Vector3(45f, 0f, 0f);

    // Variables de transición
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private Vector3 startRotation;
    private Vector3 targetRotation;

    private float transitionProgress = 0f;
    private bool isTransitioning = false;

    private bool isInPosition1 = true;

    // Parámetros para la función logística
    public float logisticK = 10f; // Pendiente de la curva
    public float logisticT0 = 0.5f; // Punto medio

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

                // Establecer la posición y rotación finales exactas
                transform.position = targetPosition;
                transform.eulerAngles = targetRotation;
            }
            else
            {
                // Calcular el factor t usando la función logística normalizada
                float t = LogisticFunctionNormalized(transitionProgress, logisticK, logisticT0);

                // Calcular la nueva posición siguiendo una trayectoria en S
                Vector3 newPosition = CalculateSPathPosition(t);

                // Interpolar la rotación usando t
                Vector3 newRotation = Vector3.Lerp(startRotation, targetRotation, t);

                // Actualizar la posición y rotación de la cámara
                transform.position = newPosition;
                transform.eulerAngles = newRotation;
            }
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
        // Establecer la posición inicial correcta antes de iniciar la transición
        if (!isInPosition1)
        {
            transform.position = position2;
            transform.eulerAngles = rotation2;
        }

        StartTransition(position1, rotation1);
    }

    public void TransitionToPosition2()
    {
        // Establecer la posición inicial correcta antes de iniciar la transición
        if (isInPosition1)
        {
            transform.position = position1;
            transform.eulerAngles = rotation1;
        }

        StartTransition(position2, rotation2);
    }

    void StartTransition(Vector3 newPosition, Vector3 newRotation)
    {
        startPosition = transform.position;
        targetPosition = newPosition;
        startRotation = transform.eulerAngles;
        targetRotation = newRotation;

        transitionProgress = 0f;
        isTransitioning = true;
    }

    float LogisticFunctionNormalized(float t, float k, float t0)
    {
        float logistic = 1f / (1f + Mathf.Exp(-k * (t - t0)));
        // Normalizar entre 0 y 1
        float logisticMin = 1f / (1f + Mathf.Exp(-k * (0f - t0)));
        float logisticMax = 1f / (1f + Mathf.Exp(-k * (1f - t0)));
        return (logistic - logisticMin) / (logisticMax - logisticMin);
    }

    Vector3 CalculateSPathPosition(float t)
    {
        // Calcular la posición en X (mantener constante en este caso)
        float xPos = Mathf.Lerp(startPosition.x, targetPosition.x, t);

        // Calcular la posición en Y
        float yPos = Mathf.Lerp(startPosition.y, targetPosition.y, t);

        // Calcular la posición en Z usando la función logística para crear la curva en S
        float sCurveZ = startPosition.z + (targetPosition.z - startPosition.z) * LogisticFunctionNormalized(t, logisticK, logisticT0);

        return new Vector3(xPos, yPos, sCurveZ);
    }
}
