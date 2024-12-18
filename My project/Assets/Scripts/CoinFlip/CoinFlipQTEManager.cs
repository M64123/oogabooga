using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class CoinFlipQTEManager : MonoBehaviour
{
    public CoinFlip coinFlip;
    public CanvasGroup qteCanvasGroup;
    public BeatManager beatManager;
    public CoinFlipCameraController coinFlipCameraController;
    public QTEManager qteManager; // Asegurar que se asigne en el Inspector

    [Header("QTE Settings")]
    public int maxQTEs = 6;

    private int successfulQTEs = 0;
    private bool qteSequenceActive = false;
    private List<QTETrigger> coinFlipQTETriggers = new List<QTETrigger>();

    void Start()
    {
        // No inicia QTE aquí, se esperará a AllowQTEStart() tras los 3 zooms
        if (qteCanvasGroup != null)
        {
            qteCanvasGroup.gameObject.SetActive(true);
            qteCanvasGroup.alpha = 1f;
        }
    }

    public void AllowQTEStart()
    {
        if (coinFlip == null)
        {
            Debug.LogError("CoinFlip no asignado en CoinFlipQTEManager.");
            return;
        }

        if (beatManager == null)
        {
            Debug.LogError("BeatManager no asignado en CoinFlipQTEManager.");
            return;
        }

        if (coinFlipCameraController == null)
        {
            Debug.LogError("CoinFlipCameraController no asignado en CoinFlipQTEManager.");
            return;
        }

        if (qteManager == null)
        {
            Debug.LogError("QTEManager no asignado en CoinFlipQTEManager.");
            return;
        }

        // Habilitar QTE
        qteManager.isQTEActive = true;
        StartQTESequence();
    }

    void StartQTESequence()
    {
        qteSequenceActive = true;
        successfulQTEs = 0;

        beatManager.qteTriggers.Clear();
        coinFlipQTETriggers.Clear();

        float beatsBetweenQTEs = 1f;
        float nextBeat = beatsBetweenQTEs;

        for (int i = 0; i < maxQTEs; i++)
        {
            QTETrigger trigger = new QTETrigger
            {
                beatsAfterLastTrigger = nextBeat,
                qteType = QTEType.Both,
                customLeftPrefab = null,
                customRightPrefab = null,
                onQTEActivated = new UnityEvent()
            };

            coinFlipQTETriggers.Add(trigger);
            beatManager.qteTriggers.Add(trigger);
            nextBeat = beatsBetweenQTEs;
        }

        QTEManager.Instance.onQTESuccess.AddListener(OnQTESuccess);
        QTEManager.Instance.onQTEFail.AddListener(OnQTEFail);
    }

    void OnQTESuccess()
    {
        if (!qteSequenceActive) return;

        successfulQTEs++;

        if (coinFlipCameraController != null)
        {
            coinFlipCameraController.QTESuccessFeedback();
        }

        if (successfulQTEs >= maxQTEs)
        {
            qteSequenceActive = false;
            QTEManager.Instance.onQTESuccess.RemoveListener(OnQTESuccess);
            QTEManager.Instance.onQTEFail.RemoveListener(OnQTEFail);
            beatManager.qteTriggers.Clear();
            StartCoroutine(FadeOutQTECanvasAndLaunchCoin());
        }
    }

    void OnQTEFail()
    {
        if (!qteSequenceActive) return;

        qteSequenceActive = false;
        QTEManager.Instance.onQTESuccess.RemoveListener(OnQTESuccess);
        QTEManager.Instance.onQTEFail.RemoveListener(OnQTEFail);
        beatManager.qteTriggers.Clear();
        StartCoroutine(FadeOutQTECanvasAndLaunchCoin());
    }

    IEnumerator FadeOutQTECanvasAndLaunchCoin()
    {
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

        coinFlip.upwardForce = 5f + successfulQTEs * 2f;
        coinFlip.FlipCoin();
    }
}
