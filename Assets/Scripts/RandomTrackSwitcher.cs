using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomTrackSwitcher : MonoBehaviour
{
    public Switcher switcher;
    public int switchVal = 0;

    //[SerializeField]
    //private DNNScript dNN;

    RandomTrackSetup tsScript;

    private void OnTriggerEnter(Collider other)
    {
        switcher.Switch(tsScript.trackId, false);
        tsScript.ChangeSetup();
    }

    // Start is called before the first frame update
    void Start()
    {
        tsScript = GameObject.FindObjectOfType<RandomTrackSetup>();
    }
}
