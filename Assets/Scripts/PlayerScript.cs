using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using ZenFulcrum.Track;

public class PlayerScript : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject projectile;
    public static AudioSource[] sfx;

    public AudioClip aclip;
    AudioSource asource;

    

    void Start()
    {
        sfx = GameObject.FindWithTag("gamedata").GetComponentsInChildren<AudioSource>();
        asource = GetComponent<AudioSource>();
    }
}
