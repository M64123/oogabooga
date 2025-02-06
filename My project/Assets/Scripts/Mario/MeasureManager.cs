using UnityEngine;

public class MeasureManager : MonoBehaviour
{
    // BPM de la canción (p. ej.: 120)
    public float BPM = 120f;
    // Beats por compás (por ejemplo, en 4/4 serían 4)
    public int beatsPerMeasure = 4;
    // Cada cuántos compases se realiza el cambio
    public int measuresToToggle = 20;

    // Duración de un compás en segundos
    private float measureDuration;
    // Acumulador de tiempo para contar compases
    private float timer;
    // Contador de compases transcurridos
    private int measureCount;

    // Estados booleanos: al inicio ataque es true y defensa false
    private bool ataque = true;
    private bool defensa = false;
    public GameObject moneda;
    public SpriteRenderer monedaAtaque;
    public SpriteRenderer defensaAtaque;

    void Start()
    {
        // Calcula la duración de un compás
        measureDuration = (60f / BPM) * beatsPerMeasure;
        Debug.Log("Inicio: ataque " + ataque + ", defensa " + defensa);
    }

    void Update()
    {
        // Acumula el tiempo transcurrido
        timer += Time.deltaTime;

        // Cuando se completa un compás
        if (timer >= measureDuration)
        {
            // Resta la duración del compás (para mantener el exceso de tiempo)
            timer -= measureDuration;
            // Incrementa el contador de compases
            measureCount++;

            // Cada 'measuresToToggle' compases se alterna el estado de los booleanos
            if (measureCount % measuresToToggle == 0)
            {
                SpriteRenderer referencia= moneda.GetComponent<SpriteRenderer>();
                // Si actualmente 'ataque' es true, se cambia a false y se activa 'defensa'
                if (ataque)
                {
                    ataque = false;
                    defensa = true;
                    referencia.sprite= monedaAtaque.sprite;
                }
                else
                {
                    ataque = true;
                    defensa = false;
                    referencia.sprite = defensaAtaque.sprite;
                }
                Debug.Log("Compás " + measureCount + ": ataque " + ataque + ", defensa " + defensa);
            }
        }
    }
}