using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

/// <summary>
/// Parse de MIDI (Melanchall) y genera QTEIndicators
/// asociando cada nota a un LaneMapping.
/// </summary>
public class GuitarHeroMIDIManager : MonoBehaviour
{
    [Header("Lane Mappings (para cada lane)")]
    [Tooltip("Cada LaneMapping tiene un containerRect para instanciar QTEs en la UI, " +
             "y un array de (midiNote -> QTETypeDefinition).")]
    public List<LaneMapping> laneMappings = new List<LaneMapping>();

    [Header("LeadTime (segundos)")]
    public float leadTime = 2f;

    [Header("Margen de 'perfect'")]
    public float perfectMargin = 0.1f;

    [Header("Omitir notas si spawnTime < dspNow?")]
    public bool skipLateNotes = true;

    /// <summary>
    /// Llamada principal: parsea el MidiAsset y spawnea QTEs en el rango dspStart...dspEnd
    /// </summary>
    public void ParseAndGenerateQTEs(MidiAsset midiAsset, double dspStart, double dspEnd)
    {
        if (midiAsset == null)
        {
            Debug.LogWarning("[GuitarHeroMIDIManager] MidiAsset es nulo => no QTE.");
            return;
        }

        Debug.Log($"[GuitarHeroMIDIManager] ParseAndGenerateQTEs => dspRange=({dspStart:F2}-{dspEnd:F2})");

        byte[] data = midiAsset.MidiBytes;
        if (data == null || data.Length == 0)
        {
            Debug.LogWarning("[GuitarHeroMIDIManager] MidiAsset vacío => 0 QTE.");
            return;
        }

        MidiFile midiFile;
        try
        {
            using (var mem = new MemoryStream(data))
            {
                midiFile = MidiFile.Read(mem);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[GuitarHeroMIDIManager] Error parseando MIDI => {e.Message}");
            return;
        }

        var tempoMap = midiFile.GetTempoMap();
        var notes = midiFile.GetNotes();

        double dspNow = AudioSettings.dspTime;
        int countSpawned = 0;

        // Recorremos las notas del MIDI
        foreach (var note in notes)
        {
            var metricTime = note.TimeAs<MetricTimeSpan>(tempoMap);
            double noteSec = metricTime.TotalSeconds;

            double dspEvent = dspStart + noteSec;
            if (dspEvent < dspStart || dspEvent > dspEnd)
                continue;

            double spawnTime = dspEvent - leadTime;
            if (skipLateNotes && spawnTime < dspNow)
                continue;

            int midiNote = note.NoteNumber;

            // Buscar Lane
            LaneMapping lane = FindLaneForNote(midiNote);
            if (lane == null)
                continue;

            // Buscar QTETypeDefinition
            GHQTETypeMapping typeMap = lane.FindQTEType(midiNote);
            if (typeMap == null || typeMap.qteType == null)
                continue;

            KeyCode finalKey = (typeMap.overrideKey != KeyCode.None)
                ? typeMap.overrideKey
                : typeMap.qteType.defaultKey;

            // Creamos GHQTEData
            GHQTEData qteData = new GHQTEData
            {
                laneID = lane.laneID,
                note = midiNote,
                key = finalKey,
                perfectTime = (float)dspEvent,
                qteType = typeMap.qteType
            };

            // Instanciamos el prefab via GHQTEManager
            SpawnIndicatorViaManager(lane, qteData);

            countSpawned++;
        }

        Debug.Log($"[GuitarHeroMIDIManager] => Generados {countSpawned} QTEs.");
    }

    /// <summary>
    /// Llama al GHQTEManager para instanciar el QTEIndicator.
    /// </summary>
    private void SpawnIndicatorViaManager(LaneMapping lane, GHQTEData data)
    {
        if (!lane.containerRect)
        {
            Debug.LogWarning("[GuitarHeroMIDIManager] Lane sin containerRect => no QTE");
            return;
        }

        // GHQTEManager usa su método SpawnQTE(...)
        GHQTEManager.Instance.SpawnQTE(data, lane.containerRect);
    }

    private LaneMapping FindLaneForNote(int midiNote)
    {
        foreach (var lane in laneMappings)
        {
            if (lane.ContainsNote(midiNote))
                return lane;
        }
        return null;
    }
}
