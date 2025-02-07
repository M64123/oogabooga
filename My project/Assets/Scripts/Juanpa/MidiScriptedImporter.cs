using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using System.IO;

/// <summary>
/// ScriptedImporter para archivos con extensión .mid.
/// Con esto, Unity creará un MidiAsset que contiene los bytes del archivo.
/// </summary>
[ScriptedImporter(1, "mid")]
public class MidiScriptedImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        // Leer bytes del archivo
        byte[] data = File.ReadAllBytes(ctx.assetPath);

        // Crear un MidiAsset
        MidiAsset midiAsset = ScriptableObject.CreateInstance<MidiAsset>();
        midiAsset.SetData(data);

        // Registrarlo como sub-asset y marcarlo principal
        ctx.AddObjectToAsset("MidiAsset", midiAsset);
        ctx.SetMainObject(midiAsset);
    }
}
