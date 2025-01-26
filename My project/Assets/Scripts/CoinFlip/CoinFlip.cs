using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CoinFlip : MonoBehaviour
{
    private Rigidbody rb;
    private bool hasLanded = false;
    private bool thrown = false;

    [Header("Force Settings")]
    public float upwardForce = 5f; // Fuerza inicial de 5

    [Header("Result")]
    public string coinSide; // "Cara" o "Cruz"
    public Text resultText; // Opcional: asigna el elemento de texto en el inspector

    public bool IsThrown
    {
        get { return thrown; }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Asegurar sin gravedad al inicio
        rb.useGravity = false;

        // Configurar la detección de colisiones y la interpolación
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

            // Rotación inicial aleatoria
            transform.rotation = Random.rotation;

            // Aplicar fuerza hacia arriba
            rb.AddForce(Vector3.up * upwardForce, ForceMode.Impulse);

            // Aplicar torque aleatorio (ajusta el multiplicador según necesites)
            Vector3 randomTorque = Random.insideUnitSphere * upwardForce * 100f;
            rb.AddTorque(randomTorque, ForceMode.Impulse);

            if (resultText != null)
            {
                resultText.text = "Lanzando...";
            }

            // Iniciar la rutina para checar cuándo aterriza
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
        // Esperar 1 seg antes de verificar
        yield return new WaitForSeconds(1f);

        // Esperar hasta que la moneda se detenga
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

        // Lógica según resultado
        HandleResult();
    }

    private void HandleResult()
    {
        if (coinSide == "Cara")
        {
            // Ir a la escena de Gacha
            NodeSceneManager.Instance.LoadGachaScene();
        }
        else if (coinSide == "Cruz")
        {
            // Volver al tablero directamente
            NodeSceneManager.Instance.LoadBoardScene();
        }
    }
}
