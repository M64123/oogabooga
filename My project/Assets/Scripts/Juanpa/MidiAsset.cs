using UnityEngine;

/// <summary>
/// Representa un ScriptableObject que almacena los bytes crudos de un archivo .mid.
/// Unity lo creará al importar el .mid con el ScriptedImporter.
/// </summary>
public class MidiAsset : ScriptableObject
{
    [SerializeField, HideInInspector]
    private byte[] midiBytes;

    public byte[] MidiBytes => midiBytes;

    public void SetData(byte[] data)
    {
        midiBytes = data;
    }
}
