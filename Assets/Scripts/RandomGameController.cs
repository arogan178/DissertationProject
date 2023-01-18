using Assets;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class RandomGameController : MonoBehaviour
{
    RandomTrackSetup trackSetup;
    TrackSwitcher trackSwitcher;
    BaseLineAcquisition baseLineAcquisition;

    public int currentState = 3;

    private void Awake()
    {
        trackSetup = GameObject.FindObjectOfType<RandomTrackSetup>();
        baseLineAcquisition = GameObject.FindObjectOfType<BaseLineAcquisition>();
        trackSwitcher = GameObject.FindObjectOfType<TrackSwitcher>();

        currentState = 3;
    }

    private void Start()
    {
        InvokeRepeating("CheckPlayerState", 60.0f, 60.0f);
    }

    // Start is called before the first frame update


    void CheckPlayerState()
    {
        Debug.Log("Check State");

        int output = UnityEngine.Random.Range(0, 2);

        currentState = output;
        if (currentState == 0)
        {
            trackSetup.trackId = 1;
            trackSetup.track = 1;
        }
        else if (currentState == 1)
        {
            trackSetup.trackId = 0;
            trackSetup.track = 0;
        }
    }
}
