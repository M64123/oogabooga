using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Interval
{
    [Range(0.1f, 10f)] public float beatNumber;
    public UnityEvent onIntervalTrigger;
}

public class BeatManagerCombat : MonoBehaviour
{
    public static BeatManagerCombat Instance { get; private set; }

    public float bpm = 120f;
    public List<Interval> intervals;

    private float beatInterval;
    private float nextBeatTime;
    private int currentBeat = 0;

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
        beatInterval = 60f / bpm;
        nextBeatTime = Time.time + beatInterval;
    }

    void Update()
    {
        if (Time.time >= nextBeatTime)
        {
            currentBeat++;
            nextBeatTime += beatInterval;

            foreach (var interval in intervals)
            {
                if (Mathf.Approximately(currentBeat % interval.beatNumber, 0))
                {
                    interval.onIntervalTrigger.Invoke();
                }
            }
        }
    }

    public float GetBeatInterval()
    {
        return beatInterval;
    }
}