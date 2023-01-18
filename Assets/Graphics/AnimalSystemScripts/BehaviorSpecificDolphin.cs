using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviorSpecificDolphin : AnimalShoal
{
    //public GameObject sea;
    float seaLevel;


    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        HasUniqueBehavior = true;
        
        seaLevel = _containerDepth.y + _containerCentre.y;
        //Debug.Log("seaLevel = " + seaLevel);
        //Debug.Log("DolphinLevel = " + transform.position.y);
        switchAction();

    }

    protected override void UniqueBehaviorRun()
    {
        
        if(transform.position.y > (seaLevel - 0.5f))
        {
            switchAction();
            triggerAction();
        }
    }


    void switchAction()
    {

        if(Random.Range(1, 4) == 1)
        {
            animator.SetBool("Walk_Jump", true);
            //Debug.Log("is walk");
        }
        else
        {
            animator.SetBool("Walk_Jump", false);
            //Debug.Log("is Jump");
        }

    }

    void triggerAction()
    {
        if(Random.Range(0, 10) < 1)
        {
            animator.SetTrigger("Action");
            //Debug.Log("Action Triggered");
            //Debug.Log("DolphinLevel = " + transform.position.y);
            //animator.ResetTrigger("Action");
            KeepUpRight();
            _renderer.material.SetColor("_Color", c_custom3);
        }
    }

}
