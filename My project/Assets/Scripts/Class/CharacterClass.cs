using UnityEngine;

/// <summary>
/// Define la clase de un personaje.
/// Tiene slots para los atributos, recursos y habilidades
/// </summary>
[CreateAssetMenu]
public class CharacterClass : ScriptableObject
{
    /// <summary>
    /// Identificador numérico 
    /// </summary>
    public int iD;

    /// <summary>
    /// Nombre de la clase
    /// </summary>
    public new string name;

    /// <summary>
    /// Lista de atributos
    /// </summary>
    public AttributeProgression[] attributes;

    /// <summary>    
    /// Lista de recursos
    /// </summary>
    public Resource[] resources;

    /// <summary>
    /// Array de habilidades
    /// </summary>
    public Hability[] habilities;

}
