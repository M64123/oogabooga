// QTETrigger.cs
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class QTETrigger
{
    [Tooltip("Número de beats después del inicio o del último QTE.")]
    public float beatsAfterLastTrigger = 1f; // Permitir fracciones como 0.5

    [Tooltip("Tipo de QTE a activar: Izquierda, Derecha o Ambos.")]
    public QTEType qteType = QTEType.Both;

    [Tooltip("Prefab personalizado para el indicador izquierdo (opcional).")]
    public GameObject customLeftPrefab;

    [Tooltip("Prefab personalizado para el indicador derecho (opcional).")]
    public GameObject customRightPrefab;

    [Tooltip("Evento a invocar cuando este QTE es activado.")]
    public UnityEvent onQTEActivated;
}
