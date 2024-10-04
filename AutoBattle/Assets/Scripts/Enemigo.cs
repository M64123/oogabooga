using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemigo : MonoBehaviour
{
    public int hpE;
    public int attackE;
    public GameObject enemy;

    void Start()
    {
        if (hpE == 0)
        {
            Destroy(enemy);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
