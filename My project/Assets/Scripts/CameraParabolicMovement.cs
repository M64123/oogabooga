using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraParabolicMovement : MonoBehaviour
{
   public Transform[] points;    // Arreglo de puntos (inicio y destinos)
    public float height = 5f;     // Altura máxima del salto
    public float duration = 2f;   // Duración total del salto
    public float landingBounceDistance = 0.2f; // Distancia hacia adelante en cada rebote
    public float landingBounceHeight = 0.1f;   // Altura de los rebotes
    public float bounceDuration = 0.5f;        // Duración total de los rebotes
    public float rotationSpeed = 2f;           // Velocidad de la rotación hacia el destino

    private int currentPointIndex = 0; // Índice del punto actual
    private float elapsedTime = 0f;
    private bool isMoving = false;
    private bool isLanding = false;
    private int bounceStep = 0;

    void Update()
    {
        // Validar que haya suficientes puntos para el movimiento
        if (points == null || points.Length < 2)
        {
            Debug.LogError("Debe haber al menos dos puntos en el arreglo para realizar el movimiento.");
            return;
        }

        // Iniciar el salto al presionar la tecla espacio
        if (Input.GetKeyDown(KeyCode.Space) && !isMoving && !isLanding)
        {
            if (currentPointIndex < points.Length - 1)
            {
                isMoving = true;
                elapsedTime = 0f;
                Debug.Log($"Saltando desde {points[currentPointIndex].name} a {points[currentPointIndex + 1].name}.");
            }
            else
            {
                Debug.Log("Todos los puntos han sido alcanzados.");
            }
        }

        // Realizar el movimiento principal
        if (isMoving)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // Interpolación con suavidad (más natural)
            t = Mathf.SmoothStep(0f, 1f, t);

            if (t >= 1f)
            {
                t = 1f;
                isMoving = false;
                isLanding = true; // Activar el rebote de aterrizaje
                elapsedTime = 0f;
                bounceStep = 0;
                transform.position = points[currentPointIndex + 1].position;
                currentPointIndex++;
                Debug.Log($"Movimiento completado. Ahora en {points[currentPointIndex].name}.");
                return;
            }

            // Movimiento parabólico
            Vector3 startPosition = points[currentPointIndex].position;
            Vector3 targetPosition = points[currentPointIndex + 1].position;

            // Interpolación lineal
            Vector3 horizontalPosition = Vector3.Lerp(startPosition, targetPosition, t);

            // Altura parabólica
            float verticalOffset = height * Mathf.Sin(Mathf.PI * t); // Usar seno para una parábola natural
            Vector3 parabolicPosition = new Vector3(horizontalPosition.x, horizontalPosition.y + verticalOffset, horizontalPosition.z);

            // Actualizar posición de la cámara
            transform.position = parabolicPosition;

            // Rotar la cámara hacia el siguiente punto
            Vector3 direction = targetPosition - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        // Realizar los rebotes al aterrizar
        if (isLanding)
        {
            elapsedTime += Time.deltaTime;
            float bounceT = elapsedTime / bounceDuration;

            if (bounceT >= 1f)
            {
                bounceT = 0f;
                elapsedTime = 0f;
                bounceStep++;

                // Finalizar rebotes después del tercer paso
                if (bounceStep >= 3)
                {
                    isLanding = false;
                    bounceStep = 0;
                    Debug.Log($"Aterrizaje completado en {points[currentPointIndex].name}.");
                    return;
                }
            }

            // Movimiento hacia adelante y arriba en el rebote
            Vector3 forwardDirection = (points[currentPointIndex].position - points[currentPointIndex - 1].position).normalized;
            Vector3 forwardOffset = forwardDirection * landingBounceDistance * (1f - bounceT);
            float upwardOffset = landingBounceHeight * Mathf.Sin(bounceT * Mathf.PI);

            transform.position = points[currentPointIndex].position + forwardOffset + new Vector3(0f, upwardOffset, 0f);

            // Rotar hacia el próximo objetivo durante el rebote
            if (currentPointIndex < points.Length - 1)
            {
                Vector3 direction = points[currentPointIndex + 1].position - transform.position;
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
        }
    }

    private void OnDrawGizmos()
    {
        // Dibujar líneas para visualizar las conexiones entre los puntos
        if (points != null && points.Length > 1)
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < points.Length - 1; i++)
            {
                if (points[i] != null && points[i + 1] != null)
                {
                    Gizmos.DrawLine(points[i].position, points[i + 1].position);
                }
            }
        }
    }
}
