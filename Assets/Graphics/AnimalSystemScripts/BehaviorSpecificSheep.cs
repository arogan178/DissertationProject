using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviorSpecificSheep : AnimalShoal
{
    float varEatOld;
    float varEat;
    //Animator animator;
    //bool HasUniqueBehavior = true;



    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        
        varEat = Random.Range(-2.0f, 2.0f);
        //animator = GetComponent<Animator>();
        HasUniqueBehavior = true;
        ////Debug.Log("HasUniqueBehavior is now" + HasUniqueBehavior);
        //GetComponent<AnimalShoal>().UniqueBehaviorScript = this;
        //UniqueBehaviorRun();
    }

    protected override void UniqueBehaviorRun()
    {
        idleChoice();
    }

    void idleChoice()
    {

        if (Random.Range(0, 3000) < 1)
        {
            varEatOld = varEat;
            varEat = Random.Range(-2.0f, 2.0f);
            //Debug.Log(varEatOld + " = Eatmix changed = " + varEat);
        }


        animator.SetFloat("Idle_Eat", Mathf.Lerp(varEat, varEatOld, Time.deltaTime * 0.1f));

    }
}
