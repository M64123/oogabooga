using UnityEngine;

[CreateAssetMenu(menuName = "DinoAbilities/ShamanAbility")]
public class ShamanAbility : DinoAbility
{
    public int healAmount;

    public override void ActivateAbility(DropSlot[] slots)
    {
        // Se activa si hay un dino en el segundo slot (que posee la habilidad Shaman)
        if (slots.Length >= 1 && slots[0].item != null)
        {
            Dinosaurio dino = slots[0].item.GetComponent<Dinosaurio>();
            if (dino != null)
            {
                dino.ReceiveHeal(healAmount);
                Debug.Log($"Shaman cura al Dino en el primer slot en {healAmount} de vida.");
            }
        }
    }
}