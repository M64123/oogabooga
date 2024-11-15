using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class EggBehaviour : MonoBehaviour
{
    [Header("Egg Models")]
    public List<GameObject> eggStages; // Asigna los modelos del huevo en orden

    [Header("Egg Animation")]
    public float rotationAmount = 15f; // Grados de inclinación
    public float rotationSpeed = 5f;   // Velocidad de inclinación
    public float rotationPauseDuration = 0.2f; // Pausa entre inclinaciones

    [Header("Dinosaur Display")]
    public Image dinoDisplay;          // Imagen UI para mostrar el dinosaurio
    public Text dinoInfoText;          // Texto para mostrar información del dinosaurio
    public float dinoScaleDuration = 1f; // Duración de la animación de escala

    [Header("Dinosaur Sprites")]
    public List<Sprite> commonDinos;
    public List<Sprite> rareDinos;
    public List<Sprite> shinyCommonDinos;
    public List<Sprite> shinyRareDinos;

    // **Nuevo**: Lista de prefabs de dinosaurios
    [Header("Dinosaur Prefabs")]
    public List<GameObject> commonDinoPrefabs;
    public List<GameObject> rareDinoPrefabs;
    public List<GameObject> shinyCommonDinoPrefabs;
    public List<GameObject> shinyRareDinoPrefabs;

    private int clickCount = 0;
    private bool isAnimating = false;
    private GameObject currentEggModel; // Modelo de huevo activo
    private Quaternion initialRotation;

    void Start()
    {
        // Inicializar el huevo al primer modelo
        SetEggStage(0);

        // Obtener el modelo de huevo activo
        currentEggModel = eggStages[0];
        initialRotation = currentEggModel.transform.rotation;

        // Asegurarse de que la imagen del dinosaurio esté oculta al inicio
        if (dinoDisplay != null)
        {
            dinoDisplay.gameObject.SetActive(false);
            dinoDisplay.transform.localScale = Vector3.zero; // Escala inicial cero
        }

        // Asegurarse de que el texto de información esté vacío
        if (dinoInfoText != null)
        {
            dinoInfoText.text = "";
        }
    }

    void OnMouseDown()
    {
        if (!isAnimating)
        {
            StartCoroutine(EggClickRoutine());
        }
    }

    IEnumerator EggClickRoutine()
    {
        isAnimating = true;

        // Animar el huevo (inclinación hacia ambos lados)
        yield return StartCoroutine(RotateEggBothSides());

        // Avanzar al siguiente estado del huevo
        clickCount++;
        if (clickCount < eggStages.Count)
        {
            SetEggStage(clickCount);
        }
        else
        {
            // El huevo se rompe y aparece un dinosaurio
            HatchEgg();
        }

        isAnimating = false;
    }

    IEnumerator RotateEggBothSides()
    {
        // Determinar aleatoriamente el orden de inclinación
        int firstDirection = Random.Range(0, 2) == 0 ? -1 : 1;
        int secondDirection = -firstDirection;

        // Inclinación hacia el primer lado
        yield return StartCoroutine(RotateEggToSide(firstDirection));

        // Pequeña pausa
        yield return new WaitForSeconds(rotationPauseDuration);

        // Inclinación hacia el segundo lado
        yield return StartCoroutine(RotateEggToSide(secondDirection));

        // Regresar a la rotación inicial
        yield return StartCoroutine(RotateEggToInitial());
    }

    IEnumerator RotateEggToSide(int direction)
    {
        Quaternion targetRotation = Quaternion.Euler(0, 0, rotationAmount * direction) * initialRotation;
        while (Quaternion.Angle(currentEggModel.transform.rotation, targetRotation) > 0.1f)
        {
            currentEggModel.transform.rotation = Quaternion.RotateTowards(currentEggModel.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime * 100f);
            yield return null;
        }
        currentEggModel.transform.rotation = targetRotation;
    }

    IEnumerator RotateEggToInitial()
    {
        while (Quaternion.Angle(currentEggModel.transform.rotation, initialRotation) > 0.1f)
        {
            currentEggModel.transform.rotation = Quaternion.RotateTowards(currentEggModel.transform.rotation, initialRotation, rotationSpeed * Time.deltaTime * 100f);
            yield return null;
        }
        currentEggModel.transform.rotation = initialRotation;
    }

    void SetEggStage(int stage)
    {
        for (int i = 0; i < eggStages.Count; i++)
        {
            eggStages[i].SetActive(i == stage);
        }

        // Actualizar el modelo de huevo activo y su rotación inicial
        if (stage < eggStages.Count)
        {
            currentEggModel = eggStages[stage];
            initialRotation = currentEggModel.transform.rotation;
        }
    }

    void HatchEgg()
    {
        // Ocultar los modelos del huevo
        foreach (GameObject eggModel in eggStages)
        {
            eggModel.SetActive(false);
        }

        // Determinar el dinosaurio obtenido
        Sprite dinoSprite;
        string dinoInfo;
        DinosaurType dinoType;
        GameObject dinoPrefab;
        GetRandomDino(out dinoSprite, out dinoInfo, out dinoType, out dinoPrefab);

        // Mostrar el dinosaurio en pantalla con animación de escala
        StartCoroutine(DisplayDinoWithScale(dinoSprite, dinoInfo, dinoType, dinoPrefab));
    }

    void GetRandomDino(out Sprite dinoSprite, out string dinoInfo, out DinosaurType dinoType, out GameObject dinoPrefab)
    {
        float randomValue = Random.Range(0f, 100f);

        if (randomValue < 4f)
        {
            // 4% Shiny Raro
            dinoSprite = GetRandomSpriteFromList(shinyRareDinos);
            dinoInfo = "¡Dinosaurio Shiny Raro!";
            dinoType = DinosaurType.ShinyRaro;
            dinoPrefab = GetRandomPrefabFromList(shinyRareDinoPrefabs);
        }
        else if (randomValue < 10f)
        {
            // 6% Shiny Común
            dinoSprite = GetRandomSpriteFromList(shinyCommonDinos);
            dinoInfo = "¡Dinosaurio Shiny Común!";
            dinoType = DinosaurType.ShinyComun;
            dinoPrefab = GetRandomPrefabFromList(shinyCommonDinoPrefabs);
        }
        else if (randomValue < 40f)
        {
            // 30% Raro
            dinoSprite = GetRandomSpriteFromList(rareDinos);
            dinoInfo = "Dinosaurio Raro";
            dinoType = DinosaurType.Raro;
            dinoPrefab = GetRandomPrefabFromList(rareDinoPrefabs);
        }
        else
        {
            // 60% Común
            dinoSprite = GetRandomSpriteFromList(commonDinos);
            dinoInfo = "Dinosaurio Común";
            dinoType = DinosaurType.Comun;
            dinoPrefab = GetRandomPrefabFromList(commonDinoPrefabs);
        }
    }

    Sprite GetRandomSpriteFromList(List<Sprite> spriteList)
    {
        int index = Random.Range(0, spriteList.Count);
        return spriteList[index];
    }

    GameObject GetRandomPrefabFromList(List<GameObject> prefabList)
    {
        int index = Random.Range(0, prefabList.Count);
        return prefabList[index];
    }

    IEnumerator DisplayDinoWithScale(Sprite dinoSprite, string dinoInfo, DinosaurType dinoType, GameObject dinoPrefab)
    {
        // Activar el display del dinosaurio
        dinoDisplay.gameObject.SetActive(true);
        dinoDisplay.sprite = dinoSprite;

        // Ajustar el tamaño de la imagen al tamaño del sprite
        RectTransform rectTransform = dinoDisplay.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(dinoSprite.rect.width, dinoSprite.rect.height);

        // Restablecer la escala a cero
        dinoDisplay.transform.localScale = Vector3.zero;

        // Animación de escala
        float elapsedTime = 0f;
        while (elapsedTime < dinoScaleDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / dinoScaleDuration;
            // Usar una interpolación suave
            float scale = Mathf.SmoothStep(0f, 1f, t);
            dinoDisplay.transform.localScale = new Vector3(scale, scale, scale);
            yield return null;
        }

        // Asegurarse de que la escala final sea exacta
        dinoDisplay.transform.localScale = Vector3.one;

        // Mostrar la información del dinosaurio
        if (dinoInfoText != null)
        {
            dinoInfoText.text = dinoInfo;
        }

        // **Añadir el dinosaurio al inventario**
        AddDinoToInventory(dinoSprite, dinoInfo, dinoType, dinoPrefab);

        // **Esperar 3 segundos y regresar al tablero**
        yield return new WaitForSeconds(3f);

        // Regresar al tablero
        ReturnToBoard();
    }

    void AddDinoToInventory(Sprite dinoSprite, string dinoInfo, DinosaurType dinoType, GameObject dinoPrefab)
    {
        // Crear una instancia del dinosaurio y añadirlo al GameManager
        int uniqueID = GenerateUniqueID();
        Dinosaur newDino = new Dinosaur(dinoInfo + " " + uniqueID, uniqueID, dinoType, IsShiny(dinoType), dinoPrefab);
        GameManager.Instance.AddDinosaur(newDino);
    }

    int GenerateUniqueID()
    {
        // Genera un ID único para el dinosaurio
        return GameManager.Instance.playerDinosaurs.Count + 1;
    }

    bool IsShiny(DinosaurType dinoType)
    {
        return dinoType == DinosaurType.ShinyComun || dinoType == DinosaurType.ShinyRaro;
    }

    void ReturnToBoard()
    {
        // Regresar al tablero
        // Asegúrate de que el estado del tablero se mantiene
        SceneManager.LoadScene("BoardScene");
    }
}
