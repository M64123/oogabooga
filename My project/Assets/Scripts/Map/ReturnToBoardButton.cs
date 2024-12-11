// ReturnToBoardButton.cs
using UnityEngine;

public class ReturnToBoardButton : MonoBehaviour
{
    public void ReturnToBoard()
    {
        if (NodeSceneManager.Instance != null)
        {
            NodeSceneManager.Instance.LoadBoardScene();
        }
        else
        {
            Debug.LogError("NodeSceneManager no está presente.");
        }
    }
}