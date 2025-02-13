using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MoveToDeathScene : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void movetoDeathScene()
    {
        SceneManager.LoadScene("DeathScene");
    }
    public void movetoPickTeam()
    {
        SceneManager.LoadScene("PickTeam");
    }
    public void movetotablero()
    {
        SceneManager.LoadScene("Tablero");
    }
}
