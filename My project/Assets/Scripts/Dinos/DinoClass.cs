using UnityEngine;

[CreateAssetMenu(fileName = "NuevaDinoClass", menuName = "DinoSystem/DinoClass")]
public class DinoClass : ScriptableObject
{
    [Header("Modificadores de Clase")]
    public string nombreClase;
    public float multiplicadorAtaque = 1.2f;
    public float multiplicadorDefensa = 1.1f;
    public float multiplicadorVelocidad = 1.0f;

    [Header("Habilidades de Clase")]
    public Hability[] habilities;
}