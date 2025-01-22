using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// AudioController que:
/// - Reproduce Intro una sola vez (opcional),
/// - Luego Combate ↔ Defensa indefinidamente,
/// - Usa PlayScheduled + SetScheduledEndTime en dos AudioSources, sin cortes.
/// - Transiciones opcionales (si transicionClips tiene algo).
/// - Si en cada lista hay un solo clip, ese mismo se repetirá en cada turno.
/// - Se arregla para que NO se detenga tras primera Defensa.
/// - Se han eliminado todos los Debug.Log excepto los warnings para indicar fallos.
/// </summary>
public class AudioController : MonoBehaviour
{
    public static AudioController Instance { get; private set; }

    // ------------------------------------------------------------------------
    //          LISTAS DE AUDIO: Intro, Combate, Defensa, Transiciones
    // ------------------------------------------------------------------------
    [Header("Listas de Audio")]
    [Tooltip("Si hay al menos un clip, lo reproduce una sola vez al principio.")]
    public List<AudioClip> introClips;

    [Tooltip("Clips de Combate, se alternan con Defensa.")]
    public List<AudioClip> combateClips;

    [Tooltip("Clips de Defensa, se alternan con Combate.")]
    public List<AudioClip> defensaClips;

    [Tooltip("Clips de Transición (opcional). Si está vacío, no hay transiciones.")]
    public List<AudioClip> transicionClips;

    // ------------------------------------------------------------------------
    //                      CONFIGURACIÓN DE BPM y EVENTOS
    // ------------------------------------------------------------------------
    [Header("Configuración Rítmica")]
    [SerializeField]
    private float _bpm = 120f;

    [Tooltip("Cuántos beats dura cada segmento (Combate/Defensa) antes de cambiar.")]
    public int beatsPerSegment = 16;

    [Header("Eventos (opcionales)")]
    public UnityEvent onBPMChanged;
    public UnityEvent onSongChanged;
    [Tooltip("Evento que se dispara en cada beat (60/BPM seg).")]
    public UnityEvent onBeat;

    [Header("Volumen General")]
    [Range(0f, 1f)]
    public float volume = 1f;

    // ------------------------------------------------------------------------
    //                      MÁQUINA DE ESTADOS Y AUDIO SOURCES
    // ------------------------------------------------------------------------
    private enum MusicState { None, Intro, Combate, Defensa, Transicion }
    private MusicState currentState = MusicState.None;

    private AudioSource sourceA;
    private AudioSource sourceB;
    private bool isSourceAPlaying = true; // alternamos para no cortar

    // ------------------------------------------------------------------------
    //               CONTROL DE TIEMPOS: DSP y flags
    // ------------------------------------------------------------------------
    private double dspTimeStartOfMusic = 0.0;
    private double nextBeatTimeDSP = 0.0;
    private double currentSegmentEndDSP = 0.0;

    private bool hasStarted = false;
    private bool nextSectionScheduled = false;

    private const float SCHEDULE_LOOKAHEAD = 0.2f;

    /// <summary>
    /// BPM actual. Al cambiarlo, se dispara onBPMChanged.
    /// </summary>
    public float BPM
    {
        get => _bpm;
        set
        {
            if (Mathf.Abs(_bpm - value) > 0.01f)
            {
                _bpm = value;
                onBPMChanged.Invoke();
            }
        }
    }

    // ------------------------------------------------------------------------
    //                                AWAKE
    // ------------------------------------------------------------------------
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        sourceA = gameObject.AddComponent<AudioSource>();
        sourceB = gameObject.AddComponent<AudioSource>();
        sourceA.playOnAwake = false;
        sourceB.playOnAwake = false;

        sourceA.volume = volume;
        sourceB.volume = volume;
    }

    // ------------------------------------------------------------------------
    //                                START
    // ------------------------------------------------------------------------
    private void Start()
    {
        StartMusic();
    }

    // ------------------------------------------------------------------------
    //                                UPDATE
    // ------------------------------------------------------------------------
    private void Update()
    {
        if (!hasStarted) return;

        double dspNow = AudioSettings.dspTime;

        // 1) Disparar onBeat
        if (dspNow >= nextBeatTimeDSP)
        {
            onBeat.Invoke();
            double secPerBeat = 60.0 / BPM;
            nextBeatTimeDSP += secPerBeat;
        }

        // 2) Programar la siguiente sección si se acerca el final
        if (!nextSectionScheduled && (dspNow + SCHEDULE_LOOKAHEAD >= currentSegmentEndDSP))
        {
            ScheduleNextSection();
            nextSectionScheduled = true;
        }
    }

    // ------------------------------------------------------------------------
    //                             INICIO DE MÚSICA
    // ------------------------------------------------------------------------
    /// <summary>
    /// Arranca la música:
    /// - Si hay Intro, la reproduce una vez,
    /// - Luego pasa a Combate,
    /// - Después alterna Combate ↔ Defensa indefinidamente.
    /// </summary>
    public void StartMusic()
    {
        if (hasStarted) return;
        hasStarted = true;

        dspTimeStartOfMusic = AudioSettings.dspTime;
        double secPerBeat = 60.0 / BPM;
        nextBeatTimeDSP = dspTimeStartOfMusic + secPerBeat;

        if (introClips != null && introClips.Count > 0)
        {
            currentState = MusicState.Intro;
            AudioClip introClip = GetRandomClip(introClips);

            if (introClip != null)
            {
                double introStart = dspTimeStartOfMusic;
                double introEnd = introStart + introClip.length;
                PlayOnNextSource(introClip, introStart, introEnd);
                currentSegmentEndDSP = introEnd;
            }
            else
            {
                // Si la lista existe pero getRandomClip devolvió null
                Debug.LogWarning("[AudioController] Intro asignada pero clip nulo. Se salta Intro.");
                GoToCombateDirect();
            }
        }
        else
        {
            GoToCombateDirect();
        }

        nextSectionScheduled = false;
    }

    private void GoToCombateDirect()
    {
        currentState = MusicState.Combate;

        double startT = dspTimeStartOfMusic;
        double segDur = GetSegmentDuration();

        AudioClip cClip = GetRandomClip(combateClips);
        if (cClip == null)
        {
            Debug.LogWarning("[AudioController] No hay clips de Combate. Nada se reproducirá.");
            return;
        }

        double endT = startT + segDur;
        PlayOnNextSource(cClip, startT, endT);
        currentSegmentEndDSP = endT;
    }

    // ------------------------------------------------------------------------
    //                           SCHEDULE NEXT SECTION
    // ------------------------------------------------------------------------
    private void ScheduleNextSection()
    {
        switch (currentState)
        {
            case MusicState.Intro:
                GoToState(MusicState.Combate);
                break;

            case MusicState.Combate:
                GoToState(MusicState.Defensa);
                break;

            case MusicState.Defensa:
                GoToState(MusicState.Combate);
                break;

            case MusicState.Transicion:
                // Normalmente no se permanece en Transicion;
                // se salta enseguida a Combate/Defensa.
                break;
        }
        // Después de programar el siguiente estado
        nextSectionScheduled = false;
    }

    /// <summary>
    /// Pasa del estado actual a 'next' (Combate o Defensa).
    /// Si hay transiciones, las reproduce primero.
    /// </summary>
    private void GoToState(MusicState next)
    {
        bool hasTrans = (transicionClips != null && transicionClips.Count > 0);

        if (hasTrans)
        {
            AudioClip tClip = GetRandomClip(transicionClips);
            if (tClip != null)
            {
                double transitionStart = currentSegmentEndDSP;
                double transitionEnd = transitionStart + tClip.length;

                currentState = MusicState.Transicion;
                PlayOnNextSource(tClip, transitionStart, transitionEnd);

                // Luego programar el estado principal
                double segDur = GetSegmentDuration();
                AudioClip mainClip = (next == MusicState.Combate)
                                     ? GetRandomClip(combateClips)
                                     : GetRandomClip(defensaClips);

                if (mainClip == null)
                {
                    Debug.LogWarning($"[AudioController] No hay clips para {next}. Se detiene.");
                    return;
                }

                double mainStart = transitionEnd;
                double mainEnd = mainStart + segDur;

                PlayOnNextSource(mainClip, mainStart, mainEnd);

                currentState = next;
                currentSegmentEndDSP = mainEnd;
            }
            else
            {
                // Lista de transiciones, pero clip null
                Debug.LogWarning("[AudioController] Transiciones definidas pero devuelven null. Vamos directo.");
                ScheduleDirect(next);
            }
        }
        else
        {
            // Sin transiciones
            ScheduleDirect(next);
        }
    }

    /// <summary>
    /// Programa directamente Combate o Defensa sin transición.
    /// </summary>
    private void ScheduleDirect(MusicState next)
    {
        double startT = currentSegmentEndDSP;
        double segDur = GetSegmentDuration();

        AudioClip clip = (next == MusicState.Combate)
            ? GetRandomClip(combateClips)
            : GetRandomClip(defensaClips);

        if (clip == null)
        {
            Debug.LogWarning($"[AudioController] No hay clips en {next}. Se detiene.");
            return;
        }

        double endT = startT + segDur;
        currentState = next;
        PlayOnNextSource(clip, startT, endT);
        currentSegmentEndDSP = endT;
    }

    // ------------------------------------------------------------------------
    //                   CAMBIO DE CANCIÓN PRINCIPAL (opcional)
    // ------------------------------------------------------------------------
    public void ChangeSong(AudioClip newSong, float newBPM)
    {
        BPM = newBPM;
        onSongChanged.Invoke();

        double dspNow = AudioSettings.dspTime;
        if (newSong != null)
        {
            AudioSource s = isSourceAPlaying ? sourceB : sourceA;
            isSourceAPlaying = !isSourceAPlaying;

            double endT = dspNow + newSong.length;
            s.volume = volume;
            s.clip = newSong;
            s.PlayScheduled(dspNow);
            s.SetScheduledEndTime(endT);

            currentSegmentEndDSP = endT;
            currentState = MusicState.None;
            nextSectionScheduled = false;
        }
    }

    // ------------------------------------------------------------------------
    //                       AUXILIARES
    // ------------------------------------------------------------------------
    private double GetSegmentDuration()
    {
        return beatsPerSegment * (60.0 / BPM);
    }

    private void PlayOnNextSource(AudioClip clip, double startDSP, double endDSP)
    {
        if (clip == null)
        {
            Debug.LogWarning("[AudioController] Clip nulo en PlayOnNextSource. No se reproduce nada.");
            return;
        }

        AudioSource nextSource = isSourceAPlaying ? sourceB : sourceA;
        isSourceAPlaying = !isSourceAPlaying;

        nextSource.clip = clip;
        nextSource.volume = volume;
        nextSource.PlayScheduled(startDSP);
        nextSource.SetScheduledEndTime(endDSP);
    }

    private AudioClip GetRandomClip(List<AudioClip> clips)
    {
        if (clips == null || clips.Count == 0)
            return null;

        int index = Random.Range(0, clips.Count);
        return clips[index];
    }
}
