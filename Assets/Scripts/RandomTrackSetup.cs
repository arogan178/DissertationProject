using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZenFulcrum.Track;
using static ZenFulcrum.Track.Track;
using static ZenFulcrum.Track.Track.SpeedAndForce;
using TMPro;

public class RandomTrackSetup : MonoBehaviour
{
    TrackCart cart;
    GameObject[] rewards;
    SpeedAndForce acceleration = new SpeedAndForce();
    SpeedAndForce brakes = new SpeedAndForce();
    GameObject gameData;
    RandomGameController randomGameController;

    public static AudioSource[] sfx;
    public bool fastModeOn = false;

    public TMP_Text nudgingMessage;
    int prevTrack;

    public int trackId = 0;

    DNNScript mlObject;

    public int track = 0;

    public GameObject crossHair;

    public void ChangeSetup()
    {
        //track = trackId;

        if (track == 1)
        {
            fastModeOn = true;

            crossHair.SetActive(true);

            foreach (GameObject reward in rewards)
            {
                reward.SetActive(true);
            }

            if (!sfx[3].isPlaying)
            {
                sfx[3].volume = 0;
                sfx[3].Play();
                StartCoroutine(FadeAudioSource.StartFade(sfx[3], 2.0f, 0.9f));
            }

            if (sfx[3].volume < 0.9)
            {
                StartCoroutine(FadeAudioSource.StartFade(sfx[3], 2.0f, 0.9f));
            }

            prevTrack = track;
            nudgingMessage.enabled = true;
            nudgingMessage.text = "Hit the flying boxes!";
            StartCoroutine(Fade());
        }
        else
        {
            fastModeOn = false;

            crossHair.SetActive(false);
            foreach (GameObject reward in rewards)
            {
                reward.SetActive(false);
            }

            StartCoroutine(FadeAudioSource.StartFade(sfx[3], 2.0f, 0.05f));

            if (track != prevTrack)
            {
                prevTrack = track;
                nudgingMessage.enabled = true;
                nudgingMessage.text = "Time to slow down";
                StartCoroutine(Fade());
            }
        }
    }

    IEnumerator Fade()
    {
        yield return new WaitForSeconds(2);
        nudgingMessage.enabled = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        randomGameController = GameObject.FindObjectOfType<RandomGameController>();
        cart = GameObject.FindObjectOfType<TrackCart>();
        sfx = GameObject.FindWithTag("gamedata").GetComponentsInChildren<AudioSource>();
        mlObject = GameObject.FindObjectOfType<DNNScript>();
        rewards = GameObject.FindGameObjectsWithTag("rewards");
        foreach (GameObject reward in rewards)
        {
            reward.SetActive(false);
        }

        prevTrack = 0;
    }

    void FixedUpdate()
    {
        SpeedAndForce acceleration = new SpeedAndForce();
        SpeedAndForce brakes = new SpeedAndForce();

        if (track == 1 || randomGameController.currentState == 0)
        {
            ChangeSetupState();

            acceleration.targetSpeed = 30;
            acceleration.maxForce = 30;
            acceleration.direction = ActDirection.Forward;

            brakes.targetSpeed = 0;
            brakes.maxForce = 0;
            brakes.direction = ActDirection.Forward;
        }
        else if (track == 0 || randomGameController.currentState == 1)
        {
            ChangeSetupState();

            acceleration.targetSpeed = 15;
            acceleration.maxForce = 15;
            acceleration.direction = ActDirection.Forward;

            brakes.targetSpeed = 15;
            brakes.maxForce = 15;
            brakes.direction = ActDirection.Forward;
        }

        var currentTrack = cart.CurrentTrack;
        currentTrack.acceleration = acceleration;
        currentTrack.brakes = brakes;


    }

    public void ChangeSetupState()
    {
        if (randomGameController.currentState == 0 && !fastModeOn)
        {
            fastModeOn = true;
            crossHair.SetActive(true);

            foreach (GameObject reward in rewards)
            {
                reward.SetActive(true);
            }

            if (!sfx[3].isPlaying)
            {
                sfx[3].volume = 0;
                sfx[3].Play();
                StartCoroutine(FadeAudioSource.StartFade(sfx[3], 2.0f, 0.5f));
            }

            if (sfx[3].volume == 0)
            {
                StartCoroutine(FadeAudioSource.StartFade(sfx[3], 2.0f, 0.5f));
            }

            prevTrack = track;
            nudgingMessage.enabled = true;
            nudgingMessage.text = "Hit the flying boxes!";
            StartCoroutine(Fade());
        }
        else if (randomGameController.currentState == 1 && fastModeOn)
        {
            fastModeOn = false;
            crossHair.SetActive(false);
            foreach (GameObject reward in rewards)
            {
                reward.SetActive(false);
            }

            StartCoroutine(FadeAudioSource.StartFade(sfx[3], 2.0f, 0.2f));

            if (track != prevTrack)
            {
                prevTrack = track;
                nudgingMessage.enabled = true;
                nudgingMessage.text = "Time to slow down";
                StartCoroutine(Fade());
            }
        }
    }
}


