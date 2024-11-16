// MapNode.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Clase que representa un nodo en el mapa.
/// </summary>
public class MapNode : MonoBehaviour
{
    public string nodeID;
    public NodeType nodeType;
    public int depthLevel;
    public Vector3 position; // Esta propiedad se establece a trav�s de transform.position

    public bool isActive; // Indica si el nodo est� activo

    public List<MapNode> connectedNodes = new List<MapNode>(); // Lista de nodos conectados
    public HashSet<NodeType> nodeTypesInPaths = new HashSet<NodeType>(); // Tipos de nodos en caminos que llegan a este nodo
    public bool isTypeAssigned = false; // Indica si el tipo de nodo ya ha sido asignado

    public GameObject nodeObject; // Referencia al GameObject instanciado para este nodo

    void Start()
    {
        // Inicializaci�n adicional si es necesario
        position = transform.position;
    }

    // Otros m�todos seg�n tus necesidades
}
