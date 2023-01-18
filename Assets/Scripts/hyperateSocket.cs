using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using NativeWebSocket;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Management;
using TMPro;

public class hyperateSocket : MonoBehaviour
{
    // Put your websocket Token ID here
    public string websocketToken =
        "m7UHVlEja1eYu8wy6igRcUy1hzcUuovL8sNY8xNwz45rdlp0IiGm8SP2yG6xzqEz"; //You don't have one, get it here https://www.hyperate.io/api
    public string hyperateID;

    // Textbox to display your heart rate in
    public Text textBox;

    // Websocket for connection with Hyperate
    WebSocket websocket;
    public Text avgHRTextBox;
    public Text rrIntervalTextBox;
    public int HRValue;
    public int HRValueCount = 0;
    public int avgHR = 0;
    public float RRInterval = 0;
    public int inputValueCount = 0;

    public Button nextSceneButton;
    public TMP_Text promptText;

    private void Awake()
    {
        GetDeviceID();
    }

    async void Start()
    {
        //hyperateID = GetDeviceID();
        //StartCoroutine(GetHRData());
        websocket = new WebSocket("wss://app.hyperate.io/socket/websocket?token=" + websocketToken);
        Debug.Log("Connect!");

        websocket.OnOpen += () =>
        {
            Debug.Log("Connection open!");
            SendWebSocketMessage();
        };

        websocket.OnError += (e) =>
        {
            Debug.Log("Error! " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");
        };

        websocket.OnMessage += (bytes) =>
        {
            // getting the message as a string
            var message = System.Text.Encoding.UTF8.GetString(bytes);
            var msg = JObject.Parse(message);

            if (msg["event"].ToString() == "hr_update")
            {
                // Change textbox text into the newly received Heart Rate (integer like "86" which represents beats per minute)
                textBox.text = (string)msg["payload"]["hr"];
                HRValue = int.Parse(textBox.text);

                // Calculate the average HR
                avgHR = (avgHR * HRValueCount + HRValue) / (HRValueCount + 1);
                avgHRTextBox.text = (avgHR).ToString();
                HRValueCount++;

                // Calculate the RR interval from the HR value
                RRInterval = (60.0f / HRValue);
                rrIntervalTextBox.text = RRInterval.ToString("F2");
                enableButton();
            }
        };

        // Send heartbeat message every 25seconds in order to not suspended the connection
        InvokeRepeating("SendHeartbeat", 1.0f, 25.0f);

        // waiting for messages
        await websocket.Connect();
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket.DispatchMessageQueue();
#endif
    }

    async void SendWebSocketMessage()
    {
        if (websocket.State == WebSocketState.Open)
        {
            // Log into the "internal-testing" channel
            await websocket.SendText(
                "{\"topic\": \"hr:"
                    + hyperateID
                    + "\", \"event\": \"phx_join\", \"payload\": {}, \"ref\": 0}"
            );
        }
    }

    async void SendHeartbeat()
    {
        if (websocket.State == WebSocketState.Open)
        {
            // Send heartbeat message in order to not be suspended from the connection
            await websocket.SendText(
                "{\"topic\": \"phoenix\",\"event\": \"heartbeat\",\"payload\": {},\"ref\": 0}"
            );
        }
    }

    private async void OnApplicationQuit()
    {
        await websocket.Close();
    }

    void GetDeviceID()
    {
        // Get the name of the active player from the player prefs
        string name = PlayerPrefs.GetString("ActivePlayer");

        // Return the device ID for the active player from the player prefs
        hyperateID = PlayerPrefs.GetString("DeviceID_" + name);
    }

    void enableButton()
    {
        if (HRValue > 45)
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

public class HyperateResponse
{
    public string Event { get; set; }
    public string Payload { get; set; }
    public string Ref { get; set; }
    public string Topic { get; set; }
}
