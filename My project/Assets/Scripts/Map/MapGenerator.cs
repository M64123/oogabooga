// MapGenerator.cs
using UnityEngine;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    [System.Serializable]
    public struct NodePrefab
    {
        public NodeType nodeType;
        public GameObject prefab;
    }

    public List<NodePrefab> nodePrefabs;  // Lista de prefabs por tipo de nodo
    public GameObject playerPrefab;       // Prefab del jugador

    [HideInInspector]
    public List<MapNode> allNodes = new List<MapNode>();  // Lista de todos los nodos generados

    [Header("Map Settings")]
    public int totalLevels = 17;             // Niveles totales del mapa
    public float nodeSpacingX = 2f;          // Espaciado horizontal base entre nodos
    public float nodeSpacingZ = 2f;          // Espaciado en el eje Z entre niveles
    public float xVariation = 0.5f;          // Variación máxima en X para los nodos

    // Lista de nodos por nivel según la estructura deseada
    private List<int> nodesPerLevel = new List<int>
    {
        1, 1, 3, 3, 3, 1, 1, 3, 4, 4, 3, 1, 1, 3, 3, 3, 1
    };

    // Niveles específicos para Boss y GACHA
    private HashSet<int> bossLevels = new HashSet<int> { 6, 12, 17 };
    private HashSet<int> gachaLevels = new HashSet<int> { 2, 7, 13 };

    // Almacena los niveles generados
    private List<List<MapNode>> levels = new List<List<MapNode>>();

    // Diccionario para almacenar los nodos padres de cada nodo
    private Dictionary<MapNode, List<MapNode>> parentNodes = new Dictionary<MapNode, List<MapNode>>();

    // Listas de tipos de nodos
    private List<NodeType> goodNodes = new List<NodeType> { NodeType.Revivir, NodeType.Mana_Tamaño };
    private List<NodeType> badNodes = new List<NodeType> { NodeType.Combate, NodeType.CoinFlip };
    private List<NodeType> neutralNodes = new List<NodeType> { NodeType.Random };

    // Conteo de nodos buenos generados
    private int manaNodeCount = 0;
    private int revivirNodeCount = 0;

    [Header("Line Renderer Settings")]
    public float lineWidth = 0.05f;            // Ancho de la línea
    public Material lineMaterial;              // Material para el LineRenderer
    public Color lineColor = Color.white;      // Color de la línea

    // Objetos padres para organizar nodos y líneas en la jerarquía
    [Header("Parent Objects")]
    public Transform nodesParent;              // Parent para organizar los nodos
    public Transform linesParent;              // Parent para organizar las líneas

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
        manaNodeCount = 0;
        revivirNodeCount = 0;
        parentNodes.Clear();

        // Verificar que totalLevels coincide con nodesPerLevel
        if (nodesPerLevel.Count != totalLevels)
        {
            Debug.LogWarning("El número de elementos en nodesPerLevel no coincide con totalLevels. Ajustando totalLevels.");
            totalLevels = nodesPerLevel.Count;
        }

        Debug.Log($"Generando mapa con {totalLevels} niveles.");

        // Generar los niveles
        for (int level = 0; level < totalLevels; level++)
        {
            List<MapNode> currentLevelNodes = new List<MapNode>();
            int nodesAtLevel = nodesPerLevel[level];

            Debug.Log($"Nivel {level + 1}: Generando {nodesAtLevel} nodos.");

            List<float> nodePositionsX = new List<float>();

            // Generar posiciones X para los nodos
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

                    // Añadir una pequeña variación aleatoria
                    float xVariationValue = Random.Range(-xVariation, xVariation);
                    float xPos = baseX + xVariationValue;

                    nodePositionsX.Add(xPos);
                }
            }

            // Ordenar las posiciones X para mantener el orden y evitar cruces
            nodePositionsX.Sort();

            for (int i = 0; i < nodesAtLevel; i++)
            {
                float xPos = nodePositionsX[i];
                float zPos = level * nodeSpacingZ;
                Vector3 nodePos = new Vector3(xPos, 0, zPos);

                MapNode newNode = CreateNode(nodePos, level, i);
                allNodes.Add(newNode);
                currentLevelNodes.Add(newNode);
            }

            levels.Add(currentLevelNodes);
        }

        // Conectar los nodos entre niveles adyacentes
        ConnectNodes();

        // Asignar tipos a los nodos considerando los caminos
        AssignNodeTypes();

        // Asegurar que ambos tipos de nodos buenos estén presentes
        EnsureGoodNodesPresence();

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
    /// Guarda el estado actual del mapa en el GameManager.
    /// </summary>
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

    /// <summary>
    /// Carga el mapa desde los datos guardados en el GameManager.
    /// </summary>
    private void LoadMap()
    {
        List<MapNodeData> nodesData = GameManager.Instance.GetSavedMapData();
        allNodes.Clear();
        levels.Clear();
        parentNodes.Clear();
        manaNodeCount = 0;
        revivirNodeCount = 0;

        // Diccionario para mapear nodeID a MapNode
        Dictionary<string, MapNode> nodeDictionary = new Dictionary<string, MapNode>();

        // Reconstruir los nodos y niveles
        foreach (MapNodeData nodeData in nodesData)
        {
            // Crear un nuevo GameObject para el nodo
            GameObject nodeObj = new GameObject($"MapNode_{nodeData.nodeID}");
            nodeObj.transform.position = nodeData.position;
            nodeObj.transform.parent = nodesParent; // Organizar en la jerarquía

            // Añadir el componente MapNode
            MapNode node = nodeObj.AddComponent<MapNode>();

            // Configurar las propiedades del nodo basadas en MapNodeData
            node.nodeID = nodeData.nodeID;
            node.depthLevel = nodeData.depthLevel;
            node.nodeType = nodeData.nodeType;
            node.isActive = nodeData.isActive;
            node.connectedNodes = new List<MapNode>();
            node.nodeTypesInPaths = new HashSet<NodeType>();
            node.isTypeAssigned = true; // Asumimos que el tipo ya está asignado

            // Añadir al diccionario y a la lista general
            allNodes.Add(node);
            nodeDictionary[node.nodeID] = node;

            // Añadir el nodo al nivel correspondiente
            while (levels.Count <= node.depthLevel)
            {
                levels.Add(new List<MapNode>());
            }
            levels[node.depthLevel].Add(node);

            // Actualizar conteo de nodos buenos
            if (node.nodeType == NodeType.Mana_Tamaño)
            {
                manaNodeCount++;
            }
            else if (node.nodeType == NodeType.Revivir)
            {
                revivirNodeCount++;
            }
        }

        // Reconstruir las conexiones y los nodos padres
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

                    // Agregar al diccionario de nodos padres
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

        // Visualizar el mapa
        DrawMap();

        // Instanciar al jugador en el nodo guardado
        InstantiatePlayerAtSavedNode();

        Debug.Log("Mapa cargado desde GameManager.");
    }

    /// <summary>
    /// Asigna tipos a los nodos considerando los caminos.
    /// </summary>
    private void AssignNodeTypes()
    {
        if (levels.Count == 0)
        {
            Debug.LogError("No hay niveles generados para asignar tipos de nodos.");
            return;
        }

        // Inicializar los nodos del nivel inicial
        foreach (MapNode node in levels[0])
        {
            node.nodeTypesInPaths = new HashSet<NodeType>();
            node.nodeType = NodeType.Inicio;
            node.isTypeAssigned = true;
        }

        // Procesar los niveles restantes
        for (int level = 1; level < levels.Count; level++)
        {
            foreach (MapNode node in levels[level])
            {
                // Si el tipo ya ha sido asignado, continuar
                if (node.isTypeAssigned)
                    continue;

                // Obtener los nodos padres
                List<MapNode> parents;
                if (parentNodes.TryGetValue(node, out parents))
                {
                    // Combinar los conjuntos de tipos de los caminos que llegan a este nodo
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
                    // Si el nodo no tiene padres, iniciar nodeTypesInPaths vacío
                    node.nodeTypesInPaths = new HashSet<NodeType>();
                }

                // Asignar un tipo de nodo que no esté en typesInPath
                NodeType nodeType = GetNodeTypeForNode(node);

                // Si no se puede asignar un tipo sin repetir, permitir repeticiones
                if (nodeType == NodeType.None)
                {
                    nodeType = GetNodeTypeAllowingRepetition(node);
                }

                node.nodeType = nodeType;
                node.isTypeAssigned = true;

                // Actualizar conteo de nodos buenos
                if (nodeType == NodeType.Mana_Tamaño)
                {
                    manaNodeCount++;
                }
                else if (nodeType == NodeType.Revivir)
                {
                    revivirNodeCount++;
                }
            }
        }
    }

    /// <summary>
    /// Obtiene un tipo de nodo adecuado sin repetir tipos en el camino.
    /// </summary>
    private NodeType GetNodeTypeForNode(MapNode node)
    {
        // Niveles específicos
        int userLevel = node.depthLevel + 1;

        if (bossLevels.Contains(userLevel))
        {
            return NodeType.Boss;
        }
        else if (gachaLevels.Contains(userLevel))
        {
            return NodeType.GAMBLING;
        }

        // Obtener los tipos disponibles
        List<NodeType> availableTypes = new List<NodeType>();

        // Agregar todos los tipos posibles
        availableTypes.AddRange(goodNodes);
        availableTypes.AddRange(neutralNodes);
        availableTypes.AddRange(badNodes);

        // Remover los tipos que ya están en el camino
        foreach (NodeType type in node.nodeTypesInPaths)
        {
            availableTypes.Remove(type);
        }

        // Si no quedan tipos disponibles, retornar None
        if (availableTypes.Count == 0)
        {
            return NodeType.None;
        }

        // Seleccionar un tipo basado en probabilidades
        NodeType selectedType = SelectNodeTypeBasedOnProbability(availableTypes);

        return selectedType;
    }

    /// <summary>
    /// Obtiene un tipo de nodo permitiendo repeticiones si es necesario.
    /// </summary>
    private NodeType GetNodeTypeAllowingRepetition(MapNode node)
    {
        // Niveles específicos
        int userLevel = node.depthLevel + 1;

        if (bossLevels.Contains(userLevel))
        {
            return NodeType.Boss;
        }
        else if (gachaLevels.Contains(userLevel))
        {
            return NodeType.GAMBLING;
        }

        // Si no se pudo asignar sin repetir, permitir repeticiones
        List<NodeType> allTypes = new List<NodeType>();
        allTypes.AddRange(goodNodes);
        allTypes.AddRange(neutralNodes);
        allTypes.AddRange(badNodes);

        // Seleccionar un tipo basado en probabilidades
        NodeType selectedType = SelectNodeTypeBasedOnProbability(allTypes);

        return selectedType;
    }

    /// <summary>
    /// Selecciona un tipo de nodo basado en probabilidades definidas.
    /// </summary>
    private NodeType SelectNodeTypeBasedOnProbability(List<NodeType> availableTypes)
    {
        // Definir las probabilidades
        float goodProbability = 0.3f;
        float neutralProbability = 0.4f;
        float badProbability = 0.3f;

        // Filtrar los tipos disponibles por categoría
        List<NodeType> goodAvailable = availableTypes.FindAll(t => goodNodes.Contains(t));
        List<NodeType> neutralAvailable = availableTypes.FindAll(t => neutralNodes.Contains(t));
        List<NodeType> badAvailable = availableTypes.FindAll(t => badNodes.Contains(t));

        float totalProbability = 0f;
        if (goodAvailable.Count > 0) totalProbability += goodProbability;
        if (neutralAvailable.Count > 0) totalProbability += neutralProbability;
        if (badAvailable.Count > 0) totalProbability += badProbability;

        float randomValue = Random.value * totalProbability;

        if (randomValue < goodProbability && goodAvailable.Count > 0)
        {
            return goodAvailable[Random.Range(0, goodAvailable.Count)];
        }
        else if (randomValue < goodProbability + neutralProbability && neutralAvailable.Count > 0)
        {
            return neutralAvailable[Random.Range(0, neutralAvailable.Count)];
        }
        else if (badAvailable.Count > 0)
        {
            return badAvailable[Random.Range(0, badAvailable.Count)];
        }
        else
        {
            // Si no se pudo seleccionar según las probabilidades, elegir cualquiera disponible
            return availableTypes[Random.Range(0, availableTypes.Count)];
        }
    }

    /// <summary>
    /// Asegura que al menos un nodo de cada tipo bueno esté presente en el mapa.
    /// </summary>
    private void EnsureGoodNodesPresence()
    {
        // Si falta algún tipo de nodo bueno, reemplazar nodos existentes
        if (manaNodeCount == 0 || revivirNodeCount == 0)
        {
            foreach (MapNode node in allNodes)
            {
                if (manaNodeCount == 0 && goodNodes.Contains(node.nodeType))
                {
                    node.nodeType = NodeType.Mana_Tamaño;
                    manaNodeCount++;
                }
                else if (revivirNodeCount == 0 && goodNodes.Contains(node.nodeType))
                {
                    node.nodeType = NodeType.Revivir;
                    revivirNodeCount++;
                }

                if (manaNodeCount > 0 && revivirNodeCount > 0)
                {
                    break;
                }
            }
        }
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

            // Mantener un registro de los nodos del siguiente nivel que ya han sido conectados
            HashSet<MapNode> connectedNextNodes = new HashSet<MapNode>();

            foreach (MapNode currentNode in currentLevelNodes)
            {
                if (currentNode == null)
                {
                    Debug.LogError($"currentNode en nivel {level + 1} es null.");
                    continue;
                }

                // Determinar el número de conexiones según la probabilidad deseada
                int connections = (Random.value < 0.18f) ? 2 : 1; // 18% para 2 conexiones, 82% para 1 conexión

                // Obtener posibles conexiones
                List<MapNode> possibleConnections = GetPossibleConnections(currentNode, nextLevelNodes, connections);

                foreach (MapNode nextNode in possibleConnections)
                {
                    if (nextNode == null)
                    {
                        Debug.LogError($"nextNode es null al intentar conectar desde {currentNode.nodeID}.");
                        continue;
                    }

                    if (!currentNode.connectedNodes.Contains(nextNode))
                    {
                        currentNode.connectedNodes.Add(nextNode);
                        connectedNextNodes.Add(nextNode);

                        // Agregar al diccionario de nodos padres
                        if (!parentNodes.ContainsKey(nextNode))
                        {
                            parentNodes[nextNode] = new List<MapNode>();
                        }
                        parentNodes[nextNode].Add(currentNode);
                    }
                }
            }

            // Conectar nodos del siguiente nivel que no hayan sido conectados
            foreach (MapNode nextNode in nextLevelNodes)
            {
                if (nextNode == null)
                {
                    Debug.LogError($"nextNode en el nivel {level + 2} es null.");
                    continue;
                }

                if (!connectedNextNodes.Contains(nextNode))
                {
                    // Conectar con el nodo del nivel actual más cercano en X
                    MapNode closestNode = FindClosestNode(nextNode, currentLevelNodes);
                    if (closestNode != null)
                    {
                        closestNode.connectedNodes.Add(nextNode);
                        connectedNextNodes.Add(nextNode);

                        // Agregar al diccionario de nodos padres
                        if (!parentNodes.ContainsKey(nextNode))
                        {
                            parentNodes[nextNode] = new List<MapNode>();
                        }
                        parentNodes[nextNode].Add(closestNode);
                    }
                    else
                    {
                        Debug.LogError($"No se pudo encontrar un nodo cercano para conectar el nodo {nextNode.nodeID}.");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Obtiene posibles conexiones para un nodo dado.
    /// </summary>
    private List<MapNode> GetPossibleConnections(MapNode currentNode, List<MapNode> nextLevelNodes, int maxConnections)
    {
        List<MapNode> possibleConnections = new List<MapNode>();

        // Ordenar los nodos del siguiente nivel según la distancia al nodo actual en X
        List<MapNode> sortedNextLevelNodes = new List<MapNode>(nextLevelNodes);
        sortedNextLevelNodes.Sort((a, b) =>
        {
            float distanceA = Mathf.Abs(a.transform.position.x - currentNode.transform.position.x);
            float distanceB = Mathf.Abs(b.transform.position.x - currentNode.transform.position.x);
            return distanceA.CompareTo(distanceB);
        });

        // Agregar los nodos más cercanos hasta un máximo de 'maxConnections'
        for (int i = 0; i < Mathf.Min(maxConnections, sortedNextLevelNodes.Count); i++)
        {
            possibleConnections.Add(sortedNextLevelNodes[i]);
        }

        return possibleConnections;
    }

    /// <summary>
    /// Encuentra el nodo más cercano en X a un nodo objetivo dentro de una lista de nodos.
    /// </summary>
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

    /// <summary>
    /// Genera un identificador único para un nodo basado en su nivel y su índice.
    /// </summary>
    private string GenerateNodeID(int depth, int index)
    {
        return $"Level{depth + 1}_Node{index + 1}";
    }

    /// <summary>
    /// Crea un nodo en una posición específica con un nivel y un índice.
    /// </summary>
    private MapNode CreateNode(Vector3 position, int depth, int index)
    {
        GameObject nodeObj = new GameObject($"MapNode_{GenerateNodeID(depth, index)}");
        nodeObj.transform.position = position;
        nodeObj.transform.parent = nodesParent; // Organizar en la jerarquía

        MapNode node = nodeObj.AddComponent<MapNode>();
        node.nodeID = GenerateNodeID(depth, index);
        node.depthLevel = depth;
        node.nodeType = NodeType.None; // Asigna un tipo por defecto; se actualizará más adelante
        node.isActive = true; // Asigna el estado activo por defecto
        node.connectedNodes = new List<MapNode>(); // Inicializar la lista de nodos conectados
        node.nodeTypesInPaths = new HashSet<NodeType>(); // Inicializar el conjunto de tipos en caminos
        node.isTypeAssigned = false; // Indica que el tipo aún no ha sido asignado

        return node;
    }

    /// <summary>
    /// Dibuja el mapa instanciando prefabs de nodos y conectándolos con líneas.
    /// </summary>
    private void DrawMap()
    {
        Debug.Log($"Dibujando {allNodes.Count} nodos en el mapa.");
        foreach (MapNode node in allNodes)
        {
            // Obtener el prefab correspondiente al tipo de nodo
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
                    Debug.LogError($"No se encontró un prefab para el tipo de nodo {node.nodeType} ni para NodeType.None. Asegúrate de asignarlos en la lista nodePrefabs.");
                    continue;
                }
            }

            // Instanciar el prefab del nodo como hijo de nodesParent para mantener la jerarquía organizada
            GameObject nodeObjInstance = Instantiate(prefabToInstantiate, node.transform.position, Quaternion.identity, nodesParent);
            node.nodeObject = nodeObjInstance;

            // Asignar el MapNode al script de interacción
            NodeInteraction interaction = nodeObjInstance.GetComponent<NodeInteraction>();
            if (interaction != null)
            {
                interaction.node = node;
                interaction.InitializeNodeVisual(); // Inicializar la apariencia después de asignar 'node'
            }
            else
            {
                Debug.LogError("El objeto del nodo no tiene el componente NodeInteraction.");
            }

            // Configurar el nombre del nodo
            nodeObjInstance.name = $"Node_{node.nodeID}";

            // Dibujar las conexiones
            foreach (MapNode connectedNode in node.connectedNodes)
            {
                // Evitar dibujar líneas duplicadas
                if (string.Compare(node.nodeID, connectedNode.nodeID) < 0)
                {
                    DrawLine(node.transform.position, connectedNode.transform.position, lineColor);
                }
            }
        }
    }

    /// <summary>
    /// Obtiene el prefab correspondiente a un tipo de nodo.
    /// </summary>
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

    /// <summary>
    /// Dibuja una línea entre dos puntos utilizando un LineRenderer.
    /// </summary>
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
    /// Instancia al jugador en el nodo inicial (primer nodo del primer nivel).
    /// </summary>
    private void InstantiatePlayerAtStartNode()
    {
        if (playerPrefab != null && allNodes.Count > 0)
        {
            // Obtener el nodo inicial (primer nodo del primer nivel)
            MapNode startNode = allNodes[0];

            // Instanciar al jugador en la posición del nodo inicial
            GameObject playerObj = Instantiate(playerPrefab, startNode.transform.position, Quaternion.identity);

            // Asignar el nodo actual en el script PlayerMovement
            PlayerMovement playerMovement = playerObj.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.currentNode = startNode;
                playerMovement.InitializePlayer();

                // Guardar el nodeID del nodo actual en el GameManager
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SetCurrentPlayerNodeID(startNode.nodeID);
                }
            }
            else
            {
                Debug.LogError("PlayerMovement script no encontrado en el prefab del jugador.");
            }

            // Asignar el jugador al script CameraFollowZ
            CameraFollowZ cameraFollow = Camera.main.GetComponent<CameraFollowZ>();
            if (cameraFollow != null)
            {
                cameraFollow.target = playerObj.transform;
            }
            else
            {
                Debug.LogError("CameraFollowZ script not found on the Main Camera.");
            }
        }
        else
        {
            Debug.LogError("PlayerPrefab no asignado o no hay nodos generados.");
        }
    }

    /// <summary>
    /// Instancia al jugador en el nodo guardado.
    /// </summary>
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

            // Instanciar al jugador en la posición del nodo guardado
            GameObject playerObj = Instantiate(playerPrefab, startNode.transform.position, Quaternion.identity);

            // Asignar el nodo actual en el script PlayerMovement
            PlayerMovement playerMovement = playerObj.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.currentNode = startNode;
                playerMovement.InitializePlayer();

                // Asegurarse de que el GameManager tenga el nodeID actualizado
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SetCurrentPlayerNodeID(startNode.nodeID);
                }
            }
            else
            {
                Debug.LogError("PlayerMovement script no encontrado en el prefab del jugador.");
            }

            // Asignar el jugador al script CameraFollowZ
            CameraFollowZ cameraFollow = Camera.main.GetComponent<CameraFollowZ>();
            if (cameraFollow != null)
            {
                cameraFollow.target = playerObj.transform;
            }
            else
            {
                Debug.LogError("CameraFollowZ script not found on the Main Camera.");
            }
        }
        else
        {
            Debug.LogError("PlayerPrefab no asignado o no hay nodos generados.");
        }
    }
}
