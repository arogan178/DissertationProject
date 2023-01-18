using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameDataScript : MonoBehaviour
{
    public static GameDataScript singleton;
    public TMP_Text scoreText = null;
    int score = 0;
    public TMP_Text nudgingText = null;
    public TMP_Text highScoreText;

    string[] nudgingMessages = new string[]
    {
        "Well Done!",
        "Keep Going!",
        "Great Job!",
        "Keep it up!",
        "WHat a hit!"
    };
    int displayedAt = 0;

    int highScore = 0;
    AudioSource[] sfx;

    public int trackId;

    private void OnDestroy()
    {
        if (sfx[3].isPlaying)
        {
            sfx[3].Stop();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        sfx = GameObject.FindWithTag("gamedata").GetComponentsInChildren<AudioSource>();
        nudgingText.enabled = true;

        singleton = this;

        if (PlayerPrefs.HasKey("highScore"))
        {
            highScore = PlayerPrefs.GetInt("highScore");
        }

        highScoreText.text = "High score: " + highScore.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if (score > 0 && score % 5 == 0 && displayedAt != score && !nudgingText.isActiveAndEnabled)
        {
            int randMsg = Random.Range(0, nudgingMessages.Length);
            nudgingText.text = nudgingMessages[randMsg];
            nudgingText.enabled = true;
            StartCoroutine(Fade());
        }
    }

    IEnumerator Fade()
    {
        displayedAt = score;
        yield return new WaitForSeconds(2);
        nudgingText.enabled = false;
    }

    public void UpdateScore(int s)
    {
        score += s;

        if (score > highScore)
        {
            PlayerPrefs.SetInt("highScore", score);
            highScore = score;
            highScoreText.text = "High score: " + highScore.ToString();
            nudgingText.enabled = true;
            StartCoroutine(Fade());
        }

        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    private void OnApplicationQuit() { }
}
