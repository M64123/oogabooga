using UnityEngine;
using UnityEngine.UI;

public class CoinFlip : MonoBehaviour
{
    private Rigidbody rb;
    private bool hasLanded = false;
    private bool thrown = false;

    [Header("Force Settings")]
    public float upwardForce = 20f; // Ajusta este valor según tus necesidades
    public float torqueForce = 1000f; // Ajusta este valor según tus necesidades

    [Header("Result")]
    public string coinSide; // "Cara" o "Cruz"
    public Text resultText; // Asigna el elemento de texto en el inspector (opcional)

    void Start()
    {
        Debug.Log("Start() llamado");
        rb = GetComponent<Rigidbody>();
        // Bajar el centro de masa en el eje Y
        rb.centerOfMass = new Vector3(0, -0.1f, 0);
        ResetCoin();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !thrown && !hasLanded)
        {
            Debug.Log("Tecla Espacio presionada - FlipCoin()");
            if (resultText != null)
            {
                resultText.text = "Lanzando...";
            }
            FlipCoin();
        }

        if (rb.IsSleeping() && thrown && !hasLanded)
        {
            Debug.Log("La moneda ha aterrizado - DetermineSide()");
            hasLanded = true;
            DetermineSide();

            // Si la moneda cayó de canto, permitir volver a lanzar
            if (coinSide == "Canto")
            {
                thrown = false;
                hasLanded = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Tecla R presionada - ResetCoin()");
            ResetCoin();
        }
    }

    public void FlipCoin()
    {
        Debug.Log("FlipCoin() llamado");
        thrown = true;

        // Limpiar velocidades anteriores
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Aplicar fuerza hacia arriba y torque aleatorio
        rb.AddForce(Vector3.up * upwardForce, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * torqueForce);
    }
    void DetermineSide()
    {
        Debug.Log("DetermineSide() llamado");

        // Obtener el vector hacia arriba de la moneda en el mundo
        Vector3 coinUp = transform.up;

        // Calcular el ángulo entre el vector hacia arriba de la moneda y el eje Y
        float angle = Vector3.Angle(coinUp, Vector3.up);

        Debug.Log("Ángulo respecto al eje Y: " + angle);

        // Determinar el lado basándose en el ángulo
        if (angle < 10f)
        {
            coinSide = "Cara";
        }
        else if (angle > 170f)
        {
            coinSide = "Cruz";
        }
        else
        {
            coinSide = "Canto";
        }

        if (coinSide == "Canto")
        {
            if (resultText != null)
            {
                resultText.text = "La moneda cayó de canto. Vuelve a lanzar.";
            }
            Debug.Log("La moneda cayó de canto. Vuelve a lanzar.");
        }
        else
        {
            if (resultText != null)
            {
                resultText.text = "Resultado: " + coinSide;
            }
            Debug.Log("Resultado: " + coinSide);
        }
    }

    public void ResetCoin()
    {
        Debug.Log("ResetCoin() llamado");
        // Reiniciar variables
        hasLanded = false;
        thrown = false;
        coinSide = "";

        // Restablecer la posición y rotación
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = new Vector3(0, 1, 0); // Ajusta la posición según necesites
        transform.rotation = Quaternion.identity;

        if (resultText != null)
        {
            resultText.text = "Presiona Espacio para lanzar la moneda";
        }
    }
}
