using UnityEngine;
using System; // Necesario para usar Action

public class PlayerMovement : MonoBehaviour
{
    public MapNode currentNode;
    public float moveDuration = 2f; // Duración del movimiento hacia el nodo

    private bool isMoving = false;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float moveStartTime;

    // Evento que se disparará cuando el jugador llegue al nodo
    public event Action OnMovementFinished;

    public void InitializePlayer()
    {
        // Colocar al jugador en la posición del nodo actual
        transform.position = currentNode.position;
    }

    public void MoveToNode(MapNode targetNode)
    {
        if (isMoving)
            return;

        if (currentNode.connectedNodes.Contains(targetNode))
        {
            isMoving = true;
            startPosition = transform.position;
            targetPosition = targetNode.position;
            currentNode = targetNode;
            moveStartTime = Time.time;
            Debug.Log($"Moviendo al jugador al nodo en posición {targetNode.position}");

            // Iniciar el movimiento de la cámara
            CameraFollowZ cameraFollow = Camera.main.GetComponent<CameraFollowZ>();
            if (cameraFollow != null)
            {
                cameraFollow.MoveCameraToNode(targetPosition, moveDuration);
            }
        }
        else
        {
            Debug.LogWarning("El nodo seleccionado no está conectado al nodo actual.");
        }
    }

    void Update()
    {
        if (isMoving)
        {
            float elapsed = Time.time - moveStartTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);
            t = Mathf.SmoothStep(0f, 1f, t);

            // Interpolar la posición del jugador
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);

            if (t >= 1f)
            {
                isMoving = false;
                transform.position = targetPosition;
                Debug.Log("El jugador ha llegado al nodo.");

                // Disparar el evento para notificar que el movimiento ha terminado
                OnMovementFinished?.Invoke();
            }
        }
    }
}
