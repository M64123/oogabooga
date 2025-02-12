using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeathSceneManager : MonoBehaviour
{
    [Header("Configuración de la visualización")]
    [Tooltip("Contenedor en el Canvas donde se mostrarán las representaciones de los dinos muertos.")]
    public Transform deathContainer;

    [Tooltip("Prefab para mostrar cada dino muerto (debe tener DeadDinoDisplay).")]
    public GameObject deadDinoDisplayPrefab;

    [Tooltip("Espaciado vertical entre representaciones (si no usas Layout Group).")]
    public float verticalSpacing = 150f;

    private void Start()
    {
        if (DeathManager.Instance == null)
        {
            Debug.LogError("DeathSceneManager: No se encontró DeathManager en la escena.");
            return;
        }

        List<DinoData> deadDataList = DeathManager.Instance.deadDinosData;
        Debug.Log("DeathSceneManager: Número de dinos muertos: " + deadDataList.Count);

        // Limpia el contenedor.
        foreach (Transform child in deathContainer)
        {
            Destroy(child.gameObject);
        }

        // Instanciar una representación para cada dino muerto.
        for (int i = 0; i < deadDataList.Count; i++)
        {
            DinoData data = deadDataList[i];
            if (data != null)
            {
                GameObject displayInstance = Instantiate(deadDinoDisplayPrefab, deathContainer);
                RectTransform rt = displayInstance.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchoredPosition = new Vector2(0, -i * verticalSpacing);
                }

                DeadDinoDisplay display = displayInstance.GetComponent<DeadDinoDisplay>();
                if (display != null)
                {
                    display.SetData(data);
                }
                else
                {
                    Debug.LogWarning("DeathSceneManager: El prefab de visualización no tiene el componente DeadDinoDisplay.");
                }
            }
        }
    }
}