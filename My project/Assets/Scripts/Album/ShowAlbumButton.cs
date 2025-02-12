using UnityEngine;

public class ShowAlbumButton : MonoBehaviour
{
    // Asigna en el Inspector el objeto que contiene AlbumHUDController
    public AlbumHUDController albumHUDController;

    // Esta función se llama al pulsar el botón
    public void OnClickShowAlbum()
    {
        Debug.Log("OnClickShowAlbum() called.");
        if (albumHUDController != null)
        {
            albumHUDController.ShowAlbum();
        }
    }
}
