using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnSpecialRewardScript : MonoBehaviour
{
    [SerializeField]
    public GameObject[] rewards;

    private void OnTriggerEnter(Collider other)
    {
        var randomIdx = Random.Range(0, (rewards.Length - 1));
        var newEnemy = Instantiate(rewards[randomIdx], this.transform.parent.position, transform.rotation * Quaternion.Euler(90f, 0f, 0f));
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
