// BoardManager.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Clase que maneja la carga y visualización del tablero de nodos.
/// </summary>
public class BoardManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject nodePrefab;      // Prefab genérico de nodo (fallback)
    public GameObject playerPrefab;    // Prefab del jugador

    
    [System.Serializable]
    public struct NodePrefab
    {
        public NodeType nodeType;
        public GameObject prefab;
    }

    public List<NodePrefab> nodePrefabs;  // Lista de prefabs por tipo de nodo

    [Header("Line Renderer Settings")]
    public float lineWidth = 0.05f;            // Ancho de la línea
    public Material lineMaterial;              // Material para el LineRenderer
    public Color lineColor = Color.white;      // Color de la línea

    [Header("Parent Objects")]
    public Transform nodesParent;               // Parent para organizar los nodos
    public Transform linesParent;               // Parent para organizar las líneas

    private GameManager gameManager;

    void Start()
    {
        // Comprobaciones de referencia
        if (nodePrefab == null)
        {
            Debug.LogError("nodePrefab no ha sido asignado en el Inspector.");
            return;
        }

        if (playerPrefab == null)
        {
            Debug.LogError("playerPrefab no ha sido asignado en el Inspector.");
            return;
        }

        if (nodePrefabs == null || nodePrefabs.Count == 0)
        {
            Debug.LogError("La lista de nodePrefabs está vacía. Asegúrate de asignar al menos un prefab.");
            return;
        }

        if (nodesParent == null)
        {
            Debug.LogError("nodesParent no ha sido asignado en el Inspector.");
            return;
        }

        if (linesParent == null)
        {
            Debug.LogError("linesParent no ha sido asignado en el Inspector.");
            return;
        }

        // Obtener referencia a GameManager
        gameManager = GameManager.Instance;

        if (gameManager == null)
        {
            Debug.LogError("No se encontró una instancia de GameManager en la escena.");
            return;
        }

        // Cargar el estado del mapa desde GameManager
        List<MapNodeData> currentBoardState = gameManager.GetSavedMapData();

        if (currentBoardState != null && currentBoardState.Count > 0)
        {
            Debug.Log("Cargando estado del tablero desde GameManager.");
            LoadBoardState(currentBoardState);
        }
        else
        {
            Debug.LogWarning("No hay estado del tablero guardado en GameManager. Asegúrate de que MapGenerator haya generado el mapa.");
        }
    }

    /// <summary>
    /// Carga el estado del tablero desde los datos guardados en GameManager.
    /// </summary>
    /// <param name="boardState">Lista de datos de nodos.</param>
    private void LoadBoardState(List<MapNodeData> boardState)
    {
        // Diccionario temporal para mapear nodeID a MapNode
        Dictionary<string, MapNode> nodeDictionary = new Dictionary<string, MapNode>();

        // Crear nodos
        foreach (MapNodeData nodeData in boardState)
        {
            // Obtener el prefab correspondiente al NodeType
            GameObject prefabToUse = GetPrefabForNodeType(nodeData.nodeType);
            if (prefabToUse == null)
            {
                Debug.LogError($"No se encontró un prefab para el NodeType: {nodeData.nodeType}. Usando nodePrefab por defecto.");
                prefabToUse = nodePrefab;
            }

            // Instanciar el prefab del nodo como hijo de nodesParent
            GameObject nodeObj = Instantiate(prefabToUse, nodeData.position, Quaternion.identity, nodesParent);
            nodeObj.name = $"MapNode_{nodeData.nodeID}";

            // Obtener el componente MapNode
            MapNode node = nodeObj.GetComponent<MapNode>();
            if (node == null)
            {
                Debug.LogError($"El prefab {prefabToUse.name} no tiene el componente MapNode.");
                continue;
            }

            // Configurar las propiedades del nodo
            node.nodeID = nodeData.nodeID;
            node.depthLevel = nodeData.depthLevel;
            node.nodeType = nodeData.nodeType;
            node.isActive = nodeData.isActive;
            node.connectedNodes = new List<MapNode>();
            node.nodeTypesInPaths = new HashSet<NodeType>();
            node.isTypeAssigned = true; // Asumimos que el tipo ya ha sido asignado

            // Añadir al diccionario temporal
            nodeDictionary[node.nodeID] = node;
        }

        // Establecer conexiones entre nodos
        foreach (MapNodeData nodeData in boardState)
        {
            if (!nodeDictionary.TryGetValue(nodeData.nodeID, out MapNode currentNode))
            {
                Debug.LogError($"No se encontró el nodo con nodeID: {nodeData.nodeID}");
                continue;
            }

            foreach (string connectedID in nodeData.connectedNodeIDs)
            {
                if (nodeDictionary.TryGetValue(connectedID, out MapNode connectedNode))
                {
                    if (!currentNode.connectedNodes.Contains(connectedNode))
                    {
                        currentNode.connectedNodes.Add(connectedNode);
                    }
                }
                else
                {
                    Debug.LogError($"No se encontró el nodo conectado con nodeID: {connectedID}");
                }
            }
        }

        // Dibujar las conexiones
        DrawConnections();

        // Instanciar al jugador en la posición guardada
        InstantiatePlayer();
    }

    /// <summary>
    /// Obtiene el prefab correspondiente a un tipo de nodo.
    /// </summary>
    /// <param name="nodeType">Tipo de nodo.</param>
    /// <returns>Prefab correspondiente o null si no se encuentra.</returns>
    private GameObject GetPrefabForNodeType(NodeType nodeType)
    {
        foreach (NodePrefab np in nodePrefabs)
        {
            if (np.nodeType == nodeType)
            {
                return np.prefab;
            }
        }
        Debug.LogError($"No se encontró un prefab asignado para el NodeType: {nodeType}");
        return null;
    }

    /// <summary>
    /// Dibuja las conexiones entre nodos usando LineRenderers.
    /// </summary>
    private void DrawConnections()
    {
        // Limpiar líneas previas
        foreach (Transform child in linesParent)
        {
            Destroy(child.gameObject);
        }

        // Buscar todos los nodos en la escena
        MapNode[] allNodes = FindObjectsOfType<MapNode>();

        foreach (MapNode node in allNodes)
        {
            foreach (MapNode connectedNode in node.connectedNodes)
            {
                // Evitar dibujar líneas duplicadas
                if (string.Compare(node.nodeID, connectedNode.nodeID) < 0)
                {
                    DrawLine(node.transform.position, connectedNode.transform.position, lineColor);
                }
            }
        }

        Debug.Log($"Dibujando {allNodes.Length} nodos en el mapa.");
    }

    /// <summary>
    /// Dibuja una línea entre dos puntos utilizando un LineRenderer.
    /// </summary>
    /// <param name="start">Punto de inicio.</param>
    /// <param name="end">Punto de fin.</param>
    /// <param name="color">Color de la línea.</param>
    private void DrawLine(Vector3 start, Vector3 end, Color color)
    {
        GameObject line = new GameObject("Line");
        line.transform.parent = linesParent; // Para organizar en la jerarquía
        LineRenderer lr = line.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        // Configurar el ancho de la línea desde el Inspector
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;

        // Configurar el material desde el Inspector
        if (lineMaterial != null)
        {
            lr.material = lineMaterial;
        }
        else
        {
            // Si no se asigna un material, usar uno por defecto
            lr.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
        }

        // Configurar el color desde el Inspector
        lr.startColor = color;
        lr.endColor = color;
    }

    /// <summary>
    /// Instancia al jugador en la posición guardada en GameManager.
    /// </summary>
    private void InstantiatePlayer()
    {
        string playerNodeID = gameManager.GetCurrentPlayerNodeID();

        if (string.IsNullOrEmpty(playerNodeID))
        {
            Debug.LogWarning("No se ha guardado la posición del jugador. Instanciando al jugador en el nodo inicial.");
            // Supongamos que el nodo inicial es el primero de la lista
            if (gameManager.GetSavedMapData().Count > 0)
            {
                playerNodeID = gameManager.GetSavedMapData()[0].nodeID;
                gameManager.SetCurrentPlayerNodeID(playerNodeID);
            }
            else
            {
                Debug.LogError("No hay nodos en el boardState para instanciar al jugador.");
                return;
            }
        }

        // Buscar el nodo donde debe estar el jugador
        MapNode playerNode = FindMapNodeByID(playerNodeID);

        if (playerNode == null)
        {
            Debug.LogError($"No se encontró el nodo con nodeID: {playerNodeID} para instanciar al jugador.");
            return;
        }

        // Instanciar el prefab del jugador en la posición del nodo
        GameObject playerInstance = Instantiate(playerPrefab, playerNode.transform.position, Quaternion.identity);
        playerInstance.name = "Player";

        // Obtener el componente PlayerMovement
        PlayerMovement playerMovement = playerInstance.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.currentNode = playerNode;
            playerMovement.InitializePlayer();
        }
        else
        {
            Debug.LogError("El prefab del jugador no tiene el script PlayerMovement adjunto.");
        }

        // Asignar el jugador al CameraFollowZ
        CameraFollowZ cameraFollow = Camera.main.GetComponent<CameraFollowZ>();
        if (cameraFollow != null)
        {
            cameraFollow.target = playerInstance.transform;
        }
        else
        {
            Debug.LogError("La Main Camera no tiene el script CameraFollowZ adjunto.");
        }
    }

    /// <summary>
    /// Encuentra un MapNode por su nodeID.
    /// </summary>
    /// <param name="nodeID">ID del nodo a buscar.</param>
    /// <returns>MapNode correspondiente o null si no se encuentra.</returns>
    private MapNode FindMapNodeByID(string nodeID)
    {
        foreach (MapNode node in FindObjectsOfType<MapNode>())
        {
            if (node.nodeID == nodeID)
                return node;
        }
        return null;
    }

    /// <summary>
    /// Limpia los LineRenderers al destruir el BoardManager.
    /// </summary>
    void OnDestroy()
    {
        // Destruir todos los LineRenderers al destruir el BoardManager
        foreach (Transform child in linesParent)
        {
            if (child.GetComponent<LineRenderer>() != null)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
