using UnityEngine;
using System.Collections;

public class UnitIdleMovement : MonoBehaviour
{
    public float moveDistance = 0.5f; // Distancia m�xima por movimiento
    public float moveSpeed = 1f; // Velocidad de movimiento
    public float minWaitTime = 1f; // Tiempo m�nimo de espera entre movimientos
    public float maxWaitTime = 3f; // Tiempo m�ximo de espera entre movimientos
    public float boundaryX = 3f; // L�mite en el eje X
    public float boundaryZ = 1f; // L�mite en el eje Z

    private Vector3 startPosition;
    private bool isMoving = false;

    void Start()
    {
        startPosition = transform.position;
        StartCoroutine(MoveRoutine());
    }

    IEnumerator MoveRoutine()
    {
        while (true)
        {
            // Esperar un tiempo aleatorio antes del siguiente movimiento
            float waitTime = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(waitTime);

            // Calcular una direcci�n aleatoria dentro de los l�mites
            Vector3 targetPosition = GetRandomTargetPosition();

            // Moverse hacia la posici�n objetivo
            yield return StartCoroutine(MoveToPosition(targetPosition));
        }
    }

    Vector3 GetRandomTargetPosition()
    {
        Vector3 currentPosition = transform.position;

        // Determinar la direcci�n en X
        float directionX = Random.Range(-moveDistance, moveDistance);
        if (currentPosition.x <= -boundaryX)
        {
            // Si est� en el borde izquierdo, moverse hacia la derecha
            directionX = Mathf.Abs(directionX);
        }
        else if (currentPosition.x >= boundaryX)
        {
            // Si est� en el borde derecho, moverse hacia la izquierda
            directionX = -Mathf.Abs(directionX);
        }

        // Determinar la direcci�n en Z
        float directionZ = Random.Range(-moveDistance, moveDistance);
        if (currentPosition.z <= -boundaryZ)
        {
            // Si est� en el borde inferior, moverse hacia arriba
            directionZ = Mathf.Abs(directionZ);
        }
        else if (currentPosition.z >= boundaryZ)
        {
            // Si est� en el borde superior, moverse hacia abajo
            directionZ = -Mathf.Abs(directionZ);
        }

        // Calcular la posici�n objetivo
        Vector3 targetPosition = new Vector3(
            Mathf.Clamp(currentPosition.x + directionX, -boundaryX, boundaryX),
            currentPosition.y,
            Mathf.Clamp(currentPosition.z + directionZ, -boundaryZ, boundaryZ)
        );

        return targetPosition;
    }

    IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        isMoving = true;

        // Girar el sprite hacia la direcci�n del movimiento
        FaceDirection(targetPosition - transform.position);

        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            // Moverse hacia la posici�n objetivo
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        // Alinear la posici�n exacta
        transform.position = targetPosition;
        isMoving = false;
    }

    void FaceDirection(Vector3 direction)
    {
        // Orientar el sprite seg�n la direcci�n del movimiento
        if (direction.x != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = direction.x > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }
}
