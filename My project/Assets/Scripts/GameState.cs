using System;
using System.Collections.Generic;

[Serializable]
public class GameState
{
    // Datos del jugador
    public string currentNodeID; // ID del nodo actual donde está el jugador
    public List<string> obtainedDinoIDs; // Lista de IDs de dinosaurios obtenidos

    // Estado del tablero
    public List<SerializedNode> nodes; // Lista de nodos serializados
}

[Serializable]
public class SerializedNode
{
    public string nodeID; // Identificador único del nodo
    public float posX;
    public float posY;
    public float posZ;
    public NodeType nodeType;
    public List<string> connectedNodeIDs; // IDs de los nodos conectados
    public int depthLevel;
}
