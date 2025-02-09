using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    // Ruta del archivo JSON (se guarda en persistentDataPath para que sobreviva a las sesiones)
    private string filePath;
    private DinoSaveData saveData;

    // Clase que contiene la lista de IDs desbloqueados.
    [System.Serializable]
    public class DinoSaveData
    {
        public List<string> unlockedDinoIDs = new List<string>();
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            filePath = Path.Combine(Application.persistentDataPath, "dinoData.json");
            LoadData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Lee el archivo JSON si existe, o crea una nueva instancia.
    public void LoadData()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            saveData = JsonConvert.DeserializeObject<DinoSaveData>(json);
            if (saveData == null)
                saveData = new DinoSaveData();
        }
        else
        {
            saveData = new DinoSaveData();
        }
    }

    // Escribe el archivo JSON con el contenido de saveData.
    public void SaveDataToFile()
    {
        string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);
        File.WriteAllText(filePath, json);
    }

    // Comprueba si ya se ha desbloqueado un dino por su ID.
    public bool IsDinoUnlocked(string dinoID)
    {
        return saveData.unlockedDinoIDs.Contains(dinoID);
    }

    // Desbloquea (guarda) el dino si aún no estaba guardado.
    public void UnlockDino(string dinoID)
    {
        if (!IsDinoUnlocked(dinoID))
        {
            saveData.unlockedDinoIDs.Add(dinoID);
            SaveDataToFile();
            Debug.Log($"Dino {dinoID} desbloqueado y guardado.");
        }
    }

    // Método auxiliar para obtener la lista de dinos desbloqueados.
    public List<string> GetUnlockedDinoIDs()
    {
        return saveData.unlockedDinoIDs;
    }
}
