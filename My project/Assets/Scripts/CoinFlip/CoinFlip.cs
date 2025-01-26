using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CoinFlip : MonoBehaviour
{
    private Rigidbody rb;
    private bool hasLanded = false;
    private bool thrown = false;

    [Header("Force Settings")]
    public float upwardForce = 5f; // Ajustable desde el Inspector

    [Header("Result")]
    public string coinSide; // "Cara" o "Cruz"
    public Text resultText; // Texto opcional

    [Header("Scene Settings")]
    public string caraSceneName; // Escena si sale Cara
    public string cruzSceneName; // Escena si sale Cruz

    public bool IsThrown
    {
        get { return thrown; }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        ResetCoin();
    }

    public void FlipCoin()
    {
        if (!thrown && !hasLanded)
        {
            thrown = true;
            rb.useGravity = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            transform.rotation = Random.rotation;
            rb.AddForce(Vector3.up * upwardForce, ForceMode.Impulse);
            Vector3 randomTorque = Random.insideUnitSphere * upwardForce * 100f;
            rb.AddTorque(randomTorque, ForceMode.Impulse);

            if (resultText != null)
            {
                resultText.text = "Lanzando...";
            }

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
        yield return new WaitForSeconds(1f);
        while (!rb.IsSleeping())
        {
            yield return null;
        }

        hasLanded = true;
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

        // Cargar escena correspondiente usando NodeSceneManager (opcional, si se usa)
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
                Debug.LogWarning("No se asignaron escenas para el resultado " + coinSide);
            }
        }
        else
        {
            Debug.LogWarning("No se encontró NodeSceneManager. Asigna una escena en caraSceneName/cruzSceneName o agrega NodeSceneManager.");
        }
    }
}
