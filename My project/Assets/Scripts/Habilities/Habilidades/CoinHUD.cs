using UnityEngine;
using UnityEngine.UI;

public class CoinHUD : MonoBehaviour
{
    [Header("Referencia al UI Text para las monedas")]
    public Text coinText;

    void Start()
    {
        UpdateCoinDisplay();
    }

    void Update()
    {
        // Actualiza cada frame; si prefieres, podrías suscribirte a un evento del CoinManager para actualizar solo cuando cambie el valor.
        UpdateCoinDisplay();
    }

    /// <summary>
    /// Actualiza el texto del UI con la cantidad de monedas.
    /// </summary>
    void UpdateCoinDisplay()
    {
        if (coinText != null && CoinManager.Instance != null)
        {
            coinText.text =  CoinManager.Instance.coinCount.ToString();
        }
    }
}