using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CoinFlip : MonoBehaviour
{
    private Rigidbody rb;
    private bool hasLanded = false;
    private bool thrown = false;

    [Header("Force Settings")]
    public float upwardForce = 5f; // Fuerza inicial configurable

    [Header("Result")]
    public string coinSide; // "Cara" o "Cruz"
    public Text resultText; // Asigna el elemento de texto en el inspector (opcional)

    [Header("Scene Settings")]
    [Tooltip("Nombre de la escena a cargar si el resultado es Cara")]
    public string caraSceneName;
    [Tooltip("Nombre de la escena a cargar si el resultado es Cruz")]
    public string cruzSceneName;

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

            // Aplicar fuerza hacia arriba
            rb.AddForce(Vector3.up * upwardForce, ForceMode.Impulse);

            // Aplicar torque aleatorio
            Vector3 randomTorque = Random.insideUnitSphere * upwardForce * 100f;
            rb.AddTorque(randomTorque, ForceMode.Impulse);

            if (resultText != null)
            {
                resultText.text = "Lanzando...";
            }

            // Iniciar la corutina para verificar cuándo la moneda ha aterrizado
            StartCoroutine(CheckForLanding());
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

    private IEnumerator CheckForLanding()
    {
        // Esperar al menos 1 segundo antes de comenzar a verificar
        yield return new WaitForSeconds(1f);

        // Esperar hasta que la moneda esté en reposo
        while (!rb.IsSleeping())
        {
            yield return null;
        }

        // La moneda ha aterrizado
        hasLanded = true;

        // Determinar qué lado está hacia arriba
        float dot = Vector3.Dot(transform.up, Vector3.up);

        if (dot > 0)
        {
            coinSide = "Cara";
        }
        else
        {
            coinSide = "Cruz";
        }

        if (resultText != null)
        {
            resultText.text = "Resultado: " + coinSide;
        }

        Debug.Log("Resultado: " + coinSide);

        // Cargar la escena correspondiente usando el NodeSceneManager, si está presente
        NodeSceneManager nsm = NodeSceneManager.Instance;
        if (nsm != null)
        {
            if (coinSide == "Cara" && !string.IsNullOrEmpty(caraSceneName))
            {
                nsm.LoadSceneByName(caraSceneName);
            }
            else if (coinSide == "Cruz" && !string.IsNullOrEmpty(cruzSceneName))
            {
                nsm.LoadSceneByName(cruzSceneName);
            }
            else
            {
                Debug.LogWarning("No se asignaron nombres de escena para el resultado " + coinSide);
            }
        }
        else
        {
            Debug.LogWarning("No se encontró un NodeSceneManager en la escena. Asigna uno o revisa su instancia.");
        }
    }
}
