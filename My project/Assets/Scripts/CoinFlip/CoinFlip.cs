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

    public Collider caraCollider;  // Collider para detectar "Cara"
    public Collider cruzCollider;  // Collider para detectar "Cruz"

    public bool IsThrown
    {
        get { return thrown; }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Asegurarse de que la gravedad está desactivada inicialmente
        rb.useGravity = false;

        // Configurar Collision Detection y Interpolation
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        ResetCoin();
    }

    public void FlipCoin()
    {
        if (!thrown && !hasLanded)
        {
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

    public void ResetCoin()
    {
        hasLanded = false;
        thrown = false;
        coinSide = "";

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.position = new Vector3(0, 1f, 0);
        transform.rotation = Quaternion.identity;

        rb.useGravity = false;

        if (resultText != null)
        {
            resultText.text = "Presiona Espacio para lanzar la moneda";
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (thrown && !hasLanded)
        {
            // Verificar cuál de los colliders (cara o cruz) ha tocado el suelo
            if (other == caraCollider)
            {
                coinSide = "Cara";
            }
            else if (other == cruzCollider)
            {
                coinSide = "Cruz";
            }

            // Mostrar el resultado
            if (resultText != null)
            {
                resultText.text = "Resultado: " + coinSide;
            }
            Debug.Log("Resultado: " + coinSide);

            hasLanded = true;
        }
    }
}
