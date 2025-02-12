using UnityEngine;

public class Dinosaurio : CombatCharacter
{
    [Header("Referencias a Scriptable Objects")]
    public DinoStats statsBase; // ScriptableObject con las estadísticas base
    public DinoClass claseDino; // Modificadores y habilidades
    public DinoAbility[] habilities;

    [Header("ID")]
    // ID único generado al instanciar (para otros fines)
    public string idUnico;
    // Este campo se asignará con el ID proveniente de la lista original (playerDinoList)
    public string playerDinoID;

    private int vidaFinal;
    private int ataqueFinal;
    private int defensaFinal;
    private float velocidadFinal;
    public int shield { get; private set; }
    public int temporaryBonusDamage { get; private set; }

    public int MaxHealth { get; private set; }
    public int CurrentHealth { get; private set; }

    public bool muerto { get; private set; } = false;

    void Awake()
    {
        GenerarIDUnico();
        CalcularStatsFinales();
        MaxHealth = statsBase.vidaBase;
        CurrentHealth = MaxHealth;
        muerto = false;
    }

    void GenerarIDUnico()
    {
        idUnico = System.Guid.NewGuid().ToString();
    }

    void CalcularStatsFinales()
    {
        vidaFinal = statsBase.vidaBase;
        ataqueFinal = Mathf.RoundToInt(statsBase.ataqueBase * claseDino.multiplicadorAtaque);
        defensaFinal = Mathf.RoundToInt(statsBase.defensaBase * claseDino.multiplicadorDefensa);
        velocidadFinal = statsBase.velocidadBase * claseDino.multiplicadorVelocidad;
    }

    public void ReceiveDamage(int damage)
    {
        CurrentHealth -= damage;
        if (CurrentHealth < 0)
            CurrentHealth = 0;
        Debug.Log("Dinosaurio recibió " + damage + " de daño. Salud actual: " + CurrentHealth);

        if (CurrentHealth == 0 && !muerto)
        {
            muerto = true; // Marca el dino como muerto en su instancia.
                           // Usar playerDinoID (que se asigna al instanciar en TeamManager) para marcarlo como muerto.
            GameManager.Instance.MarkDinoAsDead(playerDinoID);
            // Llama al DeathManager para agregar este dino (usando el ID)
            DeathManager.Instance.AddDeadDinoByID(playerDinoID);
        }
    }

    public void AddShield(int amount)
    {
        shield += amount;
        Debug.Log($"Se añadió un escudo de {amount}. Escudo actual: {shield}");
    }

    public void AddTemporaryBonusDamage(int bonus)
    {
        temporaryBonusDamage = bonus;
        Debug.Log($"Se potencia el ataque del dino con un bono de {bonus} para el próximo ataque.");
    }

    public void ResetTemporaryBonusDamage()
    {
        temporaryBonusDamage = 0;
    }

    public void ReceiveHeal(int amount)
    {
        CurrentHealth += amount;
        if (CurrentHealth > MaxHealth)
            CurrentHealth = MaxHealth;
        Debug.Log($"El dino ha sido curado en {amount}. Salud actual: {CurrentHealth}");
    }

    // La propiedad DamageValue suma el ataque base y el bonus.
    public int DamageValue
    {
        get { return ataqueFinal + temporaryBonusDamage; }
    }
}