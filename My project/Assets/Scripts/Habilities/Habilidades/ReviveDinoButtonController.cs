using UnityEngine;
using UnityEngine.UI;

public class ReviveDinoButtonController : MonoBehaviour
{
    // Referencia al botón (debe estar en el prefab, ya sea en este mismo GameObject o en uno de sus hijos).
    private Button reviveButton;
    // Referencia al componente DinoImage que almacena el ID (y otros datos) del dino.
    private DinoImage dinoImageComponent;

    private void Awake()
    {
        // Busca el botón en este GameObject o en sus hijos.
        reviveButton = GetComponentInChildren<Button>();
        dinoImageComponent = GetComponent<DinoImage>();

        if (reviveButton != null)
        {
            reviveButton.onClick.AddListener(OnReviveButtonClicked);
        }
        else
        {
            Debug.LogWarning("ReviveDinoButtonController: No se encontró el componente Button en este objeto.");
        }
    }

    private void OnReviveButtonClicked()
    {
        if (CoinManager.Instance == null)
        {
            Debug.LogError("ReviveDinoButtonController: CoinManager no existe.");
            return;
        }

        // Verifica que el jugador tenga al menos 200 monedas.
        if (CoinManager.Instance.coinCount >= 200)
        {
            // Gasta 200 monedas.
            CoinManager.Instance.SpendCoins(200);

            // Obtén el ID del dino desde el componente DinoImage.
            if (dinoImageComponent != null)
            {
                string dinoID = dinoImageComponent.dinoID;
                // Revive el dino en el GameManager.
                GameManager.Instance.ReviveDino(dinoID);
                Debug.Log($"ReviveDinoButtonController: Dino con ID {dinoID} revivido.");

                // Opcional: Elimina esta imagen de la UI, ya que el dino ha sido revivido.
                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning("ReviveDinoButtonController: No se encontró el componente DinoImage para obtener el dinoID.");
            }
        }
        else
        {
            Debug.Log("ReviveDinoButtonController: No tienes suficientes monedas para revivir el dino.");
        }
    }
}