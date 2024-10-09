using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitMovement : MonoBehaviour
{
    public float speed = 1f;
    private Vector3 startpos;
    private float minX;
    private float maxX;
    void Start()
    {
        minX = -((10 * 1.024f) / 2) * 0.9f;
        maxX = ((10 * 1.024f) / 2) * 0.9f;

        float randomX = UnityEngine.Random.Range(minX, maxX);
        startpos = new Vector3(randomX, transform.position.y, transform.position.z);
        transform.position = startpos;
    }


    void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);


        if (transform.position.x > maxX || transform.position.x < minX)
        {
            speed = -speed;

            Vector3 scale = transform.localScale;
            scale.x = -scale.x;
            transform.localScale = scale;

        }
    }
}
