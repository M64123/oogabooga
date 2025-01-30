using System.Collections.Generic;
using UnityEngine;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System.IO;
using UnityEngine.Rendering.Universal;

public class GuitarHeroMIDIManager : MonoBehaviour
{
    [Header("QTE Manager")]
    public GHQTEManager qteManager;

    [Header("Lane Mappings")]
    public LaneMapping[] laneMappings;

    [Header("LeadTime (para spawnear QTE antes del perfectTime)")]
    public float leadTime = 2f;

    public void ParseAndGenerateQTEs(MidiAsset midiAsset, double dspStart, double dspEnd, bool isIntro)
    {
        if (midiAsset == null)
        {
            Debug.LogWarning("[GuitarHeroMIDIManager] midiAsset nulo => no generamos QTE.");
            return;
        }

        Debug.Log($"[GuitarHeroMIDIManager] ParseAndGenerateQTEs => dspRange=({dspStart:F2}-{dspEnd:F2}), isIntro={isIntro}");

        byte[] rawData = midiAsset.MidiBytes;
        if (rawData == null || rawData.Length == 0)
        {
            Debug.LogWarning("[GuitarHeroMIDIManager] MidiAsset sin datos => 0 QTE.");
            return;
        }

        MidiFile midiFile;
        try
        {
            using (var memStream = new MemoryStream(rawData))
            {
                midiFile = MidiFile.Read(memStream);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[GuitarHeroMIDIManager] Error al leer MIDI: {e.Message}");
            return;
        }

        var tempoMap = midiFile.GetTempoMap();
        var notes = midiFile.GetNotes();

        List<GHQTEData> qteList = new List<GHQTEData>();

        foreach (var note in notes)
        {
            var metricTime = note.TimeAs<MetricTimeSpan>(tempoMap);
            double noteSeconds = metricTime.TotalSeconds;

            double dspEvent = dspStart + noteSeconds;

            if (dspEvent >= dspStart && dspEvent <= dspEnd)
            {
                int midiNote = note.NoteNumber;
                // Buscar la lane
                LaneMapping laneMap = FindLaneForNote(midiNote);
                if (laneMap != null)
                {
                    GHQTEData qte = new GHQTEData
                    {
                        laneID = laneMap.laneID,
                        key = laneMap.laneKey,
                        perfectTime = (float)dspEvent
                    };
                    qteList.Add(qte);
                }
            }
        }

        qteManager.StartQTEManager();
        qteManager.AddQTEs(qteList);

        Debug.Log($"[GuitarHeroMIDIManager] => Generados {qteList.Count} QTEs del archivo .mid (isIntro={isIntro}).");
    }

    private LaneMapping FindLaneForNote(int midiNote)
    {
        // Buscamos un laneMapping que contenga ese note en su array
        foreach (var lm in laneMappings)
        {
            if (lm.midiNotes == null) continue;
            for (int i = 0; i < lm.midiNotes.Length; i++)
            {
                if (lm.midiNotes[i] == midiNote)
                {
                    return lm;
                }
            }
        }
        return null;
    }
}
