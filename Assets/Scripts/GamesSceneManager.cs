using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

public class GamesSceneManager : MonoBehaviour
{
    NewHeartRateScript hrScript;
    int dataLines = 0;
    int playedGames = 0;

    // Start is called before the first frame update
    void Start()
    {
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
        XRSettings.LoadDeviceByName("");

        // Must wait one frame after calling `XRSettings.LoadDeviceByName()`.
        yield return null;

        // Not needed, since loading the None (`""`) device takes care of this.
        // XRSettings.enabled = false;

        // Restore 2D camera settings.
        ResetCameras();
    }

    // Resets camera transform and settings on all enabled eye cameras.
    void ResetCameras()
    {
        // Camera looping logic copied from GvrEditorEmulator.cs
        for (int i = 0; i < Camera.allCameras.Length; i++)
        {
            Camera cam = Camera.allCameras[i];
            if (cam.enabled && cam.stereoTargetEye != StereoTargetEyeMask.None)
            {

                // Reset local position.
                // Only required if you change the camera's local position while in 2D mode.
                cam.transform.localPosition = Vector3.zero;

                // Reset local rotation.
                // Only required if you change the camera's local rotation while in 2D mode.
                cam.transform.localRotation = Quaternion.identity;

                // No longer needed, see issue github.com/googlevr/gvr-unity-sdk/issues/628.
                // cam.ResetAspect();

                // No need to reset `fieldOfView`, since it's reset automatically.
            }
        }
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
            }
            catch (Exception e)
            {

            }
            StartCoroutine(SwitchTo2D());
            SceneManager.LoadScene("Menu");
        }
    }
}
