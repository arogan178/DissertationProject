/**
 * <copyright>
 * Tracks and Rails Asset Package by Zen Fulcrum
 * Copyright 2015 Zen Fulcrum LLC
 * Usage is subject to Unity's Asset Store EULA (https://unity3d.com/legal/as_terms)
 * </copyright>
 */


/*
 *SCRIPT NOT USED 
 */

namespace ZenFulcrum.Track
{

    using UnityEngine;
    using System.Collections;

    /**
	    * Switches track at runtime.
	    * Switch by calling GetComponenet<TrackSwitcher>().Switch()
	    */
    [RequireComponent(typeof(Track))]
    public class TrackSwitcher : MonoBehaviour
    {
        [Tooltip("Which tracks can we switch to?")]
        public Track[] positions;
        [Tooltip("When asked to switch, how quickly should we switch to the next piece?")]
        public float switchSpeed = 1f;

        public int rnd;
        public int trackID;

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

        int turnNum = 0;
        int prevTrack;


        public void Awake()
        {
            track = GetComponent<Track>();


        }

        public void Start()
        {

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

        public void Switch()
        {
            rnd = 0;
            trackID = 0;
            turnNum += 1;

            prevTrack = PlayerPrefs.GetInt("trackID");

            if (positions.Length > 1)
            {
                
                //If trained
                if (PlayerPrefs.HasKey("trained"))
                {
                    if (PlayerPrefs.GetInt("currentState") == 0)
                    {
                        trackID = Random.Range(1, positions.Length);
                        PlayerPrefs.SetInt("trackID", trackID);

                        rnd = 1;
                    }
                    else
                    {
                        rnd = 0;
                        trackID = 0;
                    }
                }
                //else not yet trained
                else
                {
                    if (turnNum == 2)
                    {
                        trackID = Random.Range(1, positions.Length);
                        PlayerPrefs.SetInt("trackID", trackID);

                        rnd = 1;
                    }
                }
                PlayerPrefs.SetInt("switch", rnd);
            }
            else
            {
                trackID = 0;
                PlayerPrefs.SetInt("trackID", trackID);
            }

            Switch(trackID);
        }

    }
}
