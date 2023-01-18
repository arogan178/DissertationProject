using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Vitals : MonoBehaviour
{
    public Text hrVal;
    public Text avgHRTextBox;
    public Text rrIntervalTextBox;
    int HRValue;
    int HRValueCount = 0;
    public int avgHR = 0;
    public float RRInterval = 0;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GetHRData());
    }

    IEnumerator GetHRData()
    {
        yield return new WaitForSeconds(2);
        while (true)
        {
            // Wait for 1 second
            yield return new WaitForSeconds(1);

            // Get the HR data from the text box
            HRValue = int.Parse(hrVal.text);

            // Calculate the average HR
            avgHR = (avgHR * HRValueCount + HRValue) / (HRValueCount + 1);
            avgHRTextBox.text = (avgHR).ToString();
            HRValueCount++;

            // Calculate the RR interval from the HR value
            RRInterval = (60.0f / HRValue);
            rrIntervalTextBox.text = RRInterval.ToString("F2");

            //Debug.Log("HR Val: "+HRValue);
            //Debug.Log("AVG: "+avgHR);
            //Debug.Log("RR Val: "+RRInterval);



        }
    }
}
