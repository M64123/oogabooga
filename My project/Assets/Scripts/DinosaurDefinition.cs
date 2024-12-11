using UnityEngine;

[System.Serializable]
public class DinosaurDefinition
{
    public int dinoID;             // ID �nico del dinosaurio
    public GameObject dinoPrefab;  // Prefab asociado al dinosaurio
    public string dinoName;        // Nombre del dinosaurio
    public Rarity rarity;          // Rareza del dinosaurio

    // Campos adicionales para definir el dinosaurio
    public DinoStats statsBase;    // Estad�sticas base
    public DinoClass dinoClass;    // Clase del dinosaurio
}

// Enumeraci�n para definir la rareza del dinosaurio
public enum Rarity
{
    Common,
    Rare,
    ShinyCommon,
    ShinyRare
}