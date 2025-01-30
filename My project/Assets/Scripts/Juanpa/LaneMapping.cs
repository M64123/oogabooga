using UnityEngine;

[System.Serializable]
public class LaneMapping
{
    [Tooltip("Identificador de la lane (0,1,2,...).")]
    public int laneID;

    [Tooltip("Tecla para esta lane (ej: KeyCode.F).")]
    public KeyCode laneKey;

    [Tooltip("Notas MIDI que irán por esta lane.")]
    public int[] midiNotes;
}
