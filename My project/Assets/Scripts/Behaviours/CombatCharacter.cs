using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Define los elementos constituyentes y propiedades de un personaje
/// </summary>
public class CombatCharacter : MonoBehaviour
{
    /// <summary>
    /// Identificador numérico del recurso
    /// </summary>
    public int iD;

    /// <summary>
    /// Nombre del personaje
    /// </summary>
    public new string name;

    /// <summary>
    /// Nivel actual del personaje
    /// </summary>
    public int level = 1;

    /// <summary>
    /// Clase del jugador
    /// </summary>
    public CharacterClass _class;

    /// <summary>
    /// Punto de spawn de la habilidad
    /// </summary>
    public Transform spawnPointHability;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKeyDown)
        {
            if(int.TryParse(Input.inputString, out int keycode))
            {
                _class.habilities[keycode - 1].Setup(this, spawnPointHability);
                _class.habilities[keycode - 1].Play();
            }
        }
    }
}
