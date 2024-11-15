using UnityEngine;

[System.Serializable]
public class Dinosaur
{
    public string name;
    public int id;
    public DinosaurType type;
    public bool isShiny;
    public GameObject prefab; // Prefab del modelo 3D del dinosaurio

    // Constructor
    public Dinosaur(string name, int id, DinosaurType type, bool isShiny, GameObject prefab)
    {
        this.name = name;
        this.id = id;
        this.type = type;
        this.isShiny = isShiny;
        this.prefab = prefab;
    }
}

public enum DinosaurType
{
    Comun,
    Raro,
    ShinyComun,
    ShinyRaro
}
