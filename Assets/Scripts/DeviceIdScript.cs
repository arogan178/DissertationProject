using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class DeviceIdScript : MonoBehaviour
{
    public TMP_InputField iField;
    public GameObject buttonScrollList;
    public GameObject btnTemplate;

    public static DeviceIdScript SynchHR;

    public string deviceID;

    [SerializeField]
    ButtonListControl btnListControl;

    /// <summary>
    /// SWITCH ON ONLY FOR DEBUGGING
    /// </summary>
    /// <param deviceID="button"></param>
    //public void onAwake()
    //{
    //    PlayerPrefs.DeleteAll();
    //}

    private void Awake()
    {
        if (SynchHR == null)
        {
            SynchHR = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OpenNewUserPanel(Button button)
    {
        button.gameObject.transform.GetChild(1).gameObject.SetActive(true);
    }

    public void CloseNewUserPanel(Button button)
    {
        button.gameObject.transform.parent.gameObject.SetActive(false);
    }

    public void CreateNewUser(Button button)
    {
        var deviceID = iField.text;

        SceneManager.LoadSceneAsync("SearchDevices");
    }
}
