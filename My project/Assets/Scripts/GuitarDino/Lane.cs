using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Lane : MonoBehaviour
{
    public Melanchall.DryWetMidi.MusicTheory.NoteName noteRestriction;
    public KeyCode input;
    public GameObject notePrefab;

    // Lista que contiene las notas generadas en el loop actual.
    private List<Note> currentNotes = new List<Note>();

    // Los timeStamps se obtienen del MIDI y se reutilizan en cada loop.
    public List<double> timeStamps = new List<double>();

    private int spawnIndex = 0;
    private int inputIndex = 0;

    void Start()
    {
        // Se puede inicializar aquí si se requiere.
    }

    public void SetTimeStamps(Melanchall.DryWetMidi.Interaction.Note[] array)
    {
        // Se limpia la lista para evitar duplicados entre loops
        timeStamps.Clear();
        foreach (var note in array)
        {
            if (note.NoteName == noteRestriction)
            {
                var metricTimeSpan = TimeConverter.ConvertTo<MetricTimeSpan>(note.Time, SongManager.midiFile.GetTempoMap());
                double newTimeStamp = metricTimeSpan.Minutes * 60 + metricTimeSpan.Seconds + metricTimeSpan.Milliseconds / 1000.0;
                if (!timeStamps.Contains(newTimeStamp))
                {
                    timeStamps.Add(newTimeStamp);
                }
            }
        }
    }

    void Update()
    {
        // Spawneo de nuevas notas para el loop actual.
        if (spawnIndex < timeStamps.Count)
        {
            if (SongManager.GetAudioSourceTime() >= timeStamps[spawnIndex] + SongManager.Instance.midiOutputDelay - SongManager.Instance.noteTime)
            {
                var noteObj = Instantiate(notePrefab, transform);
                var noteComp = noteObj.GetComponent<Note>();
                // Sumar midiOutputDelay al timestamp para que el input detection se alinee
                noteComp.assignedTime = (float)(timeStamps[spawnIndex] + SongManager.Instance.midiOutputDelay);
                currentNotes.Add(noteComp);
                spawnIndex++;
            }
        }

        // Detección de input para las notas del loop actual.
        // Detección de input para las notas del loop actual.
        if (inputIndex < timeStamps.Count)
        {
            double timestamp = timeStamps[inputIndex];
            double marginOfError = SongManager.Instance.marginOfError;
            double audioTime = SongManager.GetAudioSourceTime() - (SongManager.Instance.inputDelayInMilliseconds / 1000.0);

            // Calculamos el timestamp ajustado, sumando el retraso, para que coincida con el spawn.
            double adjustedTimestamp = timestamp + SongManager.Instance.midiOutputDelay;

            if (Input.GetKeyDown(input))
            {
                if (Math.Abs(audioTime - adjustedTimestamp) < marginOfError)
                {
                    Hit();
                    Debug.Log($"Hit on {inputIndex} note");
                    if (inputIndex < currentNotes.Count && currentNotes[inputIndex] != null)
                    {
                        Destroy(currentNotes[inputIndex].gameObject);
                    }
                    inputIndex++; // Se avanza solo si se acierta.
                }
                else
                {
                    Debug.Log($"Hit inaccurate on {inputIndex} note with {Math.Abs(audioTime - adjustedTimestamp)} delay");
                }
            }
            // Usamos el adjustedTimestamp también en la comprobación de fallo (miss)
            else if (adjustedTimestamp + marginOfError <= audioTime)
            {
                Miss();
                Debug.Log($"Missed {inputIndex} note");
                inputIndex++;
            }
        }
    }

    /// <summary>
    /// Al comenzar un nuevo loop, se reinician los índices y la lista de notas del loop actual,
    /// de modo que se puedan instanciar nuevas notas para la nueva ejecución de la pista.
    /// Las notas generadas en loops anteriores quedan en escena (para que no desaparezcan de golpe)
    /// pero no se tienen en cuenta para la detección de input.
    /// </summary>
    public void StartNewLoop()
    {
        spawnIndex = 0;
        inputIndex = 0;
        currentNotes = new List<Note>();
    }

    private void Hit()
    {
        ScoreManager.Hit();
    }

    private void Miss()
    {
        ScoreManager.Miss();
    }
}
