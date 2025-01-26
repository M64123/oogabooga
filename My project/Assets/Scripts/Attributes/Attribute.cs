using UnityEngine;

/// <summary>
/// Los atributos dotan de soporte a la capacidad de combate de un personaje, tanto para atacar como para defenderse.
/// </summary>
[CreateAssetMenu]
public class Attribute : ScriptableObject
{
    /// <summary>
    /// Identificador numérico del recurso
    /// </summary>
    public int iD;

    /// <summary>
    /// Nombre del atributo
    /// </summary>
    public new string name;

}
