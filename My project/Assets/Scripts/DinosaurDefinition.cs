using UnityEngine;

[System.Serializable]
public class DinosaurDefinition
{
    public int dinoID;           // El ID �nico del dinosaurio
    public GameObject dinoPrefab; // Prefab asociado a este ID
    public string dinoName;      // Opcional: nombre del dino
    public Rarity rarity;        // Opcional: rareza del dino
    // Aqu� puedes a�adir m�s campos seg�n lo que necesites
}
