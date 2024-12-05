using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Interval
{
    [Tooltip("Número de beats después del cual se activará este intervalo. Se permiten valores decimales.")]
    [Range(0.1f, 10f)]
    public float beatNumber; // Número de beats después del cual se activará
    public UnityEvent onIntervalTrigger; // Evento que se dispara cuando se llega al beat específico
}

public class BeatManagerCombat : MonoBehaviour
{
    [Header("BPM Settings")]
    public float bpm = 120f; // BPM general para toda la escena

    [Header("Intervals Settings")]
    public List<Interval> intervals; // Lista de intervalos configurables desde el Inspector

    private float beatInterval; // Tiempo entre beats
    private float nextBeatTime;
    private int currentBeat = 0; // Lleva la cuenta de qué beat estamos

    public static BeatManagerCombat Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        beatInterval = 60f / bpm; // Calcula el tiempo entre beats basándose en el BPM
        nextBeatTime = Time.time + beatInterval; // Establece el primer tiempo de beat
    }

    private void Update()
    {
        if (Time.time >= nextBeatTime)
        {
            currentBeat++;
            nextBeatTime += beatInterval;

            // Llamar a los intervalos configurados
            foreach (Interval interval in intervals)
            {
                if (Mathf.Approximately(currentBeat, interval.beatNumber) || Mathf.FloorToInt(currentBeat) == interval.beatNumber)
                {
                    interval.onIntervalTrigger.Invoke();
                    Debug.Log("Intervalo disparado en beat " + currentBeat);
                }
            }

            // Reiniciar el conteo si se excede la cantidad de intervalos para poder seguir usándolos en loop
            if (currentBeat >= GetMaxIntervalBeat())
            {
                currentBeat = 0;
            }
        }
    }

    public float GetBeatInterval()
    {
        return beatInterval;
    }

    private float GetMaxIntervalBeat()
    {
        float maxBeat = 0f;
        foreach (Interval interval in intervals)
        {
            if (interval.beatNumber > maxBeat)
            {
                maxBeat = interval.beatNumber;
            }
        }
        return maxBeat;
    }
}