using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AlbumSlot : MonoBehaviour
{
    // ID del dino asignado a este slot (se asigna automáticamente desde el GameManager)
    public string dinoID;

    // Elementos UI para mostrar la imagen y el nombre (usando Text Mesh Pro para el texto)
    public Image dinoImage;
    public TMP_Text dinoNameText;

    // Se invoca cuando el dino está desbloqueado
    public void ShowDino()
    {
        gameObject.SetActive(true);
        // Aquí podrías asignar la imagen (si cuentas con la fuente de sprite) y demás datos.
    }

    // Se invoca cuando el dino aún no está desbloqueado
    public void HideDino()
    {
        gameObject.SetActive(false);
        // Alternativamente, en lugar de desactivar el slot, podrías asignar un placeholder.
    }
}
