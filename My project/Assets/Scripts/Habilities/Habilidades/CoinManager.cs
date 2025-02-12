using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    // Cantidad actual de monedas del jugador.
    public int coinCount = 0;

    private void Awake()
    {
        // Implementación del patrón Singleton.
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persistente entre escenas.
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Agrega monedas al total.
    /// </summary>
    /// <param name="amount">Cantidad de monedas a agregar.</param>
    public void AddCoins(int amount)
    {
        coinCount += amount;
        Debug.Log("Se han añadido " + amount + " monedas. Total: " + coinCount);
    }

    /// <summary>
    /// Intenta gastar una cantidad de monedas.
    /// Devuelve true si se pudo gastar, false si no hay suficientes monedas.
    /// </summary>
    /// <param name="amount">Cantidad de monedas a gastar.</param>
    public bool SpendCoins(int amount)
    {
        if (coinCount >= amount)
        {
            coinCount -= amount;
            Debug.Log("Se han gastado " + amount + " monedas. Total: " + coinCount);
            return true;
        }
        else
        {
            Debug.Log("No hay suficientes monedas para gastar " + amount + ". Total: " + coinCount);
            return false;
        }
    }
}