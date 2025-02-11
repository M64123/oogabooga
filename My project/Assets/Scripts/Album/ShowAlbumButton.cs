using UnityEngine;

public class ShowAlbumButton : MonoBehaviour
{
    public AlbumHUDController albumHUDController;

    public void OnClickShowAlbum()
    {
        albumHUDController.ShowAlbum();
    }
}
