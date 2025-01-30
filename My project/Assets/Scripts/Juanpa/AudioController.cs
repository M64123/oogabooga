using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// AudioController con una LISTA de Canciones. 
/// Escoge UNA Cancion al inicio, reproduce sus secciones en orden cíclico (Ataque/Defensa), 
/// usando Intro y Transición si existen. 
/// Cada sección forzada a X beats, y llama a GuitarHeroMIDIManager para QTE.
/// </summary>
public class AudioController : MonoBehaviour
{
    public static AudioController Instance { get; private set; }

    [Header("Lista de Canciones")]
    public List<Cancion> canciones;

    [Header("Configuración de Beats (forzados)")]
    public float bpm = 120f;
    public int beatsIntro = 8;
    public int beatsTransicion = 4;
    public int beatsAtaque = 16;
    public int beatsDefensa = 16;

    [Header("Volumen General")]
    [Range(0f, 1f)]
    public float volume = 1f;

    [Header("MIDI Manager (para QTE)")]
    public GuitarHeroMIDIManager midiManager;

    [Header("Eventos (opcional)")]
    public UnityEvent onSongStart;

    // Internos
    private AudioSource sourceA;
    private AudioSource sourceB;
    private bool lastUsedSourceA = false;

    private enum MusicState { None, Intro, Ataque, Defensa, Transicion }
    private MusicState currentState = MusicState.None;

    private double currentSectionEndDSP = 0.0;
    private bool hasStarted = false;
    private bool nextSectionScheduled = false;
    private const float LOOKAHEAD = 0.2f;

    // Cancion actual elegida
    private Cancion currentCancion;
    // Índices para avanzar en la lista de ataque/defensa
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

        sourceA = gameObject.AddComponent<AudioSource>();
        sourceB = gameObject.AddComponent<AudioSource>();

        sourceA.playOnAwake = false;
        sourceB.playOnAwake = false;

        sourceA.volume = volume;
        sourceB.volume = volume;

        Debug.Log($"[AudioController] Awake => 2 AudioSources => A={sourceA.GetInstanceID()}, B={sourceB.GetInstanceID()}");
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

        Debug.Log("[AudioController] StartMusic => iniciamos con beats forzados");
        onSongStart.Invoke();

        if (canciones == null || canciones.Count == 0)
        {
            Debug.LogWarning("[AudioController] No hay canciones => no suena nada.");
            return;
        }

        // Elegir una cancion al azar
        currentCancion = PickRandomCancion();
        ataqueIndex = 0;
        defensaIndex = 0;

        double dspStart = AudioSettings.dspTime;

        // Si tiene Intro
        if (currentCancion.introPair != null && currentCancion.introPair.audioClip != null)
        {
            currentState = MusicState.Intro;

            double secPerBeat = 60.0 / bpm;
            double introDur = beatsIntro * secPerBeat;
            double dspEnd = dspStart + introDur;

            PlayScheduled(currentCancion.introPair, dspStart, dspEnd);
            currentSectionEndDSP = dspEnd;

            Debug.Log($"[AudioController] Intro => '{currentCancion.introPair.audioClip.name}', {beatsIntro} beats, {dspStart:F2}-{dspEnd:F2}");
            CallMIDIManager(currentCancion.introPair, dspStart, dspEnd, true);
        }
        else
        {
            // Sin intro => Ataque directo
            GoToAtaqueDirect(dspStart);
        }

        nextSectionScheduled = false;
    }

    private void GoToAtaqueDirect(double dspStart)
    {
        currentState = MusicState.Ataque;

        AudioMidiPair pair = GetNextAtaquePair();
        if (pair == null)
        {
            Debug.LogWarning("[AudioController] No clips de Ataque => no suena nada.");
            return;
        }

        double secPerBeat = 60.0 / bpm;
        double segDur = beatsAtaque * secPerBeat;
        double dspEnd = dspStart + segDur;

        PlayScheduled(pair, dspStart, dspEnd);
        currentSectionEndDSP = dspEnd;

        Debug.Log($"[AudioController] Ataque => '{pair.audioClip.name}', {beatsAtaque} beats, {dspStart:F2}-{dspEnd:F2}");
        CallMIDIManager(pair, dspStart, dspEnd, false);

        nextSectionScheduled = false;
    }

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
                Debug.LogWarning("[AudioController] Terminó Transición sin nextState definido.");
                break;
        }

        nextSectionScheduled = false;
    }

    private void GoToState(MusicState next)
    {
        Debug.Log($"[AudioController] GoToState => from {currentState} to {next}");

        // Si tenemos transicion
        if (currentCancion.transicionPair != null && currentCancion.transicionPair.audioClip != null)
        {
            double dspStart = currentSectionEndDSP;
            double secPerBeat = 60.0 / bpm;
            double transDur = beatsTransicion * secPerBeat;
            double dspEnd = dspStart + transDur;

            currentState = MusicState.Transicion;
            PlayScheduled(currentCancion.transicionPair, dspStart, dspEnd);

            Debug.Log($"[AudioController] Trans => '{currentCancion.transicionPair.audioClip.name}', {beatsTransicion} beats, {dspStart:F2}-{dspEnd:F2}");
            CallMIDIManager(currentCancion.transicionPair, dspStart, dspEnd, false);

            // luego la sección principal
            StartCoroutine(WaitAndThenSection(next, dspEnd));
        }
        else
        {
            ScheduleDirect(next);
        }
    }

    private IEnumerator WaitAndThenSection(MusicState next, double startDSP)
    {
        double dspNow = AudioSettings.dspTime;
        float waitSec = (float)(startDSP - dspNow);
        if (waitSec > 0f) yield return new WaitForSecondsRealtime(waitSec);

        ScheduleDirect(next);
    }

    private void ScheduleDirect(MusicState next)
    {
        double dspStart = currentSectionEndDSP;
        double secPerBeat = 60.0 / bpm;

        if (next == MusicState.Ataque)
        {
            AudioMidiPair pair = GetNextAtaquePair();
            if (pair == null)
            {
                Debug.LogWarning("[AudioController] No clips de Ataque => se detiene.");
                return;
            }

            double segDurSec = beatsAtaque * secPerBeat;
            double dspEnd = dspStart + segDurSec;

            currentState = MusicState.Ataque;
            PlayScheduled(pair, dspStart, dspEnd);

            Debug.Log($"[AudioController] Ataque => '{pair.audioClip.name}', {beatsAtaque} beats, {dspStart:F2}-{dspEnd:F2}");
            currentSectionEndDSP = dspEnd;
            CallMIDIManager(pair, dspStart, dspEnd, false);
        }
        else if (next == MusicState.Defensa)
        {
            AudioMidiPair pair = GetNextDefensaPair();
            if (pair == null)
            {
                Debug.LogWarning("[AudioController] No clips de Defensa => se detiene.");
                return;
            }

            double segDurSec = beatsDefensa * secPerBeat;
            double dspEnd = dspStart + segDurSec;

            currentState = MusicState.Defensa;
            PlayScheduled(pair, dspStart, dspEnd);

            Debug.Log($"[AudioController] Defensa => '{pair.audioClip.name}', {beatsDefensa} beats, {dspStart:F2}-{dspEnd:F2}");
            currentSectionEndDSP = dspEnd;
            CallMIDIManager(pair, dspStart, dspEnd, false);
        }
    }

    // ----------------------------------------------------------------------
    //                     Aux
    // ----------------------------------------------------------------------
    private Cancion PickRandomCancion()
    {
        int idx = Random.Range(0, canciones.Count);
        return canciones[idx];
    }

    private AudioMidiPair GetNextAtaquePair()
    {
        if (currentCancion == null) return null;
        if (currentCancion.ataquePairs == null || currentCancion.ataquePairs.Count == 0) return null;

        AudioMidiPair p = currentCancion.ataquePairs[ataqueIndex];
        ataqueIndex = (ataqueIndex + 1) % currentCancion.ataquePairs.Count;
        return p;
    }

    private AudioMidiPair GetNextDefensaPair()
    {
        if (currentCancion == null) return null;
        if (currentCancion.defensaPairs == null || currentCancion.defensaPairs.Count == 0) return null;

        AudioMidiPair p = currentCancion.defensaPairs[defensaIndex];
        defensaIndex = (defensaIndex + 1) % currentCancion.defensaPairs.Count;
        return p;
    }

    private void PlayScheduled(AudioMidiPair pair, double dspStart, double dspEnd)
    {
        if (pair == null || pair.audioClip == null)
        {
            Debug.LogWarning("[AudioController] pair nulo => no suena");
            return;
        }

        AudioSource nextSrc = lastUsedSourceA ? sourceB : sourceA;
        lastUsedSourceA = !lastUsedSourceA;

        // limpiar la otra
        AudioSource other = (nextSrc == sourceA) ? sourceB : sourceA;
        other.Stop();
        other.clip = null;

        nextSrc.Stop();
        nextSrc.clip = pair.audioClip;
        nextSrc.volume = volume;

        nextSrc.PlayScheduled(dspStart);
        nextSrc.SetScheduledEndTime(dspEnd);

        Debug.Log($"[AudioController] PlayScheduled => '{pair.audioClip.name}', from {dspStart:F2} to {dspEnd:F2} on {(nextSrc == sourceA ? "sourceA" : "sourceB")}");
    }

    private void CallMIDIManager(AudioMidiPair pair, double dspStart, double dspEnd, bool isIntro)
    {
        if (midiManager != null && pair.midiFile != null)
        {
            midiManager.ParseAndGenerateQTEs(pair.midiFile, dspStart, dspEnd, isIntro);
        }
    }
}
