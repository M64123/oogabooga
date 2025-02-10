using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AlbumSlot : MonoBehaviour
{
    // ID del dino asignado a este slot (debe coincidir con el que se guarda en el JSON)
    public string dinoID;

    // Referencias a los elementos UI que mostrarán la imagen y el nombre
    public Image dinoImage;
    public TMP_Text dinoNameText;

    // Método para mostrar el slot (por ejemplo, asignar datos extra si se requiere)
    public void ShowDino()
    {
        gameObject.SetActive(true);
        // Aquí puedes asignar la imagen o el nombre si lo obtienes de otra fuente.
    }

    // Método para ocultar el slot (o mostrar un placeholder)
    public void HideDino()
    {
        gameObject.SetActive(false);
    }
}
