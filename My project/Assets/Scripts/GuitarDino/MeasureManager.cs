using UnityEngine;

public class MeasureManager : MonoBehaviour
{
    public static MeasureManager Instance;

    public float BPM = 120f;
    public int beatsPerMeasure = 4;
    public int measuresToToggle = 20;

    private float measureDuration;
    private float timer;
    private int measureCount;

    // Estado de fase: al inicio, ataque es true y defensa false
    private bool ataque = true;
    private bool defensa = false;
    // Propiedad pública para consultar el estado actual de ataque
    public bool IsAtaque { get { return ataque; } }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        measureDuration = (60f / BPM) * beatsPerMeasure;
        Debug.Log("Inicio: ataque " + ataque + ", defensa " + defensa);
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= measureDuration)
        {
            timer -= measureDuration;
            measureCount++;

            // Cada 'measuresToToggle' compases se alterna el estado
            if (measureCount % measuresToToggle == 0)
            {
                if (ataque)
                {
                    ataque = false;
                    defensa = true;
                }
                else
                {
                    ataque = true;
                    defensa = false;
                }
                Debug.Log("Compás " + measureCount + ": ataque " + ataque + ", defensa " + defensa);
            }
        }
    }
}
