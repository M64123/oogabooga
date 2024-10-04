using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityMover : MonoBehaviour
{
    public Transform target;               // The other entity to move towards
    public float moveSpeed = 5f;           // Movement speed
    public float stopDistance = 1f;        // Distance to stop before collision

    private bool hasStopped = false;       // Track if the movement has already stopped

    void Update()
    {
        // Calculate the distance between this entity and the target
        float distance = Vector3.Distance(transform.position, target.position);

      

        // Move towards the target if distance is greater than stopDistance and not stopped yet
        if (distance > stopDistance && !hasStopped)
        {
            // Move towards the target
            transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
        }
        else if (distance <= stopDistance && !hasStopped)
        {
            // Stop movement when within stopDistance
            hasStopped = true;
            
        }
    }
}
