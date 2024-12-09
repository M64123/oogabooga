using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Recursos del personaje consumidos al ejecutar habilidades
/// </summary>
[CreateAssetMenu]
public class Resource : ScriptableObject
{
    /// <summary>
    /// Identificador numérico del recurso
    /// </summary>
    public int iD;

    /// <summary>
    /// Nombre del recurso
    /// </summary>
    public new string name;

    /// <summary>
    /// Cantidad máxima del recurso
    /// </summary>
    public float maxQuantity = 100;

    /// <summary>
    /// Cantidad actual de recurso
    /// </summary>
    public float currentQuantity = 100;

    /// <summary>
    /// Color identificativo
    /// </summary>
    public Color color = Color.white;

}
