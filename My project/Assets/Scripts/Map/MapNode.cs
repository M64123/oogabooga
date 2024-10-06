// MapNode.cs
using UnityEngine;
using System.Collections.Generic;

public class MapNode
{
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
        position = pos;
        depthLevel = depth;
    }
}
