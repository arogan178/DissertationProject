using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackSwitcher : MonoBehaviour
{
    public Switcher switcher;
    public int switchVal = 0;

    //[SerializeField]
    //private DNNScript dNN;

    TrackSetup tsScript;
    RandomTrackSetup randomTsScript;

    private void OnTriggerEnter(Collider other)
    {
        
        bool isRandom = Convert.ToBoolean(PlayerPrefs.GetInt("isRandom"));
        if (!isRandom)
        {
            switcher.Switch(tsScript.trackId, false);
            tsScript.ChangeSetup();
        }
        else
        {
            switcher.Switch(randomTsScript.track, false);
            randomTsScript.ChangeSetup();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        tsScript = GameObject.FindObjectOfType<TrackSetup>();
        randomTsScript = GameObject.FindObjectOfType<RandomTrackSetup>();
    }
}
