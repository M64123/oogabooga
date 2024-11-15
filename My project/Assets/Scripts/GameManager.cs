using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    // Singleton
    public static GameManager Instance;

    // Equipo de dinosaurios
    public List<Dinosaur> playerDinosaurs = new List<Dinosaur>(); // Dinosaurios que el jugador posee

    // Estado del tablero
    public BoardState boardState;

    void Awake()
    {
        // Implementación del Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // No destruir al cambiar de escena
            InitializeGameData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeGameData()
    {
        // Inicializar o resetear datos al inicio de una partida
        playerDinosaurs = new List<Dinosaur>();
        boardState = new BoardState();
    }

    // Métodos para gestionar los dinosaurios
    public void AddDinosaur(Dinosaur newDino)
    {
        playerDinosaurs.Add(newDino);
    }

    // Métodos para gestionar el estado del tablero
    public void SaveBoardState(BoardState state)
    {
        boardState = state;
    }
}
