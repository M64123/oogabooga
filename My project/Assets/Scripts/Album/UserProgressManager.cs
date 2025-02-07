using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class UserProgressManager : MonoBehaviour
{
    public static UserProgressManager Instance { get; private set; }

    [Header("ID de usuario (para identificar su progreso)")]
    public string userID = "Player1";

    // Nombre de archivo donde guardamos los datos
    private string fileName = "userProgress.json";

    [System.Serializable]
    public class UserProgressData
    {
        public string userID;
        // Aquí puedes guardar cualquier otra info global del usuario
        public List<GameManager.DinoData> ownedDinos;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Cargar al iniciar (opcional si quieres cargar nada más arrancar)
            LoadProgressFromJSON();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Ejemplo: Llamar esto cuando quieras guardar
    public void SaveProgressToJSON()
    {
        // Recopilamos los dinos que el jugador posee desde el GameManager
        var allDinos = GameManager.Instance.playerDinoList;
        UserProgressData data = new UserProgressData
        {
            userID = userID,
            ownedDinos = allDinos
        };

        string json = JsonUtility.ToJson(data, true);

        string path = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllText(path, json);

        Debug.Log($"[UserProgressManager] Datos guardados en JSON: {path}");
    }

    // Cargar los datos y pasarlos a GameManager
    public void LoadProgressFromJSON()
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            UserProgressData data = JsonUtility.FromJson<UserProgressData>(json);

            if (data != null)
            {
                userID = data.userID;

                // Borramos la lista actual del GameManager para llenarla
                GameManager.Instance.playerDinoList.Clear();
                // OJO: Si el diccionario playerDinoDictionary lo usas, también sincronizarlo
                foreach (var dinoData in data.ownedDinos)
                {
                    // En lugar de meterlo directamente, usamos AddDinosaur del GameManager
                    // para mantener la lógica interna (niveles, dictionary, etc.).
                    GameManager.Instance.AddDinosaur(dinoData.dinoID, dinoData.dinoName, dinoData.rarity);
                }

                Debug.Log($"[UserProgressManager] Datos cargados desde JSON. ID de usuario: {data.userID}");
            }
        }
        else
        {
            Debug.LogWarning($"No existe archivo en: {path}. Se ignora carga.");
        }
    }
}
