using UnityEngine;

[System.Serializable]
public class DinoData
{
    public string dinoID;
    public string dinoName;
    public Rarity rarity;
    public int level = 1;
    public bool isAlive = true;
    public Sprite dinoSprite; // Opcional: para almacenar la imagen, si se requiere.
}