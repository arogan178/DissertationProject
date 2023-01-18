using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    class GameHelpers
    {
        public static void ClearPlayerPrefs()
        {
            var activePlayer = PlayerPrefs.GetString("ActivePlayer");
            var dataLinesKey = activePlayer + "-dataLines";
            var dataLines = PlayerPrefs.GetInt("dataLinesKey");
            var modelDateKey = activePlayer + "-modelDownloadedDate";
            var trainingIterationsKey = activePlayer + "-modelIterations";

            var trainingIterations = PlayerPrefs.GetInt(trainingIterationsKey);
            var modelDate = PlayerPrefs.GetString(modelDateKey);
            var dataLinesNum = PlayerPrefs.GetInt(activePlayer + "-dataItems");
            var highScore = PlayerPrefs.GetInt("highScore");
            PlayerPrefs.DeleteAll();

            PlayerPrefs.SetInt("highScore",highScore);
            PlayerPrefs.SetInt(trainingIterationsKey, trainingIterations);
            PlayerPrefs.SetInt(dataLinesKey,dataLines);
            PlayerPrefs.SetInt(activePlayer + "-dataItems", dataLinesNum);
            PlayerPrefs.SetString(modelDateKey,modelDate);

        }
    }
}
