using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{

    public int health = 100;
    public int damage = 10;
    public float attackRange = 2.0f;
    public float attackCooldown = 1.0f;
    private float attackTimer = 0f;

    public Transform target;

    // Update is called once per frame
    void Update()
    {
        attackTimer += Time.deltaTime;

        if (target != null && attackTimer >= attackCooldown)
        {
            Attack();
            attackTimer = 0f;
        }
    }

    void Attack()
    {
        if (Vector3.Distance(transform.position, target.position) <= attackRange)
        {
            // Attack logic here
            Debug.Log($"{gameObject.name} attacks {target.name} for {damage} damage.");
        }
    }
}
