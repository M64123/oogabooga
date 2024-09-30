using UnityEngine;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    public int totalLevels = 10;             // Niveles totales del mapa
    public float nodeSpacingX = 2f;          // Espaciado horizontal base entre nodos
    public float nodeSpacingZ = 2f;          // Espaciado en el eje Z entre niveles
    public float xVariation = 0.5f;          // Variación máxima en X para los nodos
    public GameObject nodePrefab;            // Prefab del nodo
    public GameObject playerPrefab;          // Prefab del jugador

    [HideInInspector]
    public List<MapNode> allNodes = new List<MapNode>();  // Lista de todos los nodos generados

    // Niveles de bifurcación y número de opciones en cada uno
    private Dictionary<int, int> branchingLevels = new Dictionary<int, int>
    {
        { 2, 3 },  // En el nivel 2, hay 3 opciones
        { 5, 4 },  // En el nivel 5, hay 4 opciones
        { 8, 3 }   // En el nivel 8, hay 3 opciones
    };

    // Niveles donde las bifurcaciones convergen
    private HashSet<int> convergingLevels = new HashSet<int> { 4, 7 };

    // Mover levels fuera de GenerateMap() para que sea accesible en toda la clase
    private List<List<MapNode>> levels = new List<List<MapNode>>();

    void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        // Limpiar las listas por si se llama más de una vez
        levels.Clear();
        allNodes.Clear();

        // Generar los niveles
        for (int level = 0; level < totalLevels; level++)
        {
            List<MapNode> currentLevelNodes = new List<MapNode>();
            int nodesAtLevel = 1;

            // Verificar si es el último nivel
            if (level == totalLevels - 1)
            {
                nodesAtLevel = 1;  // Último nivel siempre con un solo nodo
            }
            // Verificar si es un nivel de bifurcación
            else if (branchingLevels.ContainsKey(level))
            {
                nodesAtLevel = branchingLevels[level];
            }
            // Verificar si es un nivel de convergencia
            else if (convergingLevels.Contains(level))
            {
                nodesAtLevel = 1;  // Convergencia a un solo camino
            }
            // Si no, mantener el número de nodos del nivel anterior
            else if (level > 0)
            {
                nodesAtLevel = levels[level - 1].Count;
            }

            List<float> nodePositionsX = new List<float>();

            // Generar posiciones X para los nodos
            if (level == 0)
            {
                // Nodo inicial en X = 0
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

                // Asignar un tipo de evento al nodo
                NodeType nodeType = GetRandomNodeType(level);

                MapNode newNode = CreateNode(nodePos, level, nodeType);
                allNodes.Add(newNode);
                currentLevelNodes.Add(newNode);
            }

            levels.Add(currentLevelNodes);
        }

        // Conectar los nodos entre niveles adyacentes
        for (int level = 0; level < totalLevels - 1; level++)
        {
            List<MapNode> currentLevelNodes = levels[level];
            List<MapNode> nextLevelNodes = levels[level + 1];

            // Mantener un registro de qué nodos del siguiente nivel han sido conectados
            HashSet<MapNode> connectedNextNodes = new HashSet<MapNode>();

            for (int i = 0; i < currentLevelNodes.Count; i++)
            {
                MapNode currentNode = currentLevelNodes[i];

                // Determinar el número de conexiones (1 o 2)
                int connections = Random.Range(1, 3); // 1 o 2 conexiones
                List<MapNode> possibleConnections = GetPossibleConnections(currentNode, nextLevelNodes, connections);

                foreach (MapNode nextNode in possibleConnections)
                {
                    if (!currentNode.connectedNodes.Contains(nextNode))
                    {
                        currentNode.connectedNodes.Add(nextNode);
                        connectedNextNodes.Add(nextNode); // Marcar el nodo como conectado
                    }
                }
            }

            // Verificar si hay nodos en el siguiente nivel que no han sido conectados
            foreach (MapNode nextNode in nextLevelNodes)
            {
                if (!connectedNextNodes.Contains(nextNode))
                {
                    // Encontrar el nodo más cercano en el nivel actual
                    MapNode closestCurrentNode = FindClosestNode(nextNode, currentLevelNodes);

                    // Añadir la conexión
                    closestCurrentNode.connectedNodes.Add(nextNode);
                    connectedNextNodes.Add(nextNode); // Marcar el nodo como conectado
                }
            }
        }

        // Visualizar el mapa
        DrawMap();

        // Instanciar al jugador en el nodo inicial
        InstantiatePlayerAtStartNode();
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

    private List<MapNode> GetPossibleConnections(MapNode currentNode, List<MapNode> nextLevelNodes, int maxConnections)
    {
        // Ordenar los nodos del siguiente nivel según su posición X
        List<MapNode> sortedNextLevelNodes = new List<MapNode>(nextLevelNodes);
        sortedNextLevelNodes.Sort((a, b) => a.position.x.CompareTo(b.position.x));

        List<MapNode> possibleConnections = new List<MapNode>();

        // Ordenar los nodos del nivel actual según su posición X
        List<MapNode> currentLevelNodes = levels[currentNode.depthLevel];
        List<MapNode> sortedCurrentLevelNodes = new List<MapNode>(currentLevelNodes);
        sortedCurrentLevelNodes.Sort((a, b) => a.position.x.CompareTo(b.position.x));

        // Encontrar el índice del nodo actual en el nivel actual
        int currentIndex = sortedCurrentLevelNodes.IndexOf(currentNode);

        if (currentIndex == -1)
        {
            Debug.LogError("Nodo actual no encontrado en el nivel actual.");
            return possibleConnections;
        }

        // Asegurarse de que currentIndex no excede el tamaño de sortedNextLevelNodes
        if (currentIndex >= sortedNextLevelNodes.Count)
        {
            currentIndex = sortedNextLevelNodes.Count - 1;
        }

        // Conectar con el nodo de mismo índice en el siguiente nivel
        if (!possibleConnections.Contains(sortedNextLevelNodes[currentIndex]))
        {
            possibleConnections.Add(sortedNextLevelNodes[currentIndex]);
        }

        // Si se permite una segunda conexión, conectar con el nodo adyacente
        if (maxConnections > 1)
        {
            int adjacentIndex = currentIndex + 1;

            if (adjacentIndex < sortedNextLevelNodes.Count)
            {
                if (!possibleConnections.Contains(sortedNextLevelNodes[adjacentIndex]))
                {
                    possibleConnections.Add(sortedNextLevelNodes[adjacentIndex]);
                }
            }
            else if (currentIndex - 1 >= 0)
            {
                // Si no hay nodo a la derecha, conectar con el nodo a la izquierda
                adjacentIndex = currentIndex - 1;
                if (!possibleConnections.Contains(sortedNextLevelNodes[adjacentIndex]))
                {
                    possibleConnections.Add(sortedNextLevelNodes[adjacentIndex]);
                }
            }
        }

        return possibleConnections;
    }

    private NodeType GetRandomNodeType(int level)
    {
        // Asignar el tipo de nodo según el nivel
        if (level == totalLevels - 1)
        {
            return NodeType.Boss;
        }

        NodeType[] possibleTypes = { NodeType.Battle, NodeType.Shop, NodeType.Upgrade, NodeType.Rest, NodeType.Treasure };
        int index = Random.Range(0, possibleTypes.Length);
        return possibleTypes[index];
    }

    private MapNode CreateNode(Vector3 position, int depth, NodeType type)
    {
        MapNode node = new MapNode(position, depth, type);
        return node;
    }

    private void DrawMap()
    {
        foreach (MapNode node in allNodes)
        {
            // Instanciar el prefab del nodo
            GameObject nodeObj = Instantiate(nodePrefab, node.position, Quaternion.identity);
            node.nodeObject = nodeObj;

            // Asignar el MapNode al script de interacción
            NodeInteraction interaction = nodeObj.GetComponent<NodeInteraction>();
            if (interaction != null)
            {
                interaction.node = node;
                interaction.SetNodeVisual(node.nodeType); // Configurar la apariencia según el tipo de nodo
            }

            // Configurar el nombre del nodo
            nodeObj.name = $"Node_Level{node.depthLevel}_{node.nodeType}";

            // Dibujar las conexiones
            foreach (MapNode connectedNode in node.connectedNodes)
            {
                DrawLine(node.position, connectedNode.position, Color.white);
            }
        }
    }

    private void DrawLine(Vector3 start, Vector3 end, Color color)
    {
        GameObject line = new GameObject("Line");
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
