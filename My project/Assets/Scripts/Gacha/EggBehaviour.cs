using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class EggBehaviour : MonoBehaviour
{
    [Header("Egg Models")]
    public List<GameObject> eggStages;

    [Header("Egg Animation")]
    public float rotationAmount = 15f;
    public float rotationSpeed = 5f;
    public float rotationPauseDuration = 0.2f;
    [Tooltip("Duraci�n de la rotaci�n agresiva final antes de eclosionar.")]
    public float aggressiveRotationDuration = 1f;
    [Tooltip("Grados de rotaci�n para la rotaci�n agresiva final.")]
    public float aggressiveRotationDegrees = 30f;

    [Header("Cube Settings")]
    public GameObject cubePrefab;
    public float cubeUpwardForce = 7f;
    public float cubeTorqueForce = 1000f;
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

    [Header("Dinosaur IDs")]
    public List<string> commonDinoIDs;
    public List<string> rareDinoIDs;
    public List<string> shinyCommonDinoIDs;
    public List<string> shinyRareDinoIDs;

    [Header("Timing Settings")]
    public float maxWaitTime = 5f;

    [Header("QTE Settings")]
    public QTEManager qteManager;
    public BeatManager beatManager;
    public int maxQTEs = 4; // 4 QTEs totales

    [Header("QTE Canvas Settings")]
    public CanvasGroup qteCanvasGroup;

    private int successfulQTEs = 0;
    private int currentQTEAttempt = 0;
    private bool isAnimating = false;
    private bool isHatching = false;
    private GameObject currentEggModel;
    private Quaternion initialRotation;

    private Sprite obtainedDinoSprite;
    private string obtainedDinoInfo;
    private Rarity obtainedDinoRarity;
    private string obtainedDinoID;
    private string obtainedDinoName;

    private CamaraControllerGacha camaraController;

    // Nombres tribales
    private static string[] tribalNames = new string[100]
    {
        "Amani","Zahir","Leilani","Kian","Naya","Kai","Zara","Idris","Amara","Malik",
        "Suri","Rohan","Kalani","Taj","Eshe","Zane","Nyah","Kira","Jamal","Yasmin",
        "Kairo","Saffron","Omari","Ayana","Zaki","Nala","Tariq","Keanu","Zola","Rashid",
        "Alina","Nazir","Kiara","Malikah","Zarae","Kiani","Sura","Amir","Anaya","Zaneh",
        "Leila","Kamal","Amira","Zain","Laila","Kamil","Inaya","Farid","Nia","Jamal",
        "Sanaa","Razi","Amaya","Tariqah","Kalila","Zaid","Malaika","Nadir","Safiya","Rahim",
        "Aisha","Karim","Zora","Hakim","Samira","Idrisah","Liyana","Faris","Nadiya","Zahirah",
        "Amaniha","Kamran","Kiana","Raheem","Ayda","Jamil","Anisa","Tahir","Zahra","Nasir",
        "Elara","Haris","Layla","Iman","Zayn","Amirah","Azaan","Farah","Ishaan","Samara",
        "Kahlil","Amari","Zaira","Kamari","Aziza","Hamza","Sade","Nabil","Alaya","Suriya"
    };

    // Flag para permitir el inicio de QTE s�lo tras los 3 zooms
    private bool qteStartAllowed = false;

    void Start()
    {
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
            // Desactivar QTE hasta que se permitan tras los 3 zooms
            qteManager.isQTEActive = false;
        }
        else
        {
            Debug.LogError("QTEManager no asignado en EggBehaviour.");
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

        // Esperamos a que la camara haga 3 zoom in. La l�gica de zoom est� en CamaraControllerGacha.
        // Cuando la c�mara termine, llamar� a AllowQTEStart().
    }

    void OnDestroy()
    {
        if (qteManager != null)
        {
            qteManager.onQTESuccess.RemoveListener(OnQTESuccess);
            qteManager.onQTEFail.RemoveListener(OnQTEFail);
        }
    }

    // Llamado por la c�mara cuando acaba los 3 zoom+shake
    public void AllowQTEStart()
    {
        qteStartAllowed = true;
        if (qteManager != null)
        {
            qteManager.isQTEActive = true;
        }
    }

    public void OnQTESuccess()
    {
        if (isAnimating || isHatching) return;

        successfulQTEs++;
        currentQTEAttempt++;
        Debug.Log($"QTE Exitoso. Total exitosos: {successfulQTEs}");

        // Camera shake en cada QTE �xito
        if (camaraController != null)
        {
            camaraController.QTESuccessFeedback();
        }

        if (currentQTEAttempt >= maxQTEs)
        {
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
            // No mejora el huevo por fallo
        }
    }

    IEnumerator EndQTESequence()
    {
        isHatching = true;

        // Quitar eventos QTE
        if (qteManager != null)
        {
            qteManager.onQTESuccess.RemoveListener(OnQTESuccess);
            qteManager.onQTEFail.RemoveListener(OnQTEFail);
        }

        // Fade out QTECanvas
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

        // Rotar agresivamente antes de eclosionar
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
        float elapsed = 0f;
        Quaternion startRot = currentEggModel.transform.rotation;
        while (elapsed < aggressiveRotationDuration)
        {
            elapsed += Time.deltaTime;
            float angle = Mathf.Sin(elapsed * 20f) * aggressiveRotationDegrees;
            currentEggModel.transform.rotation = startRot * Quaternion.Euler(0, 0, angle);
            yield return null;
        }
        currentEggModel.transform.rotation = initialRotation;
    }

    void UpdateEggModel()
    {
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
        if (currentEggModel != null)
        {
            currentEggModel.SetActive(false);
        }

        Collider eggCollider = GetComponent<Collider>();
        if (eggCollider != null)
        {
            eggCollider.enabled = false;
        }

        int eggLevel = Mathf.Clamp(successfulQTEs, 0, 3);

        GetRandomDino(eggLevel, out obtainedDinoSprite, out obtainedDinoInfo, out obtainedDinoRarity, out obtainedDinoID);

        obtainedDinoName = GetRandomTribalName();

        GameManager.Instance.AddDinosaur(obtainedDinoID, obtainedDinoName, obtainedDinoRarity);

        LaunchCube();
    }

    void GetRandomDino(int eggLevel, out Sprite dinoSprite, out string dinoInfo, out Rarity rarity, out string dinoID)
    {
        dinoSprite = null;
        dinoInfo = "";
        rarity = Rarity.Common;
        dinoID = "";

        float randomValue = Random.Range(0f, 100f);

        (string chosenID, Sprite chosenSprite) GetFromList(List<string> idList, List<Sprite> spriteList)
        {
            int idx = Random.Range(0, spriteList.Count);
            return (idList[idx], spriteList[idx]);
        }

        switch (eggLevel)
        {
            case 0:
                if (randomValue < 70f)
                {
                    var result = GetFromList(commonDinoIDs, commonDinos);
                    dinoID = result.chosenID; dinoSprite = result.chosenSprite;
                    dinoInfo = "Dinosaurio Com�n"; rarity = Rarity.Common;
                }
                else
                {
                    var result = GetFromList(rareDinoIDs, rareDinos);
                    dinoID = result.chosenID; dinoSprite = result.chosenSprite;
                    dinoInfo = "Dinosaurio Raro"; rarity = Rarity.Rare;
                }
                break;
            case 1:
                if (randomValue < 57f)
                {
                    var result = GetFromList(commonDinoIDs, commonDinos);
                    dinoID = result.chosenID; dinoSprite = result.chosenSprite;
                    dinoInfo = "Dinosaurio Com�n"; rarity = Rarity.Common;
                }
                else if (randomValue < 97f)
                {
                    var result = GetFromList(rareDinoIDs, rareDinos);
                    dinoID = result.chosenID; dinoSprite = result.chosenSprite;
                    dinoInfo = "Dinosaurio Raro"; rarity = Rarity.Rare;
                }
                else
                {
                    var result = GetFromList(shinyCommonDinoIDs, shinyCommonDinos);
                    dinoID = result.chosenID; dinoSprite = result.chosenSprite;
                    dinoInfo = "�Dinosaurio Shiny Com�n!"; rarity = Rarity.ShinyCommon;
                }
                break;
            case 2:
                if (randomValue < 45f)
                {
                    var result = GetFromList(commonDinoIDs, commonDinos);
                    dinoID = result.chosenID; dinoSprite = result.chosenSprite;
                    dinoInfo = "Dinosaurio Com�n"; rarity = Rarity.Common;
                }
                else if (randomValue < 90f)
                {
                    var result = GetFromList(rareDinoIDs, rareDinos);
                    dinoID = result.chosenID; dinoSprite = result.chosenSprite;
                    dinoInfo = "Dinosaurio Raro"; rarity = Rarity.Rare;
                }
                else if (randomValue < 97f)
                {
                    var result = GetFromList(shinyCommonDinoIDs, shinyCommonDinos);
                    dinoID = result.chosenID; dinoSprite = result.chosenSprite;
                    dinoInfo = "�Dinosaurio Shiny Com�n!"; rarity = Rarity.ShinyCommon;
                }
                else
                {
                    var result = GetFromList(shinyRareDinoIDs, shinyRareDinos);
                    dinoID = result.chosenID; dinoSprite = result.chosenSprite;
                    dinoInfo = "�Dinosaurio Shiny Raro!"; rarity = Rarity.ShinyRare;
                }
                break;
            case 3:
                if (randomValue < 35f)
                {
                    var result = GetFromList(commonDinoIDs, commonDinos);
                    dinoID = result.chosenID; dinoSprite = result.chosenSprite;
                    dinoInfo = "Dinosaurio Com�n"; rarity = Rarity.Common;
                }
                else if (randomValue < 80f)
                {
                    var result = GetFromList(rareDinoIDs, rareDinos);
                    dinoID = result.chosenID; dinoSprite = result.chosenSprite;
                    dinoInfo = "Dinosaurio Raro"; rarity = Rarity.Rare;
                }
                else if (randomValue < 92f)
                {
                    var result = GetFromList(shinyCommonDinoIDs, shinyCommonDinos);
                    dinoID = result.chosenID; dinoSprite = result.chosenSprite;
                    dinoInfo = "�Dinosaurio Shiny Com�n!"; rarity = Rarity.ShinyCommon;
                }
                else
                {
                    var result = GetFromList(shinyRareDinoIDs, shinyRareDinos);
                    dinoID = result.chosenID; dinoSprite = result.chosenSprite;
                    dinoInfo = "�Dinosaurio Shiny Raro!"; rarity = Rarity.ShinyRare;
                }
                break;
            default:
                Debug.LogError("Nivel de huevo desconocido: " + eggLevel);
                if (commonDinos.Count > 0 && commonDinoIDs.Count == commonDinos.Count)
                {
                    var result = GetFromList(commonDinoIDs, commonDinos);
                    dinoID = result.chosenID; dinoSprite = result.chosenSprite;
                    dinoInfo = "Dinosaurio Com�n"; rarity = Rarity.Common;
                }
                else
                {
                    Debug.LogError("No se ha obtenido un dino por defecto.");
                }
                break;
        }

        if (dinoSprite == null)
        {
            Debug.LogError("No se ha obtenido un sprite de dinosaurio v�lido.");
            if (commonDinos.Count > 0 && commonDinoIDs.Count == commonDinos.Count)
            {
                dinoSprite = commonDinos[0];
                dinoID = commonDinoIDs[0];
                dinoInfo = "Dinosaurio Com�n";
                rarity = Rarity.Common;
            }
        }
    }

    string GetRandomTribalName()
    {
        int idx = Random.Range(0, tribalNames.Length);
        return tribalNames[idx];
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
            Debug.LogError("No se ha obtenido un sprite de dinosaurio v�lido.");
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
