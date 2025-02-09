using UnityEngine;

public class AbilityDebugger : MonoBehaviour
{
    /// <summary>
    /// Imprime en la consola los detalles de la habilidad.
    /// </summary>
    /// <param name="ability">La habilidad de tipo DinoAbility.</param>
    public void PrintAbilityDetails(DinoAbility ability)
    {
        if (ability == null)
        {
            Debug.Log("No se encontró la habilidad.");
            return;
        }

        string details = $"Habilidad: {ability.abilityName}\n" +
                         $"Descripción: {ability.description}\n";

        // Usamos patrones de coincidencia para determinar el tipo específico de habilidad.
        if (ability is FighterAbility fighter)
        {
            details += $"Shield Amount: {fighter.shieldAmount}\n";
        }
        else if (ability is RangerAbility ranger)
        {
            details += $"Damage: {ranger.damage}\n";
        }
        else if (ability is TankAbility tank)
        {
            details += $"Bonus Damage: {tank.bonusDamage}\n";
        }
        else if (ability is ShamanAbility shaman)
        {
            details += $"Heal Amount: {shaman.healAmount}\n";
        }
        else
        {
            details += "Tipo de habilidad desconocido.\n";
        }

        Debug.Log(details);
    }
}