using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DinosaurDatabase", menuName = "MyGame/Dinosaur Database")]
public class DinosaurDatabase : ScriptableObject
{
    public List<DinosaurDefinition> dinosaurs = new List<DinosaurDefinition>();
}
