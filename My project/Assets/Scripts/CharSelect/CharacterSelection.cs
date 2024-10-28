using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelection : MonoBehaviour
{
    public TMP_Text Name;
    public GameObject[] characters;
    public int selectedCharacter = 0;
    public string[] characterNames;

    public Camera mainCamera;
    public Vector3 cameraOffset;
    public float cameraMoveDuration = 0.5f;
    public Vector3 originalCameraPosition;

    public Button cameraMoveButton;

    public void NextCharacter()
    {
        characters[selectedCharacter].SetActive(false);
        selectedCharacter = (selectedCharacter + 1) % characters.Length;
        characters[selectedCharacter].SetActive(true);
        UpdateCharacterName();
    }

    public void PreviousCharacter()
    {
        characters[selectedCharacter].SetActive(false);
        selectedCharacter--;
        if (selectedCharacter < 0)
        {
            selectedCharacter += characters.Length;
        }
        characters[selectedCharacter].SetActive(true);
        UpdateCharacterName();
    }

    private void UpdateCharacterName()
    {
        // Ensure the characterNames array is not empty
        if (characterNames != null && characterNames.Length > 0 && selectedCharacter < characterNames.Length)
        {
            Name.text = characterNames[selectedCharacter];  // Set the displayed name to the selected character's name
        }
    }

    public void StartGame()
    {
        PlayerPrefs.SetInt("selectedCharacter", selectedCharacter);

    }


    public IEnumerator CameraMoveFeedback()
    {
        Vector3 targetPosition = originalCameraPosition + cameraOffset;

        float elapsedTime = 0f;
        while (elapsedTime < cameraMoveDuration)
        {
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPosition, elapsedTime / cameraMoveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        mainCamera.transform.position = targetPosition;

        yield return new WaitForSeconds(0.2f);

        elapsedTime = 0f;
        while (elapsedTime < cameraMoveDuration)
        {
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, originalCameraPosition, elapsedTime / cameraMoveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.position = originalCameraPosition;
    }

    public void OnCameraMoveButtonPressed()
    {
        StartCoroutine(CameraMoveFeedback());
    }
}