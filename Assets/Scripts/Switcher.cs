using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZenFulcrum.Track;

/**
    * Switches track at runtime.
    * Switch by calling GetComponenet<TrackSwitcher>().Switch()
    */
[RequireComponent(typeof(Track))]
public class Switcher : MonoBehaviour
{
    [Tooltip("Which tracks can we switch to?")]
    public Track[] positions;
    [Tooltip("When asked to switch, how quickly should we switch to the next piece?")]
    public float switchSpeed = 1f;

    public int rnd;
    public static int trackID;

    DNNScript mlObject;
    BaseLineAcquisition bla;
    TrackSetup trackSetup;
    RandomTrackSetup randomTrackSetup;

    RandomTrackSwitcher randomTrackSwitcher;
    TrackSwitcher trackSwitcher;

    NewHeartRateScript heartRateScript;

    public int currentState = 0;

    public enum SwitchSide
    {
        SwitchStart,
        SwitchEnd,
    }

    [Tooltip("Which side of *this* track will be switched?")]
    public SwitchSide switchSide = SwitchSide.SwitchEnd;

    protected Track track;

    protected bool switching = false;
    protected int desiredPosition = 0;

    protected SimpleTransform lastPosition, targetPosition;
    protected float switchStartTime;

    int prevTrack;

    public void Awake()
    {
        track = GetComponent<Track>();
    }

    public void Start()
    {
        mlObject = GameObject.FindObjectOfType<DNNScript>();
        bla = GameObject.FindObjectOfType<BaseLineAcquisition>();

        trackSetup = GameObject.FindObjectOfType<TrackSetup>();
        randomTrackSetup = GameObject.FindObjectOfType<RandomTrackSetup>();
        trackSwitcher = GameObject.FindObjectOfType<TrackSwitcher>();
        randomTrackSwitcher = GameObject.FindObjectOfType<RandomTrackSwitcher>();
        heartRateScript = GameObject.FindObjectOfType<NewHeartRateScript>();
    }

    private bool endSwitching
    {
        get { return switchSide == SwitchSide.SwitchEnd; }
    }

    public void FixedUpdate()
    {
        if (!switching) return;

        var percent = (Time.time - switchStartTime) / switchSpeed;

        if (switchSpeed == 0 || percent >= 1)
        {
            if (endSwitching)
            {
                track.ConnectTo(positions[desiredPosition]);

                try
                {
                    positions[desiredPosition].GetComponent<MeshRenderer>().enabled = true;
                    //track.PrevTrack.GetComponent<MeshRenderer>().enabled = false;

                    MeshRenderer[] childrenNext = positions[desiredPosition].GetComponentsInChildren<MeshRenderer>();
                    foreach (MeshRenderer renderer in childrenNext)
                    {
                        renderer.enabled = true;
                    }

                    //MeshRenderer[] childrenPrev = track.PrevTrack.GetComponentsInChildren<MeshRenderer>();
                    //foreach (MeshRenderer renderer in childrenPrev)
                    //{
                    //    renderer.enabled = false;
                    //}
                }
                catch (Exception e)
                {

                }
            }
            else
            {
                track.SnapTogether(positions[desiredPosition], true, false);
            }
            switching = false;
        }
        else
        {
            if (endSwitching)
            {
                track.TrackAbsoluteEnd = SimpleTransform.Lerp(
                    lastPosition,
                    positions[desiredPosition].TrackAbsoluteStart,
                    percent
                );

                try
                {
                    positions[desiredPosition].GetComponent<MeshRenderer>().enabled = true;
                    //track.PrevTrack.GetComponent<MeshRenderer>().enabled = false;

                    MeshRenderer[] childrenNext = positions[desiredPosition].GetComponentsInChildren<MeshRenderer>();
                    foreach (MeshRenderer renderer in childrenNext)
                    {
                        renderer.enabled = true;
                    }

                    //MeshRenderer[] childrenPrev = track.PrevTrack.GetComponentsInChildren<MeshRenderer>();
                    //foreach (MeshRenderer renderer in childrenPrev)
                    //{
                    //    renderer.enabled = false;
                    //}
                }
                catch (Exception e)
                {

                }
            }
            else
            {
                track.TrackAbsoluteStart = SimpleTransform.Lerp(
                    lastPosition,
                    positions[desiredPosition].TrackAbsoluteEnd,
                    percent
                );
            }
        }

        
    }

    /** Starts switching to the given position. */
    public void Switch(int index)
    {
        lastPosition = endSwitching ? track.TrackAbsoluteEnd : track.TrackAbsoluteStart;
        desiredPosition = index;
        switchStartTime = Time.time;
        switching = true;

        foreach (var position in positions)
        {
            if (endSwitching) position.PrevTrack = null;
            else position.NextTrack = null;
        }
    }

    public void Switch(int trackId, bool trainingComplete)
    {
        //if (!heartRateScript.trainingComplete)
        //if (heartRateScript.allWeightsAreZero && !heartRateScript.trainingComplete)
        //{
        //    if (bla.elapsedTime <= 120)
        //    {
        //        trackID = 1;

        //        //trackSwitcher.switchVal = 1;
        //    }
        //    else if (bla.elapsedTime > 120)
        //    {
        //        trackID = 0;
        //    }
        //}
        ///else if(heartRateScript.isInitialized || heartRateScript.trainingComplete)
        //else if(heartRateScript.trainingComplete)
        //{
        //    if (heartRateScript.currentState == 0)
        //    {
        //        trackID = 1;
        //    }
        //    else if (heartRateScript.currentState == 1)
        //    {
        //        trackID = 0;
        //    }

        //    //trackSwitcher.switchVal = 1;
        //}

        bool isRandom = Convert.ToBoolean(PlayerPrefs.GetInt("isRandom"));

        if (!isRandom)
        {
            //if (((heartRateScript.trainingComplete || heartRateScript.isInitialized) && bla.baselineGathered) || heartRateScript.isInitialized)
            if (heartRateScript.trainingComplete && bla.baselineGathered)
            {
                if (heartRateScript.currentState == 0)
                {
                    trackID = 1;
                }
                else if (heartRateScript.currentState == 1)
                {
                    trackID = 0;
                }
            }
            else
            {
                trackID = trackId;
            }

            trackSwitcher.switchVal = trackID;
            trackSetup.trackId = trackID;
            Switch(trackID);
        }
        else
        {
            trackSwitcher.switchVal = trackId;
            randomTrackSetup.trackId = trackId;
            Switch(trackId);
        }

        
    }

}
