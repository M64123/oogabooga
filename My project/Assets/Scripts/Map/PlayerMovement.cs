using UnityEngine;
using System;

public class PlayerMovement : MonoBehaviour
{
    public MapNode currentNode;
    public float moveSpeed = 5f;

    private bool isMoving = false;
    private Vector3 targetPosition;

    // Evento para notificar cuando el movimiento ha terminado
    public event Action OnMovementFinished;

    void Start()
    {
        if (currentNode != null)
        {
            transform.position = currentNode.position;
        }
    }

    public void InitializePlayer()
    {
        if (currentNode != null)
        {
            transform.position = currentNode.position;
        }
    }

    void Update()
    {
        if (isMoving)
        {
            MoveTowardsTarget();
        }
    }

    public void MoveToNode(MapNode targetNode)
    {
        if (!isMoving)
        {
            currentNode = targetNode;
            targetPosition = targetNode.position;
            isMoving = true;

            // Guardar el nodeID del nodo actual en el GameManager
            GameManager.Instance.SetCurrentPlayerNodeID(currentNode.nodeID);
        }
    }

    private void MoveTowardsTarget()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            isMoving = false;
            transform.position = targetPosition;

            // Notificar que el movimiento ha terminado
            OnMovementFinished?.Invoke();
        }
    }
}
