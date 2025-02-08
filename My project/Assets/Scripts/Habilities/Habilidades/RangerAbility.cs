using UnityEngine;

[CreateAssetMenu(menuName = "DinoAbilities/RangerAbility")]
public class RangerAbility : DinoAbility
{
    public int damage;

    public override void ActivateAbility(DropSlot[] slots)
    {
        // Se activa si hay un dino en el segundo slot (quien posee la habilidad)
        if (slots.Length >= 2 && slots[1].item != null)
        {
            // Buscamos al enemigo por tag
            GameObject enemyGO = GameObject.FindGameObjectWithTag("Enemigo");
            if (enemyGO != null)
            {
                EnemyDinosaur enemy = enemyGO.GetComponent<EnemyDinosaur>();
                if (enemy != null)
                {
                    enemy.ReceiveDamage(damage);
                    Debug.Log($"Ranger hace {damage} de daño al enemigo.");
                }
            }
        }
    }
}