using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BeatManager : MonoBehaviour
{
    [SerializeField] private float _bpm = 120f; // BPM por defecto
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private List<Intervals> _intervals; // Lista de intervalos configurables desde el inspector

    [Header("Beat Events")]
    public UnityEvent onBeat; // Evento que se dispara en cada beat
    public UnityEvent onBPMChanged; // Evento que se dispara cuando el BPM cambia
    public UnityEvent onSongChanged; // Evento que se dispara cuando cambia la canción

    [Header("QTE Triggers")]
    [Tooltip("Lista de QTETriggers para esta escena.")]
    public List<QTETrigger> qteTriggers = new List<QTETrigger>();

    private float beatInterval;
    private float nextBeatTime;

    // Implementación del Singleton
    public static BeatManager Instance { get; private set; }

    // Propiedad pública para acceder al BPM
    public float BPM
    {
        get => _bpm;
        set
        {
            if (_bpm != value)
            {
                _bpm = value;
                beatInterval = 60f / _bpm;
                onBPMChanged.Invoke(); // Notificar sobre el cambio de BPM
            }
        }
    }

    // Propiedad pública para acceder a los intervalos
    public List<Intervals> Intervals => _intervals;

    private void Awake()
    {
        // Asegurarse de que solo haya una instancia de BeatManager
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        
    }

    private void Start()
    {
        if (_audioSource != null && _audioSource.isPlaying)
        {
            beatInterval = 60f / _bpm;
            nextBeatTime = _audioSource.time + beatInterval;
        }
        else
        {
            Debug.LogError("AudioSource no está asignado o no está reproduciendo.");
        }
    }

    private void Update()
    {
        if (_audioSource == null || !_audioSource.isPlaying)
            return;

        float currentTime = _audioSource.time;

        if (currentTime >= nextBeatTime)
        {
            // Invocar el evento de beat
            onBeat.Invoke();

            // Notificar a QTEManager sobre el beat
            QTEManager.Instance?.HandleBeat();

            // Calcular el siguiente tiempo de beat
            nextBeatTime += beatInterval;
        }

        foreach (Intervals interval in _intervals)
        {
            // Calcula la longitud del intervalo
            float intervalLength = interval.GetIntervalLength(_bpm);

            // Calcula el número de intervalos que han pasado
            float sampledTime = currentTime / intervalLength;

            // Verifica y dispara el evento si es necesario
            interval.CheckForNewInterval(sampledTime);
        }
    }

    /// <summary>
    /// Método para cambiar la canción y el BPM.
    /// </summary>
    /// <param name="newSong">El nuevo clip de audio.</param>
    /// <param name="newBPM">El nuevo BPM.</param>
    public void ChangeSong(AudioClip newSong, float newBPM)
    {
        _audioSource.clip = newSong;
        BPM = newBPM;
        _audioSource.Play();
        onSongChanged.Invoke(); // Notificar sobre el cambio de canción
    }
}

[System.Serializable]
public class Intervals
{
    [SerializeField] private float _steps;
    [SerializeField] private UnityEvent _trigger; // Evento a disparar cuando se cumple el intervalo
    private int _lastInterval;

    // Propiedad pública para acceder al trigger
    public UnityEvent Trigger => _trigger;

    public float GetIntervalLength(float bpm)
    {
        return 60f / (bpm * _steps);
    }

    public void CheckForNewInterval(float interval)
    {
        if (Mathf.FloorToInt(interval) != _lastInterval)
        {
            _lastInterval = Mathf.FloorToInt(interval);
            _trigger.Invoke(); // Disparar el evento
        }
    }
}
