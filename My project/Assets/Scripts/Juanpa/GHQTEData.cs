using UnityEngine;

/// <summary>
/// Datos de un QTE instanciado:
/// - laneID, note, key, perfectTime
/// - referencia a QTETypeDefinition (prefab, evento, etc.)
/// </summary>
public class GHQTEData
{
    public int laneID;
    public int note;
    public KeyCode key;
    public float perfectTime;

    public QTETypeDefinition qteType;
}
