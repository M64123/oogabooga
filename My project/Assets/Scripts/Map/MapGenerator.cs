// MapGenerator.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;


public class MapGenerator : MonoBehaviour
{
    [System.Serializable]
    public struct NodePrefab
    {
        public NodeType nodeType;
        public GameObject prefab;
    }

    [Header("Prefabs & Player")]
    public List<NodePrefab> nodePrefabs;    // Lista de prefabs por tipo de nodo
    public GameObject playerPrefab;         // Prefab del jugador

    [HideInInspector]
    public List<MapNode> allNodes = new List<MapNode>();  // Lista de todos los nodos generados

    [Header("Map Settings")]
    [Tooltip("Cantidad total de niveles del mapa. Esto debe coincidir con la longitud de nodesPerLevel.")]
    public int totalLevels = 17;
    [Tooltip("Lista de nodos por nivel. Editar desde el Inspector.")]
    public List<int> nodesPerLevel = new List<int>(); // Ahora editable desde el inspector

    [Tooltip("Niveles donde se colocan Boss (1-based index). Editar desde el inspector.")]
    public List<int> bossLevels = new List<int>();
    [Tooltip("Niveles donde se colocan GACHA (1-based index). Editar desde el inspector.")]
    public List<int> gachaLevels = new List<int>();

    [Tooltip("Espaciado horizontal base entre nodos.")]
    public float nodeSpacingX = 2f;
    [Tooltip("Espaciado en el eje Z entre niveles.")]
    public float nodeSpacingZ = 2f;
    [Tooltip("Variación máxima en X para la posición de un nodo.")]
    public float xVariation = 0.5f;

    [Header("Categories")]
    [Tooltip("Tipo de nodo considerado bueno.")]
    public NodeType goodNodeType = NodeType.CoinFlip;
    [Tooltip("Tipo de nodo considerado malo.")]
    public NodeType badNodeType = NodeType.Combate;
    [Tooltip("Tipo de nodo considerado neutral.")]
    public NodeType neutralNodeType = NodeType.Random;

    [Header("Line Renderer Settings")]
    public float lineWidth = 0.05f;            // Ancho de la línea
    public Material lineMaterial;              // Material para el LineRenderer
    public Color lineColor = Color.white;      // Color de la línea

    [Header("Parent Objects")]
    public Transform nodesParent;              // Parent para organizar los nodos
    public Transform linesParent;              // Parent para organizar las líneas

    // Almacena los niveles generados
    private List<List<MapNode>> levels = new List<List<MapNode>>();

    // Diccionario para almacenar los nodos padres de cada nodo
    private Dictionary<MapNode, List<MapNode>> parentNodes = new Dictionary<MapNode, List<MapNode>>();

    // Para facilitar la lógica, mantenemos las categorías internamente:
    // Ajustadas a los requerimientos actuales: solo uno bueno, uno malo, uno neutral.
    // Inicio y Boss y GACHA son manejados por reglas fijas.
    private NodeType startNodeType = NodeType.Inicio;
    private NodeType gachaNodeType = NodeType.GAMBLING;
    private NodeType bossNodeType = NodeType.Boss;

    void Start()
    {
        // Verificar si hay un mapa guardado en el GameManager
        if (GameManager.Instance != null && GameManager.Instance.GetSavedMapData() != null && GameManager.Instance.GetSavedMapData().Count > 0)
        {
            LoadMap();
        }
        else
        {
            GenerateMap();
        }
    }

    /// <summary>
    /// Genera el mapa desde cero.
    /// </summary>
    public void GenerateMap()
    {
        // Limpiar las listas por si se llama más de una vez
        levels.Clear();
        allNodes.Clear();
        parentNodes.Clear();

        // Verificar que totalLevels coincide con nodesPerLevel
        if (nodesPerLevel.Count != totalLevels)
        {
            Debug.LogWarning("El número de elementos en nodesPerLevel no coincide con totalLevels. Ajustando totalLevels.");
            totalLevels = nodesPerLevel.Count;
        }

        Debug.Log($"Generando mapa con {totalLevels} niveles.");

        // Generar los niveles (solo nodos con posición, tipo None inicial)
        for (int level = 0; level < totalLevels; level++)
        {
            List<MapNode> currentLevelNodes = GenerateLevelNodes(level, nodesPerLevel[level]);
            levels.Add(currentLevelNodes);
            allNodes.AddRange(currentLevelNodes);
        }

        // Conectar los nodos entre niveles adyacentes
        ConnectNodes();

        // Asignar tipos a los nodos considerando los caminos
        AssignNodeTypes();

        // Visualizar el mapa
        DrawMap();

        // Instanciar al jugador en el nodo inicial o guardado
        if (GameManager.Instance != null && !string.IsNullOrEmpty(GameManager.Instance.GetCurrentPlayerNodeID()))
        {
            InstantiatePlayerAtSavedNode();
        }
        else
        {
            InstantiatePlayerAtStartNode();
        }

        // Guardar el estado del mapa en el GameManager
        SaveMapState();
    }

    /// <summary>
    /// Crea los nodos de un nivel dado.
    /// </summary>
    private List<MapNode> GenerateLevelNodes(int level, int nodesAtLevel)
    {
        Debug.Log($"Nivel {level + 1}: Generando {nodesAtLevel} nodos.");

        List<MapNode> currentLevelNodes = new List<MapNode>();
        List<float> nodePositionsX = GenerateNodePositionsX(nodesAtLevel);

        for (int i = 0; i < nodesAtLevel; i++)
        {
            float xPos = nodePositionsX[i];
            float zPos = level * nodeSpacingZ;
            Vector3 nodePos = new Vector3(xPos, 0, zPos);

            MapNode newNode = CreateNode(nodePos, level, i);
            currentLevelNodes.Add(newNode);
        }

        return currentLevelNodes;
    }

    /// <summary>
    /// Genera las posiciones X de los nodos de un nivel.
    /// </summary>
    private List<float> GenerateNodePositionsX(int nodesAtLevel)
    {
        List<float> nodePositionsX = new List<float>();

        if (nodesAtLevel == 1)
        {
            // Nodo único en X = 0
            nodePositionsX.Add(0f);
        }
        else
        {
            float startX = -((nodesAtLevel - 1) * nodeSpacingX) / 2f;
            for (int i = 0; i < nodesAtLevel; i++)
            {
                float baseX = startX + i * nodeSpacingX;
                float xVariationValue = Random.Range(-xVariation, xVariation);
                float xPos = baseX + xVariationValue;
                nodePositionsX.Add(xPos);
            }
        }

        // Ordenar las posiciones X para mantener el orden y evitar cruces
        nodePositionsX.Sort();
        return nodePositionsX;
    }

    /// <summary>
    /// Carga el mapa desde los datos guardados en el GameManager.
    /// </summary>
    private void LoadMap()
    {
        List<MapNodeData> nodesData = GameManager.Instance.GetSavedMapData();
        allNodes.Clear();
        levels.Clear();
        parentNodes.Clear();

        Dictionary<string, MapNode> nodeDictionary = new Dictionary<string, MapNode>();

        // Reconstruir los nodos y niveles
        foreach (MapNodeData nodeData in nodesData)
        {
            GameObject nodeObj = new GameObject($"MapNode_{nodeData.nodeID}");
            nodeObj.transform.position = nodeData.position;
            nodeObj.transform.parent = nodesParent;

            MapNode node = nodeObj.AddComponent<MapNode>();
            node.nodeID = nodeData.nodeID;
            node.depthLevel = nodeData.depthLevel;
            node.nodeType = nodeData.nodeType;
            node.isActive = nodeData.isActive;
            node.connectedNodes = new List<MapNode>();
            node.nodeTypesInPaths = new HashSet<NodeType>();
            node.isTypeAssigned = true;

            allNodes.Add(node);
            nodeDictionary[node.nodeID] = node;

            while (levels.Count <= node.depthLevel)
            {
                levels.Add(new List<MapNode>());
            }
            levels[node.depthLevel].Add(node);
        }

        // Reconstruir las conexiones
        foreach (MapNodeData nodeData in nodesData)
        {
            if (!nodeDictionary.TryGetValue(nodeData.nodeID, out MapNode node))
            {
                Debug.LogError($"No se encontró el nodo con nodeID: {nodeData.nodeID}");
                continue;
            }

            foreach (string connectedID in nodeData.connectedNodeIDs)
            {
                if (nodeDictionary.TryGetValue(connectedID, out MapNode connectedNode))
                {
                    node.connectedNodes.Add(connectedNode);

                    if (!parentNodes.ContainsKey(connectedNode))
                    {
                        parentNodes[connectedNode] = new List<MapNode>();
                    }
                    parentNodes[connectedNode].Add(node);
                }
                else
                {
                    Debug.LogError($"No se encontró el nodo conectado con nodeID: {connectedID}");
                }
            }
        }

        DrawMap();
        InstantiatePlayerAtSavedNode();

        Debug.Log("Mapa cargado desde GameManager.");
    }

    /// <summary>
    /// Asigna tipos a los nodos considerando los caminos.
    /// Reglas:
    /// - El primer nivel: Inicio (primer nodo)
    /// - Después del inicio (el siguiente nivel definido): GACHA
    /// - Boss en niveles definidos por bossLevels.
    /// - GACHA en niveles definidos por gachaLevels.
    /// - El resto de nodos: Combate (malo), CoinFlip (bueno), Random (neutral) según disponibilidad.
    /// </summary>
    private void AssignNodeTypes()
    {
        if (levels.Count == 0)
        {
            Debug.LogError("No hay niveles generados para asignar tipos de nodos.");
            return;
        }

        // Nivel inicial: Inicio
        foreach (MapNode node in levels[0])
        {
            node.nodeTypesInPaths = new HashSet<NodeType>();
            node.nodeType = startNodeType;
            node.isTypeAssigned = true;
        }

        // Procesar los siguientes niveles
        for (int level = 1; level < levels.Count; level++)
        {
            foreach (MapNode node in levels[level])
            {
                if (node.isTypeAssigned)
                    continue;

                // Obtener nodos padres
                List<MapNode> parents;
                if (parentNodes.TryGetValue(node, out parents))
                {
                    HashSet<NodeType> typesInPath = new HashSet<NodeType>();
                    foreach (MapNode parent in parents)
                    {
                        typesInPath.UnionWith(parent.nodeTypesInPaths);
                        typesInPath.Add(parent.nodeType);
                    }
                    node.nodeTypesInPaths = typesInPath;
                }
                else
                {
                    node.nodeTypesInPaths = new HashSet<NodeType>();
                }

                // Asignar tipo especial si es nivel Boss o GACHA
                int currentLevel = node.depthLevel + 1; // niveles 1-based
                if (bossLevels.Contains(currentLevel))
                {
                    node.nodeType = bossNodeType;
                }
                else if (gachaLevels.Contains(currentLevel))
                {
                    node.nodeType = gachaNodeType;
                }
                else
                {
                    // Asignar uno de los tipos (bueno, malo, neutral)
                    node.nodeType = SelectNodeTypeForRegularNode(node.nodeTypesInPaths);
                }

                node.isTypeAssigned = true;
            }
        }
    }

    /// <summary>
    /// Selecciona el tipo de nodo (bueno, malo, neutral) para un nodo "regular"
    /// asegurando variedad. Puedes ajustar las probabilidades aquí si lo requieres.
    /// </summary>
    private NodeType SelectNodeTypeForRegularNode(HashSet<NodeType> typesInPath)
    {
        // Tipos disponibles
        // Según las restricciones actuales:
        // Bueno: CoinFlip
        // Malo: Combate
        // Neutral: Random
        // Se puede agregar una lógica de probabilidad si se desea.
        // Por ahora, intentaremos no repetir el mismo tipo ya en el camino.

        List<NodeType> candidates = new List<NodeType> { goodNodeType, badNodeType, neutralNodeType };
        candidates.RemoveAll(t => typesInPath.Contains(t));

        if (candidates.Count == 0)
        {
            // Si no podemos evitar repetir, repetimos
            candidates = new List<NodeType> { goodNodeType, badNodeType, neutralNodeType };
        }

        // Aquí se podría meter una lógica de pesos, por simplicidad elegimos aleatorio uniforme
        return candidates[Random.Range(0, candidates.Count)];
    }

    /// <summary>
    /// Conecta los nodos entre niveles adyacentes.
    /// </summary>
    private void ConnectNodes()
    {
        parentNodes = new Dictionary<MapNode, List<MapNode>>();

        for (int level = 0; level < levels.Count - 1; level++)
        {
            List<MapNode> currentLevelNodes = levels[level];
            List<MapNode> nextLevelNodes = levels[level + 1];

            HashSet<MapNode> connectedNextNodes = new HashSet<MapNode>();

            foreach (MapNode currentNode in currentLevelNodes)
            {
                if (currentNode == null) continue;

                // Determinar conexiones: 18% para 2 conexiones, 82% para 1 conexión
                int connections = (Random.value < 0.18f) ? 2 : 1;

                List<MapNode> possibleConnections = GetPossibleConnections(currentNode, nextLevelNodes, connections);

                foreach (MapNode nextNode in possibleConnections)
                {
                    if (nextNode == null) continue;
                    if (!currentNode.connectedNodes.Contains(nextNode))
                    {
                        currentNode.connectedNodes.Add(nextNode);
                        connectedNextNodes.Add(nextNode);

                        if (!parentNodes.ContainsKey(nextNode))
                            parentNodes[nextNode] = new List<MapNode>();
                        parentNodes[nextNode].Add(currentNode);
                    }
                }
            }

            // Conectar nodos del siguiente nivel que no hayan sido conectados
            foreach (MapNode nextNode in nextLevelNodes)
            {
                if (nextNode == null)
                    continue;

                if (!connectedNextNodes.Contains(nextNode))
                {
                    MapNode closestNode = FindClosestNode(nextNode, currentLevelNodes);
                    if (closestNode != null && !closestNode.connectedNodes.Contains(nextNode))
                    {
                        closestNode.connectedNodes.Add(nextNode);
                        connectedNextNodes.Add(nextNode);

                        if (!parentNodes.ContainsKey(nextNode))
                            parentNodes[nextNode] = new List<MapNode>();
                        parentNodes[nextNode].Add(closestNode);
                    }
                }
            }
        }
    }

    private List<MapNode> GetPossibleConnections(MapNode currentNode, List<MapNode> nextLevelNodes, int maxConnections)
    {
        List<MapNode> sortedNextLevelNodes = new List<MapNode>(nextLevelNodes);
        sortedNextLevelNodes.Sort((a, b) =>
        {
            float distA = Mathf.Abs(a.transform.position.x - currentNode.transform.position.x);
            float distB = Mathf.Abs(b.transform.position.x - currentNode.transform.position.x);
            return distA.CompareTo(distB);
        });

        return sortedNextLevelNodes.Take(Mathf.Min(maxConnections, sortedNextLevelNodes.Count)).ToList();
    }

    private MapNode FindClosestNode(MapNode targetNode, List<MapNode> nodes)
    {
        MapNode closestNode = null;
        float minDistance = float.MaxValue;

        foreach (MapNode node in nodes)
        {
            float distance = Mathf.Abs(node.transform.position.x - targetNode.transform.position.x);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestNode = node;
            }
        }

        return closestNode;
    }

    private string GenerateNodeID(int depth, int index)
    {
        return $"Level{depth + 1}_Node{index + 1}";
    }

    private MapNode CreateNode(Vector3 position, int depth, int index)
    {
        GameObject nodeObj = new GameObject($"MapNode_{GenerateNodeID(depth, index)}");
        nodeObj.transform.position = position;
        nodeObj.transform.parent = nodesParent; // Organizar en la jerarquía

        MapNode node = nodeObj.AddComponent<MapNode>();
        node.nodeID = GenerateNodeID(depth, index);
        node.depthLevel = depth;
        node.nodeType = NodeType.None;
        node.isActive = true;
        node.connectedNodes = new List<MapNode>();
        node.nodeTypesInPaths = new HashSet<NodeType>();
        node.isTypeAssigned = false;

        return node;
    }

    private void DrawMap()
    {
        Debug.Log($"Dibujando {allNodes.Count} nodos en el mapa.");
        foreach (MapNode node in allNodes)
        {
            GameObject prefabToInstantiate = GetPrefabForNodeType(node.nodeType);
            if (prefabToInstantiate == null)
            {
                // Intentar obtener el prefab para NodeType.None
                NodePrefab nonePrefab = nodePrefabs.Find(np => np.nodeType == NodeType.None);
                if (nonePrefab.prefab != null)
                {
                    prefabToInstantiate = nonePrefab.prefab;
                }
                else
                {
                    Debug.LogError($"No se encontró un prefab para {node.nodeType} ni para None.");
                    continue;
                }
            }

            GameObject nodeObjInstance = Instantiate(prefabToInstantiate, node.transform.position, Quaternion.identity, nodesParent);
            node.nodeObject = nodeObjInstance;

            NodeInteraction interaction = nodeObjInstance.GetComponent<NodeInteraction>();
            if (interaction != null)
            {
                interaction.node = node;
                interaction.InitializeNodeVisual();
            }
            else
            {
                Debug.LogError("El objeto del nodo no tiene el componente NodeInteraction.");
            }

            nodeObjInstance.name = $"Node_{node.nodeID}";

            foreach (MapNode connectedNode in node.connectedNodes)
            {
                if (string.Compare(node.nodeID, connectedNode.nodeID) < 0)
                {
                    DrawLine(node.transform.position, connectedNode.transform.position, lineColor);
                }
            }
        }
    }

    private GameObject GetPrefabForNodeType(NodeType nodeType)
    {
        foreach (NodePrefab np in nodePrefabs)
        {
            if (np.nodeType == nodeType)
            {
                return np.prefab;
            }
        }
        return null;
    }

    private void DrawLine(Vector3 start, Vector3 end, Color color)
    {
        GameObject line = new GameObject("Line");
        line.transform.parent = linesParent;
        LineRenderer lr = line.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;

        if (lineMaterial != null)
        {
            lr.material = lineMaterial;
        }
        else
        {
            lr.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
        }

        lr.startColor = color;
        lr.endColor = color;
    }

    private void InstantiatePlayerAtStartNode()
    {
        if (playerPrefab != null && allNodes.Count > 0)
        {
            MapNode startNode = allNodes[0];
            GameObject playerObj = Instantiate(playerPrefab, startNode.transform.position, Quaternion.identity);
            PlayerMovement playerMovement = playerObj.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.currentNode = startNode;
                playerMovement.InitializePlayer();
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SetCurrentPlayerNodeID(startNode.nodeID);
                }
            }
            else
            {
                Debug.LogError("PlayerMovement script no encontrado en el prefab del jugador.");
            }

            CameraFollowZ cameraFollow = Camera.main.GetComponent<CameraFollowZ>();
            if (cameraFollow != null)
            {
                cameraFollow.target = playerObj.transform;
            }
            else
            {
                Debug.LogError("CameraFollowZ script no encontrado en la cámara principal.");
            }
        }
        else
        {
            Debug.LogError("PlayerPrefab no asignado o no hay nodos generados.");
        }
    }

    private void InstantiatePlayerAtSavedNode()
    {
        if (playerPrefab != null && allNodes.Count > 0)
        {
            string nodeID = GameManager.Instance.GetCurrentPlayerNodeID();
            MapNode startNode = allNodes.Find(node => node.nodeID == nodeID);

            if (startNode == null)
            {
                Debug.LogError("No se encontró el nodo guardado. Instanciando en el nodo inicial.");
                startNode = allNodes[0];
            }

            GameObject playerObj = Instantiate(playerPrefab, startNode.transform.position, Quaternion.identity);
            PlayerMovement playerMovement = playerObj.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.currentNode = startNode;
                playerMovement.InitializePlayer();
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SetCurrentPlayerNodeID(startNode.nodeID);
                }
            }
            else
            {
                Debug.LogError("PlayerMovement script no encontrado en el prefab del jugador.");
            }

            CameraFollowZ cameraFollow = Camera.main.GetComponent<CameraFollowZ>();
            if (cameraFollow != null)
            {
                cameraFollow.target = playerObj.transform;
            }
            else
            {
                Debug.LogError("CameraFollowZ script no encontrado en la cámara principal.");
            }
        }
        else
        {
            Debug.LogError("PlayerPrefab no asignado o no hay nodos generados.");
        }
    }

    private void SaveMapState()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SaveMapState(allNodes);
            Debug.Log("Estado del mapa guardado en GameManager.");
        }
        else
        {
            Debug.LogError("GameManager.Instance es null. Asegúrate de que GameManager esté presente en la escena.");
        }
    }
}
