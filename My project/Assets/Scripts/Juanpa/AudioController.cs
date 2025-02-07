using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// AudioController donde cada Cancion tiene:
///  - baseClip (audio en loop),
///  - introPair/transicionPair (opcionales),
///  - bool useIntro, bool useTransicion para controlarlo desde Inspector,
///  - ataqueMidis/defensaMidis (solo para QTE).
/// 
/// Se fuerza la duración de intro/transición/ataque/defensa a beatsX*(60/bpm).
/// Se añade beatsAlignment para que los primeros X beats sean de ajuste
/// antes de que suenen notas de la sección, ayudando a sincronizar QTE.
/// 
/// La baseClip suena en loop, y generamos QTE parseando MIDIs distintos
/// en Ataque y Defensa. La Intro y Transición (si existen) no tienen QTE.
/// </summary>
public class AudioController : MonoBehaviour
{
    public static AudioController Instance { get; private set; }

    [Header("Lista de Canciones")]
    public List<Cancion> canciones;

    [Header("Configuración de Beats (forzados)")]
    public float bpm = 120f;

    [Tooltip("Beats forzados para la Intro (si se usa)")]
    public int beatsIntro = 8;

    [Tooltip("Beats forzados para la Transición (si se usa)")]
    public int beatsTransicion = 4;

    [Tooltip("Beats totales de la sección de ATAQUE (suma alignment + notas)")]
    public int beatsAtaque = 16;

    [Tooltip("Beats totales de la sección de DEFENSA (suma alignment + notas)")]
    public int beatsDefensa = 16;

    [Tooltip("Beats iniciales de 'alineación' (pre-roll) en cada sección (Ataque o Defensa). " +
             "Durante estos beats no se parsean notas del MIDI, " +
             "sirve para sincronizar el spawn con la música.")]
    public int beatsAlignment = 4;

    [Header("Volumen General")]
    [Range(0f, 1f)] public float volume = 1f;

    [Header("MIDI Manager (para QTE)")]
    public GuitarHeroMIDIManager midiManager;

    [Header("Eventos (opcional)")]
    public UnityEvent onSongStart;

    private AudioSource baseSource;  // Reproduce baseClip en loop
    private AudioSource tempSource;  // Reproduce Intro/Transición
    private bool hasStarted = false;

    private enum MusicState { None, Intro, Ataque, Defensa, Transicion }
    private MusicState currentState = MusicState.None;

    private double currentSectionEndDSP = 0.0;
    private bool nextSectionScheduled = false;
    private const float LOOKAHEAD = 0.2f;

    // Cancion actual
    private Cancion currentCancion;
    private int ataqueIndex = 0;
    private int defensaIndex = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // AudioSource para la base en loop
        baseSource = gameObject.AddComponent<AudioSource>();
        baseSource.playOnAwake = false;
        baseSource.loop = true;
        baseSource.volume = volume;

        // AudioSource temporal para Intro/Transición
        tempSource = gameObject.AddComponent<AudioSource>();
        tempSource.playOnAwake = false;
        tempSource.loop = false;
        tempSource.volume = volume;

        Debug.Log($"[AudioController] Awake => baseSrc={baseSource.GetInstanceID()}, tempSrc={tempSource.GetInstanceID()}");
    }

    private void Start()
    {
        StartMusic();
    }

    private void Update()
    {
        if (!hasStarted) return;

        double dspNow = AudioSettings.dspTime;
        if (!nextSectionScheduled && (dspNow + LOOKAHEAD >= currentSectionEndDSP))
        {
            Debug.Log($"[AudioController] => SCHEDULE NEXT. dspNow={dspNow:F2}, endDSP={currentSectionEndDSP:F2}");
            ScheduleNextSection();
            nextSectionScheduled = true;
        }
    }

    // ----------------------------------------------------------------------
    //                      StartMusic
    // ----------------------------------------------------------------------
    public void StartMusic()
    {
        if (hasStarted) return;
        hasStarted = true;

        Debug.Log("[AudioController] StartMusic => onSongStart.Invoke()");
        onSongStart.Invoke();

        // Escogemos una Cancion
        if (canciones == null || canciones.Count == 0)
        {
            Debug.LogWarning("[AudioController] No hay canciones => no suena nada.");
            return;
        }
        currentCancion = PickRandomCancion();
        ataqueIndex = 0;
        defensaIndex = 0;

        // 1) baseClip en loop
        if (currentCancion.baseClip != null)
        {
            baseSource.clip = currentCancion.baseClip;
            baseSource.volume = volume;
            baseSource.Play(); // Loop indefinido
            Debug.Log($"[AudioController] BaseClip => '{currentCancion.baseClip.name}' en loop.");
        }
        else
        {
            Debug.LogWarning("[AudioController] Cancion sin baseClip => no hay música de fondo");
        }

        double dspStart = AudioSettings.dspTime;

        // 2) Intro (opcional)
        if (currentCancion.useIntro
            && currentCancion.introPair != null
            && currentCancion.introPair.audioClip != null)
        {
            currentState = MusicState.Intro;

            double secPerBeat = 60.0 / bpm;
            double introDurSec = beatsIntro * secPerBeat;
            double dspEnd = dspStart + introDurSec;

            PlayTempScheduled(currentCancion.introPair.audioClip, dspStart, dspEnd);
            currentSectionEndDSP = dspEnd;

            Debug.Log($"[AudioController] Intro => '{currentCancion.introPair.audioClip.name}', {beatsIntro} beats, {dspStart:F2}-{dspEnd:F2}");
            // Sin QTE => no MIDI
        }
        else
        {
            // Sin intro => arrancar Ataque
            GoToAtaqueDirect(dspStart);
        }

        nextSectionScheduled = false;
    }

    private Cancion PickRandomCancion()
    {
        int idx = Random.Range(0, canciones.Count);
        return canciones[idx];
    }

    // ----------------------------------------------------------------------
    //             ScheduleNextSection
    // ----------------------------------------------------------------------
    private void ScheduleNextSection()
    {
        double dspNow = AudioSettings.dspTime;
        Debug.Log($"[AudioController] ScheduleNextSection => current={currentState}, dspNow={dspNow:F2}, endDSP={currentSectionEndDSP:F2}");

        switch (currentState)
        {
            case MusicState.Intro:
                GoToState(MusicState.Ataque);
                break;
            case MusicState.Ataque:
                GoToState(MusicState.Defensa);
                break;
            case MusicState.Defensa:
                GoToState(MusicState.Ataque);
                break;
            case MusicState.Transicion:
                // Esperar corrutina WaitAndThen
                break;
        }

        nextSectionScheduled = false;
    }

    // ----------------------------------------------------------------------
    //             GoToState => si hay Transición
    // ----------------------------------------------------------------------
    private void GoToState(MusicState next)
    {
        Debug.Log($"[AudioController] GoToState => from {currentState} to {next}");

        // Usamos transicion si useTransicion==true y transicionPair existe
        if (next != MusicState.Intro && next != MusicState.Transicion)
        {
            if (currentCancion.useTransicion
                && currentCancion.transicionPair != null
                && currentCancion.transicionPair.audioClip != null)
            {
                double dspStart = currentSectionEndDSP;
                double secPerBeat = 60.0 / bpm;
                double dur = beatsTransicion * secPerBeat;
                double dspEnd = dspStart + dur;

                currentState = MusicState.Transicion;
                PlayTempScheduled(currentCancion.transicionPair.audioClip, dspStart, dspEnd);

                Debug.Log($"[AudioController] Trans => '{currentCancion.transicionPair.audioClip.name}', {beatsTransicion} beats, {dspStart:F2}-{dspEnd:F2}");

                // Sin QTE => no MIDI manager
                StartCoroutine(WaitAndThenSection(next, dspEnd));
                return;
            }
        }

        // Si no hay transicion => directo
        ScheduleDirect(next);
    }

    private IEnumerator WaitAndThenSection(MusicState next, double dspEnd)
    {
        double dspNow = AudioSettings.dspTime;
        float waitSec = (float)(dspEnd - dspNow);
        if (waitSec > 0f)
            yield return new WaitForSecondsRealtime(waitSec);

        ScheduleDirect(next);
    }

    private void ScheduleDirect(MusicState next)
    {
        if (next == MusicState.Ataque)
        {
            GoToAtaqueDirect(currentSectionEndDSP);
        }
        else if (next == MusicState.Defensa)
        {
            GoToDefensaDirect(currentSectionEndDSP);
        }
    }

    // ----------------------------------------------------------------------
    //       ATAQUE => Generamos QTE con "alineación" + "notas"
    // ----------------------------------------------------------------------
    private void GoToAtaqueDirect(double dspStart)
    {
        currentState = MusicState.Ataque;

        double secPerBeat = 60.0 / bpm;

        // Sección total = beatsAtaque
        // Queremos que los primeros "beatsAlignment" sean de "pre-roll" sin notas
        // => dspQTEStart = dspStart + (beatsAlignment*secPerBeat)
        double totalSec = beatsAtaque * secPerBeat;
        double dspEnd = dspStart + totalSec;

        // Momento en que empezamos a parsear el MIDI
        double dspQTEStart = dspStart + (beatsAlignment * secPerBeat);

        currentSectionEndDSP = dspEnd;

        Debug.Log($"[AudioController] Ataque => total {beatsAtaque} beats. " +
                  $"Se parsea MIDI a partir de {dspQTEStart:F2} hasta {dspEnd:F2}");

        // Generar QTE con MIDI
        MidiAsset midiA = GetNextAtaqueMidi();
        if (midiA != null && midiManager != null)
        {
            midiManager.ParseAndGenerateQTEs(midiA, dspQTEStart, dspEnd);
        }
        else
        {
            Debug.LogWarning("[AudioController] No hay MIDI de Ataque => no QTE.");
        }

        nextSectionScheduled = false;
    }

    private void GoToDefensaDirect(double dspStart)
    {
        currentState = MusicState.Defensa;

        double secPerBeat = 60.0 / bpm;

        // Sección total = beatsDefensa
        double totalSec = beatsDefensa * secPerBeat;
        double dspEnd = dspStart + totalSec;

        // Alineación
        double dspQTEStart = dspStart + (beatsAlignment * secPerBeat);

        currentSectionEndDSP = dspEnd;

        Debug.Log($"[AudioController] Defensa => total {beatsDefensa} beats. " +
                  $"Se parsea MIDI a partir de {dspQTEStart:F2} hasta {dspEnd:F2}");

        // Generar QTE con MIDI
        MidiAsset midiD = GetNextDefensaMidi();
        if (midiD != null && midiManager != null)
        {
            midiManager.ParseAndGenerateQTEs(midiD, dspQTEStart, dspEnd);
        }
        else
        {
            Debug.LogWarning("[AudioController] No hay MIDI de Defensa => no QTE.");
        }

        nextSectionScheduled = false;
    }

    // ----------------------------------------------------------------------
    //         Elegir MIDI en orden cíclico
    // ----------------------------------------------------------------------
    private MidiAsset GetNextAtaqueMidi()
    {
        if (currentCancion == null) return null;
        if (currentCancion.ataqueMidis == null || currentCancion.ataqueMidis.Count == 0) return null;

        MidiAsset midi = currentCancion.ataqueMidis[ataqueIndex];
        ataqueIndex = (ataqueIndex + 1) % currentCancion.ataqueMidis.Count;
        return midi;
    }

    private MidiAsset GetNextDefensaMidi()
    {
        if (currentCancion == null) return null;
        if (currentCancion.defensaMidis == null || currentCancion.defensaMidis.Count == 0) return null;

        MidiAsset midi = currentCancion.defensaMidis[defensaIndex];
        defensaIndex = (defensaIndex + 1) % currentCancion.defensaMidis.Count;
        return midi;
    }

    // ----------------------------------------------------------------------
    //         Reproducir Intro/Transición en tempSource programado
    // ----------------------------------------------------------------------
    private void PlayTempScheduled(AudioClip clip, double dspStart, double dspEnd)
    {
        if (!clip) return;

        tempSource.Stop();
        tempSource.clip = clip;
        tempSource.volume = volume;

        tempSource.PlayScheduled(dspStart);
        tempSource.SetScheduledEndTime(dspEnd);

        Debug.Log($"[AudioController] PlayTempScheduled => '{clip.name}', {dspStart:F2}-{dspEnd:F2}");
    }
}
