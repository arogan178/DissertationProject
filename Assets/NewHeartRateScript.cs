using Assets;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using NativeWebSocket;
using TMPro;
using Unity.Notifications.Android;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;
using System.Text;

public class NewHeartRateScript : MonoBehaviour
{
    // HypeRate Script
    // Put your websocket Token ID here
    private string websocketToken =
        "m7UHVlEja1eYu8wy6igRcUy1hzcUuovL8sNY8xNwz45rdlp0IiGm8SP2yG6xzqEz"; //You don't have one, get it here https://www.hyperate.io/api
    public string hyperateID;

    public TMP_Text hrText;
    public TMP_Text rmssdText;
    public TMP_Text rrIntervalText;

    // Websocket for connection with Hyperate
    WebSocket websocket;

    // End of HypeRate Script
    private DateTime serverModelDate;
    private DateTime localModelDate;
    public double[] ANNmodelWeights = new double[27];

    List<int> prevStates = new List<int>();
    public double maxRRValue;
    public double minRRValue;

    public int HRValue;
    public double maxHRValue;
    public double minHRValue;
    public double maxHRChange;

    private int numOfPlayedGames;
    private int dataLinesNum;

    [SerializeField]
    private DNNScript dNN;
    public bool minMaxSet;
    public bool isInitialized;
    private double[] downloadedWeightsANNSGD;
    private DownloadModel downloadedData;

    private int startingHr = 0;

    private List<int> RRReadings = new List<int>();
    private List<int> hrReadings = new List<int>();

    [SerializeField]
    public float readingDelay = 20f;

    [SerializeField]
    private float rrReadingsLimit = 30f;

    [SerializeField]
    private float hrReadingsLimit = 120f;

    private float elapsedTime;
    public int outputClass = 3;

    private TrackSetup trackSetup;
    TrackSwitcher trackSwitcher;

    private List<string> affectiveStatesList = new List<string>();
    private List<double[]> winningAffectiveStatesList = new List<double[]>();
    private List<double> rmssdList = new List<double>();
    public List<double[]> trainDataList = new List<double[]>();
    public List<double[]> testDataList = new List<double[]>();

    Thread trainingThread;
    BaseLineAcquisition baseLineAcquisition;

    List<int> checkStateHr = new List<int>();
    List<int> checkStateRR = new List<int>();

    public int currentState = 3;
    public bool trainingStarted = false;
    public bool trainingComplete = false;

    public bool allWeightsAreZero = false;
    public bool newUser = false;

    private string activePlayer = String.Empty;
    public int globalDataAmount = 0;
    public int localDataAmount = 0;

    //HypeRate Script
    async void Start()
    {
        GetDeviceID();
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
                HRValue = (int)msg["payload"]["hr"];
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

    async void OnApplicationQuit()
    {
        await websocket.Close();
        dNN.WriteAnalytics(trainDataList, testDataList);
        UpdateServerWeights();
    }

    public void GetDeviceID()
    {
        // Get the name of the active player from the player prefs
        string name = PlayerPrefs.GetString("ActivePlayer");

        // Return the device ID for the active player from the player prefs
        hyperateID = PlayerPrefs.GetString("DeviceID_" + name);
    }

    //End of Hyperate Script

    public void UpdateServerWeights()
    {
        StartCoroutine(UploadTrainData(trainDataList));
        StartCoroutine(UploadTestData(testDataList));
    }

    private void Awake()
    {
        GetDeviceID();
        websocket = new WebSocket("wss://app.hyperate.io/socket/websocket?token=" + websocketToken);
        Debug.Log("Connect!");

        //Remove all notifications sent
        AndroidNotificationCenter.CancelAllDisplayedNotifications();

        AndroidNotificationChannel channel1 = new AndroidNotificationChannel();
        channel1.Id = "1";
        channel1.Name = "Notifications 1";
        channel1.Description = "Affective State Triggered";
        channel1.EnableVibration = true;

        AndroidNotificationCenter.RegisterNotificationChannel(channel1);

        AndroidNotificationChannel channel2 = new AndroidNotificationChannel();
        channel2.Id = "2";
        channel2.Name = "Notifications 2";
        channel2.Description = "Affective State Triggered";
        channel2.EnableVibration = true;

        AndroidNotificationCenter.RegisterNotificationChannel(channel2);

        AndroidNotificationChannel channel3 = new AndroidNotificationChannel();
        channel3.Id = "3";
        channel3.Name = "Notifications 3";
        channel3.Description = "Affective State Triggered";
        channel3.EnableVibration = true;

        AndroidNotificationCenter.RegisterNotificationChannel(channel3);

        AndroidNotificationChannel channel4 = new AndroidNotificationChannel();
        channel4.Id = "4";
        channel4.Name = "Notifications 4";
        channel4.Description = "Affective State Triggered";
        channel4.EnableVibration = true;

        AndroidNotificationCenter.RegisterNotificationChannel(channel4);

        StartCoroutine(GetModelData(Initialize));
        trackSetup = UnityEngine.Object.FindObjectOfType<TrackSetup>();
        baseLineAcquisition = UnityEngine.Object.FindObjectOfType<BaseLineAcquisition>();
        trackSwitcher = UnityEngine.Object.FindObjectOfType<TrackSwitcher>();
        currentState = 3;
    }

    private void Initialize(DownloadModel downloadedData)
    {
        newUser = Convert.ToBoolean(PlayerPrefs.GetInt("NewPlayer"));
        if (downloadedData != null && !string.IsNullOrEmpty(downloadedData.ModelData))
        {
            Debug.Log("Downloaded Data is not null");

            DateTime serverModelDate = DateTime.ParseExact(
                downloadedData.Date,
                "ddd, dd MMM yyyy HH:mm:ss 'GMT'",
                CultureInfo.InvariantCulture
            );
            serverModelDate = Convert.ToDateTime(downloadedData.Date);

            globalDataAmount = downloadedData.totalDataItems;
            if (downloadedData.ModelData.Length > 1)
            {
                downloadedWeightsANNSGD = Array.ConvertAll(
                    downloadedData.ModelData.TrimStart('[').TrimEnd(']').Split(','),
                    Double.Parse
                );
            }
            else
            {
                downloadedWeightsANNSGD = new double[32];
            }
        }
        else
        {
            Debug.Log("Init Else");
            downloadedWeightsANNSGD = new double[27]; //27
        }
        if (newUser)
        {
            Debug.Log("New User");
            //downloadedWeightsANNSGD = new double[27]; //27
            dNN.InitializeModelANNBackProp(downloadedWeightsANNSGD);
            ANNmodelWeights = downloadedWeightsANNSGD;
        }
        else
        {
            activePlayer = PlayerPrefs.GetString("ActivePlayer");
            var localModelDateKey = activePlayer + "-localModelDate";
            var localData = Helpers.LoadModel(activePlayer);

            var localWeightsANNSGD = Array.ConvertAll(
                localData[0].modelData.Split(','),
                Double.Parse
            );

            Debug.Log("Local Data: " + localData.ToString());
            Debug.Log("localWeightsANNSGD: " + localWeightsANNSGD.ToString());

            try
            {
                localModelDate = DateTime.Parse(PlayerPrefs.GetString(localModelDateKey));
            }
            catch (Exception)
            {
                localModelDate = DateTime.Now.Date;
            }

            try
            {
                DateTimeOffset dateTimeOffset = DateTimeOffset.ParseExact(
                    downloadedData.Date,
                    "ddd, dd MMM yyyy HH:mm:ss 'GMT'",
                    CultureInfo.InvariantCulture
                );
                var serverDate = dateTimeOffset.DateTime;
                if (DateTime.Compare(serverModelDate, localModelDate) > 0)
                {
                    Debug.Log("Local Model Date: " + localModelDate);
                    Debug.Log("Server Model Date: " + serverModelDate);
                    Debug.Log(
                        "downloadedWeightsANNSGD: " + string.Join(", ", downloadedWeightsANNSGD)
                    );
                    //average the weights and initialise
                    var averagedModelWeights = Helpers.AverageWeights(
                        downloadedWeightsANNSGD,
                        localWeightsANNSGD
                    );
                    dNN.InitializeModelANNBackProp(averagedModelWeights);
                    ANNmodelWeights = averagedModelWeights;
                }
                else
                {
                    Debug.Log("Local Model Date > Server Model Date");
                    dNN.InitializeModelANNBackProp(localWeightsANNSGD);
                    ANNmodelWeights = localWeightsANNSGD;
                }
            }
            catch (FormatException)
            {
                // Handle the exception when the date string is in an incorrect format
                Debug.LogError("Incorrect date format for server date");
            }
            catch (ArgumentNullException)
            {
                // Handle the exception when the date string is null or empty
                Debug.LogError("Server date is null or empty");
            }
        }

        isInitialized = true;
        Invoke("ReadStartingHr", readingDelay);
        InvokeRepeating("ReadRRData", readingDelay, 1f);
        InvokeRepeating("ReadHRData", readingDelay, 1f);
        InvokeRepeating("CheckPlayerState", readingDelay, 1f);
    }

    public void FixedUpdate()
    {
        // If the elapsed time (time passed since the game started) is greater than the delay between readings
        if (elapsedTime > readingDelay)
        {
            if (RRReadings.Count == rrReadingsLimit)
            {
                // Store the list of RR readings in a separate list, then clear the RRReadings list
                // Calculate the root mean square of successive differences (RMSSD) for the RR readings
                // Add the calculated RMSSD to the rmssdList
                List<int> RRreadingsList = RRReadings;
                Debug.Log("Vitals - RRreadingsList: " + string.Join(",", RRreadingsList.ToArray()));

                RRReadings = new List<int>();
                double rmssd = Helpers.RMSSD(RRreadingsList);
                rmssdList.Add(rmssd);
                rmssdText.text = rmssd.ToString("F3");
                Debug.Log("Vitals - RRMSD: " + rmssd);

                // Calculate the winning affective state from the affectiveStatesList
                // Add the calculated winning affective state to the winningAffectiveStatesList
                // Clear the affectiveStatesList
                double[] winningStates = Helpers.WinningState(affectiveStatesList);
                winningAffectiveStatesList.Add(winningStates);
                affectiveStatesList = new List<string>();
            }

            // If the number of HR readings has reached the limit
            if (hrReadings.Count == hrReadingsLimit)
            {
                // Store the list of HR readings in a separate list, then clear the hrReadings list
                // Calculate the maximum heart rate change for the HR readings
                // Loop through the rmssdList
                Debug.Log("Vitals - RMSSD Count: " + rmssdList.Count);
                for (int i = 0; i < rmssdList.Count; i++)
                {
                    List<int> HRList = hrReadings;
                    maxHRChange = Helpers.HRChange(HRList, startingHr);
                    // If the baseline has not been gathered
                    // Create an array with the RMSSD value, the maximum HR change, and the winning affective state
                    double[] resultArray = new double[]
                    {
                        Math.Round(rmssdList[i], 2),
                        Math.Round(maxHRChange, 2),
                        winningAffectiveStatesList.Last()[0],
                        winningAffectiveStatesList.Last()[1]
                    };
                    if (!baseLineAcquisition.baselineGathered)
                    {
                        // Add the array to the trainDataList
                        trainDataList.Add(resultArray);
                        Debug.Log("Info - trainDataList Count" + trainDataList.Count);
                        Debug.Log(
                            "Info - trainDataList "
                                + JsonConvert.SerializeObject(trainDataList, Formatting.Indented)
                        );
                    }
                    // If the baseline has not been gathered
                }

                // Clear the lists
                hrReadings = new List<int>();
                rmssdList = new List<double>();
            }
        }
        else // If the elapsed time is less than the reading delay
        {
            // Increment the elapsed time by the time passed since the last frame (Time.deltaTime)
            elapsedTime = elapsedTime + Time.deltaTime;
        }
        // If the baseline has been gathered and the minimum and maximum values have not been set
        if (baseLineAcquisition.baselineGathered && !minMaxSet && trainDataList.Count > 0) //&& !dNN.trainingComplete && !trainingStarted)
        {
            List<double> minMaxValues;
            // Calculate the minimum and maximum values for the RMSSD and HR

            if (trainDataList.Count == 0)
            {
                throw new InvalidOperationException("Cannot perform operation on empty list");
            }
            else
            {
                minMaxValues = Helpers.MinMax(trainDataList);
            }

            maxRRValue = minMaxValues[0];
            minRRValue = minMaxValues[1];
            maxHRValue = minMaxValues[2];
            minHRValue = minMaxValues[3];

            minMaxSet = true;

            trainingStarted = true;
            if (trainingThread == null || !trainingThread.IsAlive)
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                string playerDataPath =
                    Application.persistentDataPath
                    + "/Users/"
                    + PlayerPrefs.GetString("ActivePlayer")
                    + ".txt";

                if (PlayerPrefs.HasKey(activePlayer + "-dataItems"))
                {
                    dataLinesNum = PlayerPrefs.GetInt(activePlayer + "-dataItems");
                    dataLinesNum += trainDataList.Count();
                    PlayerPrefs.SetInt(activePlayer + "-dataItems", dataLinesNum);
                }
                else
                {
                    dataLinesNum = trainDataList.Count();
                    PlayerPrefs.SetInt(activePlayer + "-dataItems", trainDataList.Count());
                }

                trainDataList = Helpers.NormalizeTrainingData(trainDataList);

                List<double[]> normalizedData = trainDataList;

                dict.Add("DataPath", playerDataPath);
                dict.Add("TrainingData", normalizedData);
                dict.Add("DownloadedWeightsANNSGD", downloadedWeightsANNSGD);
                dict.Add("GlobalDataAmount", globalDataAmount);
                dict.Add("LocalDataAmount", localDataAmount);
                dict.Add("StartingWeights", ANNmodelWeights.ToList());

                var playedGamesKey = activePlayer + "-GamesPlayed";

                dict.Add("NumOfPlayedGames", numOfPlayedGames);
                dict.Add("maxHR", maxHRValue);
                dict.Add("minHR", minHRValue);
                dict.Add("maxRR", maxRRValue);
                dict.Add("minRR", minRRValue);

                trainingThread = new Thread(dNN.TrainModel);
                trainingThread.Start(dict);
            }
        }
        if (!dNN.trainingComplete || trainingComplete)
        {
            return;
        }
        trainingComplete = true;
        activePlayer = PlayerPrefs.GetString("ActivePlayer");
        string localModelDateKey = activePlayer + "-localModelDate";
        string trainingIterationsKey = activePlayer + "-modelIterations";
        int modelIterations;

        if (!PlayerPrefs.HasKey(trainingIterationsKey))
        {
            modelIterations = 1;
        }
        else
        {
            modelIterations = PlayerPrefs.GetInt(trainingIterationsKey) + 1;
        }
        PlayerPrefs.SetInt(trainingIterationsKey, modelIterations);
        PlayerPrefs.SetString(localModelDateKey, DateTime.Now.ToString());
        if (modelIterations > 0 && modelIterations % 5 == 0)
        {
            PlayerPrefs.SetInt(activePlayer + "-dataItems", 0);
            if (trainingComplete && !dNN.weightsUploaded)
            {
                StartCoroutine(dNN.UploadNewWeightsANNSGD(localDataAmount + trainDataList.Count));
            }
        }
    }

    /*
    The ReadStartingHr() function is used to read the user's starting heart rate.
    If the application is running in an editor or is not connected to a device, a random heart rate value between 60 and 70 is assigned to startingHr.
    If the application is connected to a device, startingHr is set to the value of the heart rate data received from the device.
    */

    void ReadStartingHr()
    {
        startingHr = HRValue;
        Debug.Log("Vitals - Starting HR: " + startingHr);
    }

    //read the user's heart rate data.
    void ReadHRData()
    {
        hrText.text = HRValue.ToString();

        if (!Application.isEditor)
        {
            hrReadings.Add(HRValue);
        }
        else
        {
            if (trackSwitcher.switchVal != 0)
            {
                hrReadings.Add(UnityEngine.Random.Range(65, 80));
            }
            else
            {
                hrReadings.Add(UnityEngine.Random.Range(60, 70));
            }
        }

        //If the baseline is true and the training process is true, HR is added to the checkStateHr list.
        //if (baseLineAcquisition.baselineGathered && (dNN.trainingComplete || isInitialized) || isInitialized)
        if (baseLineAcquisition.baselineGathered && dNN.trainingComplete && HRValue != 0)
        {
            checkStateHr.Add(HRValue);
        }
    }

    /*
      The ReadRRData() function is used to read the user's respiratory rate data.
      If the baseline and  training  is true, RR is also added to the checkStateRR list.
      */
    void ReadRRData()
    {
        float RR = 0;

        if (!Application.isEditor)
        {
            RR = ((60 / (float)HRValue) * 1000);
        }
        else
        {
            if (trackSwitcher.switchVal != 0)
            {
                RR = UnityEngine.Random.Range(700, 850);
            }
            else
            {
                RR = UnityEngine.Random.Range(800, 1100);
            }
        }
        rrIntervalText.text = ((int)RR).ToString();

        //if (baseLineAcquisition.baselineGathered && (dNN.trainingComplete || isInitialized) || isInitialized)
        if (baseLineAcquisition.baselineGathered && dNN.trainingComplete)
        {
            checkStateRR.Add((int)RR);
        }

        RRReadings.Add((int)RR);
        affectiveStatesList.Add(Helpers.AffectiveState(trackSetup.trackId));
    }

    /*
   The CheckPlayerState() is used to check the user's current state based on their heart rate and respiratory rate data.
   If the baseline and  training process is complete,  function will execut every time the hrReadings list reaches its limit (30).
   Calculate RMSSD of the last 30 values in  checkStateRR list and  change in HR from the starting HR based on the last 30 values in the checkStateHr list.
   Values are then normalized and used as input to the neural network to predict the user's current state.
   The predicted state is then compared to the affective state of the current track to determine if the track should be changed.
   */
    private void CheckPlayerState()
    {
        if (
            !baseLineAcquisition.baselineGathered
            || !dNN.trainingComplete
            || (float)checkStateHr.Count != hrReadingsLimit
        )
        {
            return;
        }
        Debug.Log("In Check State");
        // Take the last 30 elements of checkStateRR list and calculate RMSSD
        // Calculate change in HR between startingHr and HR readings in checkStateHr list
        double rmssd = Helpers.RMSSD(
            checkStateRR.Skip(Math.Max(0, checkStateRR.Count() - 30)).ToList()
        );
        double hrChange = Helpers.HRChange(checkStateHr, startingHr);
        // Normalize rmssd and hrChange using minRRValue, maxRRValue, minHRValue, maxHRValue
        double[] normalizedInput = Helpers.NormalizeData(
            new double[2] { Math.Round(rmssd, 2), Math.Round(hrChange, 2) },
            minRRValue,
            maxRRValue,
            minHRValue,
            maxHRValue
        );

        // Compute outputs of dNN and convert to classes
        double[] result = dNN.neuralNetworkBackProp.ComputeOutputs(normalizedInput);
        result = Helpers.ProbsToClasses(result);
        // Convert affective state values to doubles
        double[] affectState = Array.ConvertAll(
            Helpers.AffectiveState(trackSetup.track).Split(','),
            Double.Parse
        );
        //Convert list to array
        List<double> resList = new List<double>()
        {
            normalizedInput[0],
            normalizedInput[1],
            affectState[0],
            affectState[1]
        };
        double[] resultArr = resList.ToArray();
        //Add the result array to the testDataList.
        testDataList.Add(resultArr);

        //Compare the values in the result array and set the output variable to 0 or 1 accordingly.
        int output = ((!(result[0] < result[1])) ? 1 : 0);

        if (baseLineAcquisition.baselineGathered)
        {
            if (prevStates.Count < 2)
            {
                prevStates.Add(output);
            }
            else if (prevStates[0] == output && prevStates[1] == output)
            {
                output = ((output == 0) ? 1 : 0);
                prevStates[0] = prevStates[1];
                prevStates[1] = output;
            }
            else
            {
                prevStates[0] = prevStates[1];
                prevStates[1] = output;
            }
        }
        currentState = output;
        if (currentState == 0)
        {
            trackSetup.trackId = 1;
            SendVibrationMessageToSmartWatch();
        }
        else if (currentState == 1)
        {
            trackSetup.trackId = 0;
            SendVibrationMessageToSmartWatch();
        }
        //Clear the checkStateRR and checkStateHr lists.
        checkStateRR = new List<int>();
        checkStateHr = new List<int>();
        Debug.Log("TestDataList - " + Newtonsoft.Json.JsonConvert.SerializeObject(testDataList));
        Debug.Log("TrainDataList - " + Newtonsoft.Json.JsonConvert.SerializeObject(trainDataList));
    }

    void SendVibrationMessageToSmartWatch()
    {
        var notification = new AndroidNotification();
        notification.Title = "";
        notification.Text = "";
        notification.FireTime = System.DateTime.Now;

        var notification1 = new AndroidNotification();
        notification1.Title = "";
        notification1.Text = "";
        notification1.FireTime = System.DateTime.Now.AddSeconds(1);

        var notification2 = new AndroidNotification();
        notification2.Title = "";
        notification2.Text = "";
        notification2.FireTime = System.DateTime.Now.AddSeconds(2);

        var notification3 = new AndroidNotification();
        notification3.Title = "";
        notification3.Text = "";
        notification3.FireTime = System.DateTime.Now.AddSeconds(3);

        AndroidNotificationCenter.SendNotification(notification, "1");
        AndroidNotificationCenter.SendNotification(notification1, "2");
        AndroidNotificationCenter.SendNotification(notification2, "3");
        AndroidNotificationCenter.SendNotification(notification3, "4");
    }

    public IEnumerator UploadTrainData(List<double[]> trainDataList)
    {
        string uploadString = string.Empty;

        foreach (double[] trainDataItem in trainDataList)
        {
            string tempString =
                trainDataItem[0]
                + ","
                + trainDataItem[1]
                + ","
                + trainDataItem[2]
                + ","
                + trainDataItem[3]
                + ";";
            uploadString += tempString;
        }

        UnityWebRequest req = UnityWebRequest.Get(
            "http://arogan178.eu.pythonanywhere.com/api/Data/PostTrainData?data="
                + uploadString
                + "&mode=train"
        );

        yield return req.SendWebRequest();
    }

    public IEnumerator UploadTestData(List<double[]> testDataList)
    {
        string uploadString = string.Empty;
        foreach (double[] testDataItem in testDataList)
        {
            string tempString =
                testDataItem[0]
                + ","
                + testDataItem[1]
                + ","
                + testDataItem[2]
                + ","
                + testDataItem[3]
                + ";";
            uploadString += tempString;
        }
        UnityWebRequest req = UnityWebRequest.Get(
            "http://arogan178.eu.pythonanywhere.com/api/Data/PostTrainData?data="
                + uploadString
                + "&mode=test"
        );

        yield return req.SendWebRequest();
    }

    IEnumerator GetModelData(System.Action<DownloadModel> callbackOnFinish)
    {
        using (
            UnityWebRequest webRequest = UnityWebRequest.Get(
                "http://arogan178.eu.pythonanywhere.com/api/Test"
            )
        )
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.LogError(webRequest.error);
                var emptyData = new DownloadModel();
                Initialize(emptyData);
            }
            else
            {
                var json = webRequest.downloadHandler.text;
                if (string.IsNullOrEmpty(json))
                {
                    //create an empty object
                    var emptyData = new DownloadModel();
                    Initialize(emptyData);
                }
                else
                {
                    var downloadedData = JsonConvert.DeserializeObject<DownloadModel>(json);
                    Initialize(downloadedData);
                }
            }
            Debug.Log("Text" + webRequest.downloadHandler.text);
            Debug.Log("Network Error " + webRequest.isNetworkError);
            Debug.Log("HTTP Error " + webRequest.isHttpError);
        }
    }

    [System.Serializable]
    public class DownloadModel
    {
        public string Date;
        public double Error;
        public string ModelData;
        public int totalDataItems;
    }

    public class HyperateResponse
    {
        public string Event { get; set; }
        public string Payload { get; set; }
        public string Ref { get; set; }
        public string Topic { get; set; }
    }
}
