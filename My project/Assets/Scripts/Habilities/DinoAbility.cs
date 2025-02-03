using UnityEngine;

public abstract class DinoAbility : ScriptableObject
{
    public string abilityName;
    public string description;

    public abstract void ActivateAbility(DropSlot[] slots);
}