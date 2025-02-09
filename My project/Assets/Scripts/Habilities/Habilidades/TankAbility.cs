using UnityEngine;

[CreateAssetMenu(menuName = "DinoAbilities/TankAbility")]
public class TankAbility : DinoAbility
{
    public int bonusDamage;

    public override void ActivateAbility(DropSlot[] slots)
    {
        // Comprobamos que en el equipo haya al menos dos dinos (lo que indica que existe un dino en el segundo slot).
        if (TeamManager.Instance != null && TeamManager.Instance.teamIDs.Count >= 2)
        {
            // Suponemos que la habilidad Tank se activa para potenciar al dino activo (el del primer slot)
            // utilizando el bonus almacenado en este asset.
            if (TeamManager.Instance.activeDino != null)
            {
                Dinosaurio frontDino = TeamManager.Instance.activeDino;
                frontDino.AddTemporaryBonusDamage(bonusDamage);
                Debug.Log($"TankAbility (TeamManager): Potencia el ataque del dino activo en el primer slot en {bonusDamage}.");
            }
            else
            {
                Debug.LogWarning("TankAbility: No hay dino activo en el TeamManager para potenciar.");
            }
        }
        else
        {
            Debug.LogWarning("TankAbility: No hay suficientes dinos en el equipo para activar la habilidad.");
        }
    }
}