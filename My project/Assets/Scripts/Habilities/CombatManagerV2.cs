using UnityEngine;
using UnityEngine.SceneManagement;

public class CombatManagerV2 : MonoBehaviour
{
    public Transform spawnPoint; // Lugar donde aparecerá el dino en combate

    private void Start()
    {
        // Verifica que el TeamManager esté presente.
        if (TeamManager.Instance != null)
        {
            // Asigna el spawnPoint al TeamManager para que lo use al instanciar.
            TeamManager.Instance.spawnPoint = spawnPoint;

            // Instancia el dino del frente.
            TeamManager.Instance.SpawnFrontDino();

            // Mostrar en consola la habilidad secundaria (del dino del segundo slot).
            ShowSecondaryAbility();
        }
        else
        {
            Debug.LogWarning("CombatManagerV2: TeamManager no está presente en la escena.");
        }
    }

    /// <summary>
    /// Muestra en consola la habilidad secundaria del dino en el segundo slot,
    /// según la información almacenada en el TeamManager.
    /// </summary>
    private void ShowSecondaryAbility()
    {
        if (TeamManager.Instance.teamIDs.Count >= 2)
        {
            string secondDinoID = TeamManager.Instance.teamIDs[1];
            GameObject secondDinoPrefab = GameManager.Instance.GetDinoPrefabByID(secondDinoID);
            if (secondDinoPrefab != null)
            {
                Dinosaurio secondDino = secondDinoPrefab.GetComponent<Dinosaurio>();
                if (secondDino != null && secondDino.habilities != null && secondDino.habilities.Length > 0)
                {
                    DinoAbility ability = secondDino.habilities[0];
                    Debug.Log("Habilidad secundaria detectada:");
                    // Llamamos al método para imprimir los detalles.
                    AbilityDebugger debugger = FindObjectOfType<AbilityDebugger>();
                    if (debugger != null)
                    {
                        debugger.PrintAbilityDetails(ability);
                    }
                    else
                    {
                        // Si no tienes un objeto AbilityDebugger en la escena, también puedes llamar directamente:
                        PrintAbilityDetails(ability);
                    }
                }
                else
                {
                    Debug.Log("El dino del segundo slot no tiene habilidades asignadas.");
                }
            }
            else
            {
                Debug.Log("No se encontró prefab para el dino en el segundo slot.");
            }
        }
        else
        {
            Debug.Log("No hay un dino en el segundo slot.");
        }
    }

    // O bien, si prefieres tener la función PrintAbilityDetails en el mismo script:
    private void PrintAbilityDetails(DinoAbility ability)
    {
        if (ability == null)
        {
            Debug.Log("No se encontró la habilidad.");
            return;
        }

        string details = $"Habilidad: {ability.abilityName}\n" +
                         $"Descripción: {ability.description}\n";

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