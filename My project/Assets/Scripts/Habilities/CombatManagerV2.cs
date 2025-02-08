using UnityEngine;

public class CombatManagerV2 : MonoBehaviour
{
    // Transform donde se instanciará el dino activo en combate.
    public Transform spawnPoint;

    private void Start()
    {
        // Verifica que exista un TeamManager persistente.
        if (TeamManager.Instance != null)
        {
            // Asigna el spawn point al TeamManager para que lo use al instanciar.
            TeamManager.Instance.spawnPoint = spawnPoint;

            // Llama al método que se encarga de instanciar el dino en primera posición.
            TeamManager.Instance.SpawnFrontDino();
        }
        else
        {
            Debug.LogWarning("CombatManagerV2: TeamManager no está presente en la escena.");
        }
    }
}