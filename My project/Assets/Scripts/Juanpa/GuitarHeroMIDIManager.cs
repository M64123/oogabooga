using UnityEngine;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System.IO;

public class GuitarHeroMIDIManager : MonoBehaviour
{
    [Header("QTE Manager")]
    public GHQTEManager qteManager;

    [Header("Lane Mappings")]
    public List<LaneMapping> laneMappings;

    [Header("LeadTime (segundos)")]
    public float leadTime = 2f;

    [Header("Omitir notas si spawnTime < dspNow?")]
    public bool skipLateNotes = true;

    public void ParseAndGenerateQTEs(MidiAsset midiAsset, double dspStart, double dspEnd)
    {
        if (midiAsset == null)
        {
            Debug.LogWarning("[GuitarHeroMIDIManager] midiAsset nulo => no QTE.");
            return;
        }

        Debug.Log($"[GuitarHeroMIDIManager] ParseAndGenerateQTEs => dspRange=({dspStart:F2}-{dspEnd:F2})");

        byte[] data = midiAsset.MidiBytes;
        if (data == null || data.Length == 0)
        {
            Debug.LogWarning("[GuitarHeroMIDIManager] MidiAsset vacío => 0 QTE");
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
        List<GHQTEData> qteList = new List<GHQTEData>();

        double dspNow = AudioSettings.dspTime;

        foreach (var note in notes)
        {
            var metricTime = note.TimeAs<MetricTimeSpan>(tempoMap);
            double noteSec = metricTime.TotalSeconds;

            double dspEvent = dspStart + noteSec;
            if (dspEvent < dspStart || dspEvent > dspEnd)
                continue; // fuera de la sección

            double spawnTime = dspEvent - leadTime;
            if (skipLateNotes && spawnTime < dspNow)
                continue; // no queremos spawnear QTE a medias

            // Buscar Lane
            int midiNote = note.NoteNumber;
            LaneMapping laneMap = FindLaneForNote(midiNote);
            if (laneMap != null)
            {
                // buscar prefabMap
                GHNotePrefabMapping pm = laneMap.FindPrefabForNote(midiNote);
                if (pm != null)
                {
                    GHQTEData qte = new GHQTEData
                    {
                        laneID = laneMap.laneID,
                        note = midiNote,
                        key = pm.key,
                        prefab = pm.prefab,
                        perfectTime = (float)dspEvent
                    };
                    qteList.Add(qte);
                }
            }
        }

        qteManager.StartQTEManager();
        qteManager.AddQTEs(qteList);

        Debug.Log($"[GuitarHeroMIDIManager] => Generados {qteList.Count} QTEs.");
    }

    private LaneMapping FindLaneForNote(int midiNote)
    {
        foreach (var lane in laneMappings)
        {
            if (lane.HasNote(midiNote)) return lane;
        }
        return null;
    }
}
