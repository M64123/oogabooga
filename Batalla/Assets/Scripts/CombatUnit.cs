using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatUnit : MonoBehaviour
{
    public float speed = 2f;
    public Transform target;

    void Update()
    {
        if (target != null)
        {
            // Mover hacia el objetivo
            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        }
        else
        {
            // Encontrar un objetivo (por ejemplo, la unidad enemiga más cercana)
        }
    }
}
