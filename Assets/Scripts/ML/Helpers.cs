using Assets;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class Helpers
{
    public static double GetMedian(double[] numbers)
    {
        int numberCount = numbers.Count();
        int halfIndex = numbers.Count() / 2;
        List<double> sortedNumbers = numbers.OrderBy((double n) => n).ToList();

        if (numberCount % 2 == 0)
        {
            return (sortedNumbers.ElementAt(halfIndex) + sortedNumbers.ElementAt(halfIndex - 1))
                / 2.0;
        }
        return sortedNumbers.ElementAt(halfIndex);
    }

    public static float Square(float num)
    {
        return num * num;
    }

    public static double RMSSD(List<int> rrList)
    {
        double total = 0;

        for (int i = 0; i < rrList.Count - 1; i++)
        {
            total += Square(rrList[i] - rrList[i + 1]);
        }

        return Math.Sqrt(total);
    }

    public static double HRChange(List<int> hrList, int startingHr)
    {
        List<int> hrDisplacements = new List<int>();
        //Debug.Log("hrList " + JsonConvert.SerializeObject(hrList, Formatting.Indented));
        //Debug.Log("startingHr " + JsonConvert.SerializeObject(startingHr, Formatting.Indented));

        foreach (int hr in hrList)
        {
            hrDisplacements.Add(hr - startingHr);
        }
        /* Debug.Log(
            "hrDisplacements " + JsonConvert.SerializeObject(hrDisplacements, Formatting.Indented)
        );
        Debug.Log(
            "hrDisplacementsavg "
                + JsonConvert.SerializeObject(hrDisplacements.Average(), Formatting.Indented)
        ); */
        return hrDisplacements.Average();
    }

    public static double[] WinningState(List<string> affectiveStates)
    {
        int classSlow = 0;
        int classFast = 0;

        foreach (string affectiveState in affectiveStates)
        {
            if (affectiveState.Equals("0,1"))
            {
                classSlow++;
            }
            else
            {
                classFast++;
            }
        }

        if (classFast <= classSlow)
        {
            return new double[2] { 0.0, 1.0 };
        }
        return new double[2] { 1.0, 0.0 };
    }

    public static string AffectiveState(int trackID = 0)
    {
        if (trackID == 0)
        {
            return "0,1";
        }
        return "1,0";
    }

    public static double[] ProbsToClasses(double[] probs)
    {
        double[] result = new double[probs.Length];
        int idx = MaxIndex(probs);
        result[idx] = 1.0;
        return result;
    }

    public static int MaxIndex(double[] probs)
    {
        int maxIdx = 0;
        double maxVal = probs[0];

        for (int i = 0; i < probs.Length; i++)
        {
            if (probs[i] > maxVal)
            {
                maxVal = probs[i];
                maxIdx = i;
            }
        }
        return maxIdx;
    }

    public static double GetStandardDeviation(IEnumerable<double> values)
    {
        double standardDeviation = 0.0;
        double[] enumerable = (values as double[]) ?? values.ToArray();
        int count = enumerable.Count();
        if (count > 1)
        {
            double avg = enumerable.Average();
            standardDeviation = Math.Sqrt(
                enumerable.Sum((double d) => (d - avg) * (d - avg)) / (double)count
            );
        }
        return standardDeviation;
    }

    public static double[] AverageWeights(double[] serverWeights, double[] deviceWeights)
    {
        double[] newModelWeights = new double[serverWeights.Length];

        for (int i = 0; i < serverWeights.Length; i++)
        {
            newModelWeights[i] = serverWeights[i] * 0.8 + deviceWeights[i] * 0.2;
        }

        return newModelWeights;
    }

    public static double[] WeightDifferences(double[] serverWeights, double[] newWeights)
    {
        double[] weightDifferences = new double[serverWeights.Length];
        if (!serverWeights.Any((double v) => v != 0.0))
        {
            return newWeights;
        }
        for (int i = 0; i < serverWeights.Length; i++)
        {
            weightDifferences[i] = newWeights[i] - serverWeights[i];
            weightDifferences[i] = weightDifferences[i];
        }
        return weightDifferences;
    }

    public static double[] AverageWeights(
        double[] serverWeights,
        double[] deviceWeights,
        int numServerData,
        int numLocalData
    )
    {
        var pctServer = (double)numServerData / ((double)numServerData + (double)numLocalData);
        var pctLocal = (double)numLocalData / ((double)numServerData + (double)numLocalData);
        double[] newModelWeights = new double[serverWeights.Length];
        bool serverAreZero = !serverWeights.Any(v => v != 0);
        bool deviceAreZero = !deviceWeights.Any(v => v != 0);

        if (serverAreZero)
        {
            return deviceWeights;
        }
        if (deviceAreZero)
        {
            return serverWeights;
        }
        if (deviceAreZero && serverAreZero)
        {
            return newModelWeights;
        }
        for (int i = 0; i < serverWeights.Length; i++)
        {
            newModelWeights[i] = serverWeights[i] * pctServer - deviceWeights[i] * pctLocal;
        }
        return newModelWeights;
    }

    public static List<double[]> NormalizeData(
        List<double[]> data,
        double minRRValue,
        double maxRRValue,
        double minHRValue,
        double maxHRValue
    )
    {
        for (int i = 0; i < data.Count; i++)
        {
            data[i][0] = (data[i][0] - minRRValue) / (maxRRValue - minRRValue);
            data[i][1] = (data[i][1] - minHRValue) / (maxHRValue - minHRValue);
        }
        return data;
    }

    public static double[] NormalizeData(
        double[] testData,
        double minRRValue,
        double maxRRValue,
        double minHRValue,
        double maxHRValue
    )
    {
        testData[0] = (testData[0] - minRRValue) / (maxRRValue - minRRValue);
        if (testData[0] > 1.0)
        {
            testData[0] = 1.0;
        }
        else if (testData[0] < 0.0)
        {
            testData[0] = 0.0;
        }
        testData[1] = (testData[1] - minHRValue) / (maxHRValue - minHRValue);
        if (testData[1] > 1.0)
        {
            testData[1] = 1.0;
        }
        else if (testData[1] < 0.0)
        {
            testData[1] = 0.0;
        }

        return testData;
    }

    #region normalize train data
    public static List<double[]> NormalizeTrainingData(List<double[]> trainData)
    {
        double[] RRValues = new double[trainData.Count];
        double[] hRValues = new double[trainData.Count];
        // Debug.Log("RRValues before " + JsonConvert.SerializeObject(RRValues, Formatting.Indented));
        // Debug.Log("hRValues before " + JsonConvert.SerializeObject(hRValues, Formatting.Indented));

        for (int i = 0; i < trainData.Count; i++)
        {
            double rr = trainData[i][0];
            double hr = trainData[i][1];
            RRValues[i] = rr;
            hRValues[i] = hr;
        }

        // Debug.Log("RRValues after " + JsonConvert.SerializeObject(RRValues, Formatting.Indented));
        // Debug.Log("hRValues after " + JsonConvert.SerializeObject(hRValues, Formatting.Indented));

        double maxRRValue = RRValues.Max();
        double minRRValue = RRValues.Min();
        double maxHRValue = hRValues.Max();
        double minHRValue = hRValues.Min();

        // Debug.Log("maxRRValue max " + JsonConvert.SerializeObject(maxRRValue, Formatting.Indented));
        // Debug.Log("minRRValue min" + JsonConvert.SerializeObject(minRRValue, Formatting.Indented));
        // Debug.Log("maxHRValue max" + JsonConvert.SerializeObject(maxHRValue, Formatting.Indented));
        // Debug.Log("minHRValue min" + JsonConvert.SerializeObject(minHRValue, Formatting.Indented));

        for (int i = 0; i < trainData.Count; i++)
        {
/*             Debug.Log(
                "trainData"
                    + i
                    + 0
                    + "before "
                    + JsonConvert.SerializeObject(trainData[i][0], Formatting.Indented)
            );
            Debug.Log(
                "trainData"
                    + i
                    + 1
                    + "before "
                    + JsonConvert.SerializeObject(trainData[i][1], Formatting.Indented)
            ); */
            trainData[i][0] = (trainData[i][0] - minRRValue) / (maxRRValue - minRRValue);
            trainData[i][1] = (trainData[i][1] - minHRValue) / (maxHRValue - minHRValue);
/*             Debug.Log(
                "trainData"
                    + i
                    + 0
                    + "result "
                    + JsonConvert.SerializeObject(trainData[i][0], Formatting.Indented)
            );
            Debug.Log(
                "trainData"
                    + i
                    + 1
                    + "result "
                    + JsonConvert.SerializeObject(trainData[i][1], Formatting.Indented)
            ); */
        }
        #endregion
/*         Debug.Log(
            "trainData return " + JsonConvert.SerializeObject(trainData, Formatting.Indented)
        ); */
        return trainData;
    }

    public static List<double> MinMax(List<double[]> trainData)
    {
        double[] RRValues = new double[trainData.Count];
        double[] hRValues = new double[trainData.Count];

        for (int i = 0; i < trainData.Count; i++)
        {
            double rr = trainData[i][0];
            double hr = trainData[i][1];

            RRValues[i] = rr;
            hRValues[i] = hr;
        }

        double maxRRValue = RRValues.Max();
        double minRRValue = RRValues.Min();

        double maxHRValue = hRValues.Max();
        double minHRValue = hRValues.Min();
        List<double> minMaxValues = new List<double>()
        {
            maxRRValue,
            minRRValue,
            maxHRValue,
            minHRValue
        };
        return minMaxValues;
    }

    public static List<ModelClass> LoadModel(string activePlayer)
    {
        List<ModelClass> modelClasses = new List<ModelClass>();

        var path = Application.persistentDataPath + "/Users/" + activePlayer + ".txt";
        double[] deviceWeights;

        string readData = File.ReadAllText(path);
        var dataModel = JsonConvert.DeserializeObject<List<ModelClass>>(readData);
        //Debug.Log("local " + JsonConvert.SerializeObject(dataModel));
        return dataModel;
    }

    public static double[][] LoadTrainData(string activePlayer)
    {
        List<ModelClass> modelClasses = new List<ModelClass>();
        double[][] trainData;

        var path =
            Application.persistentDataPath
            + "/Users/"
            + activePlayer
            + "/combinedReadingsNormalized.txt";
        string[] lines = System.IO.File.ReadAllLines(path);
        List<double[]> dataList = new List<double[]>();

        for (int i = 1; i < lines.Length; i++)
        {
            string[] columns = lines[i].Split(',');
            List<double> values = new List<double>();

            foreach (string column in columns)
            {
                values.Add(Double.Parse(column));
            }

            dataList.Add(values.ToArray());
        }

        trainData = dataList.ToArray();

        return trainData;
    }

    public static void EmptyTrainDataFile(string activePlayer)
    {
        var path =
            Application.persistentDataPath
            + "/Users/"
            + activePlayer
            + "/combinedReadingsNormalized.txt";
        File.WriteAllText(path, String.Empty);
    }
}
