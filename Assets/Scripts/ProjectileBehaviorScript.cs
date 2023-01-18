using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehaviorScript : MonoBehaviour
{
    float timeAlive = 0.0f;
    public static AudioSource[] sfx;

    public GameObject followTarget;

    public int interpolationFramesCount = 1; // Number of frames to completely interpolate between the 2 positions
    int elapsedFrames = 0;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("enemy"))
        {
            GameDataScript.singleton.UpdateScore(1);
            sfx[1].Play();

            Destroy(collision.gameObject);
            Destroy(gameObject);
        }

        if (collision.gameObject.CompareTag("diamond"))
        {
            GameDataScript.singleton.UpdateScore(5);
            sfx[2].Play();

            //StartCoroutine(HideForSeconds(collision));
            Destroy(gameObject);
        }

        if (collision.gameObject.CompareTag("hex"))
        {
            GameDataScript.singleton.UpdateScore(10);
            sfx[2].Play();

            // StartCoroutine(HideForSeconds(collision));
            Destroy(gameObject);
        }

        if (collision.gameObject.CompareTag("star"))
        {
            GameDataScript.singleton.UpdateScore(15);
            sfx[2].Play();

            //StartCoroutine(HideForSeconds(collision));
            Destroy(gameObject);
        }

        if (collision.gameObject.CompareTag("gem"))
        {
            GameDataScript.singleton.UpdateScore(20);
            sfx[2].Play();

            //StartCoroutine(HideForSeconds(collision));
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        sfx = GameObject.FindWithTag("gamedata").GetComponentsInChildren<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        timeAlive += Time.deltaTime;
        if (timeAlive > 5)
        {
            Destroy(gameObject);
        }

        if (followTarget != null)
        {
            float interpolationRatio = (float)elapsedFrames / interpolationFramesCount;

            transform.position = Vector3.Lerp(transform.position, followTarget.transform.position, interpolationRatio);

            elapsedFrames = (elapsedFrames + 1) % (interpolationFramesCount + 1);  // reset elapsedFrames to zero after it reached (interpolationFramesCount + 1)
        }
    }
}
