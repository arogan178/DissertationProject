using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseLineAcquisition : MonoBehaviour
{
    public float elapsedTime = 0.0f;
    public float baselineduration = 240;
    public bool baselineGathered = false;
    float delay = 0.0f;

    NewHeartRateScript hrScript;
    TrackSetup trackSetup;

    private void FixedUpdate()
    {
        if (delay < hrScript.readingDelay)
        {
            delay = delay + Time.deltaTime;
        }
        else
        {
            if (elapsedTime < baselineduration)
            {
                if (elapsedTime >= 0 && elapsedTime <= 120)
                {
                    trackSetup.trackId = 1;
                }
                else if (elapsedTime > 120 && elapsedTime <= baselineduration)
                {
                    trackSetup.trackId = 0;
                }

                elapsedTime = elapsedTime + Time.deltaTime;
            }
            else
            {
                baselineGathered = true;
            }
            if (elapsedTime < baselineduration)
            {
                elapsedTime = elapsedTime + Time.deltaTime;
            }
            else
            {
                baselineGathered = true;
            }
        }
    }

    private void Start()
    {
        hrScript = GameObject.FindObjectOfType<NewHeartRateScript>();
        trackSetup = GameObject.FindObjectOfType<TrackSetup>();
    }
}
