/* // Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// NewHeartRateScript
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NewHeartRateScript : MonoBehaviour
{
    public class DataItem
    {
        public string data { get; set; }

        public double error { get; set; }

        public int totalDataItems { get; set; }
    }

    public class DownloadModel
    {
        public string date { get; set; }

        public string models { get; set; }
    }

    private DateTime serverModelDate;

    private double[] ANNmodelWeights = new double[27];

    private List<int> prevStates = new List<int>();

    public double maxRRValue;

    public double minRRValue;

    public double maxHRValue;

    public double minHRValue;

    private int numOfPlayedGames;

    private int dataLinesNum;

    [SerializeField]
    private DNNScript dNN;

    public bool minMaxSet;

    public bool isInitialized;

    private double[] downloadedWeightsANNSGD;

    private DownloadModel downloadedData;

    private int startingHr;

    private char[] unitChars = new char[5] { 'b', 'p', 'm', 's', ' ' };

    private List<int> RRReadings = new List<int>();

    private List<int> hrReadings = new List<int>();

    [SerializeField]
    public float readingDelay = 20f;

    [SerializeField]
    private float rrReadingsLimit = 30f;

    [SerializeField]
    private float hrReadingsLimit = 120f;

    private float elapsedTime;

    private int outputClass = 3;

    private int outputHR;

    private int outputRR;

    private TrackSetup trackSetup;

    private TrackSwitcher trackSwitcher;

    private List<string> affectiveStatesList = new List<string>();

    private List<double[]> winningAffectiveStatesList = new List<double[]>();

    private List<double> rmssdList = new List<double>();

    public List<double[]> trainingDataList = new List<double[]>();

    public List<double[]> testDataList = new List<double[]>();

    public List<double[]> uploadTestDataList = new List<double[]>();

    private Thread trainingThread;

    private BaseLineAcquisition baseLineAcquisition;

    private List<int> checkStateHr = new List<int>();

    private List<int> checkStateRR = new List<int>();

    public int currentState = 3;

    private bool trainingStarted;

    public bool trainingComplete;

    public bool allWeightsAreZero;

    public bool newUser;

    private string activePlayer = string.Empty;

    public int globalDataAmount;

    public int localDataAmount;

    private StreamWriter combinedUserDf;

    public Text hrText;

    public Text rmssdText;

    public Text resText;

    private void OnDestroy()
    {
        string @string = PlayerPrefs.GetString("ActivePlayer");
        string path =
            Application.persistentDataPath
            + "/Users/"
            + @string
            + "/combinedReadingsNormalized.txt";
        if (!File.Exists(path))
        {
            combinedUserDf = File.CreateText(path);
        }
        else
        {
            combinedUserDf = File.AppendText(path);
        }
        foreach (double[] item in Helpers.NormalizeTrainingData(trainingDataList))
        {
            string value = string.Join(",", item);
            combinedUserDf.WriteLine(value);
        }
        combinedUserDf.Close();
    }

    public void UpdateServerWeights()
    {
        dNN.WriteAnalytics(trainingDataList, testDataList);
        StartCoroutine(UploadTrainData(trainingDataList));
        StartCoroutine(UploadTestData(testDataList));
    }

    private void Awake()
    {
        StartCoroutine(GetModelData(Initialize));
        trackSetup = UnityEngine.Object.FindObjectOfType<TrackSetup>();
        baseLineAcquisition = UnityEngine.Object.FindObjectOfType<BaseLineAcquisition>();
        trackSwitcher = UnityEngine.Object.FindObjectOfType<TrackSwitcher>();
        currentState = 3;
    }

    private void Initialize(DownloadModel downloadedData)
    {
        newUser = Convert.ToBoolean(PlayerPrefs.GetInt("NewPlayer"));
        if (downloadedData != null || downloadedData.models != string.Empty)
        {
            serverModelDate = Convert.ToDateTime(downloadedData.date);
            List<DataItem> list = JsonConvert.DeserializeObject<List<DataItem>>(
                downloadedData.models
            );
            globalDataAmount = list.First().totalDataItems;
            if (list.Count > 1)
            {
                if (string.IsNullOrEmpty(list.Last().data))
                {
                    downloadedWeightsANNSGD = new double[32];
                }
                else
                {
                    downloadedWeightsANNSGD = Array.ConvertAll(
                        list.Last().data.Split(','),
                        double.Parse
                    );
                }
            }
        }
        else
        {
            downloadedWeightsANNSGD = new double[27];
        }
        if (newUser)
        {
            dNN.InitializeModelANNBackProp(downloadedWeightsANNSGD);
            ANNmodelWeights = downloadedWeightsANNSGD;
        }
        else
        {
            activePlayer = PlayerPrefs.GetString("ActivePlayer");
            string key = activePlayer + "-modelDownloadedDate";
            double[] array = Array.ConvertAll(
                Helpers.LoadModel(activePlayer)[3].modelData.Split(','),
                double.Parse
            );
            DateTime date;
            try
            {
                date = Convert.ToDateTime(PlayerPrefs.GetString(key)).Date;
            }
            catch (Exception)
            {
                date = DateTime.Now.Date;
            }
            Convert.ToDateTime(downloadedData.date);
            if (DateTime.Compare(serverModelDate, date) > 0)
            {
                double[] array2 = Helpers.AverageWeights(downloadedWeightsANNSGD, array);
                dNN.InitializeModelANNBackProp(array2);
                ANNmodelWeights = array2;
            }
            else
            {
                dNN.InitializeModelANNBackProp(array);
                ANNmodelWeights = array;
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
        if (elapsedTime > readingDelay)
        {
            if ((float)RRReadings.Count == rrReadingsLimit)
            {
                List<int> rRReadings = RRReadings;
                RRReadings = new List<int>();
                double item = Helpers.RMSSD(rRReadings);
                rmssdList.Add(item);
                double[] item2 = Helpers.WinningState(affectiveStatesList);
                winningAffectiveStatesList.Add(item2);
                affectiveStatesList = new List<string>();
            }
            if ((float)hrReadings.Count == hrReadingsLimit)
            {
                List<int> hrList = hrReadings;
                hrReadings = new List<int>();
                double num = Helpers.HRChange(hrList, startingHr);
                for (int i = 0; i < rmssdList.Count; i++)
                {
                    double[] item3 = new double[4]
                    {
                        Math.Round(rmssdList[i], 2),
                        Math.Round(num, 2),
                        winningAffectiveStatesList.Last()[0],
                        winningAffectiveStatesList.Last()[1]
                    };
                    if (!baseLineAcquisition.baselineGathered)
                    {
                        trainingDataList.Add(item3);
                    }
                }
                rmssdList = new List<double>();
            }
        }
        else
        {
            elapsedTime += Time.deltaTime;
        }
        if (baseLineAcquisition.baselineGathered && !minMaxSet)
        {
            List<double> list = Helpers.MinMax(trainingDataList);
            maxRRValue = list[0];
            minRRValue = list[1];
            maxHRValue = list[2];
            minHRValue = list[3];
            minMaxSet = true;
            trainingStarted = true;
            if (trainingThread == null || !trainingThread.IsAlive)
            {
                Dictionary<string, object> dictionary = new Dictionary<string, object>();
                string value =
                    Application.persistentDataPath
                    + "/Users/"
                    + PlayerPrefs.GetString("ActivePlayer")
                    + ".txt";
                if (PlayerPrefs.HasKey(activePlayer + "-dataItems"))
                {
                    dataLinesNum = PlayerPrefs.GetInt(activePlayer + "-dataItems");
                    dataLinesNum += trainingDataList.Count();
                    PlayerPrefs.SetInt(activePlayer + "-dataItems", dataLinesNum);
                }
                else
                {
                    dataLinesNum = trainingDataList.Count();
                    PlayerPrefs.SetInt(activePlayer + "-dataItems", trainingDataList.Count());
                }
                trainingDataList = Helpers.NormalizeTrainingData(trainingDataList);
                List<double[]> value2 = trainingDataList;
                dictionary.Add("DataPath", value);
                dictionary.Add("TrainingData", value2);
                dictionary.Add("DownloadedWeightsANNSGD", downloadedWeightsANNSGD);
                dictionary.Add("GlobalDataAmount", globalDataAmount);
                dictionary.Add("LocalDataAmount", localDataAmount);
                dictionary.Add("StartingWeights", ANNmodelWeights.ToList());
                _ = activePlayer + "-GamesPlayed";
                dictionary.Add("NumOfPlayedGames", numOfPlayedGames);
                trainingThread = new Thread(dNN.TrainModel);
                trainingThread.Start(dictionary);
            }
        }
        if (!dNN.trainingComplete || trainingComplete)
        {
            return;
        }
        trainingComplete = true;
        activePlayer = PlayerPrefs.GetString("ActivePlayer");
        string key = activePlayer + "-modelDownloadedDate";
        string key2 = activePlayer + "-modelIterations";
        int num2 = 0;
        num2 = ((!PlayerPrefs.HasKey(key2)) ? 1 : (PlayerPrefs.GetInt(key2) + 1));
        PlayerPrefs.SetInt(key2, num2);
        PlayerPrefs.SetString(key, DateTime.Now.ToString());
        if (num2 > 0 && num2 % 10 == 0)
        {
            PlayerPrefs.SetInt(activePlayer + "-dataItems", 0);
            if (trainingComplete && !dNN.weightsUploaded)
            {
                StartCoroutine(
                    dNN.UploadNewWeightsANNSGD(localDataAmount + trainingDataList.Count)
                );
            }
        }
    }

    private void ReadStartingHr()
    {
        if (Application.isEditor || !MultiConnection.singleton.connected)
        {
            startingHr = UnityEngine.Random.Range(60, 70);
        }
        else
        {
            startingHr = Convert.ToInt32(MultiConnection.singleton.hrText.TrimEnd(unitChars));
        }
    }

    private void ReadHRData()
    {
        int num = 0;
        num = (
            outputHR = (
                (!Application.isEditor && MultiConnection.singleton.connected)
                    ? Convert.ToInt32(MultiConnection.singleton.hrText.TrimEnd(unitChars))
                    : (
                        (trackSwitcher.switchVal != 0)
                            ? UnityEngine.Random.Range(65, 80)
                            : UnityEngine.Random.Range(60, 70)
                    )
            )
        );
        hrReadings.Add(num);
        if (baseLineAcquisition.baselineGathered && dNN.trainingComplete)
        {
            checkStateHr.Add(num);
        }
    }

    private void ReadRRData()
    {
        Debug.Log("Read Data");
        int num = 0;
        num = (
            outputRR = (
                (!Application.isEditor && MultiConnection.singleton.connected)
                    ? Convert.ToInt32(MultiConnection.singleton.RRText.TrimEnd(unitChars))
                    : (
                        (trackSwitcher.switchVal != 0)
                            ? UnityEngine.Random.Range(700, 850)
                            : UnityEngine.Random.Range(800, 1100)
                    )
            )
        );
        if (baseLineAcquisition.baselineGathered && dNN.trainingComplete)
        {
            checkStateRR.Add(num);
        }
        RRReadings.Add(num);
        affectiveStatesList.Add(Helpers.AffectiveState(trackSetup.trackId));
    }

    private void CheckPlayerState()
    {
        Debug.Log("Check State");
        if (
            !baseLineAcquisition.baselineGathered
            || !dNN.trainingComplete
            || (float)checkStateHr.Count != hrReadingsLimit
        )
        {
            return;
        }
        double num = Helpers.RMSSD(
            checkStateRR.Skip(Math.Max(0, checkStateRR.Count() - 30)).ToList()
        );
        double num2 = Helpers.HRChange(checkStateHr, startingHr);
        double[] array = Helpers.NormalizeData(
            new double[2] { Math.Round(num, 2), Math.Round(num2, 2) },
            minRRValue,
            maxRRValue,
            minHRValue,
            maxHRValue
        );
        double[] probs = dNN.neuralNetworkBackProp.ComputeOutputs(array);
        probs = Helpers.ProbsToClasses(probs);
        double[] array2 = Array.ConvertAll(
            Helpers.AffectiveState(trackSetup.track).Split(','),
            double.Parse
        );
        double[] item = new List<double> { array[0], array[1], array2[0], array2[1] }.ToArray();
        testDataList.Add(item);
        int num3 = ((!(probs[0] < probs[1])) ? 1 : 0);
        if (baseLineAcquisition.baselineGathered)
        {
            if (prevStates.Count < 2)
            {
                prevStates.Add(num3);
            }
            else if (prevStates[0] == num3 && prevStates[1] == num3)
            {
                num3 = ((num3 == 0) ? 1 : 0);
                prevStates[0] = prevStates[1];
                prevStates[1] = num3;
            }
            else
            {
                prevStates[0] = prevStates[1];
                prevStates[1] = num3;
            }
        }
        currentState = num3;
        if (currentState == 0)
        {
            trackSetup.trackId = 1;
        }
        else if (currentState == 1)
        {
            trackSetup.trackId = 0;
        }
        checkStateRR = new List<int>();
        checkStateHr = new List<int>();
    }

    public IEnumerator UploadTrainData(List<double[]> trainDataList)
    {
        string text = string.Empty;
        foreach (double[] trainData in trainDataList)
        {
            string text2 =
                trainData[0] + "," + trainData[1] + "," + trainData[2] + "," + trainData[3] + ";";
            text += text2;
        }
        UnityWebRequest unityWebRequest = UnityWebRequest.Get(
            "https://morpheusapi.azurewebsites.net/api/Data/PostTrainData?data="
                + text
                + "&mode=train"
        );
        yield return unityWebRequest.SendWebRequest();
    }

    public IEnumerator UploadTestData(List<double[]> testDataList)
    {
        string text = string.Empty;
        foreach (double[] testData in testDataList)
        {
            string text2 =
                testData[0] + "," + testData[1] + "," + testData[2] + "," + testData[3] + ";";
            text += text2;
        }
        UnityWebRequest unityWebRequest = UnityWebRequest.Get(
            "https://morpheusapi.azurewebsites.net/api/Data/PostTrainData?data="
                + text
                + "&mode=test"
        );
        yield return unityWebRequest.SendWebRequest();
    }

    private IEnumerator GetModelData(Action<DownloadModel> callbackOnFinish)
    {
        UnityWebRequest www = UnityWebRequest.Get("https://morpheusapi.azurewebsites.net/api/Test");
        yield return www.SendWebRequest();
        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
            callbackOnFinish(new DownloadModel());
            yield break;
        }
        Debug.Log(www.downloadHandler.text);
        if (!string.IsNullOrEmpty(www.downloadHandler.text))
        {
            downloadedData = JsonConvert.DeserializeObject<DownloadModel>(www.downloadHandler.text);
        }
        callbackOnFinish(downloadedData);
    }
}
 */
