using UnityEngine;

[CreateAssetMenu(fileName = "NuevoDinoStats", menuName = "DinoSystem/DinoStats")]
public class DinoStats : ScriptableObject
{
    [Header("Estadísticas Base")]
    public int vidaBase = 100;
    public int ataqueBase = 20;
    public int defensaBase = 10;
    public float velocidadBase = 5.0f;

    [Header("Atributos Base")]
    public AttributeProgression[] attributes;

    [Header("Recursos Base")]
    public Resource[] resources;
}