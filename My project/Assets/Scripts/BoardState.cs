using System.Collections.Generic;

[System.Serializable]
public class BoardState
{
    public string playerNodeID; // Cambiado de Vector3 a string
    public List<string> activeNodes; // Agregado para almacenar IDs de nodos activos

    public BoardState()
    {
        playerNodeID = string.Empty;
        activeNodes = new List<string>();
    }

    public BoardState(string playerNodeID, List<string> activeNodes)
    {
        this.playerNodeID = playerNodeID;
        this.activeNodes = new List<string>(activeNodes);
    }
}
