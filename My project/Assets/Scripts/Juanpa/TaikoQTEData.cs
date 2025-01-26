using UnityEngine;

/// <summary>
/// Representa un QTE (nota) estilo Taiko:
/// - Cuándo (tiempo perfecto en segundos o DSP time),
/// - Tipo (Single o Dual),
/// - Teclas requeridas.
/// </summary>
[System.Serializable]
public class TaikoQTEData
{
    public TaikoQTEType qteType = TaikoQTEType.Single;

    [Tooltip("Momento en el que debe golpearse el QTE, en segundos DSP (o tiempo local).")]
    public float perfectTime = 0f;

    [Tooltip("Teclas requeridas. Para Single, usar el primer elemento. Para Dual, usar 2.")]
    public KeyCode[] requiredKeys;
}
