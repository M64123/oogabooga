using UnityEngine;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System;

public static class SaveLoadManager
{
    private static string saveFilePath = Path.Combine(Application.persistentDataPath, "gamestate.json");
    private static string saveFileChecksumPath = Path.Combine(Application.persistentDataPath, "gamestate_checksum.txt");

    public static void SaveGame(GameState gameState)
    {
        try
        {
            // Serializar GameState a JSON
            string jsonData = JsonUtility.ToJson(gameState);

            // Guardar los datos JSON en el archivo
            File.WriteAllText(saveFilePath, jsonData);

            // Generar checksum para seguridad
            string checksum = GenerateChecksum(jsonData);

            // Guardar el checksum en un archivo separado
            File.WriteAllText(saveFileChecksumPath, checksum);

            Debug.Log("Juego guardado en " + saveFilePath);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error al guardar el juego: " + ex.Message);
        }
    }

    public static GameState LoadGame()
    {
        if (File.Exists(saveFilePath) && File.Exists(saveFileChecksumPath))
        {
            try
            {
                // Leer los datos JSON guardados
                string jsonData = File.ReadAllText(saveFilePath);

                // Leer el checksum guardado
                string savedChecksum = File.ReadAllText(saveFileChecksumPath);

                // Verificar el checksum
                string calculatedChecksum = GenerateChecksum(jsonData);

                if (savedChecksum != calculatedChecksum)
                {
                    Debug.LogError("El archivo de guardado está corrupto o ha sido manipulado. Carga abortada.");
                    return null;
                }

                // Deserializar los datos JSON en un objeto GameState
                GameState gameState = JsonUtility.FromJson<GameState>(jsonData);

                Debug.Log("Juego cargado desde " + saveFilePath);
                return gameState;
            }
            catch (Exception ex)
            {
                Debug.LogError("Error al cargar el juego: " + ex.Message);
                return null;
            }
        }
        else
        {
            Debug.LogWarning("No se encontró un archivo de guardado en " + saveFilePath);
            return null;
        }
    }

    public static void DeleteSave()
    {
        try
        {
            if (File.Exists(saveFilePath))
            {
                File.Delete(saveFilePath);
            }
            if (File.Exists(saveFileChecksumPath))
            {
                File.Delete(saveFileChecksumPath);
            }
            Debug.Log("Archivo de guardado eliminado.");
        }
        catch (Exception ex)
        {
            Debug.LogError("Error al eliminar el archivo de guardado: " + ex.Message);
        }
    }

    private static string GenerateChecksum(string data)
    {
        // Usar SHA256 para generar un checksum
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            byte[] hashBytes = sha256.ComputeHash(dataBytes);
            return Convert.ToBase64String(hashBytes);
        }
    }
}
