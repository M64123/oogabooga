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

                // **Reducir el tamaño a 2/3 de su tamaño original**
                Vector3 reducedScale = dinoInstance.transform.localScale * (2f / 3f);
                reducedScale.x = -Mathf.Abs(reducedScale.x); // Asegurar que siga flipeado
                dinoInstance.transform.localScale = reducedScale;

                Debug.Log($"Dino {firstDinoID} instanciado en combate con tamaño reducido y FLIP en X.");
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