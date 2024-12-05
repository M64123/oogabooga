using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class QTEManagerCombat : MonoBehaviour
{
    public static QTEManagerCombat Instance { get; private set; }

    [Header("QTE Settings")]
    public KeyCode qteKey = KeyCode.Space; // La tecla que el jugador debe presionar
    public GameObject qteIndicatorPrefab; // Prefab del indicador QTE (asegúrate de que tenga un RectTransform)
    public RectTransform qteBarTransform; // Transform de la barra QTE (debe ser un RectTransform)
    public List<RectTransform> qteSpawnPoints; // Puntos desde los que pueden nacer los QTE (debe ser RectTransform para elementos de UI)
    public RectTransform referenceImage; // Imagen de referencia para detectar la zona de éxito
    public float activeZoneWidth = 100f; // Ancho de la zona de activación para el QTE
    public int requiredPressCount = 3; // Cantidad de pulsaciones requeridas

    [Header("QTE Events")]
    public UnityEvent onQTESuccess; // Evento que se dispara en éxito
    public UnityEvent onQTEFail;    // Evento que se dispara en fallo

    [Header("Streak Settings")]
    public int currentStreak = 0; // Contador de racha
    public int maxStreak = 5; // Máxima racha para cambiar feedback visual
    public Image streakIndicator; // Imagen para reflejar el cambio visual de la racha
    public List<Sprite> streakSprites; // Lista de Sprites para cada racha

    [Header("Audio Feedback")]
    public AudioClip successClip;
    public AudioClip failClip;
    private AudioSource audioSource;

    private List<QTEIndicator> activeQTEs = new List<QTEIndicator>(); // Lista para manejar QTEs activos
    private bool isQTEInProgress = false;

    void Awake()
    {
        // Implementación del Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // Inicializar AudioSource para feedback auditivo
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Suscribirse a los intervalos del BeatManager
        if (BeatManagerCombat.Instance != null)
        {
            foreach (Interval interval in BeatManagerCombat.Instance.intervals)
            {
                interval.onIntervalTrigger.AddListener(HandleBeat);
            }
        }
        else
        {
            Debug.LogError("No se encontró BeatManager en la escena.");
        }
    }

    public void HandleBeat()
    {
        if (isQTEInProgress) return;

        // Generar el número de QTEs basado en probabilidades
        int qteCount = GetRandomQTECount();

        // Activar los QTEs basados en la cantidad requerida
        StartCoroutine(ActivateQTEs(qteCount));
    }

    int GetRandomQTECount()
    {
        float randomValue = Random.value; // Genera un número aleatorio entre 0 y 1

        if (randomValue < 0.7f)
        {
            // 70% de probabilidad para un solo QTE
            return 1;
        }
        else if (randomValue < 0.9f)
        {
            // 20% de probabilidad para dos QTEs
            return 2;
        }
        else
        {
            // 10% de probabilidad para tres QTEs
            return 3;
        }
    }

    IEnumerator ActivateQTEs(int count)
    {
        isQTEInProgress = true;

        // Crear múltiples QTEs si la cantidad de pulsaciones es mayor a 1
        for (int i = 0; i < count; i++)
        {
            GameObject qteIndicatorObject = Instantiate(qteIndicatorPrefab, qteBarTransform);
            QTEIndicator qteIndicator = qteIndicatorObject.GetComponent<QTEIndicator>();

            if (qteIndicator != null)
            {
                // Elegir un punto de nacimiento aleatorio de la lista de puntos de inicio disponibles
                RectTransform chosenSpawnPoint = qteSpawnPoints[Random.Range(0, qteSpawnPoints.Count)];

                // Usar la posición de la UI para RectTransform
                Vector2 startPosition = chosenSpawnPoint.anchoredPosition;

                // Configurar el indicador con los parámetros necesarios
                qteIndicator.Setup(qteKey, activeZoneWidth, startPosition, referenceImage);
                activeQTEs.Add(qteIndicator);
            }
            else
            {
                Debug.LogError("El prefab de QTEIndicator no tiene el componente QTEIndicator.");
            }

            // Retraso entre la creación de los QTEs para simular la separación visual
            yield return new WaitForSeconds(0.15f);
        }

        isQTEInProgress = false;
    }

    public void OnQTESuccess(QTEIndicator qteIndicator)
    {
        currentStreak++;
        UpdateStreakIndicator();

        onQTESuccess.Invoke();
        if (successClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(successClip);
        }

        if (currentStreak >= maxStreak)
        {
            Debug.Log("¡Ataque Potenciado!");
        }

        // Eliminar el QTEIndicator exitoso de la lista
        activeQTEs.Remove(qteIndicator);
        Destroy(qteIndicator.gameObject);
    }

    public void OnQTEFail(QTEIndicator qteIndicator)
    {
        currentStreak = 0;
        UpdateStreakIndicator();

        onQTEFail.Invoke();
        if (failClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(failClip);
        }

        // Eliminar el QTEIndicator fallido de la lista
        activeQTEs.Remove(qteIndicator);
        Destroy(qteIndicator.gameObject);
    }

    void UpdateStreakIndicator()
    {
        if (streakIndicator == null || streakSprites == null || streakSprites.Count == 0) return;

        if (currentStreak >= maxStreak)
        {
            streakIndicator.sprite = streakSprites[maxStreak - 1];
        }
        else if (currentStreak > 0)
        {
            streakIndicator.sprite = streakSprites[currentStreak - 1];
        }
        else
        {
            streakIndicator.sprite = streakSprites[0];
        }
    }

    public QTEIndicator GetFirstQTEInActiveZone()
    {
        // Filtrar los QTEIndicators que están en la zona activa y retornar el más cercano
        QTEIndicator firstQTE = null;
        foreach (var qte in activeQTEs)
        {
            if (qte.IsInActiveZone())
            {
                if (firstQTE == null || qte.GetPosition().x < firstQTE.GetPosition().x)
                {
                    firstQTE = qte;
                }
            }
        }
        return firstQTE;
    }
}