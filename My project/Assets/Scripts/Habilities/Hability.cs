using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Da cuerpo a un ataque o a una defensa, característico de una determinada clase.
/// </summary>
[CreateAssetMenu]
public class Hability : ScriptableObject
{
    /// <summary>
    /// Identificador numérico del recurso
    /// </summary>
    public int iD;

    /// <summary>
    /// Nombre de la habilidad
    /// </summary>
    public new string name;

    /// <summary>
    /// Prefab de efecto visual de esta habilidad
    /// </summary>
    public GameObject effect;

    /// <summary>
    /// Distancia máxima a la que llega esta habilidad
    /// </summary>
    public float range = 30;

    /// <summary>
    /// Velocidad a la que se desplaza el efecto
    /// </summary>
    public float velocity = 15;

    /// <summary>
    /// Daño que inflije esta habilidad
    /// </summary>
    public float damage = 15;

    /// <summary>
    /// Nombre del parámetro que dispara la animación de esta habilidad
    /// </summary>
    public string animParamName;

    /// <summary>
    /// GameObject que contiene el efecto visual de esta habilidad
    /// </summary>
    protected GameObject myEfect;

    [HideInInspector]
    /// <summary>
    /// Punto de generación del efecto
    /// </summary>
    public Transform spawnPoint;

    /// <summary>
    /// Instigador del daño
    /// </summary>
    CombatCharacter instigator;

    /// <summary>
    /// Asigna los parámetros iniciales 
    /// de dependencia jerárquica 
    /// </summary>
    /// <param name="sp">Parent</param>
    public void Setup(CombatCharacter chrt, Transform sp)
    {
        instigator = chrt;       
        spawnPoint = sp;
    }

    /// <summary>
    /// Método que ejecuta esta habilidad
    /// </summary>
    public void Play()
    {
        //Instancia la habilidad
        myEfect = Instantiate(effect, spawnPoint.position, spawnPoint.rotation);

        //Setup del daño
        DamageModule damageModule = myEfect.AddComponent<DamageModule>();
        damageModule.damage = damage;
        damageModule.instigator = instigator;

        //Setup del movimiento
        Rigidbody rb = myEfect.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.velocity = spawnPoint.forward * velocity;

        //Ejecuta la animación asociada
        instigator.GetComponent<Animator>().SetTrigger(animParamName);

        //Setup del ciclo de vida
        Destroy(myEfect, range / velocity);
    }

    /// <summary>
    /// Método que cancela esta habilidad
    /// </summary>
    public void Cancel() { }

}
