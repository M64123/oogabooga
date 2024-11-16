using UnityEngine;
using UnityEngine.SceneManagement;

public class OpenDinoShowcaseButton : MonoBehaviour
{
    public void OnButtonClick()
    {
        SceneManager.LoadScene("DinoShowcase");
    }
}
