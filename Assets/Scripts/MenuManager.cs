using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Management;

public class MenuManager : MonoBehaviour
{
    public void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            // Esc key is pressed, load previous scene
            if (PlayerPrefs.HasKey("Name"))
            {
                PlayerPrefs.Save();
            }
            Application.Quit();
        }
    }

    public void LoadVR()
    {
        PlayerPrefs.SetInt("isRandom", 0);
        SceneManager.LoadScene("ChoosePlayerScene");
    }

    public void NonAdaptiveGame()
    {
        PlayerPrefs.SetInt("isRandom", 1);
        StartCoroutine(StartXR());
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

            SceneManager.LoadScene("RandomGame");
        }
        Debug.Log(XRGeneralSettings.Instance);
        Debug.Log(XRGeneralSettings.Instance.Manager);
    }

    async void OnApplicationQuit()
    {
        if (PlayerPrefs.HasKey("Name"))
        {
            PlayerPrefs.Save();
        }
    }

    public void Quit()
    {
        Application.Quit();
    }
}
