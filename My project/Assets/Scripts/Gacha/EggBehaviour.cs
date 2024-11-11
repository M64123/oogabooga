using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class EggBehaviour : MonoBehaviour
{
    [Header("Egg Models")]
    public List<GameObject> eggStages; // Asigna los modelos del huevo en orden

    [Header("Egg Animation")]
    public float rotationAmount = 15f; // Grados de inclinaci�n
    public float rotationSpeed = 5f;   // Velocidad de inclinaci�n
    public float rotationPauseDuration = 0.2f; // Pausa entre inclinaciones

    [Header("Cube Settings")]
    public GameObject cubePrefab;       // Prefab del cubo que ser� lanzado
    public float cubeUpwardForce = 7f;  // Fuerza hacia arriba aplicada al cubo
    public float cubeTorqueForce = 1000f; // Fuerza de torque aplicada al cubo

    [Header("Dinosaur Display")]
    public Image dinoDisplay;          // Imagen UI para mostrar el dinosaurio
    public Text dinoInfoText;          // Texto para mostrar informaci�n del dinosaurio
    public float dinoScaleDuration = 1f; // Duraci�n de la animaci�n de escala

    [Header("Dinosaur Sprites")]
    public List<Sprite> commonDinos;
    public List<Sprite> rareDinos;
    public List<Sprite> shinyCommonDinos;
    public List<Sprite> shinyRareDinos;

    private int clickCount = 0;
    private bool isAnimating = false;
    private GameObject currentEggModel; // Modelo de huevo activo
    private Quaternion initialRotation;

    // Informaci�n del dinosaurio obtenido
    private Sprite obtainedDinoSprite;
    private string obtainedDinoInfo;
    private Rarity obtainedDinoRarity;

    void Start()
    {
        // Inicializar el huevo al primer modelo
        SetEggStage(0);

        // Obtener el modelo de huevo activo
        currentEggModel = eggStages[0];
        initialRotation = currentEggModel.transform.rotation;

        // Asegurarse de que la imagen del dinosaurio est� oculta al inicio
        if (dinoDisplay != null)
        {
            dinoDisplay.gameObject.SetActive(false);
            dinoDisplay.transform.localScale = Vector3.zero; // Escala inicial cero
        }

        // Asegurarse de que el texto de informaci�n est� vac�o
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

        // Animar el huevo (inclinaci�n hacia ambos lados)
        yield return StartCoroutine(RotateEggBothSides());

        // Avanzar al siguiente estado del huevo
        clickCount++;
        if (clickCount < eggStages.Count)
        {
            SetEggStage(clickCount);
        }
        else
        {
            // El huevo se rompe y aparece un cubo
            HatchEgg();
        }

        isAnimating = false;
    }

    IEnumerator RotateEggBothSides()
    {
        // Determinar aleatoriamente el orden de inclinaci�n
        int firstDirection = Random.Range(0, 2) == 0 ? -1 : 1;
        int secondDirection = -firstDirection;

        // Inclinaci�n hacia el primer lado
        yield return StartCoroutine(RotateEggToSide(firstDirection));

        // Peque�a pausa
        yield return new WaitForSeconds(rotationPauseDuration);

        // Inclinaci�n hacia el segundo lado
        yield return StartCoroutine(RotateEggToSide(secondDirection));

        // Regresar a la rotaci�n inicial
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

        // Actualizar el modelo de huevo activo y su rotaci�n inicial
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
        GetRandomDino(out obtainedDinoSprite, out obtainedDinoInfo, out obtainedDinoRarity);

        // Lanzar el cubo correspondiente
        LaunchCube();
    }

    void GetRandomDino(out Sprite dinoSprite, out string dinoInfo, out Rarity rarity)
    {
        float randomValue = Random.Range(0f, 100f);

        if (randomValue < 4f)
        {
            // 4% Shiny Raro
            dinoSprite = GetRandomSpriteFromList(shinyRareDinos);
            dinoInfo = "�Dinosaurio Shiny Raro!";
            rarity = Rarity.ShinyRare;
        }
        else if (randomValue < 10f)
        {
            // 6% Shiny Com�n
            dinoSprite = GetRandomSpriteFromList(shinyCommonDinos);
            dinoInfo = "�Dinosaurio Shiny Com�n!";
            rarity = Rarity.ShinyCommon;
        }
        else if (randomValue < 40f)
        {
            // 30% Raro
            dinoSprite = GetRandomSpriteFromList(rareDinos);
            dinoInfo = "Dinosaurio Raro";
            rarity = Rarity.Rare;
        }
        else
        {
            // 60% Com�n
            dinoSprite = GetRandomSpriteFromList(commonDinos);
            dinoInfo = "Dinosaurio Com�n";
            rarity = Rarity.Common;
        }
    }

    Sprite GetRandomSpriteFromList(List<Sprite> spriteList)
    {
        int index = Random.Range(0, spriteList.Count);
        return spriteList[index];
    }

    void LaunchCube()
    {
        // Instanciar el cubo en la posici�n del huevo
        GameObject cube = Instantiate(cubePrefab, transform.position, Random.rotation);

        // Configurar el color y efectos del cubo seg�n la rareza
        ConfigureCube(cube, obtainedDinoRarity);

        // Obtener el Rigidbody del cubo
        Rigidbody rb = cube.GetComponent<Rigidbody>();

        // Aplicar fuerzas para lanzarlo
        rb.useGravity = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Aplicar fuerza hacia arriba con variaci�n aleatoria
        float randomUpwardForce = Random.Range(cubeUpwardForce * 0.9f, cubeUpwardForce * 1.1f);
        rb.AddForce(Vector3.up * randomUpwardForce, ForceMode.Impulse);

        // Aplicar torque aleatorio
        Vector3 randomTorque = Random.insideUnitSphere * cubeTorqueForce;
        rb.AddTorque(randomTorque, ForceMode.Impulse);

        // Iniciar coroutine para esperar a que el cubo aterrice
        StartCoroutine(WaitForCubeToLand(cube));
    }

    void ConfigureCube(GameObject cube, Rarity rarity)
    {
        Renderer cubeRenderer = cube.GetComponent<Renderer>();
        TrailRenderer trail = cube.GetComponent<TrailRenderer>();
        ParticleSystem particles = cube.GetComponentInChildren<ParticleSystem>();

        switch (rarity)
        {
            case Rarity.Common:
                // Cubo blanco con estela blanca
                cubeRenderer.material.color = Color.white;
                if (trail != null)
                {
                    trail.startColor = Color.white;
                    trail.endColor = Color.white;
                }
                if (particles != null)
                {
                    particles.gameObject.SetActive(false);
                }
                break;
            case Rarity.Rare:
                // Cubo azul con estela azul
                cubeRenderer.material.color = Color.blue;
                if (trail != null)
                {
                    trail.startColor = Color.blue;
                    trail.endColor = Color.blue;
                }
                if (particles != null)
                {
                    particles.gameObject.SetActive(false);
                }
                break;
            case Rarity.ShinyCommon:
                // Cubo blanco con estela blanca y part�culas brillantes
                cubeRenderer.material.color = Color.white;
                if (trail != null)
                {
                    trail.startColor = Color.white;
                    trail.endColor = Color.white;
                }
                if (particles != null)
                {
                    particles.gameObject.SetActive(true);
                    var main = particles.main;
                    main.startColor = new ParticleSystem.MinMaxGradient(Color.white, Color.cyan);
                }
                break;
            case Rarity.ShinyRare:
                // Cubo azul con estela azul y part�culas brillantes
                cubeRenderer.material.color = Color.blue;
                if (trail != null)
                {
                    trail.startColor = Color.blue;
                    trail.endColor = Color.blue;
                }
                if (particles != null)
                {
                    particles.gameObject.SetActive(true);
                    var main = particles.main;
                    main.startColor = new ParticleSystem.MinMaxGradient(Color.blue, Color.magenta);
                }
                break;
        }
    }

    IEnumerator WaitForCubeToLand(GameObject cube)
    {
        Rigidbody rb = cube.GetComponent<Rigidbody>();

        // Esperar hasta que la velocidad del cubo sea baja (ha aterrizado)
        while (rb.velocity.magnitude > 0.1f)
        {
            yield return null;
        }

        // Esperar un momento adicional para asegurarse
        yield return new WaitForSeconds(1f);

        // Mostrar el dinosaurio obtenido
        StartCoroutine(DisplayDinoWithScale(obtainedDinoSprite, obtainedDinoInfo));

        // Puedes destruir el cubo si lo deseas
        Destroy(cube);
    }

    IEnumerator DisplayDinoWithScale(Sprite dinoSprite, string dinoInfo)
    {
        // Activar el display del dinosaurio
        dinoDisplay.gameObject.SetActive(true);
        dinoDisplay.sprite = dinoSprite;

        // Ajustar el tama�o de la imagen al tama�o del sprite
        RectTransform rectTransform = dinoDisplay.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(dinoSprite.rect.width, dinoSprite.rect.height);

        // Restablecer la escala a cero
        dinoDisplay.transform.localScale = Vector3.zero;

        // Animaci�n de escala
        float elapsedTime = 0f;
        while (elapsedTime < dinoScaleDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / dinoScaleDuration;
            // Usar una interpolaci�n suave
            float scale = Mathf.SmoothStep(0f, 1f, t);
            dinoDisplay.transform.localScale = new Vector3(scale, scale, scale);
            yield return null;
        }

        // Asegurarse de que la escala final sea exacta
        dinoDisplay.transform.localScale = Vector3.one;

        // Mostrar la informaci�n del dinosaurio
        if (dinoInfoText != null)
        {
            dinoInfoText.text = dinoInfo;
        }
    }

    // Enum para representar la rareza
    public enum Rarity
    {
        Common,
        Rare,
        ShinyCommon,
        ShinyRare
    }
}