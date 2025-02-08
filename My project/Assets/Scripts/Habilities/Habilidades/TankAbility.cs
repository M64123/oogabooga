using UnityEngine;

[CreateAssetMenu(menuName = "DinoAbilities/TankAbility")]
public class TankAbility : DinoAbility
{
    public int bonusDamage;

    public override void ActivateAbility(DropSlot[] slots)
    {
        // Se activa si hay un dino en el segundo slot (que posee la habilidad Tank)
        if (slots.Length >= 1 && slots[0].item != null)
        {
            Dinosaurio dino = slots[0].item.GetComponent<Dinosaurio>();
            if (dino != null)
            {
                dino.AddTemporaryBonusDamage(bonusDamage);
                Debug.Log($"Tank potencia el daño del Dino en el primer slot en {bonusDamage}.");
            }
        }
    }
}