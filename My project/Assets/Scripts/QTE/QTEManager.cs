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

    [Header("QTE Detection Settings")]
    [Range(10f, 100f)]
    public float successThreshold = 50f;

    [Header("QTE Center")]
    public RectTransform qteCenterRectTransform;

    private bool isQTEInProgress = false;
    private bool wasLastQTESuccessful = false;

    void Awake()
    {
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
        // Inicializar AudioSource para feedback auditivo
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
        if (BeatManager.Instance != null)
        {
            if (BeatManager.Instance.IsCombatPhase())
            {
                StartCoroutine(ActivateQTE());
            }
        }
    }

    IEnumerator ActivateQTE()
    {
        if (isQTEInProgress)
            yield break;

        isQTEInProgress = true;

        GameObject indicatorLeft = Instantiate(qteIndicatorLeftPrefab, qteBarTransform);
        GameObject indicatorRight = Instantiate(qteIndicatorRightPrefab, qteBarTransform);

        RectTransform leftRect = indicatorLeft.GetComponent<RectTransform>();
        RectTransform rightRect = indicatorRight.GetComponent<RectTransform>();

        Vector2 centerPos = qteCenterRectTransform.anchoredPosition;

        float beatInterval = 60f / BeatManager.Instance.CombatBPM;
        float moveDuration = beatInterval;

        float elapsedTime = 0f;
        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / moveDuration);

            leftRect.anchoredPosition = Vector2.Lerp(new Vector2(-qteBarTransform.GetComponent<RectTransform>().rect.width / 2, 0), centerPos, t);
            rightRect.anchoredPosition = Vector2.Lerp(new Vector2(qteBarTransform.GetComponent<RectTransform>().rect.width / 2, 0), centerPos, t);

            if (Input.GetKeyDown(qteKey))
            {
                bool success = CheckQTESuccess(leftRect, rightRect, centerPos);
                if (success)
                {
                    OnQTESuccess();
                }
                else
                {
                    OnQTEFail();
                }

                Destroy(indicatorLeft);
                Destroy(indicatorRight);
                isQTEInProgress = false;
                yield break;
            }

            yield return null;
        }

        OnQTEFail();
        Destroy(indicatorLeft);
        Destroy(indicatorRight);
        isQTEInProgress = false;
    }

    bool CheckQTESuccess(RectTransform leftRect, RectTransform rightRect, Vector2 centerPos)
    {
        bool leftNear = Vector2.Distance(leftRect.anchoredPosition, centerPos) <= successThreshold;
        bool rightNear = Vector2.Distance(rightRect.anchoredPosition, centerPos) <= successThreshold;
        wasLastQTESuccessful = leftNear && rightNear;
        return wasLastQTESuccessful;
    }

    void OnQTESuccess()
    {
        onQTESuccess.Invoke();
        if (successClip != null)
        {
            audioSource.PlayOneShot(successClip);
        }
        StartCoroutine(RevertCenterColor(Color.green));
    }

    void OnQTEFail()
    {
        onQTEFail.Invoke();
        if (failClip != null)
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
}
