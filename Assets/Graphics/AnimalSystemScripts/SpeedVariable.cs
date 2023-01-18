using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedVariable : MonoBehaviour
{
    float speed;
    Animator animator;
    //int _id;

    // Use this for initialization
    void Start()
    {
        animator = GetComponent<Animator>();
        //_id = GetInstanceID();
    }

    // Update is called once per frame
    void Update()
    {
        speed = GetComponent<AnimalShoal>().speedActual;
        //Debug.Log("speed of " + _id + " is " + speed);
        //Debug.Log("speed is " + speed);

        //if (animator.GetParameter(1).name == "SpeedBlend")
        //{
        //    animator.SetFloat("SpeedBlend", speed);
        //}

        animator.SetFloat("SpeedBlend", speed);

    }
}
