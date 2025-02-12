using UnityEngine;
using UnityEngine.UI;

public class DeadDinoDisplay : MonoBehaviour
{
    [Header("Referencias UI")]
    public Image dinoImage;       // Imagen que representará al dino.
    public Text dinoIDText;       // Texto para mostrar el ID (o nombre) del dino.

    /// <summary>
    /// Configura la visualización del dino muerto usando los datos.
    /// </summary>
    /// <param name="data">Datos del dino muerto.</param>
    public void SetData(GameManager.DinoData data)
    {
        if (data == null)
        {
            Debug.LogWarning("DeadDinoDisplay: DinoData es null.");
            return;
        }

        if (dinoIDText != null)
        {
            dinoIDText.text = data.dinoID; // O puedes usar data.dinoName si prefieres mostrar el nombre.
        }

        
    }
}