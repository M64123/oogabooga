using UnityEngine;

public class CameraFollowZ : MonoBehaviour
{
    public Transform target;            // Transform del jugador
    public Vector3 offset;              // Desplazamiento de la c�mara respecto al jugador

    private Vector3 initialPosition;    // Posici�n inicial de la c�mara
    private Vector3 targetPosition;     // Posici�n objetivo final de la c�mara
    private bool isMovingToNode = false; // Indica si la c�mara est� movi�ndose hacia el nodo
    private float journeyLength;        // Distancia total del viaje
    private float startTime;            // Tiempo en que inicia el movimiento
    private float travelDuration = 2f;  // Duraci�n del viaje en segundos

    // Par�metros para la trayectoria c�ncava
    public float arcHeight = 2f;        // Altura m�xima de la par�bola

    void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("CameraFollowZ: El objetivo (jugador) no est� asignado en Start.");
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

            // Interpolar la posici�n en l�nea recta
            Vector3 newPosition = Vector3.Lerp(initialPosition, targetPosition, t);

            // A�adir la trayectoria c�ncava (par�bola invertida)
            float heightOffset = arcHeight * Mathf.Sin(Mathf.PI * t);
            newPosition.y += heightOffset;

            // Actualizar la posici�n de la c�mara
            transform.position = newPosition;

            if (t >= 1f)
            {
                isMovingToNode = false;
            }
        }
        else if (target != null)
        {
            // Seguimiento normal del jugador despu�s de llegar al nodo
            Vector3 desiredPosition = target.position + offset;
            transform.position = desiredPosition;
        }
    }

    // M�todo para iniciar el movimiento hacia el nodo seleccionado
    public void MoveCameraToNode(Vector3 nodePosition, float playerTravelDuration)
    {
        // Ajustar la duraci�n del viaje de la c�mara para que coincida con el del jugador
        travelDuration = playerTravelDuration;

        // Establecer la posici�n objetivo de la c�mara con el offset
        targetPosition = nodePosition + offset;

        // Reiniciar par�metros de movimiento
        initialPosition = transform.position;
        startTime = Time.time;
        journeyLength = Vector3.Distance(initialPosition, targetPosition);

        isMovingToNode = true;
    }
}
