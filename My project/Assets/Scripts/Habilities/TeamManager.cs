using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TeamManager : MonoBehaviour
{
    public static TeamManager Instance { get; private set; }

    // Lista de IDs de los dinos en el equipo (ordenados).
    public List<string> teamIDs = new List<string>();

    // Lista de referencias a los dinos instanciados.
    public List<Dinosaurio> teamDinos = new List<Dinosaurio>();

    // El dino activo en combate.
    public Dinosaurio activeDino;

    // Spawn point para instanciar el dino activo.
    public Transform spawnPoint;

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

    public void SetTeam(List<string> ids)
    {
        teamIDs = new List<string>(ids);
        Debug.Log("TeamManager: Se han guardado " + teamIDs.Count + " dinos (IDs) en el equipo.");
    }

    public string GetFrontDinoID()
    {
        if (teamIDs.Count > 0)
            return teamIDs[0];
        return "";
    }

    public void SpawnFrontDino()
    {
        if (teamIDs.Count > 0)
        {
            string frontID = teamIDs[0];
            GameObject dinoPrefab = GameManager.Instance.GetDinoPrefabByID(frontID);
            if (dinoPrefab != null)
            {
                GameObject instance = Instantiate(dinoPrefab, spawnPoint.position, spawnPoint.rotation);
                Vector3 newScale = instance.transform.localScale * (2f / 3f);
                newScale.x = -Mathf.Abs(newScale.x);
                instance.transform.localScale = newScale;

                Dinosaurio dino = instance.GetComponent<Dinosaurio>();
                if (dino != null)
                {
                    activeDino = dino;
                    // Asigna el ID del dino original (de la lista) a la instancia.
                    activeDino.playerDinoID = frontID;
                    teamDinos.Add(dino);
                    Debug.Log("TeamManager: Dino instanciado: " + dino.name + " (playerDinoID: " + frontID + ")");
                }
                else
                {
                    Debug.LogError("TeamManager: El prefab con dinoID " + frontID + " no contiene un componente Dinosaurio.");
                }
            }
            else
            {
                Debug.LogError("TeamManager: No se encontró prefab para el dinoID " + frontID);
            }
        }
        else
        {
            Debug.Log("TeamManager: No quedan dinos en el equipo. Reiniciando la escena del Tablero desde cero.");
            // Reinicia la escena y, antes, resetea los datos del GameManager.
            GameManager.Instance.ResetGameData();
            SceneManager.LoadScene("Tablero", LoadSceneMode.Single);
        }
    }

    public void RemoveDino(Dinosaurio dino)
    {
        if (dino != null)
        {
            if (teamDinos.Contains(dino))
            {
                teamDinos.Remove(dino);
                Debug.Log("TeamManager: Se removió el dino: " + dino.name);
            }
            if (teamIDs.Count > 0 && teamIDs[0] == dino.playerDinoID)
            {
                teamIDs.RemoveAt(0);
            }
        }
    }

    public void OnActiveDinoDeath()
    {
        if (activeDino != null && activeDino.CurrentHealth <= 0)
        {
            Debug.Log("TeamManager: El dino " + activeDino.name + " ha muerto.");
            // Usa el ID del dino en el frente para actualizar el estado en DeathManager y GameManager.
            if (DeathManager.Instance != null)
            {
                DeathManager.Instance.AddDeadDinoByID(teamIDs[0]);
            }
            else
            {
                Debug.LogWarning("TeamManager: No se encontró DeathManager.");
            }
            GameManager.Instance.MarkDinoAsDead(teamIDs[0]);

            RemoveDino(activeDino);
            Destroy(activeDino.gameObject);
            activeDino = null;
            // Intenta instanciar el siguiente dino.
            SpawnFrontDino();
        }
    }

    private void Update()
    {
        if (activeDino != null && activeDino.CurrentHealth <= 0)
        {
            OnActiveDinoDeath();
        }
    }
}