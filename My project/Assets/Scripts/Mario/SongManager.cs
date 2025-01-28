using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System.IO;
using UnityEngine.Networking;

public class SongManager : MonoBehaviour
{
    public static SongManager Instance;
    private float GetTiempoPorSección()
    {
        // Calcula la duración de la sección (en segundos)
        return (60f / bpm) * beatsPorCompas * compasesPorSección;
    }
    [Header("Duración del Cambio de Pista")]
    public float bpm = 120f; // Ajusta el BPM de la canción
    public int beatsPorCompas = 4; // Número de beats por compás (4/4 = 4 beats)
    public int compasesPorSección = 8; // Número de compases por sección
    [Header("Audio Configuración")]
    public AudioSource audioSource; // Reproducción de audio
    public Lane[] lanes; // Lanes que reciben las notas
    public float songDelayInSeconds;

    [Header("Errores de Sincronización")]
    public double marginOfError; // En segundos
    public int inputDelayInMilliseconds;

    [Header("Notas")]
    public float noteTime;
    public float noteSpawnY;
    public float noteTapY;
    public float noteDespawnY => noteTapY - (noteSpawnY - noteTapY);

    [Header("Pistas")]
    public List<AudioTrack> tracks; // Lista de pistas (audio + MIDI)
    private int currentTrackIndex = 0; // Índice de la pista actual
    public static MidiFile midiFile; // Archivo MIDI actual
    

    private void Start()
    {
        Instance = this;

        if (tracks.Count > 0)
        {
            StartTrack(); // Inicia con la primera pista
        }
    }

    private void Update()
    {
        // Cambiar de pista cuando termine la actual
        if (!audioSource.isPlaying && tracks.Count > 1)
        {
            NextTrack();
        }
    }

    private void StartTrack()
    {
        if (currentTrackIndex < 0 || currentTrackIndex >= tracks.Count)
        {
            Debug.LogError("[SongManager] Índice de pista inválido.");
            return;
        }

        // Limpieza completa de notas y estado
        ResetNotes();

        AudioTrack track = tracks[currentTrackIndex];

        // Configurar el audio
        audioSource.Stop();
        audioSource.clip = track.audioClip;
        audioSource.Play();

        // Leer el archivo MIDI asociado
        LoadMidiFile(track.midiFileName);

        Debug.Log($"[SongManager] Reproduciendo pista: {track.audioClip.name} con MIDI: {track.midiFileName}");

        // Programar el cambio de pista
        float tiempoPorSección = GetTiempoPorSección();
        Invoke(nameof(NextTrack), tiempoPorSección); // Cambiar pista después de la duración calculada
    }

    // Método para limpiar notas y reiniciar estado
    private void ResetNotes()
    {
        // Limpia todas las notas en pantalla
        foreach (var lane in lanes)
        {
            lane.ClearNotes();
        }
        Debug.Log("[SongManager] Notas limpiadas al cambiar de pista.");
    }

    private void NextTrack()
    {
        currentTrackIndex = (currentTrackIndex + 1) % tracks.Count;
        StartTrack();
    }

    private void LoadMidiFile(string midiFileName)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, midiFileName);

        if (!File.Exists(filePath))
        {
            Debug.LogError($"[SongManager] El archivo MIDI '{filePath}' no existe.");
            return;
        }

        try
        {
            midiFile = MidiFile.Read(filePath);
            Debug.Log($"[SongManager] Archivo MIDI cargado correctamente: {midiFileName}");
            GetDataFromMidi();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SongManager] Error al leer el archivo MIDI '{filePath}': {ex.Message}");
        }
    }

    private void GetDataFromMidi()
    {
        if (midiFile == null)
        {
            Debug.LogError("[SongManager] No hay un archivo MIDI cargado para procesar.");
            return;
        }

        var notes = midiFile.GetNotes();
        var array = new Melanchall.DryWetMidi.Interaction.Note[notes.Count];
        notes.CopyTo(array, 0);

        foreach (var lane in lanes)
        {
            lane.SetTimeStamps(array);
        }

        // Sincroniza el inicio del audio con la generación de notas
        audioSource.PlayDelayed(songDelayInSeconds);
    }

    public static double GetAudioSourceTime()
    {
        return (double)Instance.audioSource.timeSamples / Instance.audioSource.clip.frequency;
    }
}

[System.Serializable]
public class AudioTrack
{
    public AudioClip audioClip; // El clip de audio
    public string midiFileName; // El archivo MIDI asociado
}