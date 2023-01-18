using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lookAtY : MonoBehaviour
{
    [SerializeField] GameObject target;
    //[SerializeField] float damping;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //var lookPos = target.transform.position - transform.position;
        //lookPos.y = 0;
        //var rotation = Quaternion.LookRotation(lookPos);
        //transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * damping);

        Vector3 targetPostition = new Vector3(target.transform.position.x, this.transform.position.y, target.transform.position.z);
        this.transform.LookAt(targetPostition);
    }
}
