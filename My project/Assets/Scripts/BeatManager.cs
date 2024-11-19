// BeatManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BeatManager : MonoBehaviour
{
    [Header("BPM Settings")]
    [SerializeField] private float movementBPM = 120f; // BPM para la fase de movimiento
    [SerializeField] private float combatBPM = 100f; // BPM para la fase de combate
    [SerializeField] private AudioSource audioSource;

    [Header("Beat Events")]
    public UnityEvent onMovementBeat; // Evento para cada beat de movimiento
    public UnityEvent onCombatBeat; // Evento para cada beat de combate
    public UnityEvent onBeat; // Evento general para cada beat

    private float beatIntervalMovement;
    private float beatIntervalCombat;
    private float nextBeatTime;
    private bool isCombatPhase = false;

    // Implementación del Singleton
    public static BeatManager Instance { get; private set; }

    // Propiedades públicas para acceder a los BPM
    public float MovementBPM => movementBPM;
    public float CombatBPM => combatBPM;

    private void Awake()
    {
        // Asegurarse de que solo haya una instancia de BeatManager
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            beatIntervalMovement = 60f / movementBPM;
            beatIntervalCombat = 60f / combatBPM;
            nextBeatTime = audioSource.time + (isCombatPhase ? beatIntervalCombat : beatIntervalMovement);
        }
        else
        {
            Debug.LogError("AudioSource no está asignado o no está reproduciendo.");
        }
    }

    private void Update()
    {
        if (audioSource == null || !audioSource.isPlaying)
            return;

        float currentTime = audioSource.time;

        if (currentTime >= nextBeatTime)
        {
            // Invocar el evento de beat general
            onBeat.Invoke();

            // Invocar evento correspondiente según la fase
            if (isCombatPhase)
            {
                onCombatBeat.Invoke();
            }
            else
            {
                onMovementBeat.Invoke();
            }

            // Calcular el siguiente tiempo de beat
            nextBeatTime += isCombatPhase ? beatIntervalCombat : beatIntervalMovement;
        }
    }

    /// <summary>
    /// Método para cambiar la canción y los BPM para las fases específicas.
    /// </summary>
    /// <param name="newSong">El nuevo clip de audio.</param>
    /// <param name="newMovementBPM">El nuevo BPM para la fase de movimiento.</param>
    /// <param name="newCombatBPM">El nuevo BPM para la fase de combate.</param>
    public void ChangeSong(AudioClip newSong, float newMovementBPM, float newCombatBPM)
    {
        audioSource.clip = newSong;
        movementBPM = newMovementBPM;
        combatBPM = newCombatBPM;
        audioSource.Play();
    }

    /// <summary>
    /// Método para iniciar la fase de combate.
    /// </summary>
    public void StartCombatPhase()
    {
        isCombatPhase = true;
        beatIntervalCombat = 60f / combatBPM;
        nextBeatTime = audioSource.time + beatIntervalCombat;
    }

    /// <summary>
    /// Método para iniciar la fase de movimiento.
    /// </summary>
    public void StartMovementPhase()
    {
        isCombatPhase = false;
        beatIntervalMovement = 60f / movementBPM;
        nextBeatTime = audioSource.time + beatIntervalMovement;
    }

    /// <summary>
    /// Verifica si la fase actual es la fase de combate.
    /// </summary>
    /// <returns>True si está en fase de combate, de lo contrario False.</returns>
    public bool IsCombatPhase()
    {
        return isCombatPhase;
    }
}
