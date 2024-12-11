using UnityEngine;

public class Dinosaurio : CombatCharacter
{
    [Header("Referencias a Scriptable Objects")]
    public DinoStats statsBase; // Referencia a las estadísticas base
    public DinoClass claseDino; // Referencia a la clase del dinosaurio

    [Header("ID Único del Dinosaurio")]
    public string idUnico;

    private int vidaFinal;
    private int ataqueFinal;
    private int defensaFinal;
    private float velocidadFinal;

    void Awake()
    {
        GenerarIDUnico();
        CalcularStatsFinales();
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

    public void UsarHabilidad(int indice)
    {
        if (indice >= 0 && indice < claseDino.habilities.Length)
        {
            Transform spawnPoint = transform;
            claseDino.habilities[indice].Setup(this, spawnPoint);
            claseDino.habilities[indice].Play();
        }
    }
}