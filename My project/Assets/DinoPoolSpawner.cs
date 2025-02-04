using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Importante para trabajar con UI

public class DinoPoolSpawner : MonoBehaviour
{
    public Transform poolParent; // Contenedor donde aparecerán las imágenes de los dinosaurios (Debe ser un GameObject dentro de un Canvas)
    public GameObject dinoImagePrefab; // Prefab de la imagen UI (Debe tener un componente Image)
    public Vector3 startPosition = new Vector3(0, 0, 0); // Posición inicial para instanciar
    public float spacing = 200f; // Espaciado entre imágenes en la UI

    private void Start()
    {
        CreateDinoPool();
    }

    private void CreateDinoPool()
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
            // Obtener el prefab del dinosaurio
            GameObject dinoPrefab = GameManager.Instance.GetDinoPrefabByID(dinoData.dinoID);

            if (dinoPrefab == null)
            {
                Debug.LogWarning($"No se encontró prefab para el dinoID {dinoData.dinoID}.");
                continue;
            }

            // Obtener el Sprite del prefab
            SpriteRenderer prefabSpriteRenderer = dinoPrefab.GetComponentInChildren<SpriteRenderer>();
            if (prefabSpriteRenderer == null)
            {
                Debug.LogWarning($"El prefab {dinoPrefab.name} no tiene un SpriteRenderer.");
                continue;
            }
            Sprite dinoSprite = prefabSpriteRenderer.sprite;

            // Instanciar la imagen UI
            GameObject dinoImageInstance = Instantiate(dinoImagePrefab, poolParent);
            dinoImageInstance.transform.localPosition = spawnPosition; // Ajustar posición relativa en el UI
            dinoImageInstance.GetComponent<RectTransform>().localScale = new Vector3(-1, 1, 1);


            // Asignar el sprite al componente Image
            Image imageComponent = dinoImageInstance.GetComponent<Image>();
            if (imageComponent != null)
            {
                imageComponent.sprite = dinoSprite;
            }
            else
            {
                Debug.LogWarning($"El prefab de UI no tiene un componente Image.");
            }

            // Asignar el ID y nombre del dinosaurio
            DinoImage dinoImageComponent = dinoImageInstance.GetComponent<DinoImage>();
            if (dinoImageComponent != null)
            {
                dinoImageComponent.dinoID = dinoData.dinoID;
                dinoImageComponent.dinoName = dinoData.dinoName;
            }

            Debug.Log($"Instanciada imagen UI de {dinoData.dinoName} en el Pool con ID: {dinoData.dinoID}");

            // Mover la posición para el siguiente dino en la UI
            spawnPosition += new Vector3(spacing, 0, 0);
        }
    }
}