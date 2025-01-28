using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lane : MonoBehaviour
{
    public Melanchall.DryWetMidi.MusicTheory.NoteName noteRestriction;
    public KeyCode input;
    public GameObject notePrefab;
    private List<Note> notes = new List<Note>();
    public List<double> timeStamps = new List<double>();

    private int spawnIndex = 0;
    private int inputIndex = 0;

    public void SetTimeStamps(Melanchall.DryWetMidi.Interaction.Note[] array)
    {
        if (array == null || array.Length == 0)
        {
            Debug.LogWarning("[Lane] No hay notas compatibles en el archivo MIDI.");
            return;
        }

        foreach (var note in array)
        {
            if (note.NoteName == noteRestriction)
            {
                var metricTimeSpan = TimeConverter.ConvertTo<MetricTimeSpan>(
                    note.Time,
                    SongManager.midiFile.GetTempoMap() // Cambiado de CurrentMidiFile a midiFile
                );

                timeStamps.Add((double)metricTimeSpan.Minutes * 60f +
                               metricTimeSpan.Seconds +
                               (double)metricTimeSpan.Milliseconds / 1000f);
            }
        }

        if (timeStamps.Count == 0)
        {
            Debug.LogWarning("[Lane] No se encontraron notas que coincidan con las restricciones.");
        }
    }
    public void ClearNotes()
    {
        foreach (var note in notes)
        {
            if (note != null)
            {
                Destroy(note.gameObject); // Elimina la nota visualmente
            }
        }
        notes.Clear(); // Limpia la lista de notas
        timeStamps.Clear(); // Limpia los timestamps de la pista anterior
        spawnIndex = 0; // Reinicia el índice de generación
        inputIndex = 0; // Reinicia el índice de entrada
        Debug.Log("[Lane] Notas y estados reiniciados.");
    }

    private void Update()
    {
        if (spawnIndex < timeStamps.Count)
        {
            if (SongManager.GetAudioSourceTime() >= timeStamps[spawnIndex] - SongManager.Instance.noteTime)
            {
                var note = Instantiate(notePrefab, transform);
                notes.Add(note.GetComponent<Note>());
                note.GetComponent<Note>().assignedTime = (float)timeStamps[spawnIndex];
                spawnIndex++;
            }
        }

        if (inputIndex < timeStamps.Count)
        {
            double timeStamp = timeStamps[inputIndex];
            double marginOfError = SongManager.Instance.marginOfError;
            double audioTime = SongManager.GetAudioSourceTime() - (SongManager.Instance.inputDelayInMilliseconds / 1000.0);

            if (Input.GetKeyDown(input))
            {
                if (Math.Abs(audioTime - timeStamp) < marginOfError)
                {
                    Hit();
                    Debug.Log($"Hit on {inputIndex} note");
                    Destroy(notes[inputIndex].gameObject);
                    inputIndex++;
                }
                else
                {
                    Debug.Log($"Hit inaccurate on {inputIndex} note with {Math.Abs(audioTime - timeStamp)} delay");
                }
            }
            if (timeStamp + marginOfError <= audioTime)
            {
                Miss();
                Debug.Log($"Missed {inputIndex} note");
                inputIndex++;
            }
        }
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