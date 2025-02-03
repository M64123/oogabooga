using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// AudioController estilo FNF con intros y transiciones sin QTE, 
/// y secciones de Ataque/Defensa en orden cíclico con QTE.
/// </summary>
public class AudioController : MonoBehaviour
{
    public static AudioController Instance { get; private set; }

    [Header("Lista de Canciones")]
    public List<Cancion> canciones;

    [Header("Configuración de Beats (forzados)")]
    public float bpm = 120f;
    public int beatsIntro = 8;        // duracion para introPair
    public int beatsTransicion = 4;   // duracion para transicionPair
    public int beatsAtaque = 16;      // duracion para cada clip de ataque
    public int beatsDefensa = 16;     // duracion para cada clip de defensa

    [Header("Volumen General")]
    [Range(0f, 1f)] public float volume = 1f;

    [Header("MIDI Manager (para QTE)")]
    public GuitarHeroMIDIManager midiManager;

    [Header("Eventos (opcional)")]
    public UnityEvent onSongStart;

    private AudioSource sourceA;
    private AudioSource sourceB;
    private bool lastUsedSourceA = false;

    private enum MusicState { None, Intro, Ataque, Defensa, Transicion }
    private MusicState currentState = MusicState.None;

    private double currentSectionEndDSP = 0.0;
    private bool hasStarted = false;
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
            Debug.Log($"[AudioController] => SCHEDULE NEXT SECTION. dspNow={dspNow:F2}, endDSP={currentSectionEndDSP:F2}");
            ScheduleNextSection();
            nextSectionScheduled = true;
        }
    }

    public void StartMusic()
    {
        if (hasStarted) return;
        hasStarted = true;

        Debug.Log("[AudioController] StartMusic => invocamos onSongStart");
        onSongStart.Invoke();

        // Escoger cancion
        if (canciones == null || canciones.Count == 0)
        {
            Debug.LogWarning("[AudioController] No hay canciones => no suena nada.");
            return;
        }
        currentCancion = PickRandomCancion();
        ataqueIndex = 0;
        defensaIndex = 0;

        double dspStart = AudioSettings.dspTime;

        // 1) Reproducir introPair de la cancion (si existe) sin QTE
        if (currentCancion.introPair != null && currentCancion.introPair.audioClip != null)
        {
            currentState = MusicState.Intro;

            double secPerBeat = 60.0 / bpm;
            double introDurSec = beatsIntro * secPerBeat;
            double dspEnd = dspStart + introDurSec;

            PlayScheduled(currentCancion.introPair, dspStart, dspEnd);
            currentSectionEndDSP = dspEnd;

            Debug.Log($"[AudioController] Intro => '{currentCancion.introPair.audioClip.name}', {beatsIntro} beats, {dspStart:F2}-{dspEnd:F2}");
            // No QTE => no llamamos a CallMIDIManager para la intro
        }
        else
        {
            // Sin intro => arranca ATAQUE
            GoToAtaqueDirect(dspStart);
        }

        nextSectionScheduled = false;
    }

    private Cancion PickRandomCancion()
    {
        int idx = Random.Range(0, canciones.Count);
        return canciones[idx];
    }

    private void ScheduleNextSection()
    {
        double dspNow = AudioSettings.dspTime;
        Debug.Log($"[AudioController] ScheduleNextSection => current={currentState}, dspNow={dspNow:F2}, endDSP={currentSectionEndDSP:F2}");

        switch (currentState)
        {
            case MusicState.Intro:
                // Termina intro => ATAQUE
                GoToState(MusicState.Ataque);
                break;
            case MusicState.Ataque:
                // ATAQUE => DEFENSA
                GoToState(MusicState.Defensa);
                break;
            case MusicState.Defensa:
                // DEFENSA => ATAQUE
                GoToState(MusicState.Ataque);
                break;
            case MusicState.Transicion:
                // Al terminar la trans, iremos a la seccion next (ver Corrutina WaitAndThen)
                break;
        }

        nextSectionScheduled = false;
    }

    private void GoToState(MusicState next)
    {
        Debug.Log($"[AudioController] GoToState => from {currentState} to {next}");

        // Ver si hay transicionPair
        if (currentCancion.transicionPair != null && currentCancion.transicionPair.audioClip != null)
        {
            // reproducir transicion sin QTE
            double dspStart = currentSectionEndDSP;
            double secPerBeat = 60.0 / bpm;
            double transDur = beatsTransicion * secPerBeat;
            double dspEnd = dspStart + transDur;

            currentState = MusicState.Transicion;
            PlayScheduled(currentCancion.transicionPair, dspStart, dspEnd);
            Debug.Log($"[AudioController] Trans => '{currentCancion.transicionPair.audioClip.name}', {beatsTransicion} beats, {dspStart:F2}-{dspEnd:F2}");

            // no QTE => no CallMIDIManager

            StartCoroutine(WaitAndThenSection(next, dspEnd));
        }
        else
        {
            // sin trans => directo
            ScheduleDirect(next);
        }
    }

    private IEnumerator WaitAndThenSection(MusicState next, double dspEndTrans)
    {
        double dspNow = AudioSettings.dspTime;
        float waitSec = (float)(dspEndTrans - dspNow);
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

    private void GoToAtaqueDirect(double dspStart)
    {
        currentState = MusicState.Ataque;

        AudioMidiPair pair = GetNextAtaquePair();
        if (pair == null || pair.audioClip == null)
        {
            Debug.LogWarning("[AudioController] No hay clips de Ataque => no suena nada");
            return;
        }

        double secPerBeat = 60.0 / bpm;
        double durSec = beatsAtaque * secPerBeat;
        double dspEnd = dspStart + durSec;

        PlayScheduled(pair, dspStart, dspEnd);
        currentSectionEndDSP = dspEnd;

        Debug.Log($"[AudioController] Ataque => '{pair.audioClip.name}', {beatsAtaque} beats, {dspStart:F2}-{dspEnd:F2}");

        // Generar QTE
        CallMIDIManager(pair, dspStart, dspEnd);

        nextSectionScheduled = false;
    }

    private void GoToDefensaDirect(double dspStart)
    {
        currentState = MusicState.Defensa;

        AudioMidiPair pair = GetNextDefensaPair();
        if (pair == null || pair.audioClip == null)
        {
            Debug.LogWarning("[AudioController] No hay clips de Defensa => no suena nada");
            return;
        }

        double secPerBeat = 60.0 / bpm;
        double durSec = beatsDefensa * secPerBeat;
        double dspEnd = dspStart + durSec;

        PlayScheduled(pair, dspStart, dspEnd);
        currentSectionEndDSP = dspEnd;

        Debug.Log($"[AudioController] Defensa => '{pair.audioClip.name}', {beatsDefensa} beats, {dspStart:F2}-{dspEnd:F2}");

        // Generar QTE
        CallMIDIManager(pair, dspStart, dspEnd);

        nextSectionScheduled = false;
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
        AudioSource nextSrc = (lastUsedSourceA ? sourceB : sourceA);
        lastUsedSourceA = !lastUsedSourceA;

        AudioSource other = (nextSrc == sourceA) ? sourceB : sourceA;
        other.Stop();
        other.clip = null;

        nextSrc.Stop();
        nextSrc.clip = pair.audioClip;
        nextSrc.volume = volume;

        nextSrc.PlayScheduled(dspStart);
        nextSrc.SetScheduledEndTime(dspEnd);

        Debug.Log($"[AudioController] PlayScheduled => '{pair.audioClip.name}', from {dspStart:F2} to {dspEnd:F2}");
    }

    private void CallMIDIManager(AudioMidiPair pair, double dspStart, double dspEnd)
    {
        if (midiManager != null && pair.midiFile != null)
        {
            midiManager.ParseAndGenerateQTEs(pair.midiFile, dspStart, dspEnd);
        }
    }
}
