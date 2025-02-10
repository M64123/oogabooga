using UnityEngine;
using UnityEngine.UI;

public class AbilityTooltipUI : MonoBehaviour
{
    [Header("Elementos de UI")]
    public Text abilityNameText;
    public Text abilityDescriptionText;
    public Text abilityValueText;

    /// <summary>
    /// Configura la información del tooltip a partir de la habilidad.
    /// </summary>
    /// <param name="ability">La habilidad de tipo DinoAbility.</param>
    public void SetAbility(DinoAbility ability)
    {
        if (ability == null)
        {
            abilityNameText.text = "Sin Habilidad";
            abilityDescriptionText.text = "";
            abilityValueText.text = "";
            return;
        }

        abilityNameText.text = ability.abilityName;
        abilityDescriptionText.text = ability.description;

        // Dependiendo del tipo de habilidad, se muestra un valor entero.
        if (ability is RangerAbility ranger)
        {
            abilityValueText.text = "Damage: " + ranger.damage.ToString();
        }
        else if (ability is FighterAbility fighter)
        {
            abilityValueText.text = "Shield: " + fighter.shieldAmount.ToString();
        }
        else if (ability is TankAbility tank)
        {
            abilityValueText.text = "Bonus Damage: " + tank.bonusDamage.ToString();
        }
        else if (ability is ShamanAbility shaman)
        {
            abilityValueText.text = "Heal: " + shaman.healAmount.ToString();
        }
        else
        {
            abilityValueText.text = "";
        }
    }
}