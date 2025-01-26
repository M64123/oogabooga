using UnityEngine;
using System.Collections;

public class CamaraControllerGacha : MonoBehaviour
{
    [Header("Configuración de Interacción con el Huevo")]
    public Transform eggTransform;
    public float shakeIntensity = 0.1f;
    public float shakeDuration = 0.2f;
    public float moveForwardAmount = 0.5f;

    [Header("Configuración de Seguimiento del Cubo")]
    public Vector3 cubeFollowOffset = new Vector3(0f, 2f, -5f);
    public float followSmoothTime = 0.5f;
    public float lookAtSmoothTime = 0.2f;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isFollowingCube = false;
    private Transform cubeTransform;

    private Vector3 currentVelocity = Vector3.zero;

    private bool isShaking = false;

    [Header("Countdown Zoom Settings (Antes de QTE)")]
    public BeatManager beatManager; // Asignar
    public EggBehaviour eggBehaviour; // Asignar
    [Tooltip("Número de acercamientos antes de iniciar QTE")]
    public int countdownSteps = 3;
    [Tooltip("Beats entre cada mini-zoom.")]
    public int beatsPerCountdownStep = 1;
    [Tooltip("Incremento de zoom en cada paso de countdown")]
    public float countdownZoomIncrement = 0.5f;
    [Tooltip("Duración del zoom+shake para cada acercamiento")]
    public float countdownZoomDuration = 0.2f;
    [Tooltip("Magnitud del camera shake durante el zoom de countdown")]
    public float countdownShakeMagnitude = 0.2f;

    private int currentCountdownStep = 0;
    private int beatsSinceLastZoom = 0;
    private bool isCountdownZooming = false;

    [Header("QTE Success Feedback")]
    public float qteSuccessShakeDuration = 0.2f;
    public float qteSuccessShakeMagnitude = 0.1f;

    void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        // Suscribirse a onBeat para los zooms
        if (beatManager != null)
        {
            beatManager.onBeat.AddListener(OnBeatReceivedForCountdown);
        }
    }

    void LateUpdate()
    {
        if (isFollowingCube && cubeTransform != null)
        {
            Vector3 desiredPosition = cubeTransform.position + cubeFollowOffset;
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, followSmoothTime);

            Vector3 direction = cubeTransform.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime / lookAtSmoothTime);
        }
    }

    void OnBeatReceivedForCountdown()
    {
        // Hasta completar los 3 zooms no iniciamos QTE
        if (currentCountdownStep < countdownSteps && !isCountdownZooming)
        {
            beatsSinceLastZoom++;
            if (beatsSinceLastZoom >= beatsPerCountdownStep && eggTransform != null)
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

        Vector3 directionToEgg = (eggTransform.position - transform.position).normalized;
        Vector3 startPos = transform.position;
        Vector3 endPos = transform.position + directionToEgg * countdownZoomIncrement;

        float elapsed = 0f;
        while (elapsed < countdownZoomDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / countdownZoomDuration);

            Vector3 basePos = Vector3.Lerp(startPos, endPos, t);
            float x = Random.Range(-1f, 1f) * countdownShakeMagnitude;
            float y = Random.Range(-1f, 1f) * countdownShakeMagnitude;

            transform.position = new Vector3(basePos.x + x, basePos.y + y, basePos.z);

            yield return null;
        }

        transform.position = endPos;

        isCountdownZooming = false;

        if (currentCountdownStep == countdownSteps)
        {
            // Ya completamos los 3 zoom/shakes, ahora sí permitir QTE
            if (eggBehaviour != null)
            {
                eggBehaviour.AllowQTEStart();
            }
        }
    }

    public void QTESuccessFeedback()
    {
        StartCoroutine(QTESuccessFeedbackRoutine());
    }

    IEnumerator QTESuccessFeedbackRoutine()
    {
        float elapsed = 0f;
        Vector3 originalPos = transform.position;
        while (elapsed < qteSuccessShakeDuration)
        {
            elapsed += Time.deltaTime;
            float x = Random.Range(-1f, 1f) * qteSuccessShakeMagnitude;
            float y = Random.Range(-1f, 1f) * qteSuccessShakeMagnitude;
            transform.position = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
            yield return null;
        }

        transform.position = originalPos;
    }

    public void OnEggClicked()
    {
        if (!isShaking)
        {
            StartCoroutine(ShakeAndMoveForward());
        }
    }

    public void StartFollowingCube(Transform cube)
    {
        cubeTransform = cube;
        isFollowingCube = true;
    }

    public void StopFollowingCube()
    {
        isFollowingCube = false;
        StartCoroutine(RestoreCameraPosition());
    }

    IEnumerator ShakeAndMoveForward()
    {
        isShaking = true;

        Vector3 startPosition = transform.position;
        Vector3 targetPosition = transform.position + transform.forward * moveForwardAmount;

        float elapsedTime = 0f;

        while (elapsedTime < shakeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / shakeDuration;

            transform.position = Vector3.Lerp(startPosition, targetPosition, t);

            Vector3 shakeOffset = Random.insideUnitSphere * shakeIntensity * (1f - t);
            transform.position += shakeOffset;

            yield return null;
        }

        transform.position = targetPosition;
        originalPosition = transform.position;

        isShaking = false;
    }

    IEnumerator RestoreCameraPosition()
    {
        float duration = 1f;
        float elapsedTime = 0f;

        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            transform.position = Vector3.Lerp(startPosition, originalPosition, t);
            transform.rotation = Quaternion.Slerp(startRotation, originalRotation, t);

            yield return null;
        }

        transform.position = originalPosition;
        transform.rotation = originalRotation;
    }
}
