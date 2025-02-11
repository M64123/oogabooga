using UnityEngine;

public class CloseAlbumButton : MonoBehaviour
{
    public AlbumHUDController albumHUDController;

    public void OnClickCloseAlbum()
    {
        albumHUDController.HideAlbum();
    }
}
