using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// AudioController con 2 AudioSources y logs de depuración detallados.
/// - Reproduce Intro (opcional) => Combate ↔ Defensa indefinidamente,
/// - Usa transiciones opcionales entre Combate y Defensa,
/// - Cada sección se reproduce desde 0 y se corta tras X beats exactos (SetScheduledEndTime).
/// - nextSectionScheduled se resetea a false siempre, evitando que quede atascado.
/// Sincroniza cambios con SongManager.
/// </summary>
public class AudioController : MonoBehaviour
{
    // ----------------------------------------------------------------------
    //                      LISTAS DE AUDIO
    // ----------------------------------------------------------------------
    [Header("Listas de Audio (Opcionales)")]
    [Tooltip("Si hay al menos 1 clip, se reproduce 1 vez antes del ciclo Combate/Defensa.")]
    public List<AudioClip> introClips;

    [Tooltip("Clips de Combate: alternan con Defensa. Debe tener al menos 1 clip para que suene Combate.")]
    public List<AudioClip> combateClips;

    [Tooltip("Clips de Defensa: alternan con Combate. Debe tener al menos 1 clip para que suene Defensa.")]
    public List<AudioClip> defensaClips;

    [Tooltip("Transiciones opcionales entre Combate y Defensa. Si vacío, se salta la transición.")]
    public List<AudioClip> transicionClips;

    // ----------------------------------------------------------------------
    //                DURACIONES EN BEATS: Intro, Transición, Segmentos
    // ----------------------------------------------------------------------
    [Header("Duraciones en Beats")]
    [Tooltip("Beats que dura la Intro (si existen introClips).")]
    public int beatsIntro = 8;

    [Tooltip("Beats que dura la transición (si existen transicionClips).")]
    public int beatsTransicion = 4;

    [Tooltip("Beats que dura cada sección Combate o Defensa.")]
    public int beatsPerSegment = 16;

    // ----------------------------------------------------------------------
    //                     BPM y EVENTOS
    // ----------------------------------------------------------------------
    [Header("BPM y Eventos")]
    [SerializeField]
    private float _bpm = 120f;
    public UnityEvent onBPMChanged;
    public UnityEvent onSongChanged;
    public UnityEvent onCombateStart;
    public UnityEvent onDefensaStart;
    [Tooltip("Se invoca en cada beat (60/BPM).")]
    public UnityEvent onBeat;

    // ----------------------------------------------------------------------
    //                           VOLUMEN
    // ----------------------------------------------------------------------
    [Header("Volumen")]
    [Range(0f, 1f)] public float volume = 1f;

    // ----------------------------------------------------------------------
    //              DOS AUDIOSOURCES Y CONTROL DE FLUJO
    // ----------------------------------------------------------------------
    private AudioSource sourceA;
    private AudioSource sourceB;
    private bool lastUsedSourceA = false; // alternamos entre sourceA y sourceB

    // Estados principales
    private enum MusicState { None, Intro, Combate, Defensa, Transicion }
    private MusicState currentState = MusicState.None;

    // dspTime al que terminará la sección actual
    private double currentSectionEndDSP = 0.0;

    // Evitar programar 2 veces en un solo frame
    private bool nextSectionScheduled = false;

    // Para el cálculo de beats (onBeat)
    private double nextBeatDSP = 0.0;

    // Evitar arrancar StartMusic() varias veces
    private bool hasStarted = false;

    // Margen para programar la siguiente sección
    private const float LOOKAHEAD = 0.2f;

    // ----------------------------------------------------------------------
    //                   PROPIEDAD BPM
    // ----------------------------------------------------------------------
    public float BPM
    {
        get => _bpm;
        set
        {
            if (Mathf.Abs(_bpm - value) > 0.01f)
            {
                _bpm = value;
                onBPMChanged.Invoke();
                Debug.Log($"[AudioController] BPM changed to {_bpm}");
            }
        }
    }

    // ----------------------------------------------------------------------
    //                           AWAKE
    // ----------------------------------------------------------------------
    private void Awake()
    {
        sourceA = gameObject.AddComponent<AudioSource>();
        sourceB = gameObject.AddComponent<AudioSource>();
        sourceA.playOnAwake = false;
        sourceB.playOnAwake = false;

        sourceA.volume = volume;
        sourceB.volume = volume;

        Debug.Log($"[AudioController] Awake => Created 2 AudioSources. sourceA={sourceA.GetInstanceID()}, sourceB={sourceB.GetInstanceID()}");
    }

    // ----------------------------------------------------------------------
    //                           START
    // ----------------------------------------------------------------------
    private void Start()
    {
        Debug.Log("[AudioController] Start => llamamos a StartMusic()");
        StartMusic();
    }

    // ----------------------------------------------------------------------
    //                           UPDATE
    // ----------------------------------------------------------------------
    private void Update()
    {
        if (!hasStarted) return;

        double dspNow = AudioSettings.dspTime;

        // 1) Invocamos onBeat cada 60/BPM seg
        if (dspNow >= nextBeatDSP)
        {
            onBeat.Invoke();
            double secPerBeat = 60.0 / BPM;
            nextBeatDSP += secPerBeat;
            Debug.Log($"[AudioController] BEAT -> dspNow={dspNow:F2}, nextBeatDSP={nextBeatDSP:F2}");
        }

        // 2) Programar la siguiente sección cuando se acerca el final
        if (!nextSectionScheduled && (dspNow + LOOKAHEAD >= currentSectionEndDSP))
        {
            Debug.Log($"[AudioController] Update => SCHEDULE NEXT SECTION. dspNow={dspNow:F2}, endDSP={currentSectionEndDSP:F2}");
            ScheduleNextSection();
            // Marcamos nextSectionScheduled=true dentro de ScheduleNextSection
        }
    }

    // ----------------------------------------------------------------------
    //                           START MUSIC
    // ----------------------------------------------------------------------
    public void StartMusic()
    {
        if (hasStarted)
        {
            Debug.LogWarning("[AudioController] StartMusic() se llamó varias veces, ignoramos.");
            return;
        }
        hasStarted = true;

        double dspNow = AudioSettings.dspTime;
        double secPerBeat = 60.0 / BPM;
        nextBeatDSP = dspNow + secPerBeat;

        bool hasIntro = (introClips != null && introClips.Count > 0);
        Debug.Log($"[AudioController] StartMusic => dspNow={dspNow:F2}, hasIntro={hasIntro}");

        if (hasIntro)
        {
            currentState = MusicState.Intro;
            AudioClip introClip = GetRandomClip(introClips);
            if (introClip == null)
            {
                Debug.LogWarning("[AudioController] introClips exist but returned null => Combate direct");
                GoToCombateDirect(dspNow);
                return;
            }

            double introDurSec = beatsIntro * secPerBeat;
            PlaySection(introClip, dspNow, introDurSec, "Intro");
            currentSectionEndDSP = dspNow + introDurSec;

            Debug.Log($"[AudioController] StartMusic => Scheduled Intro from {dspNow:F2} to {(dspNow + introDurSec):F2}");
        }
        else
        {
            GoToCombateDirect(dspNow);
        }

        // Aún no programamos la siguiente sección en este frame
        nextSectionScheduled = false;
    }

    private void GoToCombateDirect(double dspNow)
    {
        currentState = MusicState.Combate;

        double secPerBeat = 60.0 / BPM;
        double durSec = beatsPerSegment * secPerBeat;

        AudioClip cClip = GetRandomClip(combateClips);
        if (cClip == null)
        {
            Debug.LogWarning("[AudioController] No hay clips de Combate => no se reproduce nada.");
            return;
        }

        PlaySection(cClip, dspNow, durSec, "Combate(Direct)");
        currentSectionEndDSP = dspNow + durSec;

        Debug.Log($"[AudioController] GoToCombateDirect => dspNow={dspNow:F2}, endDSP={currentSectionEndDSP:F2}");

        nextSectionScheduled = false;

        // Invocar evento de Combate
        onCombateStart?.Invoke();
    }

    private void GoToDefensaDirect(double dspNow)
    {
        currentState = MusicState.Defensa;

        double secPerBeat = 60.0 / BPM;
        double durSec = beatsPerSegment * secPerBeat;

        AudioClip dClip = GetRandomClip(defensaClips);
        if (dClip == null)
        {
            Debug.LogWarning("[AudioController] No hay clips de Defensa => no se reproduce nada.");
            return;
        }

        PlaySection(dClip, dspNow, durSec, "Defensa(Direct)");
        currentSectionEndDSP = dspNow + durSec;

        Debug.Log($"[AudioController] GoToDefensaDirect => dspNow={dspNow:F2}, endDSP={currentSectionEndDSP:F2}");

        nextSectionScheduled = false;

        // Invocar evento de Defensa
        onDefensaStart?.Invoke();
    }

    // ----------------------------------------------------------------------
    //                      SCHEDULE NEXT SECTION
    // ----------------------------------------------------------------------
    private void ScheduleNextSection()
    {
        double dspNow = AudioSettings.dspTime;
        Debug.Log($"[AudioController] ScheduleNextSection => currentState={currentState}, dspNow={dspNow:F2}, endDSP={currentSectionEndDSP:F2}");

        // Evitamos programar 2 veces en un solo frame
        nextSectionScheduled = true;

        switch (currentState)
        {
            case MusicState.Intro:
                Debug.Log("[AudioController] => Intro Finished => GoToState(Combate)");
                GoToState(MusicState.Combate);
                break;

            case MusicState.Combate:
                Debug.Log("[AudioController] => Combate Finished => GoToState(Defensa)");
                GoToState(MusicState.Defensa);
                break;

            case MusicState.Defensa:
                Debug.Log("[AudioController] => Defensa Finished => GoToState(Combate)");
                GoToState(MusicState.Combate);
                break;

            case MusicState.Transicion:
                Debug.Log("[AudioController] => Transicion Finished => la corrutina se encargará de programar el main segment");
                // No hacemos nada más aquí. La corrutina WaitAndThen se encargará.
                break;
        }

        // Este es el final de la función
        // nextSectionScheduled se pondrá a false en DirectSegment o WaitAndThen cuando la sección principal comience
        // (Para no quedarnos atascados en true)
    }

    /// <summary>
    /// Pasa del estado actual a 'next' (Combate o Defensa).
    /// Si hay transiciones, se reproduce primero.
    /// </summary>
    private void GoToState(MusicState next)
    {
        double dspNow = AudioSettings.dspTime;
        double secPerBeat = 60.0 / BPM;

        Debug.Log($"[AudioController] GoToState => from {currentState} to {next}, dspNow={dspNow:F2}");

        bool hasTrans = (transicionClips != null && transicionClips.Count > 0);
        if (hasTrans)
        {
            AudioClip tClip = GetRandomClip(transicionClips);
            if (tClip != null)
            {
                currentState = MusicState.Transicion;
                double tDurSec = beatsTransicion * secPerBeat;
                Debug.Log($"[AudioController] Transicion => dspNow={dspNow:F2}, durSec={tDurSec:F2}, clip='{tClip.name}'");

                PlaySection(tClip, dspNow, tDurSec, "Transicion");
                currentSectionEndDSP = dspNow + tDurSec;

                // Programar la sección principal tras la transición con una corrutina
                StartCoroutine(WaitAndThenEndOfTransition(currentSectionEndDSP, next));
                return;
            }
        }

        // Si no hay transiciones o clip nulo => directo
        DirectSegment(next, dspNow);
    }

    // Esperamos a que la transición termine, luego programamos la siguiente sección
    private IEnumerator WaitAndThenEndOfTransition(double endDSP, MusicState next)
    {
        double dspNowNow = AudioSettings.dspTime;
        float waitSec = (float)(endDSP - dspNowNow);
        if (waitSec > 0f)
        {
            Debug.Log($"[AudioController] WaitAndThenEndOfTransition => Esperamos {waitSec:F2} s para transicion");
            yield return new WaitForSecondsRealtime(waitSec);
        }

        double dspAfterWait = AudioSettings.dspTime;
        Debug.Log($"[AudioController] WaitAndThenEndOfTransition => dspNow={dspAfterWait:F2}, going to {next}");

        // Llamamos a DirectSegment => así pasa a Combate o Defensa
        DirectSegment(next, dspAfterWait);
    }

    private void DirectSegment(MusicState next, double dspNow)
    {
        currentState = next;
        double secPerBeat = 60.0 / BPM;
        double durSec = beatsPerSegment * secPerBeat;

        AudioClip mainClip = (next == MusicState.Combate)
            ? GetRandomClip(combateClips)
            : GetRandomClip(defensaClips);

        if (mainClip == null)
        {
            Debug.LogWarning($"[AudioController] No hay clips en {next} => Se detiene.");
            return;
        }

        Debug.Log($"[AudioController] DirectSegment => {next}, dspNow={dspNow:F2}, dur={durSec:F2}, clip='{mainClip.name}'");
        PlaySection(mainClip, dspNow, durSec, next.ToString());

        currentSectionEndDSP = dspNow + durSec;

        // Reiniciamos nextSectionScheduled para que se pueda programar la siguiente
        nextSectionScheduled = false;

        Debug.Log($"[AudioController] DirectSegment => done scheduling {next}, new endDSP={currentSectionEndDSP:F2}");

        // Invocar eventos correspondientes
        if (next == MusicState.Combate) onCombateStart?.Invoke();
        else if (next == MusicState.Defensa) onDefensaStart?.Invoke();
    }

    private void PlaySection(AudioClip clip, double dspStart, double durationSec, string sectionName)
    {
        if (clip == null)
        {
            Debug.LogWarning($"[AudioController] PlaySection => clip nulo, section={sectionName}, no se reproduce nada.");
            return;
        }

        // Elige "otro" audioSource
        AudioSource nextSrc = (lastUsedSourceA) ? sourceB : sourceA;
        lastUsedSourceA = !lastUsedSourceA;

        // Limpia la fuente opuesta
        AudioSource other = (nextSrc == sourceA) ? sourceB : sourceA;
        CleanupSource(other);

        // Programamos
        nextSrc.Stop();
        nextSrc.clip = clip;
        nextSrc.volume = volume;

        double endDSP = dspStart + durationSec;
        nextSrc.PlayScheduled(dspStart);
        nextSrc.SetScheduledEndTime(endDSP);

        Debug.Log($"[AudioController] PlaySection => Section='{sectionName}', clip='{clip.name}', scheduling from {dspStart:F2} to {endDSP:F2} on {(nextSrc == sourceA ? "sourceA" : "sourceB")}");
    }

    private void CleanupSource(AudioSource src)
    {
        if (src == null) return;

        Debug.Log($"[AudioController] CleanupSource => {(src == sourceA ? "sourceA" : "sourceB")}, Stop & clip=null");
        src.Stop();
        src.clip = null;
    }

    private AudioClip GetRandomClip(List<AudioClip> clips)
    {
        if (clips == null || clips.Count == 0)
        {
            Debug.LogWarning("[AudioController] GetRandomClip => lista vacía");
            return null;
        }

        int idx = Random.Range(0, clips.Count);
        return clips[idx];
    }
}
