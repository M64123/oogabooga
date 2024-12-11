using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DinosaurDatabase", menuName = "MyGame/Dinosaur Database")]
public class DinosaurDatabase : ScriptableObject
{
    [Header("Lista de todos los dinosaurios")]
    public List<DinosaurDefinition> dinosaurs = new List<DinosaurDefinition>();

    // Método para buscar un dinosaurio por ID
    public DinosaurDefinition GetDinosaurByID(int id)
    {
        return dinosaurs.Find(dino => dino.dinoID == id);
    }
}