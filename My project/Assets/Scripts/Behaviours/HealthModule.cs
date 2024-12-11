using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Módulo de salud de un personaje.
/// </summary>
public class HealthModule : MonoBehaviour
{
    /// <summary>
    /// Salud actual del personaje
    /// </summary>
    [SerializeField]
    float health = 100;

    /// <summary>
    /// Salud máxima
    /// </summary>
    float maxHealth = 1000;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetDamage(float damage)
    {
        //De momento es un daño plano, sin tener en cuenta las resistencias o la armadura del personaje
        health -= damage;
        health = Mathf.Clamp(health, 0, maxHealth);
    }
}
