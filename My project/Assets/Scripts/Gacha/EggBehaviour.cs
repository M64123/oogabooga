// EggBehaviour.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class EggBehaviour : MonoBehaviour
{
    [Header("Egg Models")]
    public List<GameObject> eggStages; // 0=Azul, 1=Morado, 2=Rojo, 3=Dorado

    [Header("Egg Animation")]
    public float rotationAmount = 15f; // Grados de inclinación para la animación QTE
    public float rotationSpeed = 5f;   // Velocidad de inclinación para la animación QTE
    public float rotationPauseDuration = 0.2f; // Pausa entre inclinaciones

    [Tooltip("Duración de la rotación agresiva final antes de eclosionar.")]
    public float aggressiveRotationDuration = 1f;
    [Tooltip("Grados de rotación para la rotación agresiva final.")]
    public float aggressiveRotationDegrees = 30f;

    [Header("Cube Settings")]
    public GameObject cubePrefab;           // Prefab del cubo
    public float cubeUpwardForce = 7f;      // Fuerza hacia arriba aplicada al cubo
    public float cubeTorqueForce = 1000f;   // Fuerza de torque aplicada al cubo
    public float sidewaysForceVariation = 0.1f;

    [Header("Dinosaur Display")]
    public Image dinoDisplay;
    public Text dinoInfoText;
    public float dinoScaleDuration = 1f;
    public float dinoScaleMultiplier = 0.5f;

    [Header("Dinosaur Sprites")]
    public List<Sprite> commonDinos;
    public List<Sprite> rareDinos;
    public List<Sprite> shinyCommonDinos;
    public List<Sprite> shinyRareDinos;

    [Header("Timing Settings")]
    public float maxWaitTime = 5f;

    [Header("QTE Settings")]
    public QTEManager qteManager;
    public BeatManager beatManager;
    public int maxQTEs = 4; // 4 QTEs totales

    [Header("QTE Canvas Settings")]
    [Tooltip("CanvasGroup del QTECanvas para hacer fade out.")]
    public CanvasGroup qteCanvasGroup;

    private int successfulQTEs = 0; // QTEs exitosos
    private int currentQTEAttempt = 0; // Intentos totales (acierto o fallo)
    private bool isAnimating = false;
    private bool isHatching = false;
    private GameObject currentEggModel;
    private Quaternion initialRotation;

    private Sprite obtainedDinoSprite;
    private string obtainedDinoInfo;
    private Rarity obtainedDinoRarity;

    private CamaraControllerGacha camaraController;

    void Start()
    {
        // Inicializar el primer huevo (Azul)
        SetEggStage(0);

        currentEggModel = eggStages[0];
        initialRotation = currentEggModel.transform.rotation;

        if (dinoDisplay != null)
        {
            dinoDisplay.gameObject.SetActive(false);
            dinoDisplay.transform.localScale = Vector3.zero;
        }

        if (dinoInfoText != null)
        {
            dinoInfoText.text = "";
        }

        camaraController = Camera.main.GetComponent<CamaraControllerGacha>();

        if (qteManager != null)
        {
            qteManager.onQTESuccess.AddListener(OnQTESuccess);
            qteManager.onQTEFail.AddListener(OnQTEFail);
        }
        else
        {
            Debug.LogError("QTEManager no asignado.");
        }

        if (qteCanvasGroup != null)
        {
            qteCanvasGroup.gameObject.SetActive(true);
            qteCanvasGroup.alpha = 1f;
        }
        else
        {
            Debug.LogWarning("qteCanvasGroup no asignado en EggBehaviour.");
        }
    }

    void OnDestroy()
    {
        if (qteManager != null)
        {
            qteManager.onQTESuccess.RemoveListener(OnQTESuccess);
            qteManager.onQTEFail.RemoveListener(OnQTEFail);
        }
    }

    public void OnQTESuccess()
    {
        if (isAnimating || isHatching) return;

        successfulQTEs++;
        currentQTEAttempt++;
        Debug.Log($"QTE Exitoso. Total exitosos: {successfulQTEs}");

        if (currentQTEAttempt >= maxQTEs)
        {
            // Terminar secuencia QTE
            StartCoroutine(EndQTESequence());
        }
        else
        {
            StartCoroutine(EggQTERoutine());
            UpdateEggModel();
        }
    }

    public void OnQTEFail()
    {
        if (isAnimating || isHatching) return;

        currentQTEAttempt++;
        Debug.Log($"QTE Fallido. Intentos: {currentQTEAttempt}");

        if (currentQTEAttempt >= maxQTEs)
        {
            StartCoroutine(EndQTESequence());
        }
        else
        {
            StartCoroutine(EggQTERoutine());
            // No mejora el huevo porque falló el QTE
        }
    }

    IEnumerator EndQTESequence()
    {
        isHatching = true;

        // Desvincular eventos QTE
        qteManager.onQTESuccess.RemoveListener(OnQTESuccess);
        qteManager.onQTEFail.RemoveListener(OnQTEFail);

        // Fade out del QTECanvas
        if (qteCanvasGroup != null)
        {
            float duration = 1f;
            float elapsed = 0f;
            float startAlpha = qteCanvasGroup.alpha;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                qteCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
                yield return null;
            }
            qteCanvasGroup.alpha = 0f;
            qteCanvasGroup.gameObject.SetActive(false);
        }

        // Rotar agresivamente de izq a derecha antes de eclosionar
        yield return StartCoroutine(RotateEggAggressively());

        // Eclosionar
        HatchEgg();
    }

    IEnumerator EggQTERoutine()
    {
        isAnimating = true;
        yield return StartCoroutine(RotateEggBothSides());
        isAnimating = false;
    }

    IEnumerator RotateEggBothSides()
    {
        int firstDirection = Random.Range(0, 2) == 0 ? -1 : 1;
        int secondDirection = -firstDirection;

        yield return StartCoroutine(RotateEggToSide(firstDirection));
        yield return new WaitForSeconds(rotationPauseDuration);
        yield return StartCoroutine(RotateEggToSide(secondDirection));
        yield return StartCoroutine(RotateEggToInitial());
    }

    IEnumerator RotateEggToSide(int direction)
    {
        Quaternion targetRotation = Quaternion.Euler(0, 0, rotationAmount * direction) * initialRotation;
        while (Quaternion.Angle(currentEggModel.transform.rotation, targetRotation) > 0.1f)
        {
            currentEggModel.transform.rotation = Quaternion.RotateTowards(
                currentEggModel.transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime * 100f
            );
            yield return null;
        }
        currentEggModel.transform.rotation = targetRotation;
    }

    IEnumerator RotateEggToInitial()
    {
        while (Quaternion.Angle(currentEggModel.transform.rotation, initialRotation) > 0.1f)
        {
            currentEggModel.transform.rotation = Quaternion.RotateTowards(
                currentEggModel.transform.rotation,
                initialRotation,
                rotationSpeed * Time.deltaTime * 100f
            );
            yield return null;
        }
        currentEggModel.transform.rotation = initialRotation;
    }

    IEnumerator RotateEggAggressively()
    {
        // Rotar el huevo agresivamente en el eje Z de izq a der
        float elapsed = 0f;
        Quaternion startRot = currentEggModel.transform.rotation;
        while (elapsed < aggressiveRotationDuration)
        {
            elapsed += Time.deltaTime;
            // Oscilar la rotación usando Mathf.Sin
            float angle = Mathf.Sin(elapsed * 20f) * aggressiveRotationDegrees;
            currentEggModel.transform.rotation = startRot * Quaternion.Euler(0, 0, angle);
            yield return null;
        }

        // Regresar a rotación inicial
        currentEggModel.transform.rotation = initialRotation;
    }

    void UpdateEggModel()
    {
        // Clampear los aciertos
        int eggIndex = Mathf.Clamp(successfulQTEs, 0, 3);
        SetEggStage(eggIndex);
        Debug.Log($"El huevo ha mejorado a: {currentEggModel.name}");
    }

    void SetEggStage(int stage)
    {
        for (int i = 0; i < eggStages.Count; i++)
        {
            eggStages[i].SetActive(i == stage);
        }
        if (stage < eggStages.Count)
        {
            currentEggModel = eggStages[stage];
            initialRotation = currentEggModel.transform.rotation;
        }
    }

    void HatchEgg()
    {
        // Ocultar el huevo actual
        if (currentEggModel != null)
        {
            currentEggModel.SetActive(false);
        }

        // Desactivar el collider del huevo
        Collider eggCollider = GetComponent<Collider>();
        if (eggCollider != null)
        {
            eggCollider.enabled = false;
        }

        // Obtener el dinosaurio basado en la rareza según el número de aciertos
        int eggLevel = Mathf.Clamp(successfulQTEs, 0, 3);

        GetRandomDino(eggLevel, out obtainedDinoSprite, out obtainedDinoInfo, out obtainedDinoRarity);

        // Lanzar el cubo
        LaunchCube();
    }

    void GetRandomDino(int eggLevel, out Sprite dinoSprite, out string dinoInfo, out Rarity rarity)
    {
        dinoSprite = null;
        dinoInfo = "";
        rarity = Rarity.Common;

        float randomValue = Random.Range(0f, 100f);

        switch (eggLevel)
        {
            case 0: // Azul
                // 70% común, 30% raro
                if (randomValue < 70f)
                {
                    dinoSprite = GetRandomSpriteFromList(commonDinos);
                    dinoInfo = "Dinosaurio Común";
                    rarity = Rarity.Common;
                }
                else
                {
                    dinoSprite = GetRandomSpriteFromList(rareDinos);
                    dinoInfo = "Dinosaurio Raro";
                    rarity = Rarity.Rare;
                }
                break;
            case 1: // Morado
                // 57% común, 40% raro, 3% shiny común
                if (randomValue < 57f)
                {
                    dinoSprite = GetRandomSpriteFromList(commonDinos);
                    dinoInfo = "Dinosaurio Común";
                    rarity = Rarity.Common;
                }
                else if (randomValue < 97f)
                {
                    dinoSprite = GetRandomSpriteFromList(rareDinos);
                    dinoInfo = "Dinosaurio Raro";
                    rarity = Rarity.Rare;
                }
                else
                {
                    dinoSprite = GetRandomSpriteFromList(shinyCommonDinos);
                    dinoInfo = "¡Dinosaurio Shiny Común!";
                    rarity = Rarity.ShinyCommon;
                }
                break;
            case 2: // Rojo
                // 45% común, 45% raro, 7% shiny común, 3% shiny raro
                if (randomValue < 45f)
                {
                    dinoSprite = GetRandomSpriteFromList(commonDinos);
                    dinoInfo = "Dinosaurio Común";
                    rarity = Rarity.Common;
                }
                else if (randomValue < 90f)
                {
                    dinoSprite = GetRandomSpriteFromList(rareDinos);
                    dinoInfo = "Dinosaurio Raro";
                    rarity = Rarity.Rare;
                }
                else if (randomValue < 97f)
                {
                    dinoSprite = GetRandomSpriteFromList(shinyCommonDinos);
                    dinoInfo = "¡Dinosaurio Shiny Común!";
                    rarity = Rarity.ShinyCommon;
                }
                else
                {
                    dinoSprite = GetRandomSpriteFromList(shinyRareDinos);
                    dinoInfo = "¡Dinosaurio Shiny Raro!";
                    rarity = Rarity.ShinyRare;
                }
                break;
            case 3: // Dorado
                // 35% común, 45% raro, 12% shiny común, 8% shiny raro
                if (randomValue < 35f)
                {
                    dinoSprite = GetRandomSpriteFromList(commonDinos);
                    dinoInfo = "Dinosaurio Común";
                    rarity = Rarity.Common;
                }
                else if (randomValue < 80f)
                {
                    dinoSprite = GetRandomSpriteFromList(rareDinos);
                    dinoInfo = "Dinosaurio Raro";
                    rarity = Rarity.Rare;
                }
                else if (randomValue < 92f)
                {
                    dinoSprite = GetRandomSpriteFromList(shinyCommonDinos);
                    dinoInfo = "¡Dinosaurio Shiny Común!";
                    rarity = Rarity.ShinyCommon;
                }
                else
                {
                    dinoSprite = GetRandomSpriteFromList(shinyRareDinos);
                    dinoInfo = "¡Dinosaurio Shiny Raro!";
                    rarity = Rarity.ShinyRare;
                }
                break;
            default:
                Debug.LogError("Nivel de huevo desconocido: " + eggLevel);
                // Asignar algo por defecto para evitar nulos
                dinoSprite = GetRandomSpriteFromList(commonDinos);
                dinoInfo = "Dinosaurio Común";
                rarity = Rarity.Common;
                break;
        }

        if (dinoSprite == null)
        {
            Debug.LogError("No se ha obtenido un sprite de dinosaurio válido.");
            // Asignar uno común por defecto si no se pudo obtener
            dinoSprite = GetRandomSpriteFromList(commonDinos);
            dinoInfo = "Dinosaurio Común";
            rarity = Rarity.Common;
        }
    }

    Sprite GetRandomSpriteFromList(List<Sprite> spriteList)
    {
        if (spriteList == null || spriteList.Count == 0)
        {
            Debug.LogError("Lista de sprites vacía o no asignada.");
            return null;
        }
        int index = Random.Range(0, spriteList.Count);
        return spriteList[index];
    }

    void LaunchCube()
    {
        GameObject cube = Instantiate(cubePrefab, transform.position, Random.rotation);
        ConfigureCube(cube, obtainedDinoRarity);

        Rigidbody rb = cube.GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("El cubo no tiene Rigidbody.");
            return;
        }

        rb.useGravity = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Vector3 forceDirection = Vector3.up + new Vector3(
            Random.Range(-sidewaysForceVariation, sidewaysForceVariation),
            0f,
            Random.Range(-sidewaysForceVariation, sidewaysForceVariation)
        );
        forceDirection.Normalize();

        float randomUpwardForce = Random.Range(cubeUpwardForce * 0.9f, cubeUpwardForce * 1.1f);
        rb.AddForce(forceDirection * randomUpwardForce, ForceMode.Impulse);

        Vector3 randomTorque = Random.insideUnitSphere * cubeTorqueForce;
        rb.AddTorque(randomTorque, ForceMode.Impulse);

        if (camaraController != null)
        {
            camaraController.StartFollowingCube(cube.transform);
        }

        StartCoroutine(WaitForMaxTimeAndShowDino(cube));
    }

    void ConfigureCube(GameObject cube, Rarity rarity)
    {
        Renderer cubeRenderer = cube.GetComponent<Renderer>();
        TrailRenderer trail = cube.GetComponent<TrailRenderer>();
        ParticleSystem particles = cube.GetComponentInChildren<ParticleSystem>(true);

        // Configurar según la rareza
        switch (rarity)
        {
            case Rarity.Common:
                if (cubeRenderer != null) cubeRenderer.material.color = Color.white;
                if (trail != null)
                {
                    trail.startColor = Color.white;
                    trail.endColor = Color.white;
                    trail.enabled = true;
                }
                if (particles != null) particles.gameObject.SetActive(false);
                break;
            case Rarity.Rare:
                if (cubeRenderer != null) cubeRenderer.material.color = Color.blue;
                if (trail != null)
                {
                    trail.startColor = Color.blue;
                    trail.endColor = Color.blue;
                    trail.enabled = true;
                }
                if (particles != null) particles.gameObject.SetActive(false);
                break;
            case Rarity.ShinyCommon:
                if (cubeRenderer != null) cubeRenderer.material.color = Color.white;
                if (trail != null)
                {
                    trail.startColor = Color.white;
                    trail.endColor = Color.white;
                    trail.enabled = true;
                }
                if (particles != null)
                {
                    particles.gameObject.SetActive(true);
                    var main = particles.main;
                    main.startColor = new ParticleSystem.MinMaxGradient(Color.white, Color.cyan);
                }
                break;
            case Rarity.ShinyRare:
                if (cubeRenderer != null) cubeRenderer.material.color = Color.blue;
                if (trail != null)
                {
                    trail.startColor = Color.blue;
                    trail.endColor = Color.blue;
                    trail.enabled = true;
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

    IEnumerator WaitForMaxTimeAndShowDino(GameObject cube)
    {
        yield return new WaitForSeconds(maxWaitTime);

        if (obtainedDinoSprite != null)
        {
            StartCoroutine(DisplayDinoWithScale(obtainedDinoSprite, obtainedDinoInfo));
        }
        else
        {
            Debug.LogError("No se ha obtenido un sprite de dinosaurio válido.");
        }

        Destroy(cube);

        if (camaraController != null)
        {
            camaraController.StopFollowingCube();
        }

        yield return new WaitForSeconds(3f);
        ReturnToBoard();
    }

    IEnumerator DisplayDinoWithScale(Sprite dinoSprite, string dinoInfo)
    {
        dinoDisplay.gameObject.SetActive(true);
        dinoDisplay.sprite = dinoSprite;

        RectTransform rectTransform = dinoDisplay.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(dinoSprite.rect.width * dinoScaleMultiplier, dinoSprite.rect.height * dinoScaleMultiplier);

        dinoDisplay.transform.localScale = Vector3.zero;

        float elapsedTime = 0f;
        while (elapsedTime < dinoScaleDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / dinoScaleDuration;
            float scale = Mathf.SmoothStep(0f, 1f, t);
            dinoDisplay.transform.localScale = new Vector3(scale, scale, scale);
            yield return null;
        }

        dinoDisplay.transform.localScale = Vector3.one;

        if (dinoInfoText != null)
        {
            dinoInfoText.text = dinoInfo;
        }
    }

    void ReturnToBoard()
    {
        SceneManager.LoadScene("Tablero");
    }
}
