using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PlayerSceneManagerScript : MonoBehaviour
{
    public TMP_InputField playerNameField;
    public TMP_InputField deviceIdField;
    public GameObject buttonScrollList;
    public GameObject btnTemplate;

    [SerializeField]
    ButtonListControl btnListControl;

    /// <summary>
    /// SWITCH ON ONLY FOR DEBUGGING
    /// </summary>
    /// <param name="button"></param>
    //public void onAwake()
    //{
    //    PlayerPrefs.DeleteAll();
    //}

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
        var name = playerNameField.text;
        var deviceID = deviceIdField.text;

        /*         if(name != "")
                {
                    string userPath = Application.persistentDataPath + "/Users/"+name+".txt";
                    StreamWriter combinedDf = File.CreateText(userPath);
                    combinedDf.Close();
        
                    PlayerPrefs.SetString("ActivePlayer", name);
                    //CloseNewUserPanel(button);
                    //btnListControl.RefreshList(Application.persistentDataPath + "/Users");
                    PlayerPrefs.SetInt("NewPlayer", 1);
                } */

        if (name != "")
        {
            string userPath = Application.persistentDataPath + "/Users/" + name + ".txt";
            StreamWriter combinedDf = File.CreateText(userPath);
            combinedDf.Close();

            PlayerPrefs.SetString("ActivePlayer", name);
            //CloseNewUserPanel(button);
            //btnListControl.RefreshList(Application.persistentDataPath + "/Users");
            PlayerPrefs.SetInt("NewPlayer", 1);
            PlayerPrefs.SetString("DeviceID_" + name, deviceID);

            SceneManager.LoadScene("SearchDevices");
            //SceneManager.LoadScene("TransitionScene");
        }
        //Debug.Log(PlayerPrefs.ToString());
    }
}
