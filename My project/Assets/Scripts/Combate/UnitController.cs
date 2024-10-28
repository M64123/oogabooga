using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitController : MonoBehaviour
{
    
    public UnitData unitData;

    private NavMeshAgent agent;
    private HealthSystem healthSystem;
    private UnitController targetEnemy;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        healthSystem = GetComponent<HealthSystem>();
        agent.speed = unitData.speed;
        healthSystem.maxHealth = unitData.maxHealth;

        FindClosestEnemy();
    }

    
    void Update()
    {
        if (targetEnemy != null)
        {
            float distance = Vector3.Distance(transform.position, targetEnemy.transform.position);

            if (distance <= unitData.detectionRange)
            {
                Attack();
            }
            else
            {
                agent.SetDestination(targetEnemy.transform.position);
            }
        }
        else
        {
            FindClosestEnemy();
        }
    }

    void Attack()
    {
        agent.ResetPath();

        targetEnemy.GetComponent<HealthSystem>().TakeDamage(unitData.damage * Time.deltaTime);
    }

    void FindClosestEnemy()
    {
        UnitController[] allUnits = FindObjectsOfType<UnitController>();
        float closestDistance = Mathf.Infinity;
        UnitController closestEnemy = null;

        foreach (UnitController unit in allUnits)
        {
            if (unit != this && IsEnemy(unit))
            {
                float distance = Vector3.Distance(transform.position, unit.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = unit;
                }
            }
        }

        targetEnemy = closestEnemy;
    }

    bool IsEnemy(UnitController otherUnit)
    {
        // Aquí puedes implementar lógica para determinar si la unidad es enemiga
        // Por ejemplo, comparar equipos o facciones
        return true; // Asumimos que todas las demás unidades son enemigas
    }
}
