using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeadDinoPoolSpawner : MonoBehaviour
{
    [Header("Configuración de UI")]
    public Transform poolParent; // Contenedor (dentro de un Canvas) donde aparecerán las imágenes
    public GameObject dinoImagePrefab; // Prefab de la imagen UI (debe tener un componente Image y, opcionalmente, un componente DinoImage)
    public Vector3 startPosition = new Vector3(0, 0, 0); // Posición inicial para instanciar
    public float spacing = 200f; // Espaciado entre imágenes en la UI

    private void Start()
    {
        CreateDeadDinoPool();
    }

    private void CreateDeadDinoPool()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("DeadDinoPoolSpawner: No se encontró GameManager en la escena.");
            return;
        }

        List<GameManager.DinoData> playerDinos = GameManager.Instance.playerDinoList;
        if (playerDinos == null || playerDinos.Count == 0)
        {
            Debug.LogWarning("DeadDinoPoolSpawner: La lista de dinosaurios del jugador está vacía.");
            return;
        }

        Vector3 spawnPosition = startPosition;

        // Recorre la lista y procesa únicamente aquellos dinos con isAlive == true (muertos según tu lógica)
        foreach (GameManager.DinoData dinoData in playerDinos)
        {
            if (!dinoData.isAlive) // Si isAlive es false (vivo), se salta.
                continue;

            // Obtener el prefab del dino mediante su ID.
            GameObject dinoPrefab = GameManager.Instance.GetDinoPrefabByID(dinoData.dinoID);
            if (dinoPrefab == null)
            {
                Debug.LogWarning($"DeadDinoPoolSpawner: No se encontró prefab para el dinoID {dinoData.dinoID}.");
                continue;
            }

            // Obtener el Sprite del prefab a partir del SpriteRenderer.
            SpriteRenderer prefabSpriteRenderer = dinoPrefab.GetComponentInChildren<SpriteRenderer>();
            if (prefabSpriteRenderer == null)
            {
                Debug.LogWarning($"DeadDinoPoolSpawner: El prefab {dinoPrefab.name} no tiene un SpriteRenderer.");
                continue;
            }
            Sprite dinoSprite = prefabSpriteRenderer.sprite;

            // Instanciar la imagen UI.
            GameObject dinoImageInstance = Instantiate(dinoImagePrefab, poolParent);
            dinoImageInstance.transform.localPosition = spawnPosition;
            // Invertir la escala en X si es necesario.
            dinoImageInstance.GetComponent<RectTransform>().localScale = new Vector3(-1, 1, 1);

            // Asignar el sprite al componente Image.
            Image imageComponent = dinoImageInstance.GetComponent<Image>();
            if (imageComponent != null)
            {
                imageComponent.sprite = dinoSprite;
            }
            else
            {
                Debug.LogWarning("DeadDinoPoolSpawner: El prefab de UI no tiene un componente Image.");
            }

            // Asignar el ID (y nombre si lo deseas) al componente DinoImage, si lo tiene.
            DinoImage dinoImageComponent = dinoImageInstance.GetComponent<DinoImage>();
            if (dinoImageComponent != null)
            {
                dinoImageComponent.dinoID = dinoData.dinoID;
                dinoImageComponent.dinoName = dinoData.dinoName;
            }

            Debug.Log($"DeadDinoPoolSpawner: Instanciada imagen UI de {dinoData.dinoName} (MUERTO) con ID: {dinoData.dinoID}");

            // Mover la posición para el siguiente dino en la UI.
            spawnPosition += new Vector3(spacing, 0, 0);
        }
    }
}