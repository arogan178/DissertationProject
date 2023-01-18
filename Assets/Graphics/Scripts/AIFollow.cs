using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIFollow : MonoBehaviour

{

    public Transform target;//set target from inspector instead of looking in Update
    float speed = 1f;
    public float multiplier = 1f;
    private float t;
    float rate;
    float Distance;
    Vector3 targetPosOld;
    Vector3 targetPosNew;
    Vector3 targetPos;
    //float newSpeed;
    float oldSpeed;
    int state = 3;
    int i = 1;



    void Start()
    {
        targetPosOld = target.position;
        targetPos = target.position;

        //////set state/////
        
        //State 1
        if (Distance <= 3f && state != 1)
        {
            state = 1;
        }

        //State 2
        if (Distance > 3f && Distance < 10f && state != 2)
        {
            state = 2;
        }

        //State 3
        if (Distance >= 10f && state != 3)
        {
            state = 3;
        }
    }

    void Update()
    {
        Distance = Vector3.Distance(transform.position, target.position);
        //print("Distance = " + Distance);

        t += Time.deltaTime ;
        rate = Mathf.SmoothStep(0.0f, 1.0f, t * speed * multiplier);
        //print("Timer = " + timer);

        //Look at the target
        if (Random.Range(0, 3) < 1)
        {
            //rotate to look at the player
            transform.LookAt(target.position);
            //transform.Rotate(new Vector3(0, 0, 0), Space.Self);//correcting the original rotation                        
        }

        //Target Position
        if (Random.Range(0, 10) < 1)
        {
            //CReate a random vector
            Vector3 randomVector = new Vector3(Random.Range(-3f, 3f), Random.Range(-3f, 3f), Random.Range(-1f, 1f));

            //Use Random vector for offsetting animal
            targetPosOld = targetPosNew;
            targetPosNew = target.position + randomVector;
        }

        //State 1
        if (Distance <= 3f && state != 1)
        {
            state = 1;

            oldSpeed = speed;
            speed = 0f;

            //print("-----AFTER-----");
            //print("OldSpeed = " + oldSpeed);
            //print("Speed = " + speed);
        }

        //State 2
        if (Distance > 3f && Distance < 10f && state != 2)
        {
            state = 2;


            oldSpeed = speed;
            speed = 5f;

            //print("-----AFTER-----");
            //print("OldSpeed = " + oldSpeed);
            //print("Speed = " + speed);
        }

        //State 3
        if (Distance >= 10f && state != 3)
        {
            state = 3;

            oldSpeed = speed;
            speed = 15f;

            //print("-----AFTER-----");
            //print("OldSpeed = " + oldSpeed);
            //print("Speed = " + speed);
        }

        //print(i + " : " + " State: " + state + " : " + "Distance: " + Distance + " : " + "Speed: " + speed + " : " + " Rate: " + rate);
        i = i+1;
        targetPos = Vector3.Lerp(targetPosOld, targetPosNew, rate);

        //float step = speed * timer;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, rate);
    }







}