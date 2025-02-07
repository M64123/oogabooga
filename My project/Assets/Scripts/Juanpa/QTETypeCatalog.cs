using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Catálogo que agrupa varios QTETypeDefinition en un solo lugar.
/// </summary>
[CreateAssetMenu(fileName = "QTETypeCatalog", menuName = "QTE/Catálogo de QTEs")]
public class QTETypeCatalog : ScriptableObject
{
    [Header("Listado de Tipos de QTE disponibles")]
    public List<QTETypeDefinition> definitions = new List<QTETypeDefinition>();

    /// <summary>
    /// Buscar un QTETypeDefinition por nombre (opcional, si quieres).
    /// </summary>
    public QTETypeDefinition GetDefinitionByName(string displayName)
    {
        foreach (var def in definitions)
        {
            if (def.displayName == displayName)
                return def;
        }
        return null;
    }
}
