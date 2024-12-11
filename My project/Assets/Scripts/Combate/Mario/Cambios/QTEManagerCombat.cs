using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class QTEManagerCombat : MonoBehaviour
{
    public static QTEManagerCombat Instance { get; private set; }

    [Header("QTE Settings")]
    public KeyCode qteKey = KeyCode.Space;
    public GameObject qteIndicatorPrefab;
    public RectTransform qteBarTransform;
    public RectTransform referenceImage;
    public RectTransform leftSpawnPoint;
    public RectTransform rightSpawnPoint;
    public float activeZoneWidth = 100f;

    [Header("QTE Appearance Probabilities")]
    [Range(0, 100)] public int doubleQTEProbability = 10; // 10% para QTE dobles
    [Range(0, 100)] public int tripleQTEProbability = 5;  // 5% para QTE triples

    [Header("QTE Events")]
    public UnityEvent onQTESuccess;
    public UnityEvent onQTEFail;

    [Header("Streak Settings")]
    public int currentStreak = 0;
    public int maxStreak = 5;
    public Image streakIndicator;
    public List<Sprite> streakSprites;

    private List<Pair<QTEIndicator>> activeQTEPairs = new List<Pair<QTEIndicator>>();
    private bool isQTEInProgress = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
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

        onQTESuccess.AddListener(NotifyActiveDino);
    }

    void Update()
    {
        if (Input.GetKeyDown(qteKey))
        {
            ProcessQTEHit();
        }
    }

    void ProcessQTEHit()
    {
        foreach (var pair in activeQTEPairs)
        {
            if (pair.Left.IsInActiveZone() && pair.Right.IsInActiveZone())
            {
                OnQTESuccess(pair);
                return;
            }
        }
    }

    public void HandleBeat()
    {
        if (isQTEInProgress) return;

        int count = DetermineQTECount();
        StartCoroutine(ActivateQTEPair(count));
    }

    int DetermineQTECount()
    {
        int randomValue = Random.Range(0, 100);
        if (randomValue < tripleQTEProbability)
        {
            return 3; // QTE triple
        }
        else if (randomValue < doubleQTEProbability + tripleQTEProbability)
        {
            return 2; // QTE doble
        }
        return 1; // QTE individual
    }

    IEnumerator ActivateQTEPair(int count)
    {
        isQTEInProgress = true;

        for (int i = 0; i < count; i++)
        {
            GameObject leftQTEObject = Instantiate(qteIndicatorPrefab, qteBarTransform);
            GameObject rightQTEObject = Instantiate(qteIndicatorPrefab, qteBarTransform);

            QTEIndicator leftQTE = leftQTEObject.GetComponent<QTEIndicator>();
            QTEIndicator rightQTE = rightQTEObject.GetComponent<QTEIndicator>();

            if (leftQTE != null && rightQTE != null)
            {
                leftQTE.Setup(qteKey, activeZoneWidth, leftSpawnPoint, referenceImage, true);
                rightQTE.Setup(qteKey, activeZoneWidth, rightSpawnPoint, referenceImage, false);

                activeQTEPairs.Add(new Pair<QTEIndicator>(leftQTE, rightQTE));
            }
            else
            {
                Debug.LogError("El prefab de QTEIndicator no tiene el componente QTEIndicator.");
            }

            yield return new WaitForSeconds(0.15f);
        }

        isQTEInProgress = false;
    }

    public void OnQTESuccess(Pair<QTEIndicator> pair)
    {
        if (activeQTEPairs.Contains(pair))
        {
            activeQTEPairs.Remove(pair);
            Destroy(pair.Left.gameObject);
            Destroy(pair.Right.gameObject);
        }

        currentStreak++;
        UpdateStreakIndicator();

        onQTESuccess.Invoke();
    }

    public void OnQTEFail(QTEIndicator qteIndicator)
    {
        Pair<QTEIndicator> pairToRemove = null;

        foreach (var pair in activeQTEPairs)
        {
            if (pair.Left == qteIndicator || pair.Right == qteIndicator)
            {
                pairToRemove = pair;
                break;
            }
        }

        if (pairToRemove != null)
        {
            activeQTEPairs.Remove(pairToRemove);
            Destroy(pairToRemove.Left.gameObject);
            Destroy(pairToRemove.Right.gameObject);
        }

        currentStreak = 0;
        UpdateStreakIndicator();

        onQTEFail.Invoke();
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

    void NotifyActiveDino()
{
    DinoCombat activeDino = Combatgrid.Instance.GetFirstSlotDino();
    if (activeDino != null)
    {
        activeDino.ExecuteAttack(); // Ejecuta el ataque
        Debug.Log($"{activeDino.name} realizó un ataque tras acertar el QTE.");
    }
    else
    {
        Debug.LogWarning("No se encontró dinosaurio activo en el primer slot.");
    }
}

    public class Pair<T>
    {
        public T Left { get; }
        public T Right { get; }

        public Pair(T left, T right)
        {
            Left = left;
            Right = right;
        }
    }
}