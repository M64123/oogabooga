using UnityEngine;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    public int totalLevels = 17;             // Niveles totales del mapa
    public float nodeSpacingX = 2f;          // Espaciado horizontal base entre nodos
    public float nodeSpacingZ = 2f;          // Espaciado en el eje Z entre niveles
    public float xVariation = 0.5f;          // Variación máxima en X para los nodos

    // Estructura para asociar NodeType con su prefab
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

    // Lista de nodos por nivel según la estructura deseada
    private List<int> nodesPerLevel = new List<int>
    {
        1, 1, 3, 3, 3, 1, 1, 3, 4, 4, 3, 1, 1, 3, 3, 3, 1
    };

    // Listas de niveles específicos para Boss y GAMBLING
    private HashSet<int> bossLevels = new HashSet<int> { 6, 12, 17 };
    private HashSet<int> gamblingLevels = new HashSet<int> { 2, 7, 13 };

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

    void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        // Limpiar las listas por si se llama más de una vez
        levels.Clear();
        allNodes.Clear();
        manaNodeCount = 0;
        revivirNodeCount = 0;
        parentNodes.Clear();

        // Verificar que totalLevels coincide con nodesPerLevel
        if (totalLevels != nodesPerLevel.Count)
        {
            totalLevels = nodesPerLevel.Count;
            Debug.LogWarning("El total de niveles se ha ajustado para coincidir con nodesPerLevel.");
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

                MapNode newNode = CreateNode(nodePos, level);
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

        // Instanciar al jugador en el nodo inicial
        InstantiatePlayerAtStartNode();
    }

    private void AssignNodeTypes()
    {
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

    private NodeType GetNodeTypeForNode(MapNode node)
    {
        // Niveles específicos
        int userLevel = node.depthLevel + 1;

        if (bossLevels.Contains(userLevel))
        {
            return NodeType.Boss;
        }
        else if (gamblingLevels.Contains(userLevel))
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

    private NodeType GetNodeTypeAllowingRepetition(MapNode node)
    {
        // Niveles específicos
        int userLevel = node.depthLevel + 1;

        if (bossLevels.Contains(userLevel))
        {
            return NodeType.Boss;
        }
        else if (gamblingLevels.Contains(userLevel))
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
                // Determinar el número de conexiones según la probabilidad deseada
                int connections = (Random.value < 0.18f) ? 2 : 1; // 18% para 2 conexiones, 82% para 1 conexión

                // Obtener posibles conexiones
                List<MapNode> possibleConnections = GetPossibleConnections(currentNode, nextLevelNodes, connections);

                foreach (MapNode nextNode in possibleConnections)
                {
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
                if (!connectedNextNodes.Contains(nextNode))
                {
                    // Conectar con el nodo del nivel actual más cercano en X
                    MapNode closestNode = FindClosestNode(nextNode, currentLevelNodes);
                    closestNode.connectedNodes.Add(nextNode);
                    connectedNextNodes.Add(nextNode);

                    // Agregar al diccionario de nodos padres
                    if (!parentNodes.ContainsKey(nextNode))
                    {
                        parentNodes[nextNode] = new List<MapNode>();
                    }
                    parentNodes[nextNode].Add(closestNode);
                }
            }
        }
    }

    private List<MapNode> GetPossibleConnections(MapNode currentNode, List<MapNode> nextLevelNodes, int maxConnections)
    {
        List<MapNode> possibleConnections = new List<MapNode>();

        // Ordenar los nodos del siguiente nivel según la distancia al nodo actual en X
        List<MapNode> sortedNextLevelNodes = new List<MapNode>(nextLevelNodes);
        sortedNextLevelNodes.Sort((a, b) =>
        {
            float distanceA = Mathf.Abs(a.position.x - currentNode.position.x);
            float distanceB = Mathf.Abs(b.position.x - currentNode.position.x);
            return distanceA.CompareTo(distanceB);
        });

        // Agregar los nodos más cercanos hasta un máximo de 'maxConnections'
        for (int i = 0; i < Mathf.Min(maxConnections, sortedNextLevelNodes.Count); i++)
        {
            possibleConnections.Add(sortedNextLevelNodes[i]);
        }

        return possibleConnections;
    }

    private MapNode FindClosestNode(MapNode targetNode, List<MapNode> nodes)
    {
        MapNode closestNode = null;
        float minDistance = float.MaxValue;

        foreach (MapNode node in nodes)
        {
            float distance = Mathf.Abs(node.position.x - targetNode.position.x);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestNode = node;
            }
        }

        return closestNode;
    }

    private MapNode CreateNode(Vector3 position, int depth)
    {
        MapNode node = new MapNode(position, depth);
        return node;
    }

    private void DrawMap()
    {
        Debug.Log($"Dibujando {allNodes.Count} nodos en el mapa.");
        foreach (MapNode node in allNodes)
        {
            // Obtener el prefab correspondiente al tipo de nodo
            GameObject prefabToInstantiate = GetPrefabForNodeType(node.nodeType);
            if (prefabToInstantiate == null)
            {
                Debug.LogError($"No se encontró un prefab para el tipo de nodo {node.nodeType}. Asegúrate de asignarlo en el Inspector.");
                continue;
            }

            // Instanciar el prefab del nodo
            GameObject nodeObj = Instantiate(prefabToInstantiate, node.position, Quaternion.identity);
            node.nodeObject = nodeObj;

            // Asignar el MapNode al script de interacción
            NodeInteraction interaction = nodeObj.GetComponentInChildren<NodeInteraction>();
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
            nodeObj.name = $"Node_Level{node.depthLevel + 1}_{node.nodeType}";

            // Dibujar las conexiones
            foreach (MapNode connectedNode in node.connectedNodes)
            {
                DrawLine(node.position, connectedNode.position, Color.white);
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
        line.transform.parent = this.transform; // Para organizar en la jerarquía
        LineRenderer lr = line.AddComponent<LineRenderer>();
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        // Asegurarse de que el material soporte el rendering de líneas
        lr.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
        lr.startColor = color;
        lr.endColor = color;
    }

    private void InstantiatePlayerAtStartNode()
    {
        if (playerPrefab != null && allNodes.Count > 0)
        {
            // Obtener el nodo inicial (primer nodo del primer nivel)
            MapNode startNode = allNodes[0];

            // Instanciar al jugador en la posición del nodo inicial
            GameObject playerObj = Instantiate(playerPrefab, startNode.position, Quaternion.identity);

            // Asignar el nodo actual en el script PlayerMovement
            PlayerMovement playerMovement = playerObj.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.currentNode = startNode;
                playerMovement.InitializePlayer();
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
