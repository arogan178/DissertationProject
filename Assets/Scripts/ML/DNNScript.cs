using System.Collections;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.ML;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using Assets.Scripts.ML;
using UnityEngine.Networking;
using System.IO;
using Assets;
using Newtonsoft.Json;

public class DNNScript : MonoBehaviour
{
    private StreamWriter combinedUserDf;
    private StreamWriter rdf;
    private StreamWriter statisticsFile;
    private NewHeartRateScript hrUtil;

    public bool weightsUploaded;
    public bool trainingComplete;
    public bool trainingStarted;

    public bool newUserTrainingComplete;
    public bool newUserTrainingStarted;

    private string newDeviceWeights = string.Empty;
    private string newDeviceWeightsSGD = string.Empty;
    private string newDeviceWeightsANNSGD = string.Empty;
    private string newDeviceWeightsANNPSO = string.Empty;

    [SerializeField]
    public string api = "http://arogan178.eu.pythonanywhere.com/api/MorpheusData/PostValues";

    public bool isInitialized;

    public double maxRRValue;
    public double minRRValue;

    public double maxHRValue;
    public double minHRValue;

    private double[][] trainData;

    private const int numInput = 2;
    private const int numOutput = 2;

    private double[] weightChanges = new double[32]; //NOTE : Check with Luca why this is 32, for now switched to 27

    //Initialized
    public NeuralNetworkBackProp neuralNetworkBackProp;

    private double modelErrorANNBP = 0.0f;

    //ANN BP Params
    private const int numHiddenANNBP = 6;
    private double[] newWeightsANNBP;

    private int maxEpochsANNBP = 10;
    private double learnRateANNBP = 0.01;

    private double momentumANNBP = 0.01;

    public void WriteAnalytics(List<double[]> trainData, List<double[]> testData)
    {
        var activeUser = PlayerPrefs.GetString("ActivePlayer");
        var filePath = Application.persistentDataPath + "/Users/" + activeUser;

        //CombinedNormalizedReadings Path
        string combinedUserPath = filePath + "/combinedReadingsNormalized.txt";

        //Statistics Path
        string statisticsPath = filePath + "/analytics.txt";

        //Create Directory if do not exist
        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }

        if (!File.Exists(statisticsPath))
        {
            statisticsFile = File.CreateText(statisticsPath);
        }
        else
        {
            statisticsFile = File.AppendText(statisticsPath);
        }

        if (!File.Exists(combinedUserPath))
        {
            combinedUserDf = File.CreateText(combinedUserPath);
        }
        else
        {
            combinedUserDf = File.AppendText(combinedUserPath);
        }

        if (trainData.Count != 0)
        {
            var normalisedData = Helpers.NormalizeTrainingData(trainData);
            //var normalisedData = Helpers.NormalizeTrainingData(trainingDataList);
            foreach (double[] trainDataLine in normalisedData)
            {
                string line = string.Join(",", trainDataLine);
                combinedUserDf.WriteLine(line);
            }
            combinedUserDf.Close();
        }
        else
        {
            throw new InvalidOperationException("Cannot perform operation on empty list");
        }

        double acc7 = neuralNetworkBackProp.Accuracy(testData.ToArray());
        Debug.Log("Accuracy " + string.Join(", ", acc7));

        statisticsFile.WriteLine(
            "---------------------------------------------------------------------------------------------"
        );
        statisticsFile.WriteLine("Number of Data Items (test): " + testData.Count.ToString());
        statisticsFile.WriteLine("ANN Back Prop");
        statisticsFile.WriteLine("Model Accuracy (federated learning): " + acc7);
        statisticsFile.Close();
    }

    // Start is called before the first frame update
    void Start()
    {
        hrUtil = GameObject.FindObjectOfType<NewHeartRateScript>();
    }

    public void InitializeModelANNBackProp(double[] weights)
    {
        neuralNetworkBackProp = new NeuralNetworkBackProp(numInput, numHiddenANNBP, numOutput);

        var allAreZero = weights.Any(v => v == 0);

        if (!allAreZero)
        {
            neuralNetworkBackProp.SetWeights(weights);
        }
    }

    public void TrainModel(object td)
    {
        Dictionary<string, object> dataDict = td as Dictionary<string, object>;
        List<double[]> tDataArr = dataDict["TrainingData"] as List<double[]>;
        string dataPath = dataDict["DataPath"].ToString();
        List<double> startingWeights = dataDict["StartingWeights"] as List<double>;

        trainingStarted = true;
        trainingComplete = false;

        int numOfFeatures = tDataArr[0].Length;
        trainData = new double[tDataArr.Count][];

        for (int i = 0; i < tDataArr.Count; i++)
        {
            trainData[i] = new double[numOfFeatures];
        }

        for (int i = 0; i < trainData.Length; i++)
        {
            double[] data = trainData[i];
            for (int j = 0; j < data.Length; j++)
            {
                trainData[i][j] = Convert.ToDouble(data[j]);
            }
        }

        newWeightsANNBP = neuralNetworkBackProp.Train(
            trainData,
            maxEpochsANNBP,
            learnRateANNBP,
            momentumANNBP
        );
        neuralNetworkBackProp.SetWeights(newWeightsANNBP);
        modelErrorANNBP = neuralNetworkBackProp.Error(trainData, verbose: false);

        #region write local model
        newDeviceWeights = String.Empty;
        newDeviceWeightsSGD = String.Empty;
        newDeviceWeightsANNSGD = String.Join(",", newWeightsANNBP);
        newDeviceWeightsANNPSO = String.Empty;

        int numElements = Math.Min(startingWeights.Count, newWeightsANNBP.Length);
        for (int counter = 0; counter < numElements; counter++)
        {
            weightChanges[counter] = startingWeights[counter] - newWeightsANNBP[counter];
        }
        rdf = File.CreateText(dataPath);

        ModelClass mcANNSGD = new ModelClass()
        {
            modelData = newDeviceWeightsANNSGD,
            maxHR = (double)dataDict["maxHR"],
            minHR = (double)dataDict["minHR"],
            maxRR = (double)dataDict["maxRR"],
            minRR = (double)dataDict["minRR"]
        };

        List<ModelClass> modelClasses = new List<ModelClass>() { mcANNSGD };

        string jsonString = JsonConvert.SerializeObject(modelClasses);

        rdf.WriteLine(jsonString);
        rdf.Close();
        trainingComplete = true;
        #endregion
    }

    public IEnumerator UploadNewWeightsANNSGD(int dataLinesNum)
    {
        weightsUploaded = true;
        var uploadString = String.Join(",", weightChanges);

        // create an object to hold the data to be sent

        var data = new Dictionary<string, object>
        {
            { "data", uploadString },
            { "device", SystemInfo.deviceUniqueIdentifier + Guid.NewGuid() },
            { "error", modelErrorANNBP.ToString() },
            { "mode", "ANNSGD" },
            { "deviceData", dataLinesNum },
            { "modelDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
        };

        // convert the data object to a JSON string
        var jsonData = JsonConvert.SerializeObject(data);

        // use jsonData as the data in the UnityWebRequest
        var request = new UnityWebRequest(api, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();
        Debug.Log("Status Code: " + request.responseCode);
    }
}
