using UnityEngine;

[CreateAssetMenu(menuName = "DinoAbilities/FighterAbility")]
public class FighterAbility : DinoAbility
{
    public int shieldAmount;

    public override void ActivateAbility(DropSlot[] slots)
    {
        // Se activa si hay un dino en el segundo slot (lo que indica que este dino tiene la habilidad)
        if (slots.Length >= 2 && slots[1].item != null)
        {
            // Se aplica el escudo al dino en el primer slot (índice 0)
            if (slots.Length >= 1 && slots[0].item != null)
            {
                Dinosaurio dino = slots[0].item.GetComponent<Dinosaurio>();
                if (dino != null)
                {
                    dino.AddShield(shieldAmount);
                    Debug.Log($"Fighter otorga {shieldAmount} de escudo al Dino en el primer slot.");
                }
            }
        }
    }
}