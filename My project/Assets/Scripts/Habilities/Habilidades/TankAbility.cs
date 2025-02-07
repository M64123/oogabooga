using UnityEngine;

[CreateAssetMenu(menuName = "DinoAbilities/TankAbility")]
public class TankAbility : DinoAbility
{
    public int bonusDamage;

    public override void ActivateAbility(DropSlot[] slots)
    {
        if (slots.Length >= 1 && slots[0].item != null)
        {
            Debug.Log($"Tank potencia el daño del Dino en el primer slot en {bonusDamage}.");
            
        }
    }
}