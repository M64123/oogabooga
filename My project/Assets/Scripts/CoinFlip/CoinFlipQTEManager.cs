// CoinFlipQTEManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class CoinFlipQTEManager : MonoBehaviour
{
    public CoinFlip coinFlip; // Asignar en el Inspector
    public CanvasGroup qteCanvasGroup; // Asignar el CanvasGroup del QTE UI en el Inspector
    public BeatManager beatManager; // Asignar el BeatManager en el Inspector

    private int successfulQTEs = 0;
    private int maxQTEs = 6;
    private bool qteSequenceActive = false;

    private List<QTETrigger> coinFlipQTETriggers = new List<QTETrigger>();

    void Start()
    {
        if (coinFlip == null)
        {
            Debug.LogError("CoinFlip no está asignado en CoinFlipQTEManager.");
            return;
        }

        if (beatManager == null)
        {
            Debug.LogError("BeatManager no está asignado en CoinFlipQTEManager.");
            return;
        }

        // Iniciar la secuencia de QTE
        StartQTESequence();
    }

    void StartQTESequence()
    {
        qteSequenceActive = true;
        successfulQTEs = 0;

        // Limpiar cualquier QTETrigger existente en BeatManager
        beatManager.qteTriggers.Clear();

        // Crear QTETriggers para la secuencia de lanzamiento de moneda
        float beatsBetweenQTEs = 1f; // Ajusta según sea necesario
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

            nextBeat = beatsBetweenQTEs; // Intervalo entre QTEs
        }

        // Suscribirse a los eventos de QTE
        QTEManager.Instance.onQTESuccess.AddListener(OnQTESuccess);
        QTEManager.Instance.onQTEFail.AddListener(OnQTEFail);
    }

    void OnQTESuccess()
    {
        if (!qteSequenceActive) return;

        successfulQTEs++;

        if (successfulQTEs >= maxQTEs)
        {
            // Completó todos los QTEs exitosamente
            qteSequenceActive = false;

            // Desuscribirse de los eventos de QTE
            QTEManager.Instance.onQTESuccess.RemoveListener(OnQTESuccess);
            QTEManager.Instance.onQTEFail.RemoveListener(OnQTEFail);

            // Limpiar los QTETriggers restantes
            beatManager.qteTriggers.Clear();

            // Iniciar el fade out del Canvas y lanzar la moneda
            StartCoroutine(FadeOutQTECanvasAndLaunchCoin());
        }
        else
        {
            // Continuar con el siguiente QTE
        }
    }

    void OnQTEFail()
    {
        if (!qteSequenceActive) return;

        qteSequenceActive = false;

        // Desuscribirse de los eventos de QTE
        QTEManager.Instance.onQTESuccess.RemoveListener(OnQTESuccess);
        QTEManager.Instance.onQTEFail.RemoveListener(OnQTEFail);

        // Limpiar los QTETriggers restantes
        beatManager.qteTriggers.Clear();

        // Iniciar el fade out del Canvas y lanzar la moneda
        StartCoroutine(FadeOutQTECanvasAndLaunchCoin());
    }

    IEnumerator FadeOutQTECanvasAndLaunchCoin()
    {
        // Fade out del Canvas de QTE
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

        // Calcular la fuerza hacia arriba basada en los QTEs exitosos
        coinFlip.upwardForce = 5f + successfulQTEs * 2f;

        // Lanzar la moneda
        coinFlip.FlipCoin();
    }
}
