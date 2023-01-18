using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRaycast : MonoBehaviour
{
    public GameObject projectile;
    public static AudioSource[] sfx;

    public AudioClip aclip;
    AudioSource asource;

    Material crosshairMaterial;
    public GameObject crosshair;

    // Start is called before the first frame update
    void Start()
    {
        crosshairMaterial = crosshair.GetComponent<Renderer>().material;
        //trackSwitcher = GameObject.FindObjectOfType<TrackSwitcher>();
        sfx = GameObject.FindWithTag("gamedata").GetComponentsInChildren<AudioSource>();
        asource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        // Bit shift the index of the layer (8) to get a bit mask
        int layerMask = 1 << 8;

        // This would cast rays only against colliders in layer 8.
        // But instead we want to collide against everything except layer 8. The ~ operator does this, it inverts a bitmask.
        layerMask = ~layerMask;

        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 50, layerMask))
        {
            if (hit.transform.gameObject.tag.Equals("enemy"))
            {
                crosshairMaterial.color = Color.green;
                EnemyBehaviorScript enemy = hit.transform.gameObject.GetComponent<EnemyBehaviorScript>();

                if (!enemy.missileIncoming)
                {
                    enemy.missileIncoming = true;
                    sfx[0].Play();

                    var newProjectile = Instantiate(projectile, transform.position, transform.rotation);
                    var projectileDetails = newProjectile.GetComponent<ProjectileBehaviorScript>();
                    projectileDetails.followTarget = hit.transform.gameObject;

                    Rigidbody rb = newProjectile.GetComponent<Rigidbody>();
                    //Camera cam = GetComponentInChildren<Camera>();
                    rb.velocity = this.transform.rotation * Vector3.forward * 90;

                    Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);

                    //Debug.Log("Did Hit");
                }
            }
            else if (hit.transform.gameObject.tag.Contains("diamond") || hit.transform.gameObject.tag.Contains("hex") || hit.transform.gameObject.tag.Contains("star") || hit.transform.gameObject.tag.Contains("gem"))
            {
                crosshairMaterial.color = Color.green;
                RewardManagerScript enemy = hit.transform.gameObject.GetComponent<RewardManagerScript>();

                if (!enemy.missileIncoming)
                {
                    enemy.missileIncoming = true;
                    sfx[0].Play();

                    var newProjectile = Instantiate(projectile, transform.position, transform.rotation);
                    var projectileDetails = newProjectile.GetComponent<ProjectileBehaviorScript>();
                    projectileDetails.followTarget = hit.transform.gameObject;

                    Rigidbody rb = newProjectile.GetComponent<Rigidbody>();
                    //Camera cam = GetComponentInChildren<Camera>();
                    rb.velocity = this.transform.rotation * Vector3.forward * 90;

                    Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);

                    //Debug.Log("Did Hit");
                }
            }
        }
        else
        {
            crosshairMaterial.color = Color.white;
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 50, Color.white);
            //Debug.Log("Did not Hit");
        }
    }
}
