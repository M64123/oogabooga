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
    public AudioSource audioSource;
    public Lane[] lanes;

    [Header("Timing Config")]
    public float songDelayInSeconds;
    public double marginOfError; // in seconds
    public int inputDelayInMilliseconds;

    [Header("Note Timing")]
    public float noteTime;
    public float noteSpawnY;
    public float noteTapY;
    public float noteDespawnY
    {
        get
        {
            return noteTapY - (noteSpawnY - noteTapY);
        }
    }

    [Header("File Locations")]
    public string fileLocation; // Archivo MIDI genérico (opcional)
    public string combateMidiFile = "Combate.mid"; // Archivo MIDI para Combate
    public string defensaMidiFile = "Defensa.mid"; // Archivo MIDI para Defensa

    public static MidiFile midiFile;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;

        // Suscripción a los eventos del AudioController
        AudioController audioController = FindObjectOfType<AudioController>();
        if (audioController != null)
        {
            audioController.onCombateStart.AddListener(() => LoadAndPlayMidi(combateMidiFile));
            audioController.onDefensaStart.AddListener(() => LoadAndPlayMidi(defensaMidiFile));
        }

        // Cargar el archivo MIDI inicial
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

            if (www.result != UnityWebRequest.Result.Success)
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
        string filePath = Application.streamingAssetsPath + "/" + fileLocation;
        if (File.Exists(filePath))
        {
            midiFile = MidiFile.Read(filePath);
            GetDataFromMidi();
        }
        else
        {
            Debug.LogError($"[SongManager] El archivo MIDI '{filePath}' no existe.");
        }
    }

    private void LoadAndPlayMidi(string midiFileName)
    {
        string filePath = Application.streamingAssetsPath + "/" + midiFileName;
        if (!File.Exists(filePath))
        {
            Debug.LogError($"[SongManager] El archivo MIDI '{filePath}' no existe.");
            return;
        }

        // Cargar el archivo MIDI
        midiFile = MidiFile.Read(filePath);
        Debug.Log($"[SongManager] MIDI cargado: {midiFileName}");

        // Procesar las notas y reproducir la canción
        GetDataFromMidi();
        Invoke(nameof(StartSong), songDelayInSeconds);
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
    }

    public void StartSong()
    {
        audioSource.Play();
    }

    public static double GetAudioSourceTime()
    {
        return (double)Instance.audioSource.timeSamples / Instance.audioSource.clip.frequency;
    }

    void Update()
    {
        // Aquí puedes agregar lógica adicional si es necesario
    }
}