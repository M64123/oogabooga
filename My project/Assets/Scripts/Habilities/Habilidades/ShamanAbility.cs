using UnityEngine;

[CreateAssetMenu(menuName = "DinoAbilities/ShamanAbility")]
public class ShamanAbility : DinoAbility
{
    public int healAmount;

    public override void ActivateAbility(DropSlot[] slots)
    {
        if (slots.Length >= 1 && slots[0].item != null)
        {
            Debug.Log($"Shaman cura al Dino en el primer slot en {healAmount} de vida.");
            // Lógica para curar al dinosaurio.
        }
    }
}