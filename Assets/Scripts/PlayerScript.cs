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

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetButtonDown("Fire1"))
        //{
        //    sfx[0].Play();

        //    var newProjectile = Instantiate(projectile, transform.position, transform.rotation);
        //    Rigidbody rb = newProjectile.GetComponent<Rigidbody>();
        //    Camera cam = GetComponentInChildren<Camera>();
        //    rb.velocity = cam.transform.rotation * Vector3.forward * 90;
        //}
    }
}
