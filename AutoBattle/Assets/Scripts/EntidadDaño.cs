using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntidadDaño : MonoBehaviour
{
    public float health = 100f;            // Health of the entity
    public float damageAmount = 10f;       // Damage taken per tick
    public float damageInterval = 1f;      // Time interval for damage (in seconds)
    public Transform target;               // The other entity to check proximity against
    public float damageDistance = 1.2f;    // Distance within which damage is applied

    private float nextDamageTime = 0f;     // Time tracking for the next damage application

    void Update()
    {
        // Calculate the distance between this entity and the target
        float distance = Vector3.Distance(transform.position, target.position);

       
        // If within the damage distance and it's time for the next damage tick
        if (distance <= damageDistance && Time.time >= nextDamageTime)
        {
            ApplyDamage();
            nextDamageTime = Time.time + damageInterval; // Set the next time to deal damage
        }
    }

    // Apply damage to the entity
    private void ApplyDamage()
    {
        health -= damageAmount;
        Debug.Log(gameObject.name + " took damage! Current Health: " + health);

        // Check if health is below or equal to zero and destroy the object
        if (health <= 0f)
        {
            Destroy(gameObject);
            Debug.Log(gameObject.name + " was destroyed!");
        }
    }
}
