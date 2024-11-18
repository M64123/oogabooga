using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class QTEManager : MonoBehaviour
{
    public static QTEManager Instance { get; private set; }

    [Header("QTE Settings")]
    public KeyCode qteKey = KeyCode.Space; // La tecla que el jugador debe presionar
    public GameObject qteIndicatorLeftPrefab; // Prefab del indicador izquierdo
    public GameObject qteIndicatorRightPrefab; // Prefab del indicador derecho
    public Transform qteBarTransform; // Transform de la barra QTE

    [Header("QTE Events")]
    public UnityEvent onQTESuccess; // Evento que se dispara en �xito
    public UnityEvent onQTEFail;    // Evento que se dispara en fallo

    [Header("Audio Feedback")]
    public AudioClip successClip;
    public AudioClip failClip;
    private AudioSource audioSource;

    private string currentSceneName;

    private bool isQTEActive = true; // Activado por defecto
    private float beatCounter = 0f; // Contador de beats acumulados

    [Header("QTE Detection Settings")]
    [Range(10f, 100f)]
    [Tooltip("Distancia m�xima permitida desde el centro para considerar un QTE exitoso.")]
    public float successThreshold = 50f; // Valor por defecto aumentado para facilitar el �xito

    [Header("QTE Center")]
    public RectTransform qteCenterRectTransform; // Asigna el RectTransform del QTECenter desde el Inspector

    private bool isQTEInProgress = false; // Variable para evitar m�ltiples QTEs simult�neos

    void Awake()
    {
        // Implementaci�n del Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        CheckSceneActivation();

        // Inicializar AudioSource para feedback auditivo
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Verificar que qteCenterRectTransform est� asignado
        if (qteCenterRectTransform == null)
        {
            Debug.LogError("QTECenter RectTransform no est� asignado en el Inspector.");
        }
    }

    void OnDestroy()
    {
        // No es necesario desuscribirse ya que BeatManager llama directamente a HandleBeat()
    }

    void CheckSceneActivation()
    {
        // Puedes implementar l�gica adicional para activar/desactivar QTEs basados en la escena
        isQTEActive = true; // Por simplicidad, siempre activo. Ajusta seg�n tus necesidades.
    }

    /// <summary>
    /// M�todo llamado por BeatManager en cada beat.
    /// </summary>
    public void HandleBeat()
    {
        if (!isQTEActive)
            return;

        beatCounter += 1f;

        // Iterar sobre la lista de QTETriggers en BeatManager para ver si alguno debe activarse
        foreach (QTETrigger trigger in BeatManager.Instance.qteTriggers)
        {
            if (beatCounter >= trigger.beatsAfterLastTrigger)
            {
                if (!isQTEInProgress) // Evitar m�ltiples QTEs simult�neos
                {
                    // Activar el QTE seg�n el tipo definido
                    StartCoroutine(ActivateQTE(trigger));

                    // Invocar el evento espec�fico del trigger
                    trigger.onQTEActivated.Invoke();

                    // Restar el beatsAfterLastTrigger del contador
                    beatCounter -= trigger.beatsAfterLastTrigger;
                }
            }
        }
    }

    /// <summary>
    /// Activa la secuencia QTE seg�n la configuraci�n del trigger.
    /// </summary>
    /// <param name="trigger">El trigger que define c�mo activar el QTE.</param>
    /// <returns></returns>
    IEnumerator ActivateQTE(QTETrigger trigger)
    {
        if (isQTEInProgress)
            yield break; // Evitar m�ltiples QTEs

        isQTEInProgress = true;

        // Seleccionar los prefabs personalizados si est�n definidos
        GameObject leftPrefab = trigger.customLeftPrefab != null ? trigger.customLeftPrefab : qteIndicatorLeftPrefab;
        GameObject rightPrefab = trigger.customRightPrefab != null ? trigger.customRightPrefab : qteIndicatorRightPrefab;

        // Instanciar indicadores seg�n el tipo de QTE
        GameObject indicatorLeft = null;
        GameObject indicatorRight = null;

        if (trigger.qteType == QTEType.Left || trigger.qteType == QTEType.Both)
        {
            indicatorLeft = Instantiate(leftPrefab, qteBarTransform);
            RectTransform leftRect = indicatorLeft.GetComponent<RectTransform>();
            leftRect.localScale = Vector3.one; // Asegurar escala original
        }

        if (trigger.qteType == QTEType.Right || trigger.qteType == QTEType.Both)
        {
            indicatorRight = Instantiate(rightPrefab, qteBarTransform);
            RectTransform rightRect = indicatorRight.GetComponent<RectTransform>();
            rightRect.localScale = Vector3.one; // Asegurar escala original
        }

        // Obtener el componente RectTransform para manipular posiciones
        RectTransform barRect = qteBarTransform.GetComponent<RectTransform>();

        // Usar la referencia directa a QTECenter
        if (qteCenterRectTransform == null)
        {
            Debug.LogError("QTECenter RectTransform no est� asignado. Abortando QTE.");
            yield break;
        }

        Vector2 centerPos = qteCenterRectTransform.anchoredPosition;

        // Definir posiciones de inicio
        Vector2 leftStartPos = new Vector2(-barRect.rect.width / 2, 0);
        Vector2 rightStartPos = new Vector2(barRect.rect.width / 2, 0);

        // Asignar posiciones iniciales
        if (indicatorLeft != null)
        {
            RectTransform leftRect = indicatorLeft.GetComponent<RectTransform>();
            leftRect.anchoredPosition = leftStartPos;
        }

        if (indicatorRight != null)
        {
            RectTransform rightRect = indicatorRight.GetComponent<RectTransform>();
            rightRect.anchoredPosition = rightStartPos;
        }

        // Definir duraci�n del movimiento basado en BPM
        float beatInterval = 60f / BeatManager.Instance.BPM;
        float moveDuration = beatInterval; // Tiempo para completar el movimiento

        // Mover indicadores hacia el centro
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / moveDuration);

            // Movimiento lineal hacia el centro
            if (indicatorLeft != null)
            {
                RectTransform leftRect = indicatorLeft.GetComponent<RectTransform>();
                leftRect.anchoredPosition = Vector2.Lerp(leftStartPos, centerPos, t);
            }

            if (indicatorRight != null)
            {
                RectTransform rightRect = indicatorRight.GetComponent<RectTransform>();
                rightRect.anchoredPosition = Vector2.Lerp(rightStartPos, centerPos, t);
            }

            // Si se presiona la tecla, evaluar si es �xito o fallo
            if (Input.GetKeyDown(qteKey))
            {
                bool success = CheckQTESuccess(indicatorLeft, indicatorRight, centerPos);
                if (success)
                {
                    OnQTESuccess(trigger);
                }
                else
                {
                    OnQTEFail(trigger);
                }

                // Destruir los indicadores
                if (indicatorLeft != null)
                    Destroy(indicatorLeft);
                if (indicatorRight != null)
                    Destroy(indicatorRight);

                isQTEInProgress = false;
                yield break;
            }

            yield return null;
        }

        // Si no se presiona la tecla antes de que el movimiento termine, contar como fallo
        OnQTEFail(trigger);

        // Destruir los indicadores
        if (indicatorLeft != null)
            Destroy(indicatorLeft);
        if (indicatorRight != null)
            Destroy(indicatorRight);

        isQTEInProgress = false;
    }

    /// <summary>
    /// Comprueba si el QTE ha sido exitoso en base a la distancia de los indicadores al centro.
    /// </summary>
    /// <returns>True si el QTE ha sido exitoso, False si no.</returns>
    bool CheckQTESuccess(GameObject indicatorLeft, GameObject indicatorRight, Vector2 centerPos)
    {
        bool leftNear = false;
        bool rightNear = false;

        if (indicatorLeft != null)
        {
            RectTransform leftRect = indicatorLeft.GetComponent<RectTransform>();
            float distanceLeft = Vector2.Distance(leftRect.anchoredPosition, centerPos);
            leftNear = distanceLeft <= successThreshold;
        }

        if (indicatorRight != null)
        {
            RectTransform rightRect = indicatorRight.GetComponent<RectTransform>();
            float distanceRight = Vector2.Distance(rightRect.anchoredPosition, centerPos);
            rightNear = distanceRight <= successThreshold;
        }

        return leftNear && rightNear;
    }

    /// <summary>
    /// Maneja el �xito del QTE.
    /// </summary>
    void OnQTESuccess(QTETrigger trigger)
    {
        onQTESuccess.Invoke();
        Debug.Log("QTE Exitoso!");

        // Reproducir sonido de �xito
        if (successClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(successClip);
        }

        StartCoroutine(RevertCenterColor(Color.green));
    }

    /// <summary>
    /// Maneja el fallo del QTE.
    /// </summary>
    void OnQTEFail(QTETrigger trigger)
    {
        onQTEFail.Invoke();
        Debug.Log("QTE Fallido!");

        // Reproducir sonido de fallo
        if (failClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(failClip);
        }

        StartCoroutine(RevertCenterColor(Color.red));
    }

    /// <summary>
    /// Coroutine para revertir el color de QTECenter al color original despu�s de un tiempo.
    /// </summary>
    IEnumerator RevertCenterColor(Color color)
    {
        Image centerImage = qteCenterRectTransform.GetComponent<Image>();
        if (centerImage != null)
        {
            centerImage.color = color;
            yield return new WaitForSeconds(0.5f);
            centerImage.color = Color.white; // Revertir al color original
        }
    }
}
