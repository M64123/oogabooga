using UnityEngine;

/// <summary>
/// Sirve para mapear un número de nota MIDI a un tipo de QTE (Single o Dual)
/// y las teclas requeridas. Ejemplo:
/// - midiNote=60 => Single => [KeyCode.Space]
/// - midiNote=62 => Dual   => [KeyCode.Z, KeyCode.X]
/// </summary>
[System.Serializable]
public class NoteMapping
{
    [Tooltip("Número de nota MIDI (0-127). Ej: 60 = C4")]
    public int midiNote;

    [Tooltip("Tipo de QTE: Single / Dual")]
    public TaikoQTEType qteType;

    [Tooltip("Teclas requeridas para este QTE. Para Single, usar el primero.")]
    public KeyCode[] requiredKeys;
}
