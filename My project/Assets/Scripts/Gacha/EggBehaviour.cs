// EggBehaviour.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// Clase que maneja el comportamiento del huevo en el juego, incluyendo la animaci�n del huevo,
/// la generaci�n y lanzamiento del cubo, y la visualizaci�n del dinosaurio obtenido.
/// </summary>
public class EggBehaviour : MonoBehaviour
{
    [Header("Egg Models")]
    public List<GameObject> eggStages; // Asigna los modelos del huevo en orden de rareza
    // Aseg�rate de que los prefabs est�n en el orden: Azul, Morado, Rojo, Dorado

    [Header("Egg Animation")]
    public float rotationAmount = 15f; // Grados de inclinaci�n para la animaci�n
    public float rotationSpeed = 5f;   // Velocidad de inclinaci�n
    public float rotationPauseDuration = 0.2f; // Pausa entre inclinaciones

    public float moveDistance = 1f;    // Distancia que se mover� el huevo de izquierda a derecha
    public float moveDuration = 1f;    // Duraci�n del movimiento de izquierda a derecha

    [Header("Cube Settings")]
    public GameObject cubePrefab;           // Prefab del cubo que ser� lanzado
    public float cubeUpwardForce = 7f;      // Fuerza hacia arriba aplicada al cubo
    public float cubeTorqueForce = 1000f;   // Fuerza de torque aplicada al cubo
    public float sidewaysForceVariation = 0.1f; // Variaci�n aleatoria lateral en la fuerza

    [Header("Dinosaur Display")]
    public Image dinoDisplay;               // Imagen UI para mostrar el dinosaurio
    public Text dinoInfoText;               // Texto para mostrar informaci�n del dinosaurio
    public float dinoScaleDuration = 1f;    // Duraci�n de la animaci�n de escala

    [Header("Dinosaur Scaling")]
    public float dinoScaleMultiplier = 0.5f; // Multiplicador para ajustar el tama�o del dinosaurio desde el Inspector

    [Header("Dinosaur Sprites")]
    public List<Sprite> commonDinos;
    public List<Sprite> rareDinos;
    public List<Sprite> shinyCommonDinos;
    public List<Sprite> shinyRareDinos;

    [Header("Timing Settings")]
    public float maxWaitTime = 5f; // Tiempo m�ximo de espera antes de mostrar el dinosaurio

    [Header("QTE Settings")]
    public QTEManager qteManager; // Referencia al QTEManager
    public BeatManager beatManager; // Referencia al BeatManager
    public int maxQTEs = 4; // N�mero m�ximo de QTEs (actualizado a 4)

    [Header("QTE Canvas Settings")]
    [Tooltip("Arrastra aqu� el QTECanvas para activarlo al entrar a la escena y desactivarlo al lanzar el cubo.")]
    public CanvasGroup qteCanvasGroup; // CanvasGroup para hacer el fade out

    private int successfulQTEs = 0; // Contador de QTEs exitosos
    private int currentQTEAttempt = 0; // Contador de intentos de QTE (aciertos y fallos)
    private bool isAnimating = false;
    private bool isHatching = false; // Indica si el huevo est� eclosionando
    private GameObject currentEggModel; // Modelo de huevo activo
    private Quaternion initialRotation;

    // Informaci�n del dinosaurio obtenido
    private Sprite obtainedDinoSprite;
    private string obtainedDinoInfo;
    private Rarity obtainedDinoRarity;

    private CamaraControllerGacha camaraController;

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

        // Obtener la referencia al CamaraControllerGacha
        camaraController = Camera.main.GetComponent<CamaraControllerGacha>();

        // Configurar los eventos del QTEManager
        if (qteManager != null)
        {
            qteManager.onQTESuccess.AddListener(OnQTESuccess);
            qteManager.onQTEFail.AddListener(OnQTEFail);
        }
        else
        {
            Debug.LogError("QTEManager no est� asignado en el Inspector.");
        }

        // Activar el QTECanvas al entrar a la escena
        if (qteCanvasGroup != null)
        {
            qteCanvasGroup.gameObject.SetActive(true);
            qteCanvasGroup.alpha = 1f;
        }
        else
        {
            Debug.LogWarning("QTECanvasGroup no est� asignado en EggBehaviour.");
        }
    }

    void OnDestroy()
    {
        // Desuscribirse de los eventos para evitar errores
        if (qteManager != null)
        {
            qteManager.onQTESuccess.RemoveListener(OnQTESuccess);
            qteManager.onQTEFail.RemoveListener(OnQTEFail);
        }
    }

    /// <summary>
    /// M�todo que se llama cuando el QTE es exitoso.
    /// </summary>
    public void OnQTESuccess()
    {
        if (isAnimating || isHatching) return;

        successfulQTEs++;
        currentQTEAttempt++;

        Debug.Log($"QTE Exitoso. Total exitosos: {successfulQTEs}");

        if (currentQTEAttempt >= maxQTEs)
        {
            // Finalizar secuencia de QTE y proceder con el siguiente paso
            StartCoroutine(WaitAndAnimateEggThenHatch());
        }
        else
        {
            StartCoroutine(EggQTERoutine());
            // Mejorar el huevo si es posible
            UpdateEggModel();
        }
    }

    /// <summary>
    /// M�todo que se llama cuando el QTE falla.
    /// </summary>
    public void OnQTEFail()
    {
        if (isAnimating || isHatching) return;

        currentQTEAttempt++;

        Debug.Log($"QTE Fallido. Intentos realizados: {currentQTEAttempt}");

        if (currentQTEAttempt >= maxQTEs)
        {
            // Finalizar secuencia de QTE y proceder con el siguiente paso
            StartCoroutine(WaitAndAnimateEggThenHatch());
        }
        else
        {
            // No mejorar el huevo, pero animarlo
            StartCoroutine(EggQTERoutine());
        }
    }

    IEnumerator WaitAndAnimateEggThenHatch()
    {
        isHatching = true;

        // Desactivar los eventos del QTEManager
        qteManager.onQTESuccess.RemoveListener(OnQTESuccess);
        qteManager.onQTEFail.RemoveListener(OnQTEFail);

        // Esperar 1 segundo
        yield return new WaitForSeconds(1f);

        // Mover el huevo de izquierda a derecha
        yield return StartCoroutine(MoveEggLeftRight());

        // Hacer fade out del QTECanvas (opcional)
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

        // Esperar un momento antes de eclosionar
        yield return new WaitForSeconds(0.5f);

        // Eclosionar el huevo
        HatchEgg();
    }

    IEnumerator EggQTERoutine()
    {
        isAnimating = true;

        // Animar el huevo (inclinaci�n hacia ambos lados)
        yield return StartCoroutine(RotateEggBothSides());

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

    IEnumerator MoveEggLeftRight()
    {
        Vector3 startPosition = currentEggModel.transform.position;
        Vector3 leftPosition = startPosition - new Vector3(moveDistance / 2f, 0, 0);
        Vector3 rightPosition = startPosition + new Vector3(moveDistance / 2f, 0, 0);

        float elapsedTime = 0f;

        // Mover hacia la izquierda
        while (elapsedTime < moveDuration / 2f)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / (moveDuration / 2f);
            currentEggModel.transform.position = Vector3.Lerp(startPosition, leftPosition, t);
            yield return null;
        }

        // Mover hacia la derecha
        elapsedTime = 0f;
        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / moveDuration;
            currentEggModel.transform.position = Vector3.Lerp(leftPosition, rightPosition, t);
            yield return null;
        }

        // Regresar a la posici�n inicial
        elapsedTime = 0f;
        while (elapsedTime < moveDuration / 2f)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / (moveDuration / 2f);
            currentEggModel.transform.position = Vector3.Lerp(rightPosition, startPosition, t);
            yield return null;
        }

        // Asegurarse de que la posici�n final es exacta
        currentEggModel.transform.position = startPosition;
    }

    void UpdateEggModel()
    {
        int eggIndex = Mathf.Clamp(successfulQTEs, 0, eggStages.Count - 1);
        SetEggStage(eggIndex);
        Debug.Log($"El huevo ha mejorado a: {currentEggModel.name}");
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
        // Ocultar el huevo
        currentEggModel.SetActive(false);

        // Desactivar el collider del huevo
        Collider eggCollider = GetComponent<Collider>();
        if (eggCollider != null)
        {
            eggCollider.enabled = false;
        }

        // Determinar el dinosaurio obtenido basado en la rareza del huevo
        GetRandomDino(successfulQTEs, out obtainedDinoSprite, out obtainedDinoInfo, out obtainedDinoRarity);

        // Lanzar el cubo correspondiente
        LaunchCube();
    }

    void GetRandomDino(int eggLevel, out Sprite dinoSprite, out string dinoInfo, out Rarity rarity)
    {
        float randomValue = Random.Range(0f, 100f);
        dinoSprite = null;
        dinoInfo = "";
        rarity = Rarity.Common;

        // Determinar las probabilidades seg�n el nivel del huevo
        switch (eggLevel)
        {
            case 0: // Huevo Azul
                if (randomValue < 70f)
                {
                    // 70% Com�n
                    dinoSprite = GetRandomSpriteFromList(commonDinos);
                    dinoInfo = "Dinosaurio Com�n";
                    rarity = Rarity.Common;
                }
                else
                {
                    // 30% Raro
                    dinoSprite = GetRandomSpriteFromList(rareDinos);
                    dinoInfo = "Dinosaurio Raro";
                    rarity = Rarity.Rare;
                }
                break;

            case 1: // Huevo Morado
                if (randomValue < 57f)
                {
                    // 57% Com�n
                    dinoSprite = GetRandomSpriteFromList(commonDinos);
                    dinoInfo = "Dinosaurio Com�n";
                    rarity = Rarity.Common;
                }
                else if (randomValue < 97f)
                {
                    // 40% Raro
                    dinoSprite = GetRandomSpriteFromList(rareDinos);
                    dinoInfo = "Dinosaurio Raro";
                    rarity = Rarity.Rare;
                }
                else
                {
                    // 3% Shiny Com�n
                    dinoSprite = GetRandomSpriteFromList(shinyCommonDinos);
                    dinoInfo = "�Dinosaurio Shiny Com�n!";
                    rarity = Rarity.ShinyCommon;
                }
                break;

            case 2: // Huevo Rojo
                if (randomValue < 45f)
                {
                    // 45% Com�n
                    dinoSprite = GetRandomSpriteFromList(commonDinos);
                    dinoInfo = "Dinosaurio Com�n";
                    rarity = Rarity.Common;
                }
                else if (randomValue < 90f)
                {
                    // 45% Raro
                    dinoSprite = GetRandomSpriteFromList(rareDinos);
                    dinoInfo = "Dinosaurio Raro";
                    rarity = Rarity.Rare;
                }
                else if (randomValue < 97f)
                {
                    // 7% Shiny Com�n
                    dinoSprite = GetRandomSpriteFromList(shinyCommonDinos);
                    dinoInfo = "�Dinosaurio Shiny Com�n!";
                    rarity = Rarity.ShinyCommon;
                }
                else
                {
                    // 3% Shiny Raro
                    dinoSprite = GetRandomSpriteFromList(shinyRareDinos);
                    dinoInfo = "�Dinosaurio Shiny Raro!";
                    rarity = Rarity.ShinyRare;
                }
                break;

            case 3: // Huevo Dorado
                if (randomValue < 35f)
                {
                    // 35% Com�n
                    dinoSprite = GetRandomSpriteFromList(commonDinos);
                    dinoInfo = "Dinosaurio Com�n";
                    rarity = Rarity.Common;
                }
                else if (randomValue < 80f)
                {
                    // 45% Raro
                    dinoSprite = GetRandomSpriteFromList(rareDinos);
                    dinoInfo = "Dinosaurio Raro";
                    rarity = Rarity.Rare;
                }
                else if (randomValue < 92f)
                {
                    // 12% Shiny Com�n
                    dinoSprite = GetRandomSpriteFromList(shinyCommonDinos);
                    dinoInfo = "�Dinosaurio Shiny Com�n!";
                    rarity = Rarity.ShinyCommon;
                }
                else
                {
                    // 8% Shiny Raro
                    dinoSprite = GetRandomSpriteFromList(shinyRareDinos);
                    dinoInfo = "�Dinosaurio Shiny Raro!";
                    rarity = Rarity.ShinyRare;
                }
                break;

            default:
                Debug.LogError("Nivel de huevo desconocido.");
                break;
        }
    }

    Sprite GetRandomSpriteFromList(List<Sprite> spriteList)
    {
        if (spriteList == null || spriteList.Count == 0)
        {
            Debug.LogError("Lista de sprites est� vac�a o no asignada.");
            return null;
        }

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
        if (rb == null)
        {
            Debug.LogError("El cubo no tiene un componente Rigidbody.");
            return;
        }

        // Aplicar fuerzas para lanzarlo
        rb.useGravity = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Calcular la direcci�n de la fuerza con variaci�n aleatoria
        Vector3 forceDirection = Vector3.up + new Vector3(
            Random.Range(-sidewaysForceVariation, sidewaysForceVariation),
            0f,
            Random.Range(-sidewaysForceVariation, sidewaysForceVariation)
        );
        forceDirection.Normalize();

        // Aplicar fuerza hacia arriba con variaci�n aleatoria
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

        // Iniciar coroutine para esperar el tiempo m�ximo antes de mostrar el dinosaurio
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
                if (cubeRenderer != null)
                {
                    cubeRenderer.material.color = Color.white;
                }
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
                if (cubeRenderer != null)
                {
                    cubeRenderer.material.color = Color.blue;
                }
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
                // Cubo blanco con estela blanca y part�culas brillantes
                if (cubeRenderer != null)
                {
                    cubeRenderer.material.color = Color.white;
                }
                if (trail != null)
                {
                    trail.startColor = Color.white;
                    trail.endColor = Color.white;
                    trail.enabled = true;
                }
                if (particles != null)
                {
                    particles.gameObject.SetActive(true);
                    // Configurar las part�culas para shiny common
                    var main = particles.main;
                    main.startColor = new ParticleSystem.MinMaxGradient(Color.white, Color.cyan);
                }
                break;
            case Rarity.ShinyRare:
                // Cubo azul con estela azul y part�culas brillantes
                if (cubeRenderer != null)
                {
                    cubeRenderer.material.color = Color.blue;
                }
                if (trail != null)
                {
                    trail.startColor = Color.blue;
                    trail.endColor = Color.blue;
                    trail.enabled = true;
                }
                if (particles != null)
                {
                    particles.gameObject.SetActive(true);
                    // Configurar las part�culas para shiny rare
                    var main = particles.main;
                    main.startColor = new ParticleSystem.MinMaxGradient(Color.blue, Color.magenta);
                }
                break;
        }
    }

    IEnumerator WaitForMaxTimeAndShowDino(GameObject cube)
    {
        // Esperar durante el tiempo m�ximo especificado
        yield return new WaitForSeconds(maxWaitTime);

        // Mostrar el dinosaurio obtenido
        if (obtainedDinoSprite != null)
        {
            StartCoroutine(DisplayDinoWithScale(obtainedDinoSprite, obtainedDinoInfo));
        }
        else
        {
            Debug.LogError("No se ha obtenido un sprite de dinosaurio v�lido.");
        }

        // Destruir el cubo
        Destroy(cube);

        // Notificar al CamaraControllerGacha que deje de seguir al cubo
        if (camaraController != null)
        {
            camaraController.StopFollowingCube();
        }

        // Esperar 3 segundos antes de regresar al tablero
        yield return new WaitForSeconds(3f);

        // Regresar al tablero
        ReturnToBoard();
    }

    IEnumerator DisplayDinoWithScale(Sprite dinoSprite, string dinoInfo)
    {
        // Activar el display del dinosaurio
        dinoDisplay.gameObject.SetActive(true);
        dinoDisplay.sprite = dinoSprite;

        // Ajustar el tama�o de la imagen al tama�o del sprite, multiplicado por el multiplicador de escala
        RectTransform rectTransform = dinoDisplay.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(dinoSprite.rect.width * dinoScaleMultiplier, dinoSprite.rect.height * dinoScaleMultiplier);

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

    void ReturnToBoard()
    {
        // Regresar al tablero sin perder el estado
        SceneManager.LoadScene("Tablero");
    }
}

// Aseg�rate de tener la enumeraci�n Rarity definida en Rarity.cs o en otro lugar
/*
public enum Rarity
{
    Common,
    Rare,
    ShinyCommon,
    ShinyRare
}
*/
