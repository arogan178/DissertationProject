using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZenFulcrum.Track;

public class TrackVisibility : MonoBehaviour
{
    protected Track track;

    // Start is called before the first frame update
    void Start()
    {
        track.NextTrack.GetComponent<MeshRenderer>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
