using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetAnimSpeed : MonoBehaviour
{
    //Transform animal;//set target from inspector instead of looking in Update
    GameObject animal;
    Animator m_Animator;
    string m_ClipName;
    AnimatorClipInfo[] m_CurrentClipInfo;

    float m_CurrentClipLength;


    // Start is called before the first frame update
    void Start()
    {
        //Get them_Animator, which you attach to the GameObject you intend to animate.
        m_Animator = gameObject.GetComponent<Animator>();
        //Fetch the current Animation clip information for the base layer
        m_CurrentClipInfo = this.m_Animator.GetCurrentAnimatorClipInfo(0);
        //Access the current length of the clip
        m_CurrentClipLength = m_CurrentClipInfo[0].clip.length;
        //Access the Animation clip name
        m_ClipName = m_CurrentClipInfo[0].clip.name;

        //print(gameObject.name + "Before  =  " + m_Animator.speed);
        m_Animator.speed = Random.Range(0.5f, 1.5f);
        //animation["Idle"].time = Random.Range(0.0, animation["Idle"].length);
        //print(gameObject.name + "After  =  " + m_Animator.speed);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
