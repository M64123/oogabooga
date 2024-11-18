// QTEManager.cs
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
    public UnityEvent onQTESuccess; // Evento que se dispara en éxito
    public UnityEvent onQTEFail;    // Evento que se dispara en fallo

    [Header("Audio Feedback")]
    public AudioClip successClip;
    public AudioClip failClip;
    private AudioSource audioSource;

    private string currentSceneName;

    private bool isQTEActive = true; // Activado por defecto
    private float beatCounter = 0f; // Contador de beats acumulados

    void Awake()
    {
        // Implementación del Singleton
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
    }

    void OnDestroy()
    {
        // No es necesario desuscribirse ya que BeatManager llama directamente a HandleBeat()
    }

    void CheckSceneActivation()
    {
        // Puedes implementar lógica adicional para activar/desactivar QTEs basados en la escena
        isQTEActive = true; // Por simplicidad, siempre activo. Ajusta según tus necesidades.
    }

    /// <summary>
    /// Método llamado por BeatManager en cada beat.
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
                // Activar el QTE según el tipo definido
                StartCoroutine(ActivateQTE(trigger));

                // Invocar el evento específico del trigger
                trigger.onQTEActivated.Invoke();

                // Restar el beatsAfterLastTrigger del contador
                beatCounter -= trigger.beatsAfterLastTrigger;
            }
        }
    }

    /// <summary>
    /// Activa la secuencia QTE según la configuración del trigger.
    /// </summary>
    /// <param name="trigger">El trigger que define cómo activar el QTE.</param>
    /// <returns></returns>
    IEnumerator ActivateQTE(QTETrigger trigger)
    {
        // Seleccionar los prefabs personalizados si están definidos
        GameObject leftPrefab = trigger.customLeftPrefab != null ? trigger.customLeftPrefab : qteIndicatorLeftPrefab;
        GameObject rightPrefab = trigger.customRightPrefab != null ? trigger.customRightPrefab : qteIndicatorRightPrefab;

        // Instanciar indicadores según el tipo de QTE
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
        RectTransform centerRect = barRect.Find("QTECenter").GetComponent<RectTransform>();

        if (centerRect == null)
        {
            Debug.LogError("No se encontró el GameObject 'QTECenter' dentro de QTEBar.");
            yield break;
        }

        // Definir posiciones de inicio y fin
        Vector2 leftStartPos = new Vector2(-barRect.rect.width / 2, 0);
        Vector2 rightStartPos = new Vector2(barRect.rect.width / 2, 0);
        Vector2 centerPos = centerRect.anchoredPosition;

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

        // Definir duración del movimiento basado en BPM
        float beatInterval = 60f / BeatManager.Instance.BPM;
        float moveDuration = beatInterval; // Tiempo para completar el movimiento

        // Mover indicadores hacia el centro
        float elapsedTime = 0f;
        bool inputReceived = false;

        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / moveDuration;

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

            // Verificar si la tecla fue presionada y si los indicadores están cerca del centro
            if (Input.GetKeyDown(qteKey))
            {
                bool leftNear = true;
                bool rightNear = true;
                float successThreshold = 30f; // Ajusta según necesidad

                if (indicatorLeft != null)
                {
                    RectTransform leftRect = indicatorLeft.GetComponent<RectTransform>();
                    float distanceLeft = Vector2.Distance(leftRect.anchoredPosition, centerPos);
                    leftNear = distanceLeft < successThreshold;
                }

                if (indicatorRight != null)
                {
                    RectTransform rightRect = indicatorRight.GetComponent<RectTransform>();
                    float distanceRight = Vector2.Distance(rightRect.anchoredPosition, centerPos);
                    rightNear = distanceRight < successThreshold;
                }

                if (leftNear && rightNear)
                {
                    // Éxito del QTE
                    inputReceived = true;
                    SuccessQTE(trigger);
                    break;
                }
            }

            yield return null;
        }

        // Al finalizar el movimiento, si no se recibió input, considerar como fallo
        if (!inputReceived)
        {
            FailQTE(trigger);
        }

        // Eliminar los indicadores
        if (indicatorLeft != null)
            Destroy(indicatorLeft);
        if (indicatorRight != null)
            Destroy(indicatorRight);
    }

    /// <summary>
    /// Maneja el éxito del QTE.
    /// </summary>
    /// <param name="trigger">El trigger que activó el QTE.</param>
    void SuccessQTE(QTETrigger trigger)
    {
        onQTESuccess.Invoke();
        Debug.Log("QTE Exitoso!");

        // Reproducir sonido de éxito
        if (successClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(successClip);
        }

        // Cambiar color de QTECenter a verde
        Image centerImage = qteBarTransform.Find("QTECenter").GetComponent<Image>();
        if (centerImage != null)
        {
            centerImage.color = Color.green;
        }

        // Revertir color después de un breve tiempo
        StartCoroutine(RevertCenterColor(centerImage));
    }

    /// <summary>
    /// Maneja el fallo del QTE.
    /// </summary>
    /// <param name="trigger">El trigger que activó el QTE.</param>
    void FailQTE(QTETrigger trigger)
    {
        onQTEFail.Invoke();
        Debug.Log("QTE Fallido!");

        // Reproducir sonido de fallo
        if (failClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(failClip);
        }

        // Cambiar color de QTECenter a rojo
        Image centerImage = qteBarTransform.Find("QTECenter").GetComponent<Image>();
        if (centerImage != null)
        {
            centerImage.color = Color.red;
        }

        // Revertir color después de un breve tiempo
        StartCoroutine(RevertCenterColor(centerImage));
    }

    IEnumerator RevertCenterColor(Image centerImage)
    {
        yield return new WaitForSeconds(0.5f);
        if (centerImage != null)
        {
            centerImage.color = Color.white; // Color original
        }
    }
}
