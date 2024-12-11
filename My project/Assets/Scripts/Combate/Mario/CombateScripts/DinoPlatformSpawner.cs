using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DinoPlatformSpawner : MonoBehaviour
{
    public Transform platformParent; // Parent donde aparecerán los dinosaurios
    public Vector3 startPosition = new Vector3(0, 0, 0); // Posición inicial para instanciar
    public float spacing = 2.0f; // Espaciado entre los dinosaurios

    private void Start()
    {
        SpawnDinosaursOnPlatform();
    }

    private void SpawnDinosaursOnPlatform()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("No se encontró GameManager en la escena.");
            return;
        }

        List<GameManager.DinoData> playerDinos = GameManager.Instance.playerDinoList;

        if (playerDinos == null || playerDinos.Count == 0)
        {
            Debug.LogWarning("La lista de dinosaurios del jugador está vacía.");
            return;
        }

        Vector3 spawnPosition = startPosition;

        foreach (GameManager.DinoData dinoData in playerDinos)
        {
            // Obtener el prefab correspondiente al dinoID
            GameObject dinoPrefab = GameManager.Instance.GetDinoPrefabByID(dinoData.dinoID);

            if (dinoPrefab != null)
            {
                // Instanciar el prefab en la posición actual sin asignar un padre
                GameObject dinoInstance = Instantiate(dinoPrefab, spawnPosition, Quaternion.identity);

                dinoInstance.name = dinoData.dinoName; // Asignar nombre del dino

                // Restaurar la escala original del prefab
                dinoInstance.transform.localScale = dinoPrefab.transform.localScale;

                // Aplicar el efecto mirror horizontal (invertir escala en X)
                Vector3 mirroredScale = dinoInstance.transform.localScale;
                mirroredScale.x = -Mathf.Abs(mirroredScale.x); // Asegurar que sea negativo para reflejar
                dinoInstance.transform.localScale = mirroredScale;

                // Asignar el padre después de configurar todo
                dinoInstance.transform.SetParent(platformParent, true); // `true` mantiene la transformación global

                Debug.Log($"Instanciado {dinoData.dinoName} reflejado en la plataforma con tamaño correcto.");

                // Actualizar la posición para el siguiente dino
                spawnPosition += new Vector3(spacing, 0, 0);
            }
            else
            {
                Debug.LogWarning($"No se encontró prefab para el dinoID {dinoData.dinoID}.");
            }
        }
    }
}
