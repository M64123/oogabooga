using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "QTETypeDefinition", menuName = "QTE/Tipo de QTE")]
public class QTETypeDefinition : ScriptableObject
{
    [Header("Nombre a mostrar (para evitar error 'displayName'")]
    public string displayName = "Unnamed QTE";

    [Header("Prefab del QTE en HUD")]
    public GameObject prefab;

    [Header("Tecla por defecto")]
    public KeyCode defaultKey = KeyCode.Space;

    [Header("Al acertar (opcional)")]
    public UnityEvent onSuccess;
}
