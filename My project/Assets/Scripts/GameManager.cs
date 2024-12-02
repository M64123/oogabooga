// GameManager.cs
using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }

    // Lista para almacenar los datos de todos los nodos
    private List<MapNodeData> savedMapData = new List<MapNodeData>();

    // ID del nodo actual donde se encuentra el jugador
    private string currentPlayerNodeID = "";

    // Lista para almacenar los dinosaurios del jugador
    public List<GameObject> playerDinosaurs = new List<GameObject>();

    // Propiedad para acceder al estado del tablero (mapa)
    public List<MapNodeData> boardState
    {
        get { return savedMapData; }
        set { savedMapData = value; }
    }

    void Awake()
    {
        // Implementación del Singleton
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject); // Persiste entre escenas
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Guarda el estado actual del mapa.
    /// </summary>
    /// <param name="nodes">Lista de nodos del mapa.</param>
    public void SaveMapState(List<MapNode> nodes)
    {
        savedMapData.Clear();
        foreach (MapNode node in nodes)
        {
            MapNodeData nodeData = new MapNodeData(node);
            savedMapData.Add(nodeData);
        }
        Debug.Log("Estado del mapa guardado correctamente.");
    }

    /// <summary>
    /// Recupera los datos guardados del mapa.
    /// </summary>
    /// <returns>Lista de datos de nodos.</returns>
    public List<MapNodeData> GetSavedMapData()
    {
        return savedMapData;
    }

    /// <summary>
    /// Guarda el ID del nodo actual del jugador.
    /// </summary>
    /// <param name="nodeID">ID del nodo.</param>
    public void SetCurrentPlayerNodeID(string nodeID)
    {
        currentPlayerNodeID = nodeID;
        Debug.Log($"ID del nodo del jugador guardado: {nodeID}");
    }

    /// <summary>
    /// Recupera el ID del nodo actual del jugador.
    /// </summary>
    /// <returns>ID del nodo.</returns>
    public string GetCurrentPlayerNodeID()
    {
        return currentPlayerNodeID;
    }

    /// <summary>
    /// Añade un dinosaurio a la lista de dinosaurios del jugador.
    /// </summary>
    /// <param name="newDino">Dinosaurio a añadir.</param>
    public void AddDinosaur(GameObject newDino)
    {
        if (newDino != null)
        {
            playerDinosaurs.Add(newDino);
            Debug.Log($"Dinosaurio añadido: {newDino.name}. Total dinosaurios: {playerDinosaurs.Count}");
        }
        else
        {
            Debug.LogError("Intento de añadir un dinosaurio nulo.");
        }
    }

    /// <summary>
    /// Guarda un dinosaurio a través de EggBehaviour o cualquier otro script.
    /// </summary>
    /// <param name="dino">Dinosaurio a guardar.</param>
    public void SaveDinosaur(GameObject dino)
    {
        AddDinosaur(dino);
        // Aquí puedes agregar lógica adicional para guardar el estado del dinosaurio si es necesario.
    }
}
