using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimerManager : MonoBehaviour
{
    public float countDownTimer = 10.0f;
    public TMP_Text timerText;
    public AudioClip aclip;

    public bool timeElapsed = false;
    public static AudioSource[] sfx;

    // Start is called before the first frame update
    void Start()
    {
        sfx = GameObject.FindWithTag("gamedata").GetComponentsInChildren<AudioSource>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (countDownTimer >= 0)
        {
            countDownTimer = countDownTimer - Time.deltaTime;
            int timer = (int)Math.Round(countDownTimer);
            if (timer > 0)
            {
                timerText.text = timer.ToString();
            }
            else
            {
                timerText.text = "GO!";
            }
            //sfx.Last().Play(aclip);
        }
        else
        {
            timeElapsed = true;
            timerText.text = "";
        }
    }
}
