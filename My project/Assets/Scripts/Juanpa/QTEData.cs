using UnityEngine;

/// <summary>
/// Datos concretos de un QTE en curso:
/// - laneID
/// - note (midiNote)
/// - key que hay que pulsar
/// - tiempo dsp en el que hay que acertar
/// - referencia al QTETypeDefinition (para invocar su acción si aciertas)
/// </summary>
public class QTEData
{
    public int laneID;
    public int note;
    public KeyCode key;
    public float perfectTime;
    public QTETypeDefinition typeDefinition;
}
