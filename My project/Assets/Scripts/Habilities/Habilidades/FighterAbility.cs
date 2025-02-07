using UnityEngine;

[CreateAssetMenu(menuName = "DinoAbilities/FighterAbility")]
public class FighterAbility : DinoAbility
{
    public int shieldAmount;

    public override void ActivateAbility(DropSlot[] slots)
    {
        if (slots.Length >= 2 && slots[1].item != null)
        {
            Debug.Log($"Fighter otorga {shieldAmount} de escudo al Dino en el segundo slot.");
            // Aquí puedes agregar la lógica para aplicar el escudo al dinosaurio.
        }
    }
}