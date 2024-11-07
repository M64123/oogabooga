using UnityEngine;

public class CameraFollowZ : MonoBehaviour
{
    public Transform target;            // Transform del jugador
    public float smoothSpeed = 0.125f;  // Velocidad de suavizado
    public Vector3 offset;              // Desplazamiento de la c�mara respecto al jugador

    private Vector3 initialPosition;    // Posici�n inicial de la c�mara
    private bool isTransitioning = false; // Indica si la c�mara est� en transici�n
    public float transitionDuration = 2f; // Duraci�n de la transici�n en segundos
    private float transitionTimer = 0f;   // Temporizador para la transici�n

    void Start()
    {
        // Guardar la posici�n inicial de la c�mara
        initialPosition = transform.position;

        if (target == null)
        {
            Debug.LogWarning("CameraFollowZ: El objetivo (jugador) no est� asignado en Start.");
        }
        else
        {
            // Iniciar la transici�n
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

                // Calcular el porcentaje completado de la transici�n
                float t = transitionTimer / transitionDuration;

                // Suavizar el valor de t
                t = Mathf.SmoothStep(0f, 1f, t);

                // Posici�n objetivo con offset
                Vector3 targetPosition = target.position + offset;

                // Interpolar entre la posici�n inicial y la posici�n objetivo
                Vector3 newPosition = Vector3.Lerp(initialPosition, targetPosition, t);

                // Actualizar la posici�n de la c�mara
                transform.position = newPosition;

                // Finalizar la transici�n cuando se alcance la duraci�n
                if (t >= 1f)
                {
                    isTransitioning = false;
                }
            }
            else
            {
                // Una vez finalizada la transici�n, seguir al jugador manteniendo el offset
                Vector3 desiredPosition = target.position + offset;

                // Suavizar el movimiento de la c�mara
                Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

                // Actualizar la posici�n de la c�mara
                transform.position = smoothedPosition;
            }
        }
    }
}
