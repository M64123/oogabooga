using UnityEngine;
using UnityEngine.UI;

public class CoinFlip : MonoBehaviour
{
    private Rigidbody rb;
    private bool hasLanded = false;
    private bool thrown = false;

    [Header("Force Settings")]
    public float upwardForce = 7f; // Ajusta según sea necesario
    public float torqueForce = 1000f; // Ajusta según sea necesario

    [Header("Result")]
    public string coinSide; // "Cara" o "Cruz"
    public Text resultText; // Asigna el elemento de texto en el inspector (opcional)

    // Propiedad pública para acceder al estado 'thrown'
    public bool IsThrown
    {
        get { return thrown; }
    }

    void Start()
    {
        Debug.Log("Start() llamado");
        rb = GetComponent<Rigidbody>();

        // Bajar el centro de masa en el eje Y para evitar que caiga de canto
        rb.centerOfMass = new Vector3(0, -0.1f, 0);

        // Asegurarse de que la gravedad está desactivada inicialmente
        rb.useGravity = false;

        // Configurar Collision Detection y Interpolation
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        ResetCoin();
    }

    void Update()
    {
        // Nota: Eliminamos el Input.GetKeyDown(KeyCode.Space) de aquí si ahora el lanzamiento lo controla la cámara
    }

    public void FlipCoin()
    {
        if (!thrown && !hasLanded)
        {
            Debug.Log("FlipCoin() llamado");
            thrown = true;

            // Activar la gravedad
            rb.useGravity = true;

            // Limpiar velocidades anteriores
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // Aplicar una rotación inicial aleatoria
            transform.rotation = Random.rotation;

            // Aplicar fuerza hacia arriba con variación aleatoria
            float randomUpwardForce = Random.Range(upwardForce * 0.9f, upwardForce * 1.1f);
            rb.AddForce(Vector3.up * randomUpwardForce, ForceMode.Impulse);

            // Aplicar torque aleatorio
            Vector3 randomTorque = Random.insideUnitSphere * torqueForce;
            rb.AddTorque(randomTorque, ForceMode.Impulse);

            if (resultText != null)
            {
                resultText.text = "Lanzando...";
            }
        }
    }

    void DetermineSide()
    {
        Debug.Log("DetermineSide() llamado");

        // Calcular el producto punto entre el vector hacia arriba de la moneda y el eje Y
        float dot = Vector3.Dot(transform.up, Vector3.up);
        Debug.Log("Valor del producto punto: " + dot);

        // Umbral para considerar si está de canto
        float threshold = 0.1f; // Puedes ajustar este valor

        if (dot > threshold)
        {
            coinSide = "Cara";
        }
        else if (dot < -threshold)
        {
            coinSide = "Cruz";
        }
        else
        {
            // Forzar el resultado a "Cara" o "Cruz" si está cerca del canto
            coinSide = (Random.value > 0.5f) ? "Cara" : "Cruz";
            Debug.Log("La moneda cayó cerca del canto. Resultado forzado a: " + coinSide);
        }

        // Mostrar el resultado
        if (resultText != null)
        {
            resultText.text = "Resultado: " + coinSide;
        }
        Debug.Log("Resultado: " + coinSide);
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

        // Establecer la posición inicial en Y = 1
        transform.position = new Vector3(0, 1f, 0);
        transform.rotation = Quaternion.identity;

        // Desactivar la gravedad
        rb.useGravity = false;

        if (resultText != null)
        {
            resultText.text = "Presiona Espacio para lanzar la moneda";
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (thrown && !hasLanded)
        {
            Debug.Log("La moneda ha aterrizado - DetermineSide()");
            hasLanded = true;
            DetermineSide();
        }
    }
}
