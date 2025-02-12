using UnityEngine;
using UnityEngine.UI;

public class AlbumHUDController : MonoBehaviour
{
    [Header("Album Panel Settings")]
    // Panel que cubre toda la pantalla (se asigna en el Inspector)
    public GameObject albumPanel;
    // CanvasGroup opcional para controlar la interactividad y bloquear raycasts
    public CanvasGroup albumCanvasGroup;

    void Start()
    {
        // Iniciamos el álbum oculto
        HideAlbum();
    }

    // Función pública sin parámetros para mostrar el álbum
    public void ShowAlbum()
    {
        Debug.Log("ShowAlbum() called.");
        if (albumPanel != null)
        {
            albumPanel.SetActive(true);
        }
        if (albumCanvasGroup != null)
        {
            albumCanvasGroup.interactable = true;
            albumCanvasGroup.blocksRaycasts = true;
        }
    }

    // Función pública sin parámetros para ocultar el álbum
    public void HideAlbum()
    {
        Debug.Log("HideAlbum() called.");
        if (albumPanel != null)
        {
            albumPanel.SetActive(false);
        }
        if (albumCanvasGroup != null)
        {
            albumCanvasGroup.interactable = false;
            albumCanvasGroup.blocksRaycasts = false;
        }
    }
}
