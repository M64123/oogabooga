// GameManager.cs

using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }

    // Lista para almacenar los datos de todos los nodos (no modificar)
    private List<MapNodeData> savedMapData = new List<MapNodeData>();

    // ID del nodo actual donde se encuentra el jugador (no modificar)
    private string currentPlayerNodeID = "";

    // Lista original de dinosaurios como GameObjects (no borrar)
    public List<GameObject> playerDinosaurs = new List<GameObject>();

    [System.Serializable]
    public class DinoData
    {
        public string dinoID;
        public string dinoName;
        public Rarity rarity;
        public int level = 1;
    }

    // Diccionario interno para acceso rápido por ID
    private Dictionary<string, DinoData> playerDinoDictionary = new Dictionary<string, DinoData>();

    // Lista serializable para ver los dinos en el Inspector
    // Esta lista se mantendrá sincronizada con el diccionario
    public List<DinoData> playerDinoList = new List<DinoData>();

    public List<MapNodeData> boardState
    {
        get { return savedMapData; }
        set { savedMapData = value; }
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

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

    public List<MapNodeData> GetSavedMapData()
    {
        return savedMapData;
    }

    public void SetCurrentPlayerNodeID(string nodeID)
    {
        currentPlayerNodeID = nodeID;
        Debug.Log($"ID del nodo del jugador guardado: {nodeID}");
    }

    public string GetCurrentPlayerNodeID()
    {
        return currentPlayerNodeID;
    }

    // Método original para añadir dinosaurios como GameObject
    public void AddDinosaur(GameObject newDino)
    {
        if (newDino != null)
        {
            playerDinosaurs.Add(newDino);
            Debug.Log($"Dinosaurio añadido (GameObject): {newDino.name}. Total: {playerDinosaurs.Count}");
        }
        else
        {
            Debug.LogError("Intento de añadir un dinosaurio nulo (GameObject).");
        }
    }

    // Método original para guardar un dino como GameObject
    public void SaveDinosaur(GameObject dino)
    {
        AddDinosaur(dino);
        // No usamos esta forma ahora, pero se mantiene por compatibilidad
    }

    /// <summary>
    /// Añade o actualiza un dinosaurio por ID.
    /// Si ya existe, sube de nivel.
    /// Si no existe, lo crea nivel 1.
    /// Actualiza la lista playerDinoList para verlo en el inspector.
    /// </summary>
    public void AddDinosaur(string dinoID, string dinoName, Rarity rarity)
    {
        if (string.IsNullOrEmpty(dinoID))
        {
            Debug.LogError("dinoID es nulo o vacío, no se puede añadir el dino.");
            return;
        }

        if (playerDinoDictionary.ContainsKey(dinoID))
        {
            DinoData existingDino = playerDinoDictionary[dinoID];
            existingDino.level++;

            // Actualizar en la lista
            UpdateDinoInList(dinoID, existingDino);

            Debug.Log($"Dino con ID {dinoID} ya existe. Nivel incrementado a {existingDino.level}.");
        }
        else
        {
            DinoData newDinoData = new DinoData
            {
                dinoID = dinoID,
                dinoName = dinoName,
                rarity = rarity,
                level = 1
            };

            playerDinoDictionary.Add(dinoID, newDinoData);
            playerDinoList.Add(newDinoData); // Añadir a la lista para verlo en el inspector

            Debug.Log($"Dino con ID {dinoID} añadido por primera vez. Nivel {newDinoData.level}.");
        }
    }

    /// <summary>
    /// Actualiza la información de un dino en la lista playerDinoList para reflejar cambios del diccionario.
    /// </summary>
    private void UpdateDinoInList(string dinoID, DinoData updatedData)
    {
        for (int i = 0; i < playerDinoList.Count; i++)
        {
            if (playerDinoList[i].dinoID == dinoID)
            {
                playerDinoList[i] = updatedData;
                return;
            }
        }

        // Si no se encontró, agregarlo (no debería pasar si el dino ya existía)
        playerDinoList.Add(updatedData);
    }

    public Dictionary<string, DinoData> GetAllPlayerDinos()
    {
        return playerDinoDictionary;
    }
}

