using System.Collections.Generic;
using UnityEngine;

public class TeamManager : MonoBehaviour
{
    public static TeamManager Instance { get; private set; }

    // Lista de IDs (o datos esenciales) de los dinos en el equipo, en orden.
    public List<string> teamIDs = new List<string>();

    // El dinosaurio activo en combate (instanciado).
    public Dinosaurio activeDino;

    // Spawn point para instanciar al dino activo.
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

    /// <summary>
    /// Asigna el equipo (lista de IDs) obtenido de la escena de selección.
    /// </summary>
    /// <param name="ids">Lista de IDs de dinos</param>
    public void SetTeam(List<string> ids)
    {
        teamIDs = new List<string>(ids);
        Debug.Log("TeamManager: Se han guardado " + teamIDs.Count + " dinos (IDs) en el equipo.");
    }

    /// <summary>
    /// Devuelve el ID del dino que está en la primera posición.
    /// </summary>
    public string GetFrontDinoID()
    {
        if (teamIDs.Count > 0)
            return teamIDs[0];
        return "";
    }

    /// <summary>
    /// Instancia el dino del frente usando el primer ID de la lista.
    /// Se espera que el GameManager tenga un método GetDinoPrefabByID que retorne el prefab correspondiente.
    /// </summary>
    public void SpawnFrontDino()
    {
        if (teamIDs.Count > 0)
        {
            string frontID = teamIDs[0];
            GameObject dinoPrefab = GameManager.Instance.GetDinoPrefabByID(frontID);
            if (dinoPrefab != null)
            {
                // Instanciar el dino en la posición y rotación definidas en spawnPoint.
                GameObject instance = Instantiate(dinoPrefab, spawnPoint.position, spawnPoint.rotation);

                // Ajustar la escala a 2/3 del tamaño original y voltear en X.
                Vector3 newScale = instance.transform.localScale * (2f / 3f);
                newScale.x = -Mathf.Abs(newScale.x);  // Asegura que se voltee en X.
                instance.transform.localScale = newScale;

                activeDino = instance.GetComponent<Dinosaurio>();
                if (activeDino != null)
                {
                    Debug.Log("TeamManager: Dino instanciado: " + activeDino.name);
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
            Debug.Log("TeamManager: No quedan dinos en el equipo. Fin del combate.");
            // Aquí podrías activar la pantalla de derrota o similar.
        }
    }

    /// <summary>
    /// Se invoca cuando el dino activo muere para removerlo y avanzar al siguiente.
    /// </summary>
    public void OnActiveDinoDeath()
    {
        if (activeDino != null && activeDino.CurrentHealth <= 0)
        {
            Debug.Log("TeamManager: El dino " + activeDino.name + " ha muerto.");
            // Remover el primer ID de la lista.
            if (teamIDs.Count > 0)
                teamIDs.RemoveAt(0);
            // Destruir el objeto del dino muerto.
            Destroy(activeDino.gameObject);
            activeDino = null;
            // Instanciar el siguiente dino.
            SpawnFrontDino();
        }
    }

    private void Update()
    {
        // Comprueba continuamente el estado del dino activo.
        if (activeDino != null && activeDino.CurrentHealth <= 0)
        {
            OnActiveDinoDeath();
        }
    }
}