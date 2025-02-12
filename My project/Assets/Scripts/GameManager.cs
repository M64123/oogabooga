using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }

    private List<MapNodeData> savedMapData = new List<MapNodeData>();
    private string currentPlayerNodeID = "";

    public List<GameObject> playerDinosaurs = new List<GameObject>();

    [System.Serializable]
    public class DinoData
    {
        public string dinoID;
        public string dinoName;
        public Rarity rarity;
        public int level = 1;
        public bool isAlive = false;
        // Puedes agregar otros campos que necesites.
    }

    private Dictionary<string, DinoData> playerDinoDictionary = new Dictionary<string, DinoData>();

    [Header("Dino Prefabs Mapping")]
    public List<string> dinoIDs;
    public List<GameObject> dinoPrefabs;

    public List<MapNodeData> boardState
    {
        get { return savedMapData; }
        set { savedMapData = value; }
    }

    // Lista de dinos vivos (datos) del jugador.
    public List<DinoData> playerDinoList = new List<DinoData>();

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

    public void AddDinosaur(GameObject newDino)
    {
        if (newDino != null)
        {
            playerDinosaurs.Add(newDino);
            Debug.Log($"Dino (GameObject) añadido: {newDino.name}. Total: {playerDinosaurs.Count}");
        }
        else
        {
            Debug.LogError("Intento de añadir un dino nulo (GameObject).");
        }
    }

    public void SaveDinosaur(GameObject dino)
    {
        AddDinosaur(dino);
    }

    public void AddDinosaur(string dinoID, string dinoName, Rarity rarity)
    {
        if (string.IsNullOrEmpty(dinoID))
        {
            Debug.LogError("dinoID vacío, no se puede añadir.");
            return;
        }

        DinoData dinoData;
        if (playerDinoDictionary.TryGetValue(dinoID, out dinoData))
        {
            dinoData.level++;
            UpdateDinoInList(dinoID, dinoData);
            Debug.Log($"Dino {dinoID} existe. Nivel subido a {dinoData.level}");
        }
        else
        {
            dinoData = new DinoData
            {
                dinoID = dinoID,
                dinoName = dinoName,
                rarity = rarity,
                level = 1,
                isAlive = true
            };
            playerDinoDictionary.Add(dinoID, dinoData);
            playerDinoList.Add(dinoData);
            Debug.Log($"Dino {dinoID} añadido con nivel {dinoData.level}.");
        }
    }

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
        playerDinoList.Add(updatedData);
    }

    public Dictionary<string, DinoData> GetAllPlayerDinos()
    {
        return playerDinoDictionary;
    }

    public GameObject GetDinoPrefabByID(string dID)
    {
        // Implementación existente para retornar el prefab a partir del ID.
        // ...
        for (int i = 0; i < dinoIDs.Count; i++)
        {
            if (dinoIDs[i] == dID)
            {
                return dinoPrefabs[i];
            }
        }
        Debug.LogError("No se encontró prefab para " + dID);
        return null;
    }


    // Este método se usa para marcar a un dino como muerto y eliminarlo de la lista de vivos.
    public void MarkDinoAsDead(string dinoID)
    {
        foreach (var dino in playerDinoList)
        {
            if (dino.dinoID == dinoID)
            {
                dino.isAlive = true;
                Debug.Log($"GameManager: Dino {dinoID} marcado como muerto.");
                return;
            }
        }
        Debug.LogError($"GameManager: No se encontró DinoData con ID {dinoID} para marcarlo como muerto.");
    }
    public void ReviveDino(string dinoID)
    {
        bool found = false;
        // Recorremos toda la lista de playerDinoList
        for (int i = 0; i < playerDinoList.Count; i++)
        {
            // Si encontramos un DinoData con ese ID y que está marcado como muerto (isAlive == true)
            if (playerDinoList[i].dinoID == dinoID && playerDinoList[i].isAlive)
            {
                playerDinoList[i].isAlive = false; // Marcarlo como vivo (revivido)
                UpdateDinoInList(dinoID, playerDinoList[i]); // Actualizar la lista si es necesario
                Debug.Log($"GameManager: Dino {dinoID} revivido.");
                found = true;
                // Si solo se debe revivir la primera coincidencia, se puede salir del ciclo.
                break;
            }
        }
        if (!found)
        {
            Debug.LogError($"GameManager: No se encontró un dino en playerDinoList con ID {dinoID} marcado como muerto para revivir.");
        }
    }

}