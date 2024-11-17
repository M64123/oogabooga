// MapNodeData.cs
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MapNodeData
{
    public string nodeID;
    public Vector3 position;
    public int depthLevel;
    public NodeType nodeType;
    public List<string> connectedNodeIDs;
    public bool isActive;

    /// <summary>
    /// Constructor que inicializa MapNodeData a partir de un MapNode.
    /// </summary>
    /// <param name="node">El nodo del cual extraer los datos.</param>
    public MapNodeData(MapNode node)
    {
        this.nodeID = node.nodeID;
        this.position = node.transform.position;
        this.depthLevel = node.depthLevel;
        this.nodeType = node.nodeType;
        connectedNodeIDs = new List<string>();

        foreach (MapNode connectedNode in node.connectedNodes)
        {
            connectedNodeIDs.Add(connectedNode.nodeID);
        }
        this.isActive = node.isActive;
    }

    /// <summary>
    /// Constructor que crea un MapNodeData con nodeID e isActive.
    /// Útil para crear datos de nodos sin toda la información.
    /// </summary>
    /// <param name="nodeID">Identificador único del nodo.</param>
    /// <param name="isActive">Estado de activación del nodo.</param>
    public MapNodeData(string nodeID, bool isActive)
    {
        this.nodeID = nodeID;
        this.isActive = isActive;
        this.position = Vector3.zero; // Puedes asignar un valor por defecto o ajustarlo según tus necesidades
        this.depthLevel = 0;          // Valor por defecto
        this.nodeType = NodeType.None; // Valor por defecto, ajusta según tus tipos de nodos
        this.connectedNodeIDs = new List<string>();
    }
}
