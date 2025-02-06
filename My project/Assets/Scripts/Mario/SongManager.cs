using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System.IO;
using UnityEngine.Networking;
using System;

public class SongManager : MonoBehaviour
{
    public static SongManager Instance;
    public AudioSource audioSource;
    public Lane[] lanes;
    public float songDelayInSeconds;
    public double marginOfError; // en segundos

    public int inputDelayInMilliseconds;
    public string fileLocation;
    public float noteTime;
    public float noteSpawnY;
    public float noteTapY;
    public float noteDespawnY => noteTapY - (noteSpawnY - noteTapY);

    public static MidiFile midiFile;
    private float songDuration; // duración de la canción en segundos
    public float midiOutputDelay = 0.0f; // Retraso en segundos para la salida del MIDI
    private void Start()
    {
        Instance = this;
        audioSource.loop = false; // Controlamos el loop manualmente

        if (Application.streamingAssetsPath.StartsWith("http://") || Application.streamingAssetsPath.StartsWith("https://"))
        {
            StartCoroutine(ReadFromWebsite());
        }
        else
        {
            ReadFromFile();
        }
    }

    private IEnumerator ReadFromWebsite()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(Application.streamingAssetsPath + "/" + fileLocation))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogError(www.error);
            }
            else
            {
                byte[] results = www.downloadHandler.data;
                using (var stream = new MemoryStream(results))
                {
                    midiFile = MidiFile.Read(stream);
                    GetDataFromMidi();
                }
            }
        }
    }

    private void ReadFromFile()
    {
        midiFile = MidiFile.Read(Application.streamingAssetsPath + "/" + fileLocation);
        GetDataFromMidi();
    }

    public void GetDataFromMidi()
    {
        var notes = midiFile.GetNotes();
        var array = new Melanchall.DryWetMidi.Interaction.Note[notes.Count];
        notes.CopyTo(array, 0);

        foreach (var lane in lanes)
        {
            lane.SetTimeStamps(array);
        }

        songDuration = audioSource.clip.length; // Guarda la duración de la canción
        Invoke(nameof(StartSong), songDelayInSeconds);
    }

    public void StartSong()
    {
        audioSource.loop = false;
        audioSource.Play();
        Invoke(nameof(LoopSongAndMidi), audioSource.clip.length); // Programar loop al finalizar la canción
    }

    private void LoopSongAndMidi()
    {
        Debug.Log("[SongManager] Reiniciando la canción y el MIDI...");

        audioSource.Stop();

        // Para cada lane reiniciamos el sistema de spawn de notas del loop actual.
        // De esta forma, las notas pendientes de loops anteriores (que no se borran) quedan en escena,
        // mientras que se crean nuevas notas para el nuevo loop.
        foreach (var lane in lanes)
        {
            lane.StartNewLoop();
        }

        audioSource.Play();
        Invoke(nameof(LoopSongAndMidi), audioSource.clip.length);
    }

    public static double GetAudioSourceTime()
    {
        return (double)Instance.audioSource.timeSamples / Instance.audioSource.clip.frequency;
    }
}