using UnityEngine;
using System.Linq;
using System.Collections.Generic;

[System.Serializable]
public class LaneMapping
{
    public int laneID;
    public GHNotePrefabMapping[] notePrefabs;

    public bool HasNote(int midiNote)
    {
        return notePrefabs.Any(x => x.midiNote == midiNote);
    }

    public GHNotePrefabMapping FindPrefabForNote(int midiNote)
    {
        return notePrefabs.FirstOrDefault(x => x.midiNote == midiNote);
    }
}

[System.Serializable]
public class GHNotePrefabMapping
{
    public int midiNote;
    public GameObject prefab;
    public KeyCode key;
}
