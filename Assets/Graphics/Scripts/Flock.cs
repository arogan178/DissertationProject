using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//https://www.youtube.com/watch?v=eMpI1eCsIyM&t=373s

public class Flock : MonoBehaviour
{
    public float speed = 1.0f;
    float speedMin = 1.0f;
    float speedMax = 4.0f;
    float rotationSpeedMax = 1.0f;
    float rotationSpeedMin = 0.1f;
    Vector3 averageHeading;
    Vector3 averagePosition;
    float neighbourDistance = 12.0f;
    float avoidDistance = 0.2f;
    public float weight = 1.3f;
    Renderer fishRenderer;
    int numFish;
    Vector3 enemyLoc1;
    Vector3 shoalCentre;
    Vector3 goalPos;
    float tankSize;
    GameObject[] allFish;
    GlobalFlock shoal;
    bool turning = false;
    bool running = false;

    public void Init(GlobalFlock parentShoal)
    {
        shoal = parentShoal;
        numFish = parentShoal.numFish;
        enemyLoc1 = parentShoal.enemyLoc1;
        shoalCentre = parentShoal.shoalCentre;
        tankSize = parentShoal.tankSize;
        allFish = parentShoal.allFish;
    }
    // Start is called before the first frame update
    void Start()
    {
        /*
        GlobalFlock shoal = this.GetComponentInParent<GlobalFlock>();
        if (shoal!= null)
        {
            numFish = shoal.numFish;
            enemyLoc1 = shoal.enemyLoc1;
            shoalCentre = shoal.shoalCentre;
            tankSize = shoal.tankSize;
            allFish = shoal.allFish;
            //Debug.Log("numFish is " + numFish);
        }*/


        speed = Random.Range(speedMin, speedMax);
        fishRenderer = GetComponentInChildren<Renderer>();
        //Debug.Log("material is" + fishRenderer.material);
        //fishRenderer.material.SetColor("_Color", Color.green);
    }

    // Update is called once per frame    
    void Update()
    {
        if (shoal != null)
        {
            goalPos = shoal.goalPos;           
        }


        if (Vector3.Distance(transform.position, enemyLoc1) <= 7)
        {
            running = true;            
        }
        else
        {
            running = false;        
        }


        if (Vector3.Distance(transform.position, shoalCentre) >= (tankSize))
        {
            turning = true;            
        }
        else
        {
            turning = false;
            //Debug.Log("Is NOT turning");
        }


        if(running)
        {
            //fishRenderer.material.SetColor("_Color", Color.red);

            Vector3 direction = enemyLoc1 - transform.position;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeedMax * Time.deltaTime * 5);
            speed = Random.Range(speedMin, speedMax)*20;
            

            /*
            float step = speed * Time.deltaTime; // calculate distance to move
            //transform.position = Vector3.MoveTowards(transform.position, GlobalFlock.enemyLoc1, (-10 * step));

            Vector3 targetDir = GlobalFlock.enemyLoc1 - transform.position;
            Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, step, -1.5f);
            Debug.DrawRay(transform.position, newDir, Color.red);
            transform.rotation = Quaternion.LookRotation(newDir);
            */
        }
        else if (turning)
        {
            //fishRenderer.material.SetColor("_Color", Color.blue);

            Vector3 direction = shoalCentre - transform.position;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Random.Range(rotationSpeedMin, rotationSpeedMax) * Time.deltaTime);

            speed = Random.Range(speedMin, speedMax);
        }
        else
        
        {
            
            if (Random.Range(0,6) < 1)
                    ApplyRules();
                    //Debug.Log("RULES BEING APPLIED");
        }

        transform.Translate(0, 0, Time.deltaTime * speed);
    }

    void ApplyRules()
    {
        GameObject[] gos;
        gos = allFish;

        Vector3 vcentre = Vector3.zero;
        Vector3 vavoid = Vector3.zero;
        float gSpeed = 0.1f;

        //Vector3 goalPos = GlobalFlock.goalPos;

        float dist;

        int groupSize = 0;
        foreach (GameObject go in gos)
        {
            if(go != this.gameObject)
            {
                //fishRenderer.material.SetColor("_Color", Color.yellow);
                dist = Vector3.Distance(go.transform.position, this.transform.position);
                //Debug.Log(go + " " + dist + "neighbourDistance is" + neighbourDistance);
                if(dist <= neighbourDistance)
                {
                    vcentre += go.transform.position;
                    groupSize++;

                    if(dist < avoidDistance)
                    {
                        //fishRenderer.material.SetColor("_Color", Color.yellow);
                        vavoid = vavoid + (this.transform.position - go.transform.position);
                        //vavoid = vavoid + Vector3.Lerp(this.transform.position, go.transform.position, Time.deltaTime);
                        Vector3 direction = (vcentre - transform.position) + (weight * vavoid);
                        //Vector3 direction = (vcentre + vavoid) - go.transform.position;
                        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Random.Range(rotationSpeedMin, rotationSpeedMax) * Time.deltaTime);
                    }

                    Flock anotherFlock = go.GetComponent<Flock>();
                    gSpeed = gSpeed + anotherFlock.speed;
                }
            }
        }

        if(groupSize > 0)
        {
            //fishRenderer.material.SetColor("_Color", Color.magenta);

            vcentre = vcentre / groupSize + (goalPos - this.transform.position);
            //vcentre = (goalPos - this.transform.position);
            speed = gSpeed/groupSize;

            Vector3 direction = (goalPos - transform.position) + (weight * vavoid);
            //Vector3 direction = (vcentre + vavoid) - transform.position;
            //Debug.Log("vcentre" + vcentre);
            if (direction != Vector3.zero)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Random.Range(rotationSpeedMin, rotationSpeedMax) * Time.deltaTime);
        }

    }

}
