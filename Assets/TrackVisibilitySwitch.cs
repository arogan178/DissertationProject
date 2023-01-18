using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZenFulcrum.Track;

public class TrackVisibilitySwitch : MonoBehaviour
{
    [SerializeField]
    GameObject track;
    Track trackComponent;

    private void OnTriggerEnter(Collider other)
    {
        try
        {
            trackComponent.NextTrack.GetComponent<MeshRenderer>().enabled = true;
            trackComponent.PrevTrack.GetComponent<MeshRenderer>().enabled = false;

            GameObject childGameObjectNext = trackComponent.NextTrack.transform.GetChild(0).gameObject;
            if (childGameObjectNext.tag.Equals("rewards"))
            {
                var CG1 = childGameObjectNext.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
                var CG2 = childGameObjectNext.transform.GetChild(1).GetComponent<MeshRenderer>().enabled = true;
                var CG3 = childGameObjectNext.transform.GetChild(2).GetComponent<MeshRenderer>().enabled = true;
                var CG4 = childGameObjectNext.transform.GetChild(3).GetComponent<MeshRenderer>().enabled = true;
            }

            GameObject childGameObjectPrev = trackComponent.PrevTrack.transform.GetChild(0).gameObject;
            if (childGameObjectPrev.tag.Equals("rewards"))
            {
                var CG1 = childGameObjectPrev.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
                var CG2 = childGameObjectPrev.transform.GetChild(1).GetComponent<MeshRenderer>().enabled = false;
                var CG3 = childGameObjectPrev.transform.GetChild(2).GetComponent<MeshRenderer>().enabled = false;
                var CG4 = childGameObjectPrev.transform.GetChild(3).GetComponent<MeshRenderer>().enabled = false;
            }
        }
        catch(Exception e)
        {

        }
    }

    // Start is called before the first frame update
    void Start()
    {
        trackComponent = track.GetComponent<Track>();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
