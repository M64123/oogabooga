using System.Collections.Generic;
using UnityEngine;

public class MapNode
{
    public Vector3 position;                 // Posición del nodo en el mapa
    public List<MapNode> connectedNodes;     // Lista de nodos conectados
    public int depthLevel;                   // Nivel del nodo en el mapa
    public GameObject nodeObject;            // Referencia al objeto del nodo en la escena
    public NodeType nodeType;                // Tipo de evento del nodo

    public MapNode(Vector3 pos, int depth, NodeType type)
    {
        position = pos;
        depthLevel = depth;
        connectedNodes = new List<MapNode>();
        nodeType = type;
    }
}
