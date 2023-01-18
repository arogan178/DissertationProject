using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviorScript : MonoBehaviour
{
    // Adjust the speed for the application.
    public float speed = 1.0f;

    //GameObject followSphere;
    public GameObject explosion;

    public bool missileIncoming = false;
    private float timeAlive = 0;

    public GameObject target;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "projectile") {
            target.GetComponent<FollowCubeManager>().isOccupied = false;
            GameObject obj = Instantiate(explosion, gameObject.transform.position, Quaternion.identity);

            Destroy(obj, 2.5f);
        }    
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (timeAlive < 6)
        {
            timeAlive = timeAlive + Time.deltaTime;

            speed = 60.0f;// + Random.Range(0.0f, 5.0f);
            float step = speed * Time.deltaTime; // calculate distance to move
                                                 //transform.position = Vector3.MoveTowards(transform.position, target.transform.position, step);
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(target.transform.position.x, target.transform.position.y, target.transform.position.z), step);
        }
        else
        {
            target.GetComponent<FollowCubeManager>().isOccupied = false;
            Destroy(gameObject);
        }

        //if (Vector3.Distance(transform.position, target.transform.position) > 3.0f)
        //{

        //    if (Vector3.Distance(transform.position, target.transform.position) > 20.0f)
        //    {
        //        speed = 60.0f + Random.Range(0.0f, 5.0f);
        //    }

        //    float step = speed * Time.deltaTime; // calculate distance to move

        //    transform.position = Vector3.MoveTowards(transform.position, new Vector3(target.transform.position.x, target.transform.position.y, target.transform.position.z), step);
        //}

        //if (Vector3.Distance(transform.position, target.transform.position) > 3.0f)
        //{
        //    if (Vector3.Distance(transform.position, target.transform.position) > 20.0f)
        //    {
        //        speed = 45.0f + Random.Range(0.0f, 5.0f);
        //    }

        //    transform.position = Vector3.MoveTowards(transform.position, new Vector3(target.transform.position.x + Random.Range(-3.0f, 3.0f), target.transform.position.y + Random.Range(-2.0f, 2.0f), target.transform.position.z + Random.Range(-3.0f, 3.0f)), step);
        //}
    }
}
