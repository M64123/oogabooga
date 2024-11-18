// EggBehaviour.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// Clase que maneja el comportamiento del huevo en el juego, incluyendo la animaci�n del huevo, la generaci�n y lanzamiento del cubo, y la visualizaci�n del dinosaurio obtenido.
/// </summary>
public class EggBehaviour : MonoBehaviour
{
    [Header("Egg Models")]
    public List<GameObject> eggStages; // Asigna los modelos del huevo en orden

    [Header("Egg Animation")]
    public float rotationAmount = 15f; // Grados de inclinaci�n
    public float rotationSpeed = 5f;   // Velocidad de inclinaci�n
    public float rotationPauseDuration = 0.2f; // Pausa entre inclinaciones

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

    [Header("QTE Canvas Settings")]
    [Tooltip("Arrastra aqu� el QTECanvas para activarlo al entrar a la escena y desactivarlo al lanzar el cubo.")]
    public GameObject qteCanvas; // Referencia al QTECanvas que se activar� y desactivar�

    private int currentStage = 0;
    private bool isAnimating = false;
    private GameObject currentEggModel; // Modelo de huevo activo
    private Quaternion initialRotation;

    // Informaci�n del dinosaurio obtenido
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

        // Suscribirse al evento de beat del BeatManager
        if (beatManager != null)
        {
            beatManager.onBeat.AddListener(OnBeat);
        }
        else
        {
            Debug.LogError("BeatManager no est� asignado en el Inspector.");
        }

        // Activar el QTECanvas al entrar a la escena
        if (qteCanvas != null)
        {
            qteCanvas.SetActive(true);
            Debug.Log("QTECanvas activado al iniciar la escena.");
        }
        else
        {
            Debug.LogWarning("QTECanvas no est� asignado en EggBehaviour.");
        }
    }

    void OnDestroy()
    {
        // Desuscribirse de los eventos para evitar errores
        if (beatManager != null)
        {
            beatManager.onBeat.RemoveListener(OnBeat);
        }
        if (qteManager != null)
        {
            qteManager.onQTESuccess.RemoveListener(OnQTESuccess);
            qteManager.onQTEFail.RemoveListener(OnQTEFail);
        }
    }

    void OnBeat()
    {
        if (!isAnimating && !hasHatched && currentStage < eggStages.Count)
        {
            // El QTEManager ya maneja la secuencia QTE autom�ticamente en cada beat
            // Por lo tanto, no es necesario llamar a StartQTESequence() aqu�
            // El QTEManager manejar� la secuencia
        }
    }

    /// <summary>
    /// M�todo que se llama cuando el QTE es exitoso.
    /// Inicia la rutina de animaci�n del huevo.
    /// </summary>
    public void OnQTESuccess()
    {
        if (!isAnimating && !hasHatched)
        {
            StartCoroutine(EggQTERoutine());

            // Notificar al CamaraControllerGacha que el huevo ha sido interactuado
            if (camaraController != null)
            {
                camaraController.OnEggClicked();
            }
        }
    }

    /// <summary>
    /// M�todo que se llama cuando el QTE falla.
    /// Implementa retroalimentaci�n en caso de fallo.
    /// </summary>
    public void OnQTEFail()
    {
        // Implementar retroalimentaci�n en caso de fallo
        Debug.Log("QTE Fallido. Intenta nuevamente.");
        // Opcional: Reiniciar la secuencia QTE o penalizar al jugador
    }

    /// <summary>
    /// Coroutine que maneja la rutina de animaci�n del huevo tras un QTE exitoso.
    /// </summary>
    /// <returns></returns>
    IEnumerator EggQTERoutine()
    {
        isAnimating = true;

        // Animar el huevo (inclinaci�n hacia ambos lados)
        yield return StartCoroutine(RotateEggBothSides());

        // Avanzar al siguiente estado del huevo
        currentStage++;
        if (currentStage < eggStages.Count)
        {
            SetEggStage(currentStage);
            // El QTE se reiniciar� en el pr�ximo beat
        }
        else
        {
            // El huevo se rompe y aparece un cubo
            HatchEgg();
        }

        isAnimating = false;
    }

    /// <summary>
    /// Coroutine que rota el huevo hacia ambos lados aleatoriamente.
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Coroutine que rota el huevo hacia un lado espec�fico.
    /// </summary>
    /// <param name="direction">-1 para izquierda, 1 para derecha</param>
    /// <returns></returns>
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

    /// <summary>
    /// Coroutine que regresa el huevo a su rotaci�n inicial.
    /// </summary>
    /// <returns></returns>
    IEnumerator RotateEggToInitial()
    {
        while (Quaternion.Angle(currentEggModel.transform.rotation, initialRotation) > 0.1f)
        {
            currentEggModel.transform.rotation = Quaternion.RotateTowards(currentEggModel.transform.rotation, initialRotation, rotationSpeed * Time.deltaTime * 100f);
            yield return null;
        }
        currentEggModel.transform.rotation = initialRotation;
    }

    /// <summary>
    /// M�todo para establecer el modelo de huevo activo seg�n el estado actual.
    /// </summary>
    /// <param name="stage">�ndice del estado del huevo.</param>
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

    /// <summary>
    /// M�todo que maneja la rotura del huevo y la aparici�n del dinosaurio.
    /// </summary>
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

        // **Ocultar y desactivar el QTECanvas al lanzar el cubo**
        if (qteCanvas != null)
        {
            qteCanvas.SetActive(false); // Desactivar y ocultar el QTECanvas
            Debug.Log("QTECanvas desactivado y oculto al lanzar el cubo.");
        }
        else
        {
            Debug.LogWarning("QTECanvas no est� asignado en EggBehaviour.");
        }

        // Determinar el dinosaurio obtenido
        GetRandomDino(out obtainedDinoSprite, out obtainedDinoInfo, out obtainedDinoRarity);

        // Lanzar el cubo correspondiente
        LaunchCube();
    }

    /// <summary>
    /// M�todo que selecciona aleatoriamente un dinosaurio basado en probabilidades.
    /// </summary>
    /// <param name="dinoSprite">Sprite del dinosaurio obtenido.</param>
    /// <param name="dinoInfo">Informaci�n del dinosaurio obtenido.</param>
    /// <param name="rarity">Rareza del dinosaurio obtenido.</param>
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

    /// <summary>
    /// M�todo que obtiene un sprite aleatorio de una lista.
    /// </summary>
    /// <param name="spriteList">Lista de sprites.</param>
    /// <returns>Sprite seleccionado aleatoriamente.</returns>
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

    /// <summary>
    /// M�todo que instancia y lanza el cubo correspondiente al dinosaurio obtenido.
    /// </summary>
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

    /// <summary>
    /// M�todo que configura el cubo seg�n la rareza del dinosaurio.
    /// </summary>
    /// <param name="cube">Cubo a configurar.</param>
    /// <param name="rarity">Rareza del dinosaurio.</param>
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

    /// <summary>
    /// Coroutine que espera el tiempo m�ximo y luego muestra el dinosaurio obtenido.
    /// </summary>
    /// <param name="cube">Cubo lanzado.</param>
    /// <returns></returns>
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

    /// <summary>
    /// Coroutine que muestra el dinosaurio con una animaci�n de escala.
    /// </summary>
    /// <param name="dinoSprite">Sprite del dinosaurio.</param>
    /// <param name="dinoInfo">Informaci�n del dinosaurio.</param>
    /// <returns></returns>
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

        // **Nota:** He eliminado la funcionalidad relacionada con GameManager y la persistencia del dinosaurio.
    }

    DinosaurType GetDinosaurTypeFromRarity(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common:
                return DinosaurType.Comun;
            case Rarity.Rare:
                return DinosaurType.Raro;
            case Rarity.ShinyCommon:
                return DinosaurType.ShinyComun;
            case Rarity.ShinyRare:
                return DinosaurType.ShinyRaro;
            default:
                return DinosaurType.Comun;
        }
    }

    bool IsShiny(Rarity rarity)
    {
        return rarity == Rarity.ShinyCommon || rarity == Rarity.ShinyRare;
    }

    void ReturnToBoard()
    {
        // Regresar al tablero sin perder el estado
        SceneManager.LoadScene("Tablero");
    }
}

