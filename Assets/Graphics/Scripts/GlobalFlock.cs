using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//https://www.youtube.com/watch?v=eMpI1eCsIyM&t=373s
public class GlobalFlock : MonoBehaviour
{
    GameObject shoalObject;
    [SerializeField] GameObject enemyObject1;
    [HideInInspector]
    public Vector3 shoalCentre;
    [HideInInspector]
    public Vector3 enemyLoc1;
    public GameObject foodPrefab;
    public GameObject fishPrefab;
    [HideInInspector]
    public float tankSize;
    [HideInInspector]
    public float tankHeight;
    [SerializeField] float FishSizeMin = 0.5f;
    [SerializeField] float FishSizeMax = 1.5f;
    public int numFish = 20;
    //public float fishScale = 1f;
    [HideInInspector]
    public Vector3 goalPos = Vector3.zero;

    //public GameObject[] allFish = new GameObject[numFish];
    [HideInInspector]
    public GameObject[] allFish;
    public GameObject tankDimGO;
    Vector3 tankDim;

    // Start is called before the first frame update
    void Start()
    {
        tankDim = tankDimGO.transform.localScale;
        //Debug.Log(this + "TankDim =" + tankDim);
        tankSize = tankDim.x/2.2f;
        //Debug.Log(this + "TankSize =" + tankSize);
        tankHeight = tankDim.y/2.2f;
        //Debug.Log(this + "TankHeight =" + tankHeight);
        tankDimGO.transform.localScale = new Vector3(tankDim.x, tankDim.y, tankDim.x);
        tankDimGO.SetActive(false);

        shoalObject = this.gameObject;
        //Debug.Log("Game Object is " + shoalObject);
        shoalCentre = shoalObject.transform.position;
        //Debug.Log(shoalCentre);

        allFish = new GameObject[numFish];

        for (int i = 0; i < numFish; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-tankSize, tankSize), Random.Range(-tankHeight, tankHeight), Random.Range(-tankSize, tankSize)) + shoalCentre;
            allFish[i] = (GameObject)Instantiate(fishPrefab, pos, Quaternion.identity, transform);
            
            allFish[i].transform.localScale = new Vector3(1,1,1) * (Random.Range(FishSizeMin, FishSizeMax));
            allFish[i].GetComponent<Flock>().Init(this);            
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        enemyLoc1 = enemyObject1.transform.position;

        if (Random.Range(0, 10000) < 20)
        {
            goalPos = new Vector3(Random.Range(-tankSize, tankSize), Random.Range(-tankHeight, tankHeight), Random.Range(-tankSize, tankSize)) + shoalCentre;
                       
            foodPrefab.transform.position = goalPos;
            //goalPos = foodPrefab.transform.position;

        }

    }
}