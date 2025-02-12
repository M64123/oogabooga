using UnityEngine;

public class CloseAlbumButton : MonoBehaviour
{
    // Asigna en el Inspector el objeto que contiene AlbumHUDController
    public AlbumHUDController albumHUDController;

    // Esta función se llama al pulsar el botón de cerrar
    public void OnClickCloseAlbum()
    {
        Debug.Log("OnClickCloseAlbum() called.");
        if (albumHUDController != null)
        {
            albumHUDController.HideAlbum();
        }
    }
}
