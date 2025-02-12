using System.Collections.Generic;
using UnityEngine;

public class DeathManager : MonoBehaviour
{
    public static DeathManager Instance { get; private set; }

    // Lista de datos de dinos muertos.
    public List<GameManager.DinoData> deadDinosData = new List<GameManager.DinoData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persistente entre escenas.
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Busca en la lista de dinos del GameManager el DinoData con el ID dado y lo agrega a la lista de muertos.
    /// </summary>
    public void AddDeadDinoByID(string dinoID)
    {
        foreach (var dinoData in GameManager.Instance.playerDinoList)
        {
            if (dinoData.dinoID == dinoID)
            {
                // Según tu lógica, se marca como muerto (isAlive = true significa muerto).
                dinoData.isAlive = true;
                deadDinosData.Add(dinoData);
                Debug.Log($"DeathManager: Se ha agregado el DinoData con ID {dinoID} a la lista de muertos.");
                return;
            }
        }
        Debug.LogWarning($"DeathManager: No se encontró DinoData para el ID {dinoID}.");
    }
}