using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using UnityEngine.XR.Management;

public class GamesSceneManager : MonoBehaviour
{
    NewHeartRateScript hrScript;
    int dataLines = 0;
    int playedGames = 0;

    // Start is called before the first frame update
    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;

        var activePlayer = PlayerPrefs.GetString("ActivePlayer");
        var dataLinesKey = activePlayer + "-dataLines";
        var playedGamesKey = activePlayer + "-GamesPlayed";

        hrScript = GameObject.FindObjectOfType<NewHeartRateScript>();

        if (PlayerPrefs.HasKey(dataLinesKey))
        {
            var dataItemsKey = PlayerPrefs.GetInt(dataLinesKey);
            dataLines = dataItemsKey;
        }
        else
        {
            PlayerPrefs.SetInt(dataLinesKey, 0);
        }

        if (PlayerPrefs.HasKey(playedGamesKey))
        {
            var numberOfPlayedGamesKey = PlayerPrefs.GetInt(playedGamesKey);
            playedGames = numberOfPlayedGamesKey;
        }
        else
        {
            PlayerPrefs.SetInt(playedGamesKey, 0);
        }
    }

    // Call via `StartCoroutine(SwitchTo2D())` from your code. Or, use
    // `yield SwitchTo2D()` if calling from inside another coroutine.
    IEnumerator SwitchTo2D()
    {
        // Empty string loads the "None" device.
        XRGeneralSettings.Instance.Manager.StopSubsystems();

        // Must wait one frame after calling `XRSettings.LoadDeviceByName()`.
        yield return null;
        XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        // Not needed, since loading the None (`""`) device takes care of this.
        // XRSettings.enabled = false;

        // Restore 2D camera settings.
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            try
            {
                var activePlayer = PlayerPrefs.GetString("ActivePlayer");

                // close icon pressed, place appropriate code here
                hrScript.UpdateServerWeights();
                GameHelpers.ClearPlayerPrefs();

                string dataLinesKey = activePlayer + "-dataLines";
                var playedGamesKey = activePlayer + "-GamesPlayed";

                var numOfLines = hrScript.trainDataList.Count + dataLines;
                var numOfPlayedGames = playedGames + 1;

                PlayerPrefs.SetInt(dataLinesKey, numOfLines);
                PlayerPrefs.SetInt(playedGamesKey, numOfPlayedGames);
                StartCoroutine(SwitchTo2D());
                SceneManager.LoadScene("Menu");
                PlayerPrefs.Save();
            }
            catch (Exception e) { }
        }
    }
}
