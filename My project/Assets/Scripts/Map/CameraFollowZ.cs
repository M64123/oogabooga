using UnityEngine;

public class CameraFollowZ : MonoBehaviour
{
    public Transform target;            // Transform del jugador
    public float smoothSpeed = 0.125f;  // Velocidad de suavizado
    public Vector3 offset;              // Desplazamiento de la cámara respecto al jugador

    private Vector3 initialPosition;    // Posición inicial de la cámara
    private bool isTransitioning = false; // Indica si la cámara está en transición
    public float transitionDuration = 2f; // Duración de la transición en segundos
    private float transitionTimer = 0f;   // Temporizador para la transición

    void Start()
    {
        // Guardar la posición inicial de la cámara
        initialPosition = transform.position;

        if (target == null)
        {
            Debug.LogWarning("CameraFollowZ: El objetivo (jugador) no está asignado en Start.");
        }
        else
        {
            // Iniciar la transición
            isTransitioning = true;
            transitionTimer = 0f;
        }
    }

    void LateUpdate()
    {
        if (target != null)
        {
            if (isTransitioning)
            {
                // Incrementar el temporizador
                transitionTimer += Time.deltaTime;

                // Calcular el porcentaje completado de la transición
                float t = transitionTimer / transitionDuration;

                // Suavizar el valor de t
                t = Mathf.SmoothStep(0f, 1f, t);

                // Posición objetivo con offset
                Vector3 targetPosition = target.position + offset;

                // Interpolar entre la posición inicial y la posición objetivo
                Vector3 newPosition = Vector3.Lerp(initialPosition, targetPosition, t);

                // Actualizar la posición de la cámara
                transform.position = newPosition;

                // Finalizar la transición cuando se alcance la duración
                if (t >= 1f)
                {
                    isTransitioning = false;
                }
            }
            else
            {
                // Una vez finalizada la transición, seguir al jugador manteniendo el offset
                Vector3 desiredPosition = target.position + offset;

                // Suavizar el movimiento de la cámara
                Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

                // Actualizar la posición de la cámara
                transform.position = smoothedPosition;
            }
        }
    }
}
