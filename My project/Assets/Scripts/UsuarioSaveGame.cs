using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;


public class UsuarioSaveGame : MonoBehaviour
{
    SaveGame saveGame;

    // Start is called before the first frame update
    void Start()
    {
        saveGame = new SaveGame();

        saveGame.iD = 1;
        saveGame.Name = "myName";

        //To Json
        string curretSaveGame = JsonUtility.ToJson(saveGame, true);


        //Se lee el fichero
        //bla, bla
        //FromJson
        SaveGame mySaveGame = JsonUtility.FromJson<SaveGame>(curretSaveGame);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
