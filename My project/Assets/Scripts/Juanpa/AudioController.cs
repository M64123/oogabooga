using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// AudioController que:
/// - Usa 2 AudioSources para reproducir Intro/Combate/Defensa/Transición,
/// - Cada sección (Intro, Combate, Defensa, Transición) dura EXACTAMENTE X beats (definidos en Inspector),
/// - Alterna Combate ↔ Defensa indefinidamente tras la Intro (si existe),
/// - Emplea AudioMidiPair (AudioClip + MIDI) para cada sección. Al reproducir uno, se llama a TaikoMIDIManager con el MIDI correspondiente.
/// </summary>
public class AudioController : MonoBehaviour
{
    public static AudioController Instance { get; private set; }

    [Header("Intro (opcional)")]
    public List<AudioMidiPair> introPairs;

    [Header("Combate (pares audio + midi)")]
    public List<AudioMidiPair> combatePairs;

    [Header("Defensa (pares audio + midi)")]
    public List<AudioMidiPair> defensaPairs;

    [Header("Transicion (opcional)")]
    public List<AudioMidiPair> transicionPairs;

    [Header("Configuración de Beats")]
    [Tooltip("BPM global para calcular duraciones (60 / bpm).")]
    public float bpm = 120f;

    [Tooltip("Beats que dura la Intro (si existe).")]
    public int beatsIntro = 8;

    [Tooltip("Beats que dura la Transición (si existe).")]
    public int beatsTransicion = 4;

    [Tooltip("Beats que dura Combate.")]
    public int beatsCombate = 16;

    [Tooltip("Beats que dura Defensa.")]
    public int beatsDefensa = 16;

    [Header("Volumen General")]
    [Range(0f, 1f)]
    public float volume = 1f;

    [Header("MIDI Manager (para QTE)")]
    public TaikoMIDIManager midiManager;

    [Header("Eventos (opcional)")]
    public UnityEvent onSongStart;

    // ----------------------------------------------------------------------
    // Internos
    private AudioSource sourceA;
    private AudioSource sourceB;
    private bool lastUsedSourceA = false;

    private enum MusicState { None, Intro, Combate, Defensa, Transicion }
    private MusicState currentState = MusicState.None;

    private double currentSectionEndDSP = 0.0;
    private bool hasStarted = false;
    private bool nextSectionScheduled = false;

    private const float LOOKAHEAD = 0.2f;

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

        double dspStart = AudioSettings.dspTime;

        // Intro
        if (introPairs != null && introPairs.Count > 0)
        {
            currentState = MusicState.Intro;
            AudioMidiPair pair = GetRandomPair(introPairs);
            if (pair.audioClip == null)
            {
                Debug.LogWarning("[AudioController] Intro pair sin audio => voy a Combate directo");
                GoToCombateDirect(dspStart);
                return;
            }

            double secPerBeat = 60.0 / bpm;
            double introDurSec = beatsIntro * secPerBeat;
            double dspEnd = dspStart + introDurSec;

            PlayScheduled(pair.audioClip, dspStart, dspEnd);
            currentSectionEndDSP = dspEnd;

            Debug.Log($"[AudioController] Intro => '{pair.audioClip.name}', {beatsIntro} beats, {dspStart:F2}-{dspEnd:F2}");

            // MIDI
            if (midiManager != null && pair.midiFile != null)
            {
                midiManager.ParseAndGenerateQTEs(pair.midiFile, dspStart, dspEnd, true);
            }
        }
        else
        {
            GoToCombateDirect(dspStart);
        }

        nextSectionScheduled = false;
    }

    private void GoToCombateDirect(double dspStart)
    {
        currentState = MusicState.Combate;

        AudioMidiPair pair = GetRandomPair(combatePairs);
        if (pair == null || pair.audioClip == null)
        {
            Debug.LogWarning("[AudioController] No Combate pairs => no se reproduce nada.");
            return;
        }

        double secPerBeat = 60.0 / bpm;
        double segDur = beatsCombate * secPerBeat;
        double dspEnd = dspStart + segDur;

        PlayScheduled(pair.audioClip, dspStart, dspEnd);
        currentSectionEndDSP = dspEnd;

        Debug.Log($"[AudioController] Combate(Direct) => '{pair.audioClip.name}', {beatsCombate} beats, {dspStart:F2}-{dspEnd:F2}");

        if (midiManager != null && pair.midiFile != null)
        {
            midiManager.ParseAndGenerateQTEs(pair.midiFile, dspStart, dspEnd, false);
        }

        nextSectionScheduled = false;
    }

    // ----------------------------------------------------------------------
    //                     ScheduleNextSection
    // ----------------------------------------------------------------------
    private void ScheduleNextSection()
    {
        double dspNow = AudioSettings.dspTime;
        Debug.Log($"[AudioController] ScheduleNextSection => current={currentState}, dspNow={dspNow:F2}, endDSP={currentSectionEndDSP:F2}");

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
                Debug.LogWarning("[AudioController] Terminó Transición. Falta nextState?");
                break;
        }

        nextSectionScheduled = false;
    }

    private void GoToState(MusicState next)
    {
        Debug.Log($"[AudioController] GoToState => from {currentState} to {next}");

        bool hasTrans = (transicionPairs != null && transicionPairs.Count > 0);

        if (hasTrans)
        {
            AudioMidiPair tPair = GetRandomPair(transicionPairs);
            if (tPair != null && tPair.audioClip != null)
            {
                double dspStart = currentSectionEndDSP;
                double secPerBeat = 60.0 / bpm;
                double transDur = beatsTransicion * secPerBeat;
                double dspEnd = dspStart + transDur;

                currentState = MusicState.Transicion;
                PlayScheduled(tPair.audioClip, dspStart, dspEnd);
                Debug.Log($"[AudioController] Trans => '{tPair.audioClip.name}', {beatsTransicion} beats, {dspStart:F2}-{dspEnd:F2}");

                if (midiManager != null && tPair.midiFile != null)
                {
                    midiManager.ParseAndGenerateQTEs(tPair.midiFile, dspStart, dspEnd, false);
                }

                StartCoroutine(WaitAndThenSection(next, dspEnd));
            }
            else
            {
                // no clip => directo
                ScheduleDirect(next);
            }
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

        int beatsToUse = (next == MusicState.Combate) ? beatsCombate : beatsDefensa;
        AudioMidiPair pair = (next == MusicState.Combate)
            ? GetRandomPair(combatePairs)
            : GetRandomPair(defensaPairs);

        if (pair == null || pair.audioClip == null)
        {
            Debug.LogWarning($"[AudioController] No hay pares en {next}. Se detiene.");
            return;
        }

        double segDurSec = beatsToUse * secPerBeat;
        double dspEnd = dspStart + segDurSec;

        currentState = next;
        PlayScheduled(pair.audioClip, dspStart, dspEnd);

        Debug.Log($"[AudioController] {next} => '{pair.audioClip.name}', {beatsToUse} beats, {dspStart:F2}-{dspEnd:F2}");
        currentSectionEndDSP = dspEnd;

        // MIDI
        if (midiManager != null && pair.midiFile != null)
        {
            midiManager.ParseAndGenerateQTEs(pair.midiFile, dspStart, dspEnd, false);
        }
    }

    // ----------------------------------------------------------------------
    //                     Aux
    // ----------------------------------------------------------------------
    private void PlayScheduled(AudioClip clip, double dspStart, double dspEnd)
    {
        if (clip == null)
        {
            Debug.LogWarning("[AudioController] clip nulo => no suena");
            return;
        }

        AudioSource nextSrc = (lastUsedSourceA) ? sourceB : sourceA;
        lastUsedSourceA = !lastUsedSourceA;

        // limpiar la otra
        AudioSource other = (nextSrc == sourceA) ? sourceB : sourceA;
        other.Stop();
        other.clip = null;

        nextSrc.Stop();
        nextSrc.clip = clip;
        nextSrc.volume = volume;

        nextSrc.PlayScheduled(dspStart);
        nextSrc.SetScheduledEndTime(dspEnd);

        Debug.Log($"[AudioController] PlayScheduled => '{clip.name}', from {dspStart:F2} to {dspEnd:F2} on {(nextSrc == sourceA ? "sourceA" : "sourceB")}");
    }

    private AudioMidiPair GetRandomPair(List<AudioMidiPair> list)
    {
        if (list == null || list.Count == 0) return null;
        int idx = Random.Range(0, list.Count);
        return list[idx];
    }
}
