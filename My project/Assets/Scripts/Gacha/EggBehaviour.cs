using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class EggBehaviour : MonoBehaviour
{
    [Header("Egg Models")]
    public List<GameObject> eggStages; // Asigna los modelos del huevo en orden

    [Header("Egg Animation")]
    public float rotationAmount = 15f; // Grados de inclinación
    public float rotationSpeed = 5f;   // Velocidad de inclinación
    public float rotationPauseDuration = 0.2f; // Pausa entre inclinaciones

    [Header("Cube Settings")]
    public GameObject cubePrefab;       // Prefab del cubo que será lanzado
    public float cubeUpwardForce = 7f;  // Fuerza hacia arriba aplicada al cubo
    public float cubeTorqueForce = 1000f; // Fuerza de torque aplicada al cubo
    public float sidewaysForceVariation = 0.1f; // Variación aleatoria lateral en la fuerza

    [Header("Dinosaur Display")]
    public Image dinoDisplay;          // Imagen UI para mostrar el dinosaurio
    public Text dinoInfoText;          // Texto para mostrar información del dinosaurio
    public float dinoScaleDuration = 1f; // Duración de la animación de escala

    [Header("Dinosaur Scaling")]
    public float dinoScaleMultiplier = 0.5f; // Multiplicador para ajustar el tamaño del dinosaurio desde el Inspector

    [Header("Dinosaur Sprites")]
    public List<Sprite> commonDinos;
    public List<Sprite> rareDinos;
    public List<Sprite> shinyCommonDinos;
    public List<Sprite> shinyRareDinos;

    [Header("Timing Settings")]
    public float maxWaitTime = 5f; // Tiempo máximo de espera antes de mostrar el dinosaurio

    private int clickCount = 0;
    private bool isAnimating = false;
    private GameObject currentEggModel; // Modelo de huevo activo
    private Quaternion initialRotation;

    // Información del dinosaurio obtenido
    private Sprite obtainedDinoSprite;
    private string obtainedDinoInfo;
    private Rarity obtainedDinoRarity;

    private bool hasHatched = false; // Indica si el huevo ya se ha roto

    private CamaraControllerGacha camaraController;

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

        // Obtener la referencia al CamaraControllerGacha
        camaraController = Camera.main.GetComponent<CamaraControllerGacha>();
    }

    void OnMouseDown()
    {
        if (!isAnimating && !hasHatched)
        {
            StartCoroutine(EggClickRoutine());

            // Notificar al CamaraControllerGacha que el huevo ha sido clicado
            if (camaraController != null)
            {
                camaraController.OnEggClicked();
            }
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
            // El huevo se rompe y aparece un cubo
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
        // Indicar que el huevo ya se ha roto
        hasHatched = true;

        // Ocultar los modelos del huevo
        foreach (GameObject eggModel in eggStages)
        {
            eggModel.SetActive(false);
        }

        // Desactivar el collider del huevo
        Collider eggCollider = GetComponent<Collider>();
        if (eggCollider != null)
        {
            eggCollider.enabled = false;
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
            dinoInfo = "¡Dinosaurio Shiny Raro!";
            rarity = Rarity.ShinyRare;
        }
        else if (randomValue < 10f)
        {
            // 6% Shiny Común
            dinoSprite = GetRandomSpriteFromList(shinyCommonDinos);
            dinoInfo = "¡Dinosaurio Shiny Común!";
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
            // 60% Común
            dinoSprite = GetRandomSpriteFromList(commonDinos);
            dinoInfo = "Dinosaurio Común";
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
        // Instanciar el cubo en la posición del huevo
        GameObject cube = Instantiate(cubePrefab, transform.position, Random.rotation);

        // Configurar el color y efectos del cubo según la rareza
        ConfigureCube(cube, obtainedDinoRarity);

        // Obtener el Rigidbody del cubo
        Rigidbody rb = cube.GetComponent<Rigidbody>();

        // Aplicar fuerzas para lanzarlo
        rb.useGravity = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Calcular la dirección de la fuerza con variación aleatoria
        Vector3 forceDirection = Vector3.up + new Vector3(
            Random.Range(-sidewaysForceVariation, sidewaysForceVariation),
            0f,
            Random.Range(-sidewaysForceVariation, sidewaysForceVariation)
        );
        forceDirection.Normalize();

        // Aplicar fuerza hacia arriba con variación aleatoria
        float randomUpwardForce = Random.Range(cubeUpwardForce * 0.9f, cubeUpwardForce * 1.1f);
        rb.AddForce(forceDirection * randomUpwardForce, ForceMode.Impulse);

        // Aplicar torque aleatorio
        Vector3 randomTorque = Random.insideUnitSphere * cubeTorqueForce;
        rb.AddTorque(randomTorque, ForceMode.Impulse);

        // Notificar al CamaraControllerGacha que debe seguir al cubo
        if (camaraController != null)
        {
            camaraController.StartFollowingCube(cube.transform);
        }

        // Iniciar coroutine para esperar el tiempo máximo antes de mostrar el dinosaurio
        StartCoroutine(WaitForMaxTimeAndShowDino(cube));
    }

    void ConfigureCube(GameObject cube, Rarity rarity)
    {
        Renderer cubeRenderer = cube.GetComponent<Renderer>();
        TrailRenderer trail = cube.GetComponent<TrailRenderer>();
        ParticleSystem particles = cube.GetComponentInChildren<ParticleSystem>(true); // Incluir objetos inactivos

        switch (rarity)
        {
            case Rarity.Common:
                // Cubo blanco con estela blanca
                cubeRenderer.material.color = Color.white;
                if (trail != null)
                {
                    trail.startColor = Color.white;
                    trail.endColor = Color.white;
                    trail.enabled = true;
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
                    trail.enabled = true;
                }
                if (particles != null)
                {
                    particles.gameObject.SetActive(false);
                }
                break;
            case Rarity.ShinyCommon:
                // Cubo blanco con estela blanca y partículas brillantes
                cubeRenderer.material.color = Color.white;
                if (trail != null)
                {
                    trail.startColor = Color.white;
                    trail.endColor = Color.white;
                    trail.enabled = true;
                }
                if (particles != null)
                {
                    particles.gameObject.SetActive(true);
                    // Configurar las partículas para shiny common
                    var main = particles.main;
                    main.startColor = new ParticleSystem.MinMaxGradient(Color.white, Color.cyan);
                }
                break;
            case Rarity.ShinyRare:
                // Cubo azul con estela azul y partículas brillantes
                cubeRenderer.material.color = Color.blue;
                if (trail != null)
                {
                    trail.startColor = Color.blue;
                    trail.endColor = Color.blue;
                    trail.enabled = true;
                }
                if (particles != null)
                {
                    particles.gameObject.SetActive(true);
                    // Configurar las partículas para shiny rare
                    var main = particles.main;
                    main.startColor = new ParticleSystem.MinMaxGradient(Color.blue, Color.magenta);
                }
                break;
        }
    }

    IEnumerator WaitForMaxTimeAndShowDino(GameObject cube)
    {
        // Esperar durante el tiempo máximo especificado
        yield return new WaitForSeconds(maxWaitTime);

        // Mostrar el dinosaurio obtenido
        StartCoroutine(DisplayDinoWithScale(obtainedDinoSprite, obtainedDinoInfo));

        // Destruir el cubo
        Destroy(cube);

        // Notificar al CamaraControllerGacha que deje de seguir al cubo
        if (camaraController != null)
        {
            camaraController.StopFollowingCube();
        }
    }

    IEnumerator DisplayDinoWithScale(Sprite dinoSprite, string dinoInfo)
    {
        // Activar el display del dinosaurio
        dinoDisplay.gameObject.SetActive(true);
        dinoDisplay.sprite = dinoSprite;

        // Ajustar el tamaño de la imagen al tamaño del sprite, multiplicado por el multiplicador de escala
        RectTransform rectTransform = dinoDisplay.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(dinoSprite.rect.width * dinoScaleMultiplier, dinoSprite.rect.height * dinoScaleMultiplier);

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
