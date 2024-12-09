using UnityEngine;

[System.Serializable]
public class DinosaurDefinition
{
    public int dinoID;             // ID único del dinosaurio
    public GameObject dinoPrefab;  // Prefab asociado al dinosaurio
    public string dinoName;        // Nombre del dinosaurio
    public Rarity rarity;          // Rareza del dinosaurio

    // Campos adicionales para definir el dinosaurio
    public DinoStats statsBase;    // Estadísticas base
    public DinoClass dinoClass;    // Clase del dinosaurio
}

// Enumeración para definir la rareza del dinosaurio
public enum Rarity
{
    Common,
    Rare,
    ShinyCommon,
    ShinyRare
}