using UnityEngine;

public class CombatUnit : MonoBehaviour
{
    public float speed = 2f;
    public Transform target;

    void Start()
    {
        FindTarget();
    }

    void Update()
    {
        if (target != null)
        {
            // Mover hacia el objetivo
            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

            // Girar hacia el objetivo
            FaceDirection(target.position - transform.position);

            // Ataque y lógica adicional aquí...
        }
        else
        {
            // Encontrar un nuevo objetivo si es necesario
            FindTarget();
        }
    }

    void FindTarget()
    {
        // Lógica para encontrar la unidad enemiga más cercana
        // Por ejemplo, buscar unidades con etiqueta "Enemy" o "Ally" según corresponda

        string enemyTag = (gameObject.tag == "Ally") ? "Enemy" : "Ally";
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);

        float closestDistance = Mathf.Infinity;
        Transform closestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy.transform;
            }
        }

        target = closestEnemy;
    }

    void FaceDirection(Vector3 direction)
    {
        // Orientar el sprite según la dirección del movimiento
        if (direction.x != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = direction.x > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }
}
