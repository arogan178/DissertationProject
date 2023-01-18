using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using Google.XR.Cardboard;

public class SwitchToVR : MonoBehaviour
{
    //void start()
    //{
    /* if (XRGeneralSettings.Instance.Manager.activeLoader != null)
    {
        XRGeneralSettings.Instance.Manager.activeLoader.Stop();
        XRGeneralSettings.Instance.Manager.activeLoader.Deinitialize();
        Debug.Log("XR stopped completely.");
    } */
    // } // Start is called before the first frame update

    public void button()
    {
        
        if (Api.HasNewDeviceParams())
        {
            Api.ReloadDeviceParams();
        }
        StartCoroutine(SwitchToVRMode());
    }
/*  public void LoadGameScene()
    {
        bool isRandom = Convert.ToBoolean(PlayerPrefs.GetInt("isRandom"));
        if (!isRandom)
        {
            SceneManager.LoadScene("AffectiveGame");
        }
        else
        {
            SceneManager.LoadScene("RandomGame");
        }
    } */
    private IEnumerator SwitchToVRMode()
    {
        yield return XRGeneralSettings.Instance.Manager.InitializeLoader();

        Debug.Log(XRGeneralSettings.Instance.Manager.InitializeLoader());

        XRGeneralSettings.Instance.Manager.InitializeLoaderSync();
        Debug.Log("XR initialized.");

        Debug.Log("Starting XR...");
        XRGeneralSettings.Instance.Manager.StartSubsystems();
        Debug.Log("XR started.");
        SceneManager.LoadScene("AffectiveGame");
    }
}
