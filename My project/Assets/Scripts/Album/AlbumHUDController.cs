using UnityEngine;
using UnityEngine.UI;

public class AlbumHUDController : MonoBehaviour
{
    // Panel que contiene el álbum (debe cubrir toda la pantalla)
    public GameObject albumPanel;
    // CanvasGroup opcional para controlar la interactividad y bloquear raycasts
    public CanvasGroup albumCanvasGroup;

    void Start()
    {
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

    // Oculta el álbum y permite volver a interactuar con la escena
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
