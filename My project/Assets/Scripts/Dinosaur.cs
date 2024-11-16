using UnityEngine;

/// <summary>
/// Enum para representar los diferentes tipos de dinosaurios.
/// </summary>
public enum DinosaurType
{
    Comun,
    Raro,
    ShinyComun,
    ShinyRaro
}

/// <summary>
/// Clase que representa un dinosaurio en el juego.
/// </summary>
[System.Serializable]
public class Dinosaur
{
    public string name;          // Nombre asignado al dinosaurio
    public DinosaurType type;    // Tipo de dinosaurio
    public bool isShiny;         // Indica si es shiny
    public GameObject prefab;    // Prefab del dinosaurio

    /// <summary>
    /// Constructor para inicializar un dinosaurio.
    /// </summary>
    /// <param name="name">Nombre del dinosaurio.</param>
    /// <param name="type">Tipo de dinosaurio.</param>
    /// <param name="isShiny">Indica si el dinosaurio es shiny.</param>
    /// <param name="prefab">Prefab asociado al dinosaurio.</param>
    public Dinosaur(string name, DinosaurType type, bool isShiny, GameObject prefab)
    {
        this.name = name;
        this.type = type;
        this.isShiny = isShiny;
        this.prefab = prefab;
    }
}
