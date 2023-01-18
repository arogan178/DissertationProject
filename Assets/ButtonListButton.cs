using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonListButton : MonoBehaviour
{
    [SerializeField]
    private Text myText;

    public void SetText(string textString)
    {
        myText.text = textString;
        
    }

    public void OnClick(Button btn)
    {
        PlayerPrefs.SetString("ActivePlayer", btn.GetComponentInChildren<Text>().text);
        PlayerPrefs.SetInt("NewPlayer", 0);
        SceneManager.LoadScene("SearchDevices");
        //SceneManager.LoadScene("TransitionScene");
    }
}
