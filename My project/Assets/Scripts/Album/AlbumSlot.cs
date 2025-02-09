using UnityEngine;
using UnityEngine.UI;

public class AlbumSlot : MonoBehaviour
{
    public string dinoID; // ID asignado a este slot
    public Image dinoImage;
    public Text dinoNameText;
    public Text dinoStatsText;

    // Método para mostrar los datos del dino (esto puede adaptarse a tu lógica)
    public void ShowDino()
    {
        gameObject.SetActive(true);
        // Aquí podrías asignar la imagen, nombre y estadísticas obtenidas del GameManager o una base de datos.
    }

    // Método para ocultar o dejar en blanco el slot.
    public void HideDino()
    {
        gameObject.SetActive(false);
    }
}
