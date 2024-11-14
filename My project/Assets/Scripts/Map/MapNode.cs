using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MapNode
{
    public string nodeID;
    public Vector3 position;
    public int depthLevel;
    public NodeType nodeType;
    public GameObject nodeObject;
    public List<MapNode> connectedNodes = new List<MapNode>();

    // Nuevas propiedades para evitar repeticiones de tipos en los caminos
    public HashSet<NodeType> nodeTypesInPaths = new HashSet<NodeType>();
    public bool isTypeAssigned = false;

    public MapNode(Vector3 pos, int depth)
    {
        nodeID = System.Guid.NewGuid().ToString(); // Generar un ID único
        position = pos;
        depthLevel = depth;
    }
}
