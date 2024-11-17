// MapNode.cs
using UnityEngine;
using System.Collections.Generic;

public class MapNode : MonoBehaviour
{
    public string nodeID;
    public NodeType nodeType;
    public int depthLevel;
    public Vector3 position;

    public bool isActive;

    public List<MapNode> connectedNodes = new List<MapNode>();
    public HashSet<NodeType> nodeTypesInPaths = new HashSet<NodeType>();
    public bool isTypeAssigned = false;

    public GameObject nodeObject;

    void Start()
    {
        position = transform.position;
    }

    // Otros métodos según tus necesidades
}
