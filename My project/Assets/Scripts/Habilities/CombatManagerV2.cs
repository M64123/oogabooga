using UnityEngine;
using System.Collections.Generic;

public class CombatManagerV2 : MonoBehaviour
{
    public Transform spawnPoint; // Lugar donde aparecerá el primer dinosaurio en combate

    private void Start()
    {
        // Obtener el ID del dinosaurio en primera posición
        string firstDinoID = PlayerPrefs.GetString("FirstDinoID", "");

        if (!string.IsNullOrEmpty(firstDinoID))
        {
            GameObject dinoPrefab = GameManager.Instance.GetDinoPrefabByID(firstDinoID);

            if (dinoPrefab != null)
            {
                // Instanciar el dinosaurio en la escena de combate
                GameObject dinoInstance = Instantiate(dinoPrefab, spawnPoint.position, Quaternion.identity);

                // **FLIPEAR EL DINOSAURIO EN LA ESCENA DE COMBATE**
                Vector3 flippedScale = dinoInstance.transform.localScale;
                flippedScale.x = -Mathf.Abs(flippedScale.x); // Asegura que X sea negativa
                dinoInstance.transform.localScale = flippedScale;

                Debug.Log($"Dino {firstDinoID} instanciado en combate con FLIP en X.");
            }
            else
            {
                Debug.LogError($"No se encontró prefab para el dinoID {firstDinoID}.");
            }
        }
        else
        {
            Debug.LogWarning("No hay dinosaurio asignado para la primera posición en combate.");
        }
    }

}