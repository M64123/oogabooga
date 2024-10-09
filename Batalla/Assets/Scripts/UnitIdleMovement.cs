using UnityEngine;
using System.Collections;

public class UnitIdleMovement : MonoBehaviour
{
    public float moveDistance = 0.5f; // Distancia máxima por movimiento
    public float moveSpeed = 1f; // Velocidad de movimiento
    public float minWaitTime = 1f; // Tiempo mínimo de espera entre movimientos
    public float maxWaitTime = 3f; // Tiempo máximo de espera entre movimientos
    public float boundaryX = 3f; // Límite en el eje X
    public float boundaryZ = 1f; // Límite en el eje Z

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

            // Calcular una dirección aleatoria dentro de los límites
            Vector3 targetPosition = GetRandomTargetPosition();

            // Moverse hacia la posición objetivo
            yield return StartCoroutine(MoveToPosition(targetPosition));
        }
    }

    Vector3 GetRandomTargetPosition()
    {
        Vector3 currentPosition = transform.position;

        // Determinar la dirección en X
        float directionX = Random.Range(-moveDistance, moveDistance);
        if (currentPosition.x <= -boundaryX)
        {
            // Si está en el borde izquierdo, moverse hacia la derecha
            directionX = Mathf.Abs(directionX);
        }
        else if (currentPosition.x >= boundaryX)
        {
            // Si está en el borde derecho, moverse hacia la izquierda
            directionX = -Mathf.Abs(directionX);
        }

        // Determinar la dirección en Z
        float directionZ = Random.Range(-moveDistance, moveDistance);
        if (currentPosition.z <= -boundaryZ)
        {
            // Si está en el borde inferior, moverse hacia arriba
            directionZ = Mathf.Abs(directionZ);
        }
        else if (currentPosition.z >= boundaryZ)
        {
            // Si está en el borde superior, moverse hacia abajo
            directionZ = -Mathf.Abs(directionZ);
        }

        // Calcular la posición objetivo
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

        // Girar el sprite hacia la dirección del movimiento
        FaceDirection(targetPosition - transform.position);

        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            // Moverse hacia la posición objetivo
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        // Alinear la posición exacta
        transform.position = targetPosition;
        isMoving = false;
    }

    void FaceDirection(Vector3 direction)
    {
        // Orientar el sprite según la dirección del movimiento
        if (direction.x != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = direction.x > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }
}
