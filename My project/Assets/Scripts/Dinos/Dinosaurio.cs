using UnityEngine;

public class Dinosaurio : CombatCharacter
{
    [Header("Referencias a Scriptable Objects")]
    public DinoStats statsBase; // ScriptableObject que contiene las estadísticas base
    public DinoClass claseDino; // ScriptableObject o clase que contiene modificadores y habilidades

    [Header("ID Único del Dinosaurio")]
    public string idUnico;

    // Estadísticas finales calculadas (puedes incluir otras según tus necesidades)
    private int vidaFinal;
    private int ataqueFinal;
    private int defensaFinal;
    private float velocidadFinal;

    // Variables de salud que usará el HUD
    public int MaxHealth { get; private set; }
    public int CurrentHealth { get; private set; }

    void Awake()
    {
        GenerarIDUnico();
        CalcularStatsFinales();
        // Inicializamos la salud usando el valor del ScriptableObject.
        MaxHealth = statsBase.vidaBase;
        CurrentHealth = MaxHealth;
    }

    void GenerarIDUnico()
    {
        idUnico = System.Guid.NewGuid().ToString();
    }

    void CalcularStatsFinales()
    {
        // Se calcula la vida, ataque, etc. a partir de los datos del ScriptableObject y la clase
        vidaFinal = statsBase.vidaBase;
        ataqueFinal = Mathf.RoundToInt(statsBase.ataqueBase * claseDino.multiplicadorAtaque);
        defensaFinal = Mathf.RoundToInt(statsBase.defensaBase * claseDino.multiplicadorDefensa);
        velocidadFinal = statsBase.velocidadBase * claseDino.multiplicadorVelocidad;
    }

    /// <summary>
    /// Aplica daño al dinosaurio y actualiza la salud actual.
    /// </summary>
    /// <param name="damage">Daño a aplicar</param>
    public void ReceiveDamage(int damage)
    {
        CurrentHealth -= damage;
        if (CurrentHealth < 0)
            CurrentHealth = 0;
        Debug.Log("Dinosaurio recibió " + damage + " de daño. Salud actual: " + CurrentHealth);
    }

    // Propiedad para obtener el daño que hace el dinosaurio (por ejemplo, basada en ataqueFinal)
    public int DamageValue
    {
        get { return ataqueFinal; }
    }
}