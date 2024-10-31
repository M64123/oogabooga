using UnityEngine;

public class InputTest : MonoBehaviour
{
    void Update()
    {
        if (Input.anyKeyDown)
        {
            Debug.Log("Se ha presionado una tecla");
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Tecla Espacio detectada en InputTest");
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("Tecla F detectada en InputTest");
        }
    }
}
