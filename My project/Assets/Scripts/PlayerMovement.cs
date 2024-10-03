// PlayerMovement.cs
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public MapNode currentNode;
    public float moveSpeed = 5f;

    private bool isMoving = false;
    private Vector3 targetPosition;

    public void InitializePlayer()
    {
        // Colocar al jugador en la posición del nodo actual
        transform.position = currentNode.position;
    }

    public void MoveToNode(MapNode targetNode)
    {
        if (currentNode.connectedNodes.Contains(targetNode))
        {
            isMoving = true;
            targetPosition = targetNode.position;
            currentNode = targetNode;
            Debug.Log($"Moviendo al jugador al nodo en posición {targetNode.position}");
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
            // Mover al jugador hacia la posición objetivo
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                isMoving = false;
                transform.position = targetPosition;
                Debug.Log("El jugador ha llegado al nodo.");
            }
        }
    }
}
