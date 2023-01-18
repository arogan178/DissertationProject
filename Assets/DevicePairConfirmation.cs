using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Management;
using TMPro;

public class DevicePairConfirmation : MonoBehaviour
{
    public Button nextSceneButton;
    public Text textBox;
    public TMP_Text promptText;
    public Canvas canvas;
    public int inputValueCount = 0;

    void Start()
    {
        // Start the UpdateButtonInteractability coroutine
        StartCoroutine(UpdateButtonInteractability());
    }

    IEnumerator UpdateButtonInteractability()
    {
        yield return new WaitForSeconds(2);
        while (true)
        {
            // Wait for 1 second
            yield return new WaitForSeconds(1);

            // Convert the input field text to an integer
            int inputValue = int.Parse(textBox.text);

            // If the input value is greater than 45, increment the input value count
            if (inputValue > 45)
            {
                inputValueCount++;
            }
            // If the input value is not greater than 45, reset the input value count
            else
            {
                inputValueCount = 0;
            }

            if (
                Screen.orientation == ScreenOrientation.LandscapeLeft
                || Screen.orientation == ScreenOrientation.LandscapeRight
            )
            {
                // Hide the prompt
                promptText.gameObject.SetActive(false);
                // Set the button to be interactable if the input value count is 5 or more
                nextSceneButton.interactable = (inputValueCount >= 5);
            }
            else
            {
                // Show the prompt
                promptText.gameObject.SetActive(true);
                promptText.text = "Please turn your phone to landscape mode to continue.";
                //nextSceneButton.interactable = false;
                nextSceneButton.interactable = (inputValueCount >= 5);
            }
        }
    }

    public void EnterVR()
    {
        promptText.gameObject.transform.parent.gameObject.SetActive(false);
        StartCoroutine(StartXR());
        /*         if (Api.HasNewDeviceParams())
                {
                    Api.ReloadDeviceParams();
                } */
    }

    private IEnumerator StartXR()
    {
        Debug.Log("Initializing XR...");
        yield return XRGeneralSettings.Instance.Manager.InitializeLoader();

        if (XRGeneralSettings.Instance.Manager.activeLoader == null)
        {
            Debug.LogError("Initializing XR Failed.");
        }
        else
        {
            Debug.Log("XR initialized.");

            Debug.Log("Starting XR...");
            XRGeneralSettings.Instance.Manager.StartSubsystems();
            Debug.Log("XR started.");

            SceneManager.LoadScene("AffectiveGame");
        }
        Debug.Log(XRGeneralSettings.Instance);
        Debug.Log(XRGeneralSettings.Instance.Manager);
    }
}
