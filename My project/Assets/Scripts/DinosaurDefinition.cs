using UnityEngine;

[System.Serializable]
public class DinosaurDefinition
{
    public int dinoID;           // El ID único del dinosaurio
    public GameObject dinoPrefab; // Prefab asociado a este ID
    public string dinoName;      // Opcional: nombre del dino
    public Rarity rarity;        // Opcional: rareza del dino
    // Aquí puedes añadir más campos según lo que necesites
}
