using UnityEngine;
using UnityEngine.UI;

public class CoinFlip : MonoBehaviour
{
    private Rigidbody rb;
    private bool hasLanded = false;
    private bool thrown = false;

    [Header("Force Settings")]
    public float upwardForce = 10f; // Ajusta según sea necesario
    public float torqueForce = 1500f; // Incrementa este valor para mayor rotación

    [Header("Result")]
    public string coinSide; // "Cara" o "Cruz"
    public Text resultText; // Asigna el elemento de texto en el inspector (opcional)

    void Start()
    {
        Debug.Log("Start() llamado");
        rb = GetComponent<Rigidbody>();

        // Opcional: ajustar el centro de masa si es necesario
        // rb.centerOfMass = new Vector3(0, -0.1f, 0);

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

        // **Aplicar una rotación inicial aleatoria**
        transform.rotation = Random.rotation;

        // Aplicar fuerza hacia arriba
        rb.AddForce(Vector3.up * upwardForce, ForceMode.Impulse);

        // Aplicar torque aleatorio
        Vector3 randomTorque = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ).normalized * torqueForce;

        rb.AddTorque(randomTorque, ForceMode.Impulse);
    }


    void DetermineSide()
    {
        Debug.Log("DetermineSide() llamado");

        // Calcular el producto punto entre el vector hacia arriba de la moneda y el eje Y
        float dot = Vector3.Dot(transform.up, Vector3.up);
        Debug.Log("Valor del producto punto: " + dot);

        // Determinar el lado basándose en el producto punto
        if (dot > 0)
        {
            coinSide = "Cara";
        }
        else if (dot < 0)
        {
            coinSide = "Cruz";
        }
        else
        {
            coinSide = "Canto";
        }

        // Mostrar el resultado
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
