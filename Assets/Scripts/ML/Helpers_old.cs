/* // Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// Helpers
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets;
using Newtonsoft.Json;
using UnityEngine;

public class Helpers
{
    public static double GetMedian(double[] numbers)
    {
        int num = numbers.Count();
        int num2 = numbers.Count() / 2;
        List<double> source = numbers.OrderBy((double n) => n).ToList();
        if (num % 2 == 0)
        {
            return (source.ElementAt(num2) + source.ElementAt(num2 - 1)) / 2.0;
        }
        return source.ElementAt(num2);
    }

    public static float Square(float num)
    {
        return num * num;
    }

    public static double RMSSD(List<int> rrList)
    {
        double num = 0.0;
        for (int i = 0; i < rrList.Count - 1; i++)
        {
            num += (double)Square(rrList[i] - rrList[i + 1]);
        }
        return Math.Sqrt(num);
    }

    public static double HRChange(List<int> hrList, int startingHr)
    {
        List<int> list = new List<int>();
        foreach (int hr in hrList)
        {
            list.Add(hr - startingHr);
        }
        return list.Average();
    }

    public static double[] WinningState(List<string> affectiveStates)
    {
        int num = 0;
        int num2 = 0;
        foreach (string affectiveState in affectiveStates)
        {
            if (affectiveState.Equals("0,1"))
            {
                num++;
            }
            else
            {
                num2++;
            }
        }
        if (num2 <= num)
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
        double[] array = new double[probs.Length];
        int num = MaxIndex(probs);
        array[num] = 1.0;
        return array;
    }

    public static int MaxIndex(double[] probs)
    {
        int result = 0;
        double num = probs[0];
        for (int i = 0; i < probs.Length; i++)
        {
            if (probs[i] > num)
            {
                num = probs[i];
                result = i;
            }
        }
        return result;
    }

    public static double GetStandardDeviation(IEnumerable<double> values)
    {
        double result = 0.0;
        double[] source = (values as double[]) ?? values.ToArray();
        int num = source.Count();
        if (num > 1)
        {
            double avg = source.Average();
            result = Math.Sqrt(source.Sum((double d) => (d - avg) * (d - avg)) / (double)num);
        }
        return result;
    }

    public static double[] AverageWeights(double[] serverWeights, double[] deviceWeights)
    {
        double[] array = new double[serverWeights.Length];
        for (int i = 0; i < serverWeights.Length; i++)
        {
            array[i] = serverWeights[i] * 0.8 + deviceWeights[i] * 0.2;
        }
        return array;
    }

    public static double[] WeightDifferences(double[] serverWeights, double[] newWeights)
    {
        double[] array = new double[serverWeights.Length];
        if (!serverWeights.Any((double v) => v != 0.0))
        {
            return newWeights;
        }
        for (int i = 0; i < serverWeights.Length; i++)
        {
            array[i] = newWeights[i] - serverWeights[i];
            array[i] = array[i];
        }
        return array;
    }

    public static double[] AverageWeights(
        double[] serverWeights,
        double[] deviceWeights,
        int numServerData,
        int numLocalData
    )
    {
        double num = (double)numServerData / ((double)numServerData + (double)numLocalData);
        double num2 = (double)numLocalData / ((double)numServerData + (double)numLocalData);
        double[] array = new double[serverWeights.Length];
        bool flag = !serverWeights.Any((double v) => v != 0.0);
        bool flag2 = !deviceWeights.Any((double v) => v != 0.0);
        if (flag)
        {
            return deviceWeights;
        }
        if (flag2)
        {
            return serverWeights;
        }
        if (flag2 && flag)
        {
            return array;
        }
        for (int i = 0; i < serverWeights.Length; i++)
        {
            array[i] = serverWeights[i] * num - deviceWeights[i] * num2;
        }
        return array;
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

    public static List<double[]> NormalizeTrainingData(List<double[]> trainData)
    {
        double[] array = new double[trainData.Count];
        double[] array2 = new double[trainData.Count];
        for (int i = 0; i < trainData.Count; i++)
        {
            double num = trainData[i][0];
            double num2 = trainData[i][1];
            array[i] = num;
            array2[i] = num2;
        }
        double num3 = array.Max();
        double num4 = array.Min();
        double num5 = array2.Max();
        double num6 = array2.Min();
        for (int j = 0; j < trainData.Count; j++)
        {
            trainData[j][0] = (trainData[j][0] - num4) / (num3 - num4);
            trainData[j][1] = (trainData[j][1] - num6) / (num5 - num6);
        }
        return trainData;
    }

    public static List<double> MinMax(List<double[]> trainData)
    {
        double[] array = new double[trainData.Count];
        double[] array2 = new double[trainData.Count];
        for (int i = 0; i < trainData.Count; i++)
        {
            double num = trainData[i][0];
            double num2 = trainData[i][1];
            array[i] = num;
            array2[i] = num2;
        }
        double item = array.Max();
        double item2 = array.Min();
        double item3 = array2.Max();
        double item4 = array2.Min();
        return new List<double> { item, item2, item3, item4 };
    }

    public static List<ModelClass> LoadModel(string activePlayer)
    {
        new List<ModelClass>();
        return JsonConvert.DeserializeObject<List<ModelClass>>(
            File.ReadAllText(Application.persistentDataPath + "/Users/" + activePlayer + ".txt")
        );
    }

    public static double[][] LoadTrainData(string activePlayer)
    {
        new List<ModelClass>();
        string[] array = File.ReadAllLines(
            Application.persistentDataPath
                + "/Users/"
                + activePlayer
                + "/combinedReadingsNormalized.txt"
        );
        List<double[]> list = new List<double[]>();
        for (int i = 1; i < array.Length; i++)
        {
            string[] array2 = array[i].Split(',');
            List<double> list2 = new List<double>();
            string[] array3 = array2;
            foreach (string s in array3)
            {
                list2.Add(double.Parse(s));
            }
            list.Add(list2.ToArray());
        }
        return list.ToArray();
    }

    public static void EmptyTrainDataFile(string activePlayer)
    {
        File.WriteAllText(
            Application.persistentDataPath
                + "/Users/"
                + activePlayer
                + "/combinedReadingsNormalized.txt",
            string.Empty
        );
    }
}
 */