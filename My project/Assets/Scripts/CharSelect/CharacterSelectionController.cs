using UnityEngine;
using System.Collections;
using TMPro;

public class CharacterSelectionController : MonoBehaviour
{
    [Header("Characters")]
    public GameObject[] characters;
    public int selectedCharacterIndex = 0;
    public string[] characterNames;
    public TMP_Text characterNameText;

    [Header("Camera Settings")]

    // Posiciones y rotaciones de la cámara
    [Tooltip("Posición y rotación inicial de la cámara")]
    public Vector3 initialCameraPosition = new Vector3(0f, 10f, -10f);
    public Vector3 initialCameraRotation = new Vector3(45f, 0f, 0f);

    [System.Serializable]
    public class CameraPosition
    {
        public Vector3 position;
        public Vector3 rotation;
    }

    [Tooltip("Posiciones y rotaciones de la cámara para cada personaje")]
    public CameraPosition[] characterCameraPositions;

    public float cameraTransitionDuration = 1f;

    [Header("Screen Zones")]
    [Range(0f, 1f)]
    public float bottomZoneThreshold = 0.1f;
    [Range(0f, 1f)]
    public float leftZoneThreshold = 0.1f;
    [Range(0f, 1f)]
    public float rightZoneThreshold = 0.9f;

    private Camera mainCamera;
    private bool isCameraTransitioning = false;
    private bool isInInitialPosition = true;

    void Start()
    {
        mainCamera = Camera.main;

        // Establecer la posición inicial de la cámara
        mainCamera.transform.position = initialCameraPosition;
        mainCamera.transform.eulerAngles = initialCameraRotation;
        isInInitialPosition = true;

        // Asegurarse de que todos los personajes estén activos
        foreach (var character in characters)
        {
            character.SetActive(true);
        }

        UpdateCharacterName();
    }

    void Update()
    {
        if (!isCameraTransitioning)
        {
            DetectScreenZones();
        }
    }

    void DetectScreenZones()
    {
        Vector3 mousePos = Input.mousePosition;
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // Zona inferior
        if (mousePos.y <= screenHeight * bottomZoneThreshold)
        {
            if (!isInInitialPosition)
            {
                MoveCameraToInitialPosition();
            }
        }
        else if (mousePos.x <= screenWidth * leftZoneThreshold)
        {
            // Cambiar al personaje anterior
            SelectPreviousCharacter();
        }
        else if (mousePos.x >= screenWidth * rightZoneThreshold)
        {
            // Cambiar al siguiente personaje
            SelectNextCharacter();
        }
    }

    public void SelectNextCharacter()
    {
        if (!isCameraTransitioning)
        {
            int previousCharacterIndex = selectedCharacterIndex;
            selectedCharacterIndex = (selectedCharacterIndex + 1) % characters.Length;
            StartCoroutine(MoveCameraToCharacterPosition(selectedCharacterIndex));
            UpdateCharacterName();
        }
    }

    public void SelectPreviousCharacter()
    {
        if (!isCameraTransitioning)
        {
            int previousCharacterIndex = selectedCharacterIndex;
            selectedCharacterIndex = (selectedCharacterIndex - 1 + characters.Length) % characters.Length;
            StartCoroutine(MoveCameraToCharacterPosition(selectedCharacterIndex));
            UpdateCharacterName();
        }
    }

    IEnumerator MoveCameraToCharacterPosition(int characterIndex)
    {
        if (characterIndex >= 0 && characterIndex < characterCameraPositions.Length)
        {
            isCameraTransitioning = true;
            isInInitialPosition = false;

            Vector3 targetPosition = characterCameraPositions[characterIndex].position;
            Vector3 targetRotation = characterCameraPositions[characterIndex].rotation;

            yield return StartCoroutine(MoveCamera(targetPosition, targetRotation));

            isCameraTransitioning = false;
        }
        else
        {
            Debug.LogWarning("No se ha definido una posición de cámara para este personaje.");
        }
    }

    void MoveCameraToInitialPosition()
    {
        if (!isCameraTransitioning)
        {
            StartCoroutine(MoveCamera(initialCameraPosition, initialCameraRotation));
            isInInitialPosition = true;
        }
    }

    IEnumerator MoveCamera(Vector3 targetPosition, Vector3 targetRotation)
    {
        isCameraTransitioning = true;

        Vector3 startPos = mainCamera.transform.position;
        Vector3 startRot = mainCamera.transform.eulerAngles;
        Vector3 endPos = targetPosition;
        Vector3 endRot = targetRotation;

        float elapsedTime = 0f;
        while (elapsedTime < cameraTransitionDuration)
        {
            float t = elapsedTime / cameraTransitionDuration;
            mainCamera.transform.position = Vector3.Lerp(startPos, endPos, t);
            mainCamera.transform.eulerAngles = Vector3.Lerp(startRot, endRot, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.position = endPos;
        mainCamera.transform.eulerAngles = endRot;

        isCameraTransitioning = false;
    }

    private void UpdateCharacterName()
    {
        if (characterNames != null && characterNames.Length > 0 && selectedCharacterIndex < characterNames.Length)
        {
            characterNameText.text = characterNames[selectedCharacterIndex];
        }
    }

    public void OnCharacterClicked(int characterIndex)
    {
        if (!isCameraTransitioning)
        {
            if (characterIndex != selectedCharacterIndex)
            {
                selectedCharacterIndex = characterIndex;
                StartCoroutine(MoveCameraToCharacterPosition(characterIndex));
                UpdateCharacterName();
            }
            else if (isInInitialPosition)
            {
                // Si hacemos clic en el personaje seleccionado y estamos en la posición inicial, acercamos la cámara
                StartCoroutine(MoveCameraToCharacterPosition(characterIndex));
            }
        }
    }

    public void StartGame()
    {
        PlayerPrefs.SetInt("selectedCharacter", selectedCharacterIndex);
        // Cargar la siguiente escena o iniciar el juego
    }
}
