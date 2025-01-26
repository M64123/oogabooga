using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Define la progresión de un atributo base por niveles
/// </summary>
[System.Serializable]
public class AttributeProgression
{
    /// <summary>
    /// Atributo
    /// </summary>
    public Attribute attribute;

    /// <summary>
    /// Progresión
    /// </summary>
    public AnimationCurve progression;

}
