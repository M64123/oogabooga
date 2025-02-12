using UnityEngine;
using UnityEngine.UI;

public class DeadDinoDisplay : MonoBehaviour
{
    [Header("Referencias UI")]
    public Image dinoImage;       // Imagen que representará al dino.
    public Text dinoIDText;       // Texto para mostrar el ID.

    /// <summary>
    /// Configura la visualización del dino muerto usando los datos.
    /// </summary>
    public void SetData(DinoData data)
    {
        if (data == null)
        {
            Debug.LogWarning("DeadDinoDisplay: DinoData es null.");
            return;
        }

        if (dinoIDText != null)
        {
            dinoIDText.text = data.dinoID;
        }

        if (dinoImage != null)
        {
            if (data.dinoSprite != null)
                dinoImage.sprite = data.dinoSprite;
            else
                Debug.LogWarning("DeadDinoDisplay: dinoSprite es null para el ID " + data.dinoID);
        }
    }
}