using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine.Networking;
using System.IO;

public class ServerConnect : MonoBehaviour
{
    string[] lines;

    // Start is called before the first frame update
    void Start()
    {
        //string path = "Assets/Resources/rmssdData(1).txt";

        ////Read the text from directly from the test.txt file
        //StreamReader reader = new StreamReader(path);
        //string data = reader.ReadToEnd();

        //lines = data.Split(
        //        new[] { Environment.NewLine },
        //        StringSplitOptions.None
        //    );


        //var weights = Array.ConvertAll(lines,Double.Parse);

        StartCoroutine(GetText());
    }

    IEnumerator GetText()
    {
        UnityWebRequest www = UnityWebRequest.Get("https://morpheusapi20200511041703.azurewebsites.net/weights.txt");

        yield return www.SendWebRequest();
        
        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            // Show results as text
            Debug.Log(www.downloadHandler.text);

            lines = www.downloadHandler.text.Split(
                new[] { Environment.NewLine },
                StringSplitOptions.None
            );

            // Or retrieve results as binary data
            byte[] results = www.downloadHandler.data;
        }

    }

    IEnumerator UploadMultipleFiles()
    {
        WWWForm form = new WWWForm();
        var uploadString = String.Join("\r\n", lines);

        TestModel test = new TestModel() { text = "data" };
        form.AddField("data", uploadString);
        UnityWebRequest req = UnityWebRequest.Get("https://localhost:44309/api/Data/PostValues?data="+uploadString+"&device="+ SystemInfo.deviceUniqueIdentifier+"");
        
        yield return req.SendWebRequest();

        if (req.isHttpError || req.isNetworkError)
            Debug.Log(req.error);
        else
            Debug.Log("Done Successfully");
    }

    class TestModel
    {
        public string text;
    }
}
