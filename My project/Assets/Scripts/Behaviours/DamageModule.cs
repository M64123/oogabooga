using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Módulo que se añade en tiempo de ejecución al efecto de una habilidad.
/// Inflige daño/sanación en función de la propia habilidad y las estadísticas del personaje.
/// </summary>
public class DamageModule : MonoBehaviour
{

    /// <summary>
    /// Identificador numérico del recurso
    /// </summary>
    public int resource_Id;

    /// <summary>
    /// Daño que inflije esta habilidad
    /// </summary>
    public float damage;

    /// <summary>
    /// Instigador del daño
    /// </summary>
    public CombatCharacter instigator;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Daño infligido al oponente
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        
        CombatCharacter character = collision.collider.GetComponent<CombatCharacter>();

        //Comprueba que sea un personaje y que no sea quien ejecutó la habilidad
        if (character != null && character.name == instigator.name)
            return;
        
        HealthModule health = collision.collider.GetComponent<HealthModule>();

        //Comprueba que posea un módulo de salud
        if (health != null)
        {
            health.SetDamage(damage);
            Destroy(gameObject);
        }
    }
}
