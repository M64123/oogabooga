using UnityEngine;
using UnityEngine.UI;

public class ResetButtonState : MonoBehaviour
{
    public Button myButton;

    // Llamar a este método cuando se reactive el canvas o en el Update
    public void ResetState()
    {
        // Deshabilita y vuelve a habilitar el botón para forzar la actualización de su estado visual.
        myButton.enabled = false;
        myButton.enabled = true;
    }
}