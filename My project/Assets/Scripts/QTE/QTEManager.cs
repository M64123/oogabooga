using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class QTEManager : MonoBehaviour
{
    public static QTEManager Instance { get; private set; }

    [Header("QTE Settings")]
    public KeyCode qteKey = KeyCode.Space;
    public GameObject qteIndicatorLeftPrefab;
    public GameObject qteIndicatorRightPrefab;
    public Transform qteBarTransform;

    [Header("QTE Events")]
    public UnityEvent onQTESuccess;
    public UnityEvent onQTEFail;

    [Header("Audio Feedback")]
    public AudioClip successClip;
    public AudioClip failClip;
    private AudioSource audioSource;

    public bool isQTEActive = false; // Controlado externamente (por EggBehaviour)

    [Header("QTE Detection Settings")]
    [Range(10f, 100f)]
    public float successThreshold = 50f;

    [Header("QTE Center")]
    public RectTransform qteCenterRectTransform;

    private bool isQTEInProgress = false;
    private bool wasLastQTESuccessful = false;
    private float beatCounter = 0f; // Para contar beats acumulados

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (qteCenterRectTransform == null)
        {
            Debug.LogError("QTECenter RectTransform no está asignado en el Inspector.");
        }
    }

    public void HandleBeat()
    {
        if (!isQTEActive)
            return;

        // Sumar un beat al contador
        beatCounter += 1f;

        // Comprobar si algún trigger debe activarse
        foreach (QTETrigger trigger in BeatManager.Instance.qteTriggers)
        {
            if (beatCounter >= trigger.beatsAfterLastTrigger)
            {
                if (!isQTEInProgress)
                {
                    StartCoroutine(ActivateQTE(trigger));
                    trigger.onQTEActivated.Invoke();
                    // Restablecer el contador restando el valor del trigger
                    beatCounter -= trigger.beatsAfterLastTrigger;
                }
            }
        }
    }

    IEnumerator ActivateQTE(QTETrigger trigger)
    {
        if (isQTEInProgress)
            yield break;

        isQTEInProgress = true;

        GameObject leftPrefab = trigger.customLeftPrefab != null ? trigger.customLeftPrefab : qteIndicatorLeftPrefab;
        GameObject rightPrefab = trigger.customRightPrefab != null ? trigger.customRightPrefab : qteIndicatorRightPrefab;

        GameObject indicatorLeft = null;
        GameObject indicatorRight = null;

        if (trigger.qteType == QTEType.Left || trigger.qteType == QTEType.Both)
        {
            indicatorLeft = Instantiate(leftPrefab, qteBarTransform);
            RectTransform leftRect = indicatorLeft.GetComponent<RectTransform>();
            leftRect.localScale = Vector3.one;
        }

        if (trigger.qteType == QTEType.Right || trigger.qteType == QTEType.Both)
        {
            indicatorRight = Instantiate(rightPrefab, qteBarTransform);
            RectTransform rightRect = indicatorRight.GetComponent<RectTransform>();
            rightRect.localScale = Vector3.one;
        }

        RectTransform barRect = qteBarTransform.GetComponent<RectTransform>();

        if (qteCenterRectTransform == null)
        {
            Debug.LogError("QTECenter RectTransform no asignado. Abortando QTE.");
            isQTEInProgress = false;
            yield break;
        }

        Vector2 centerPos = qteCenterRectTransform.anchoredPosition;

        Vector2 leftStartPos = new Vector2(-barRect.rect.width / 2, 0);
        Vector2 rightStartPos = new Vector2(barRect.rect.width / 2, 0);

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

        float beatInterval = 60f / BeatManager.Instance.BPM;
        float moveDuration = beatInterval;
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / moveDuration);

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

                if (indicatorLeft != null)
                    Destroy(indicatorLeft);
                if (indicatorRight != null)
                    Destroy(indicatorRight);

                isQTEInProgress = false;
                yield break;
            }

            yield return null;
        }

        OnQTEFail(trigger);

        if (indicatorLeft != null)
            Destroy(indicatorLeft);
        if (indicatorRight != null)
            Destroy(indicatorRight);

        isQTEInProgress = false;
    }

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

        wasLastQTESuccessful = (indicatorLeft == null || leftNear) && (indicatorRight == null || rightNear);
        return wasLastQTESuccessful;
    }

    void OnQTESuccess(QTETrigger trigger)
    {
        onQTESuccess.Invoke();
        Debug.Log("QTE Exitoso!");

        if (successClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(successClip);
        }

        StartCoroutine(RevertCenterColor(Color.green));
    }

    void OnQTEFail(QTETrigger trigger)
    {
        onQTEFail.Invoke();
        Debug.Log("QTE Fallido!");

        if (failClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(failClip);
        }

        StartCoroutine(RevertCenterColor(Color.red));
    }

    IEnumerator RevertCenterColor(Color color)
    {
        Image centerImage = qteCenterRectTransform.GetComponent<Image>();
        if (centerImage != null)
        {
            centerImage.color = color;
            yield return new WaitForSeconds(0.5f);
            centerImage.color = Color.white;
        }
    }

    public bool WasLastQTESuccessful()
    {
        return wasLastQTESuccessful;
    }
}
