using UnityEngine;
using UnityEngine.UI;

public class AlbumHUDController : MonoBehaviour
{
    // Panel que contiene el álbum; este debe cubrir toda la pantalla
    public GameObject albumPanel;
    // CanvasGroup opcional para gestionar la interactividad (bloquea raycasts)
    public CanvasGroup albumCanvasGroup;

    void Start()
    {
        // Inicia oculto
        HideAlbum();
    }

    // Muestra el álbum de forma modal
    public void ShowAlbum()
    {
        albumPanel.SetActive(true);
        if (albumCanvasGroup != null)
        {
            albumCanvasGroup.interactable = true;
            albumCanvasGroup.blocksRaycasts = true;
        }
    }

    // Oculta el álbum y permite la interacción con el resto de la escena
    public void HideAlbum()
    {
        albumPanel.SetActive(false);
        if (albumCanvasGroup != null)
        {
            albumCanvasGroup.interactable = false;
            albumCanvasGroup.blocksRaycasts = false;
        }
    }
}
