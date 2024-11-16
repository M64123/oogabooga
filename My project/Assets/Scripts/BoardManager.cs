// BoardManager.cs
using UnityEngine;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    // Puedes a�adir variables p�blicas o privadas seg�n tus necesidades
    // Por ejemplo, referencias a prefabs, UI, etc.

    void Start()
    {
        // Obtener el estado actual del tablero desde GameManager
        List<MapNodeData> currentBoardState = GameManager.Instance.boardState;

        if (currentBoardState != null && currentBoardState.Count > 0)
        {
            Debug.Log("Cargando estado del tablero desde GameManager.");
            LoadBoardState(currentBoardState);
        }
        else
        {
            Debug.LogWarning("No hay estado del tablero guardado en GameManager. Generando un nuevo tablero.");
            // Aqu� puedes llamar a m�todos para generar un nuevo tablero si es necesario
            // Por ejemplo:
            // GenerateNewBoard();
        }
    }

    /// <summary>
    /// Carga el estado del tablero basado en los datos proporcionados.
    /// </summary>
    /// <param name="boardState">Lista de datos de nodos del tablero.</param>
    private void LoadBoardState(List<MapNodeData> boardState)
    {
        foreach (MapNodeData nodeData in boardState)
        {
            // Aqu� debes implementar la l�gica para reconstruir cada nodo en el tablero
            // Basado en nodeData.nodeID, nodeData.position, nodeData.nodeType, etc.
            // Por ejemplo:

            // 1. Buscar o crear el GameObject del nodo
            GameObject nodeObj = GameObject.Find($"MapNode_{nodeData.nodeID}");
            if (nodeObj == null)
            {
                nodeObj = new GameObject($"MapNode_{nodeData.nodeID}");
                nodeObj.transform.position = nodeData.position;

                // A�adir el componente MapNode
                MapNode node = nodeObj.AddComponent<MapNode>();
                node.nodeID = nodeData.nodeID;
                node.depthLevel = nodeData.depthLevel;
                node.nodeType = nodeData.nodeType;
                node.isActive = nodeData.isActive;
                node.connectedNodes = new List<MapNode>();
                node.nodeTypesInPaths = new HashSet<NodeType>();
                node.isTypeAssigned = true; // Asumimos que el tipo ya ha sido asignado

                // A�adir al GameManager
                GameManager.Instance.boardState.Add(nodeData);
            }

            // 2. Conectar los nodos seg�n connectedNodeIDs
            MapNode currentNode = nodeObj.GetComponent<MapNode>();
            foreach (string connectedID in nodeData.connectedNodeIDs)
            {
                GameObject connectedObj = GameObject.Find($"MapNode_{connectedID}");
                if (connectedObj != null)
                {
                    MapNode connectedNode = connectedObj.GetComponent<MapNode>();
                    if (connectedNode != null && !currentNode.connectedNodes.Contains(connectedNode))
                    {
                        currentNode.connectedNodes.Add(connectedNode);
                    }
                }
                else
                {
                    Debug.LogError($"No se encontr� el nodo conectado con nodeID: {connectedID}");
                }
            }

            // 3. Opcional: Actualizar la visualizaci�n del nodo
            if (currentNode.nodeObject != null)
            {
                NodeInteraction interaction = currentNode.nodeObject.GetComponentInChildren<NodeInteraction>();
                if (interaction != null)
                {
                    interaction.node = currentNode;
                    interaction.InitializeNodeVisual();
                }
                else
                {
                    Debug.LogError($"El GameObject {currentNode.nodeObject.name} no tiene el componente NodeInteraction.");
                }
            }
        }

        // Opcional: Actualizar otras partes del tablero seg�n sea necesario
    }

    /// <summary>
    /// Actualiza el estado del tablero y lo guarda en GameManager.
    /// </summary>
    /// <param name="newState">Nueva lista de datos de nodos del tablero.</param>
    public void UpdateBoardState(List<MapNodeData> newState)
    {
        // Actualizar el estado del tablero en GameManager
        GameManager.Instance.boardState = newState;
        Debug.Log("Estado del tablero actualizado y guardado en GameManager.");
    }

    /// <summary>
    /// M�todo opcional para generar un nuevo tablero si no hay estado guardado.
    /// Implementa la l�gica seg�n tus necesidades.
    /// </summary>
    private void GenerateNewBoard()
    {
        // Implementa la generaci�n de un nuevo tablero aqu�
        // Por ejemplo, instanciar nodos, conectarlos, etc.
    }

    // Otros m�todos y l�gica seg�n tus necesidades
}
