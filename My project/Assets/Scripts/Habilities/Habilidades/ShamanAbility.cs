using UnityEngine;

[CreateAssetMenu(menuName = "DinoAbilities/ShamanAbility")]
public class ShamanAbility : DinoAbility
{
    public int healAmount;

    public override void ActivateAbility(DropSlot[] slots)
    {
        // En combate, usamos el dino activo almacenado en el TeamManager.
        if (TeamManager.Instance != null && TeamManager.Instance.activeDino != null)
        {
            Dinosaurio frontDino = TeamManager.Instance.activeDino;
            frontDino.ReceiveHeal(healAmount);
            Debug.Log($"ShamanAbility (TeamManager): Cura al Dino activo en el primer slot en {healAmount} de vida.");
        }
        else
        {
            Debug.LogWarning("ShamanAbility: No hay un dino activo en el TeamManager para curar.");
        }
    }
}