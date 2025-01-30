using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

public class TaikoMIDIManager : MonoBehaviour
{
    public TaikoQTEManager qteManager;
    public NoteMapping[] noteMappings;
    public float leadTime = 2f; // si lo usas

    public void ParseAndGenerateQTEs(MidiAsset midiAsset, double dspStart, double dspEnd, bool isIntro)
    {
        if (midiAsset == null)
        {
            Debug.LogWarning("[TaikoMIDIManager] midiAsset nulo => no generamos QTE.");
            return;
        }

        Debug.Log($"[TaikoMIDIManager] ParseAndGenerateQTEs => dspRange=({dspStart:F2}-{dspEnd:F2}), isIntro={isIntro}");

        // Leer bytes
        byte[] rawData = midiAsset.MidiBytes;
        if (rawData == null || rawData.Length == 0)
        {
            Debug.LogWarning("[TaikoMIDIManager] MidiAsset sin datos => 0 QTE.");
            return;
        }

        // Parse con DryWetMIDI
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
            Debug.LogWarning($"[TaikoMIDIManager] Error al leer MIDI: {e.Message}");
            return;
        }

        var tempoMap = midiFile.GetTempoMap();
        var notes = midiFile.GetNotes();

        // Convertir notas => QTE
        List<TaikoQTEData> qteList = new List<TaikoQTEData>();

        foreach (var note in notes)
        {
            // tiempo en seg desde inicio del .mid
            var metricTime = note.TimeAs<MetricTimeSpan>(tempoMap);
            double noteSeconds = metricTime.TotalSeconds;

            // dspEvent => dspStart + noteSeconds
            double dspEvent = dspStart + noteSeconds;

            // Filtrar => si dspEvent está en [dspStart, dspEnd]
            if (dspEvent >= dspStart && dspEvent <= dspEnd)
            {
                int midiNote = note.NoteNumber; // 36 = C1, 40 = E1, etc.

                NoteMapping map = FindMapping(midiNote);
                if (map != null)
                {
                    TaikoQTEData qte = new TaikoQTEData
                    {
                        qteType = map.qteType,
                        requiredKeys = map.requiredKeys,
                        perfectTime = (float)dspEvent
                    };
                    qteList.Add(qte);
                }
            }
        }

        // Mandar al QTEManager
        qteManager.StartQTEManager();
        qteManager.AddQTEs(qteList);

        Debug.Log($"[TaikoMIDIManager] => Generados {qteList.Count} QTEs del archivo .mid (isIntro={isIntro}).");
    }

    private NoteMapping FindMapping(int midiNote)
    {
        foreach (var nm in noteMappings)
        {
            if (nm.midiNote == midiNote) return nm;
        }
        return null;
    }
}

// Apoyo
public class MidiEvent
{
    public double time;
    public int note;
}
