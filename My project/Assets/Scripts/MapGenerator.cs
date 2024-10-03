// MapGenerator.cs
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

    // Variables para controlar la generación de tipos de nodos según las reglas
    private bool lastNodeWasGood = false;
    private bool secondLastNodeWasGood = false;

    void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        // Limpiar las listas por si se llama más de una vez
        levels.Clear();
        allNodes.Clear();

        // Verificar que totalLevels es al menos 2
        if (totalLevels < 2)
        {
            totalLevels = 2;
            Debug.LogWarning("El total de niveles es menor que 2. Se ha ajustado a 2.");
        }

        Debug.Log($"Generando mapa con {totalLevels} niveles.");

        // Generar los niveles
        for (int level = 0; level < totalLevels; level++)
        {
            List<MapNode> currentLevelNodes = new List<MapNode>();
            int nodesAtLevel = 1;

            // Verificar si es el último nivel
            if (level == totalLevels - 1)
            {
                nodesAtLevel = 1;  // Último nivel siempre con un solo nodo (Boss)
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
                if (levels.Count > 0)
                {
                    nodesAtLevel = levels[level - 1].Count;
                    if (nodesAtLevel == 0)
                    {
                        nodesAtLevel = 1;
                        Debug.LogWarning($"El nivel anterior ({level - 1}) no tiene nodos. Estableciendo nodesAtLevel a 1 en nivel {level}.");
                    }
                }
                else
                {
                    nodesAtLevel = 1;
                    Debug.LogWarning($"El nivel anterior no existe. Estableciendo nodesAtLevel a 1 en nivel {level}.");
                }
            }

            Debug.Log($"Nivel {level}: Generando {nodesAtLevel} nodos.");

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
                NodeType nodeType = GetNodeType(level, i);

                MapNode newNode = CreateNode(nodePos, level, nodeType);
                allNodes.Add(newNode);
                currentLevelNodes.Add(newNode);
            }

            levels.Add(currentLevelNodes);
        }

        // Después de generar todos los niveles
        Debug.Log($"Total de nodos generados: {allNodes.Count}");

        // Conectar los nodos entre niveles adyacentes
        for (int level = 0; level < totalLevels - 1; level++)
        {
            List<MapNode> currentLevelNodes = levels[level];
            List<MapNode> nextLevelNodes = levels[level + 1];

            foreach (MapNode currentNode in currentLevelNodes)
            {
                // Determinar el número de conexiones (1 o 2)
                int connections = Random.Range(1, 3); // 1 o 2 conexiones
                List<MapNode> possibleConnections = GetPossibleConnections(currentNode, nextLevelNodes, connections);

                if (possibleConnections.Count == 0)
                {
                    Debug.LogWarning($"El nodo en nivel {level} no tiene conexiones posibles.");
                }

                foreach (MapNode nextNode in possibleConnections)
                {
                    if (!currentNode.connectedNodes.Contains(nextNode))
                    {
                        currentNode.connectedNodes.Add(nextNode);
                    }
                }
            }
        }

        // Visualizar el mapa
        DrawMap();

        // Instanciar al jugador en el nodo inicial
        InstantiatePlayerAtStartNode();
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

    private NodeType GetNodeType(int level, int nodeIndex)
    {
        // Nivel 0: Nodo Inicio
        if (level == 0)
        {
            return NodeType.Inicio;
        }

        // Después de un Boss, el siguiente nodo es siempre GAMBLING, excepto después del último Boss
        if (level > 0 && levels[level - 1][0].nodeType == NodeType.Boss && level != totalLevels - 1)
        {
            return NodeType.GAMBLING;
        }

        // Nivel 1: Nodo GAMBLING
        if (level == 1)
        {
            return NodeType.GAMBLING;
        }

        // Último nivel: Nodo Boss
        if (level == totalLevels - 1)
        {
            return NodeType.Boss;
        }

        // Obtener el tipo de nodo anterior
        NodeType previousNodeType = NodeType.None;
        if (level > 0)
        {
            previousNodeType = levels[level - 1][0].nodeType;
        }

        // Determinar si el nodo anterior era bueno o malo
        bool previousNodeWasGood = IsGoodNode(previousNodeType);

        // Aplicar las reglas de generación
        NodeType nodeType = NodeType.None;

        if (previousNodeWasGood)
        {
            // Si el nodo anterior era bueno, 70% de probabilidad de que este sea malo
            if (Random.value < 0.7f)
            {
                nodeType = GetRandomBadNode();
            }
            else
            {
                nodeType = GetRandomGoodNode();
            }
        }
        else
        {
            // Si el nodo anterior era malo, 50% de probabilidad de bueno o malo
            if (Random.value < 0.5f)
            {
                nodeType = GetRandomGoodNode();
            }
            else
            {
                nodeType = GetRandomBadNode();
            }
        }

        // Si los dos nodos anteriores eran buenos, 95% de probabilidad de que este sea malo
        if (lastNodeWasGood && secondLastNodeWasGood)
        {
            if (Random.value < 0.95f)
            {
                nodeType = GetRandomBadNode();
            }
            else
            {
                nodeType = GetRandomGoodNode();
            }
        }

        // Después de una casilla de combate, 40% de probabilidad de que la siguiente sea Revivir
        if (previousNodeType == NodeType.Combate)
        {
            if (Random.value < 0.4f)
            {
                nodeType = NodeType.Revivir;
            }
        }

        // Actualizar el estado de los últimos nodos
        secondLastNodeWasGood = lastNodeWasGood;
        lastNodeWasGood = IsGoodNode(nodeType);

        return nodeType;
    }

    private bool IsGoodNode(NodeType nodeType)
    {
        return nodeType == NodeType.GAMBLING || nodeType == NodeType.Mana_Tamaño || nodeType == NodeType.Revivir;
    }

    private NodeType GetRandomGoodNode()
    {
        NodeType[] goodNodes = { NodeType.GAMBLING, NodeType.Mana_Tamaño, NodeType.Revivir };
        int index = Random.Range(0, goodNodes.Length);
        return goodNodes[index];
    }

    private NodeType GetRandomBadNode()
    {
        NodeType[] badNodes = { NodeType.Combate, NodeType.Random, NodeType.CoinFlip };
        int index = Random.Range(0, badNodes.Length);
        return badNodes[index];
    }

    private MapNode CreateNode(Vector3 position, int depth, NodeType type)
    {
        MapNode node = new MapNode(position, depth, type);
        return node;
    }

    private void DrawMap()
    {
        Debug.Log($"Dibujando {allNodes.Count} nodos en el mapa.");
        foreach (MapNode node in allNodes)
        {
            // Instanciar el prefab del nodo
            GameObject nodeObj = Instantiate(nodePrefab, node.position, Quaternion.identity);
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
            nodeObj.name = $"Node_Level{node.depthLevel}_{node.nodeType}";

            // Dibujar las conexiones
            if (node.connectedNodes.Count == 0)
            {
                Debug.LogWarning($"El nodo en posición {node.position} no tiene conexiones.");
            }

            foreach (MapNode connectedNode in node.connectedNodes)
            {
                DrawLine(node.position, connectedNode.position, Color.white);
            }
        }
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
