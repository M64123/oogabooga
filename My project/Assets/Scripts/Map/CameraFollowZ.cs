using UnityEngine;

public class CameraFollowZ : MonoBehaviour
{
    public Transform target;            // Transform del jugador
    public Vector3 offset;              // Desplazamiento de la cámara respecto al jugador

    private Vector3 initialPosition;    // Posición inicial de la cámara
    private Vector3 targetPosition;     // Posición objetivo final de la cámara
    private bool isMovingToNode = false; // Indica si la cámara está moviéndose hacia el nodo
    private float journeyLength;        // Distancia total del viaje
    private float startTime;            // Tiempo en que inicia el movimiento
    private float travelDuration = 2f;  // Duración del viaje en segundos

    // Parámetros para la trayectoria cóncava
    public float arcHeight = 2f;        // Altura máxima de la parábola

    void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("CameraFollowZ: El objetivo (jugador) no está asignado en Start.");
        }
    }

    void LateUpdate()
    {
        if (isMovingToNode && target != null)
        {
            // Tiempo transcurrido desde el inicio del movimiento
            float elapsedTime = Time.time - startTime;
            float t = Mathf.Clamp01(elapsedTime / travelDuration);
            t = Mathf.SmoothStep(0f, 1f, t);

            // Interpolar la posición en línea recta
            Vector3 newPosition = Vector3.Lerp(initialPosition, targetPosition, t);

            // Añadir la trayectoria cóncava (parábola invertida)
            float heightOffset = arcHeight * Mathf.Sin(Mathf.PI * t);
            newPosition.y += heightOffset;

            // Actualizar la posición de la cámara
            transform.position = newPosition;

            if (t >= 1f)
            {
                isMovingToNode = false;
            }
        }
        else if (target != null)
        {
            // Seguimiento normal del jugador después de llegar al nodo
            Vector3 desiredPosition = target.position + offset;
            transform.position = desiredPosition;
        }
    }

    // Método para iniciar el movimiento hacia el nodo seleccionado
    public void MoveCameraToNode(Vector3 nodePosition, float playerTravelDuration)
    {
        // Ajustar la duración del viaje de la cámara para que coincida con el del jugador
        travelDuration = playerTravelDuration;

        // Establecer la posición objetivo de la cámara con el offset
        targetPosition = nodePosition + offset;

        // Reiniciar parámetros de movimiento
        initialPosition = transform.position;
        startTime = Time.time;
        journeyLength = Vector3.Distance(initialPosition, targetPosition);

        isMovingToNode = true;
    }
}
