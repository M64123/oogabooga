using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToBoardButton : MonoBehaviour
{
    public void OnButtonClick()
    {
        SceneManager.LoadScene("Tablero");
    }
}
