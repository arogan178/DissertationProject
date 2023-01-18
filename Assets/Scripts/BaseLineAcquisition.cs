using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseLineAcquisition : MonoBehaviour
{
    public float elapsedTime = 0.0f;
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
            if (elapsedTime < 240)
            {
                if (elapsedTime >= 0 && elapsedTime <= 120)
                {
                    trackSetup.trackId = 1;
                }
                else if (elapsedTime > 120 && elapsedTime <= 240)
                {
                    trackSetup.trackId = 0;
                }

                elapsedTime = elapsedTime + Time.deltaTime;
            }
            else
            {
                baselineGathered = true;
            }

            //if (hrScript.newUser)
            //{
            //    if (elapsedTime < 240)
            //    {
            //        if (elapsedTime >= 0 && elapsedTime <= 120)
            //        {
            //            trackSetup.trackId = 1;
            //        }
            //        else if (elapsedTime > 120 && elapsedTime <= 240)
            //        {
            //            trackSetup.trackId = 0;
            //        }

            //        elapsedTime = elapsedTime + Time.deltaTime;
            //    }
            //    else
            //    {
            //        baselineGathered = true;
            //    }
            //}
            //else
            //{
            if (elapsedTime < 240)
                {
                    //if (elapsedTime >= 0 && elapsedTime <= 120)
                    //{
                    //    trackSetup.trackId = 1;
                    //}
                    //else if (elapsedTime > 60 && elapsedTime <= 240)
                    //{
                    //    trackSetup.trackId = 0;
                    //}

                    elapsedTime = elapsedTime + Time.deltaTime;
                }
                else
                {
                    baselineGathered = true;
                }
            //}
        }
    }

    private void Start()
    {
        hrScript = GameObject.FindObjectOfType<NewHeartRateScript>();
        trackSetup = GameObject.FindObjectOfType<TrackSetup>();

        //PlayerPrefs.SetInt("currentState", 1);
    }
}
