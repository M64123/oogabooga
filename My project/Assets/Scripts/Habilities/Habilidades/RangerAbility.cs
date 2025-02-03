using UnityEngine;

[CreateAssetMenu(menuName = "DinoAbilities/RangerAbility")]
public class RangerAbility : DinoAbility
{
    public int damage;

    public override void ActivateAbility(DropSlot[] slots)
    {
        if (slots.Length >= 2 && slots[1].item != null)
        {
            Debug.Log($"Ranger hace {damage} de daño al Dino en el segundo slot.");
            // Lógica para aplicar daño al dinosaurio en el segundo slot.
        }
    }
}