using System.Collections.Generic;
using UnityEngine;

public class DeathManager : MonoBehaviour
{
    public static DeathManager Instance { get; private set; }

    // Lista de DinoData de los dinos muertos.
    public List<DinoData> deadDinosData = new List<DinoData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Agrega a deadDinosData el DinoData correspondiente al dino muerto, usando el ID que ya estaba en playerDinoList.
    /// Luego, llama a GameManager para marcarlo como muerto (lo elimina de playerDinoList).
    /// </summary>
    public void AddDeadDino(Dinosaurio dino)
    {
        if (dino != null)
        {
            // Usamos el playerDinoID, que debe haberse asignado en la instancia (TeamManager).
            string originalID = dino.playerDinoID;
            DinoData data = new DinoData();
            data.dinoID = originalID;
            // Opcional: podrías asignar también dinoName, rarity, level, etc.
            SpriteRenderer sr = dino.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                data.dinoSprite = sr.sprite;
            }
            else
            {
                Debug.LogWarning("DeathManager: No se encontró SpriteRenderer en " + dino.name);
            }

            deadDinosData.Add(data);
            Debug.Log("DeathManager: Se ha agregado el dino " + dino.name + " a la lista de muertos con ID: " + data.dinoID);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.MarkDinoAsDead(originalID);
            }
            else
            {
                Debug.LogWarning("DeathManager: No se encontró GameManager para remover el dino muerto.");
            }
        }
    }
}