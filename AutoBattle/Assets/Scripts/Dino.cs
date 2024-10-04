using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dino : MonoBehaviour
{
    public int hp;
    public int attack;
    public GameObject dino;
    void Start()
    {
        if (hp == 0)
        {
            Destroy(dino); 
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
