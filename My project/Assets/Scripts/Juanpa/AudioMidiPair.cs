using UnityEngine;

/// <summary>
/// Par (AudioClip + MidiAsset) para cada sección (Intro/Combate/Defensa/Transicion).
/// </summary>
[System.Serializable]
public class AudioMidiPair
{
    public AudioClip audioClip;

    [Tooltip("Arrastra aquí el .mid importado como MidiAsset.")]
    public MidiAsset midiFile;
}
