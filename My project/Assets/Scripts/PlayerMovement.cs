using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerMovement : MonoBehaviour
{
    public MapNode currentNode;
    public float moveSpeed = 5f;  // Velocidad de movimiento del jugador

    private HashSet<MapNode> visitedNodes = new HashSet<MapNode>();

    private void Start()
    {
        // El jugador será inicializado desde el MapGenerator
    }

    public void InitializePlayer()
    {
        if (currentNode != null)
        {
            transform.position = currentNode.position;
            MarkNodeAsVisited(currentNode);
            UpdateNodeColors();
        }
        else
        {
            Debug.LogError("currentNode no está asignado en PlayerMovement.");
        }
    }

    public void MoveToNode(MapNode nextNode)
    {
        if (currentNode.connectedNodes.Contains(nextNode) && !visitedNodes.Contains(nextNode))
        {
            // Verificar que el nodo está hacia adelante y no hacia atrás
            if (nextNode.position.z >= currentNode.position.z)
            {
                currentNode = nextNode;
                // Iniciar corrutina para mover al jugador suavemente
                StopAllCoroutines();
                StartCoroutine(MoveToPosition(currentNode.position));
                MarkNodeAsVisited(currentNode);
                UpdateNodeColors();
            }
            else
            {
                Debug.Log("No puedes moverte hacia atrás.");
            }
        }
        else
        {
            Debug.Log("Movimiento no permitido.");
        }
    }

    private IEnumerator MoveToPosition(Vector3 target)
    {
        while (Vector3.Distance(transform.position, target) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = target;
    }

    private void MarkNodeAsVisited(MapNode node)
    {
        visitedNodes.Add(node);
        // Reducir la opacidad del nodo
        NodeInteraction interaction = node.nodeObject.GetComponent<NodeInteraction>();
        if (interaction != null)
        {
            interaction.SetVisited();
        }
    }

    private void UpdateNodeColors()
    {
        // Restablecer el color de todos los nodos no visitados
        MapGenerator mapGenerator = FindObjectOfType<MapGenerator>();
        if (mapGenerator != null)
        {
            foreach (MapNode node in mapGenerator.allNodes)
            {
                if (!visitedNodes.Contains(node))
                {
                    NodeInteraction interaction = node.nodeObject.GetComponent<NodeInteraction>();
                    if (interaction != null)
                    {
                        interaction.ResetColor();
                    }
                }
            }
        }

        // Iluminar los nodos disponibles en verde
        foreach (MapNode node in currentNode.connectedNodes)
        {
            if (!visitedNodes.Contains(node))
            {
                NodeInteraction interaction = node.nodeObject.GetComponent<NodeInteraction>();
                if (interaction != null)
                {
                    interaction.SetAvailable();
                }
            }
        }

        // Iluminar el nodo actual en azul
        NodeInteraction currentInteraction = currentNode.nodeObject.GetComponent<NodeInteraction>();
        if (currentInteraction != null)
        {
            currentInteraction.SetCurrent();
        }
    }
}
