using UnityEngine;

[CreateAssetMenu(menuName = "DinoAbilities/RangerAbility")]
public class RangerAbility : DinoAbility
{
    public int damage;

    public override void ActivateAbility(DropSlot[] slots)
    {
        // En combate, es posible que no se pase un array válido de slots, 
        // por lo que usaremos la información almacenada en el TeamManager.
        if (TeamManager.Instance != null && TeamManager.Instance.teamIDs.Count >= 2)
        {
            string secondDinoID = TeamManager.Instance.teamIDs[1];
            GameObject secondDinoPrefab = GameManager.Instance.GetDinoPrefabByID(secondDinoID);
            if (secondDinoPrefab != null)
            {
                Dinosaurio secondDino = secondDinoPrefab.GetComponent<Dinosaurio>();
                if (secondDino != null)
                {
                    // Buscar al enemigo por tag.
                    GameObject enemyGO = GameObject.FindGameObjectWithTag("Enemigo");
                    if (enemyGO != null)
                    {
                        EnemyDinosaur enemy = enemyGO.GetComponent<EnemyDinosaur>();
                        if (enemy != null)
                        {
                            enemy.ReceiveDamage(damage);
                            Debug.Log($"RangerAbility (TeamManager): hace {damage} de daño al enemigo.");
                        }
                        else
                        {
                            Debug.LogWarning("RangerAbility: No se encontró el componente EnemyDinosaur en el objeto con tag 'Enemigo'.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("RangerAbility: No se encontró ningún objeto con el tag 'Enemigo'.");
                    }
                }
                else
                {
                    Debug.LogWarning("RangerAbility: El prefab del dino del segundo slot no tiene componente Dinosaurio.");
                }
            }
            else
            {
                Debug.LogWarning("RangerAbility: No se encontró prefab para el dino del segundo slot.");
            }
        }
        else
        {
            Debug.LogWarning("RangerAbility: No hay suficientes dinos en el equipo para activar la habilidad secundaria.");
        }
    }
}