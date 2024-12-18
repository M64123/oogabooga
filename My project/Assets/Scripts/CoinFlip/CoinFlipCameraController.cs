using UnityEngine;
using System.Collections;

public class CoinFlipCameraController : MonoBehaviour
{
    [Header("References")]
    public Transform coinTransform; // Asignar en el Inspector
    public BeatManager beatManager; // Asignar el BeatManager en el Inspector
    public CoinFlipQTEManager qteManager; // Asignar en el Inspector

    [Header("Initial-Final Movement")]
    public Vector3 initialPosition;
    public Vector3 finalPosition;
    public Vector3 initialEulerAngles;
    public Vector3 finalEulerAngles;
    public float movementDuration = 2f;
    public float followOffsetY = 2f;
    public float curveHeight = 2f;

    private bool isMoving = false;
    private float movementStartTime;
    private Quaternion initialRotation;
    private Quaternion finalRotation;
    private CoinFlip coinFlip;

    [Header("Countdown Zoom Settings")]
    [Tooltip("Número de acercamientos antes de iniciar QTE")]
    public int countdownSteps = 3;
    [Tooltip("Incremento de zoom en cada paso de countdown")]
    public float countdownZoomIncrement = 0.5f;
    [Tooltip("Beats entre cada mini-zoom. Ej: 1 significa un zoom por beat.")]
    public int beatsPerCountdownStep = 1;
    [Tooltip("Duración del zoom+shake para cada acercamiento previo al QTE")]
    public float countdownZoomDuration = 0.2f;
    [Tooltip("Magnitud del camera shake durante el zoom de countdown")]
    public float countdownShakeMagnitude = 0.2f;

    private int currentCountdownStep = 0;
    private int beatsSinceLastZoom = 0;
    private bool isCountdownZooming = false;

    [Header("QTE Success Feedback")]
    [Tooltip("Duración del screen shake tras un QTE exitoso")]
    public float qteSuccessShakeDuration = 0.2f;
    [Tooltip("Magnitud del screen shake tras un QTE exitoso")]
    public float qteSuccessShakeMagnitude = 0.1f;
    [Tooltip("Incremento de zoom tras un QTE exitoso")]
    public float qteSuccessZoomIncrement = 0.2f;

    void Start()
    {
        initialRotation = Quaternion.Euler(initialEulerAngles);
        finalRotation = Quaternion.Euler(finalEulerAngles);

        transform.position = initialPosition;
        transform.rotation = initialRotation;

        if (coinTransform != null)
        {
            coinFlip = coinTransform.GetComponent<CoinFlip>();
        }

        if (beatManager != null)
        {
            beatManager.onBeat.AddListener(OnBeatReceivedForCountdown);
        }
    }

    void Update()
    {
        if (!isMoving && coinFlip != null && coinFlip.IsThrown)
        {
            StartCameraMovement();
        }
    }

    void LateUpdate()
    {
        if (isMoving)
            return;

        if (coinFlip != null && coinFlip.IsThrown)
        {
            Vector3 offset = new Vector3(0f, followOffsetY, 0f);
            Vector3 targetPos = coinTransform.position + offset;
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 5f);
        }
    }

    void StartCameraMovement()
    {
        isMoving = true;
        movementStartTime = Time.time;
        StartCoroutine(MoveCameraAlongCurve());
    }

    IEnumerator MoveCameraAlongCurve()
    {
        float elapsedTime = 0f;

        while (elapsedTime < movementDuration)
        {
            elapsedTime = Time.time - movementStartTime;
            float t = Mathf.Clamp01(elapsedTime / movementDuration);

            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            Vector3 newPosition = CalculateCurvePosition(smoothT);
            Quaternion newRotation = Quaternion.Slerp(initialRotation, finalRotation, smoothT);

            transform.position = newPosition;
            transform.rotation = newRotation;

            yield return null;
        }

        transform.position = finalPosition;
        transform.rotation = finalRotation;
    }

    Vector3 CalculateCurvePosition(float t)
    {
        Vector3 controlPoint = (initialPosition + finalPosition) / 2 + Vector3.up * curveHeight;

        Vector3 position = Mathf.Pow(1 - t, 2) * initialPosition
                         + 2 * (1 - t) * t * controlPoint
                         + Mathf.Pow(t, 2) * finalPosition;

        return position;
    }

    void OnBeatReceivedForCountdown()
    {
        if (currentCountdownStep < countdownSteps && !isCountdownZooming)
        {
            beatsSinceLastZoom++;
            if (beatsSinceLastZoom >= beatsPerCountdownStep && coinTransform != null)
            {
                StartCoroutine(PerformCountdownZoomStep());
            }
        }
    }

    IEnumerator PerformCountdownZoomStep()
    {
        isCountdownZooming = true;
        beatsSinceLastZoom = 0;
        currentCountdownStep++;

        Vector3 directionToCoin = (coinTransform.position - transform.position).normalized;
        Vector3 startPos = transform.position;
        Vector3 endPos = transform.position + directionToCoin * countdownZoomIncrement;

        float elapsed = 0f;
        // Durante el zoom, también aplicamos shake
        while (elapsed < countdownZoomDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / countdownZoomDuration);
            // Interpolamos posición hacia el endPos
            Vector3 basePos = Vector3.Lerp(startPos, endPos, t);
            // Añadimos shake agresivo
            float x = Random.Range(-1f, 1f) * countdownShakeMagnitude;
            float y = Random.Range(-1f, 1f) * countdownShakeMagnitude;
            transform.position = new Vector3(basePos.x + x, basePos.y + y, basePos.z);

            yield return null;
        }

        // Tras terminar el zoom+shake, dejar la cámara exactamente en endPos
        transform.position = endPos;

        isCountdownZooming = false;

        if (currentCountdownStep == countdownSteps)
        {
            // Ya completamos los 3 zoom/shakes
            if (qteManager != null)
            {
                qteManager.AllowQTEStart();
            }
        }
    }

    public void QTESuccessFeedback()
    {
        StartCoroutine(QTESuccessFeedbackRoutine());
    }

    IEnumerator QTESuccessFeedbackRoutine()
    {
        // Zoom leve hacia la moneda (suave)
        if (coinTransform != null && qteSuccessZoomIncrement != 0f)
        {
            Vector3 directionToCoin = (coinTransform.position - transform.position).normalized;
            Vector3 originalPos = transform.position;
            Vector3 endPos = transform.position + directionToCoin * qteSuccessZoomIncrement;

            float elapsed = 0f;
            float zoomDuration = 0.2f;
            while (elapsed < zoomDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / zoomDuration);
                transform.position = Vector3.Lerp(originalPos, endPos, t);
                yield return null;
            }
        }

        // Screen shake tras el zoom
        float shakeElapsed = 0f;
        Vector3 finalOriginalPos = transform.position;
        while (shakeElapsed < qteSuccessShakeDuration)
        {
            shakeElapsed += Time.deltaTime;
            float x = Random.Range(-1f, 1f) * qteSuccessShakeMagnitude;
            float y = Random.Range(-1f, 1f) * qteSuccessShakeMagnitude;
            transform.position = new Vector3(finalOriginalPos.x + x, finalOriginalPos.y + y, finalOriginalPos.z);
            yield return null;
        }

        transform.position = finalOriginalPos;
    }
}
