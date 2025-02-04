using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class LaneMapping
{
    [Header("ID de la lane (solo informativo)")]
    public int laneID;

    [Header("RectTransform donde se instancian los QTE")]
    public RectTransform containerRect;

    [Header("(midiNote -> QTETypeDefinition)")]
    public List<GHQTETypeMapping> noteMappings = new List<GHQTETypeMapping>();

    public bool ContainsNote(int midiNote)
    {
        foreach (var m in noteMappings)
        {
            if (m.midiNote == midiNote)
                return true;
        }
        return false;
    }

    public GHQTETypeMapping FindQTEType(int midiNote)
    {
        foreach (var m in noteMappings)
        {
            if (m.midiNote == midiNote)
                return m;
        }
        return null;
    }
}

/// <summary>
/// GHQTETypeMapping => asocia un midiNote con un QTETypeDefinition + overrideKey opcional
/// </summary>
[System.Serializable]
public class GHQTETypeMapping
{
    public int midiNote;
    public QTETypeDefinition qteType;
    public KeyCode overrideKey = KeyCode.None;
}
