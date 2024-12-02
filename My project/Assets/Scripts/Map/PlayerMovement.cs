// PlayerMovement.cs
using UnityEngine;
using System;

/// <summary>
/// Clase que maneja el movimiento del jugador entre nodos.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    /// <summary>
    /// Nodo actual donde se encuentra el jugador.
    /// </summary>
    public MapNode currentNode;

    /// <summary>
    /// Velocidad de movimiento del jugador.
    /// </summary>
    public float moveSpeed = 5f;

    /// <summary>
    /// Indica si el jugador está en movimiento.
    /// </summary>
    private bool isMoving = false;

    /// <summary>
    /// Posición objetivo hacia donde se moverá el jugador.
    /// </summary>
    private Vector3 targetPosition;

    /// <summary>
    /// Evento para notificar cuando el movimiento ha terminado.
    /// </summary>
    public event Action OnMovementFinished;

    /// <summary>
    /// Inicializa la posición del jugador al nodo actual al iniciar.
    /// </summary>
    void Start()
    {
        InitializePlayer();
    }

    /// <summary>
    /// Inicializa el jugador al nodo actual.
    /// </summary>
    public void InitializePlayer()
    {
        if (currentNode != null)
        {
            transform.position = currentNode.transform.position;
            Debug.Log($"Jugador inicializado en el nodo: {currentNode.nodeID}");
        }
        else
        {
            Debug.LogError("currentNode es null. No se puede inicializar el jugador correctamente.");
        }
    }

    /// <summary>
    /// Actualiza el estado del jugador cada frame.
    /// </summary>
    void Update()
    {
        if (isMoving)
        {
            MoveTowardsTarget();
        }
    }

    /// <summary>
    /// Mueve al jugador hacia el nodo objetivo.
    /// </summary>
    public void MoveToNode(MapNode targetNode)
    {
        if (targetNode == null)
        {
            Debug.LogError("targetNode es null. No se puede mover al jugador.");
            return;
        }

        if (!isMoving)
        {
            // Verificar si el nodo objetivo está conectado al nodo actual
            if (currentNode.connectedNodes.Contains(targetNode))
            {
                targetPosition = targetNode.transform.position;
                isMoving = true;
                currentNode = targetNode;

                // Guardar el nodeID del nodo actual en el GameManager
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SetCurrentPlayerNodeID(currentNode.nodeID);
                    Debug.Log($"Jugador movido al nodo: {currentNode.nodeID}");
                }
                else
                {
                    Debug.LogError("GameManager.Instance es null. No se puede guardar el nodo actual del jugador.");
                }
            }
            else
            {
                Debug.LogWarning($"No se puede mover al nodo {targetNode.nodeID} porque no está conectado al nodo actual {currentNode.nodeID}.");
            }
        }
    }

    /// <summary>
    /// Mueve al jugador hacia la posición objetivo.
    /// </summary>
    private void MoveTowardsTarget()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            isMoving = false;
            transform.position = targetPosition;

            // Notificar que el movimiento ha terminado
            OnMovementFinished?.Invoke();

            Debug.Log($"Movimiento al nodo {currentNode.nodeID} completado.");
        }
    }
}
