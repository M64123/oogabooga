using UnityEngine;

[CreateAssetMenu(fileName = "UnitStats", menuName = "Units/UnitStats")]
public class UnitStats : ScriptableObject
{
    public float vida = 100f; // Vida
    public float daño = 10f; // Daño físico
    public float carisma = 0f; // Carisma (solo personajes iniciales)
    public float poderElemental = 0f; // Poder elemental (daño mágico)
    public float velocidad = 1f; // Velocidad de ataque (ataques por segundo)
    public float rango = 1.5f; // Rango de ataque
    public float moveSpeed = 2f; // Velocidad de movimiento
}
