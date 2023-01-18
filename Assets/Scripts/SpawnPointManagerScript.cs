using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPointManagerScript : MonoBehaviour
{
    NewHeartRateScript hsScript;

    public GameObject enemy;
    GameObject player;
    TrackSetup trackSetup;
    RandomTrackSetup randomTrackSetup;

    [SerializeField]
    GameObject[] followPoints = new GameObject[13];

    FollowCubeManager followPoint1Manager;
    FollowCubeManager followPoint2Manager;
    FollowCubeManager followPoint3Manager;
    FollowCubeManager followPoint4Manager;
    FollowCubeManager followPoint5Manager;
    FollowCubeManager followPoint6Manager;
    FollowCubeManager followPoint7Manager;
    FollowCubeManager followPoint8Manager;
    FollowCubeManager followPoint9Manager;
    FollowCubeManager followPoint10Manager;
    FollowCubeManager followPoint11Manager;
    FollowCubeManager followPoint12Manager;
    FollowCubeManager followPoint13Manager;
    
    List<FollowCubeManager> followCubeManagers = new List<FollowCubeManager>();
    List<GameObject> followCubeList = new List<GameObject>();
    
    // Start is called before the first frame update
    void Start()
    {
        //followPoint1 = GameObject.FindGameObjectWithTag("followCube1");
        //followPoint1Manager = followPoint1.GetComponent<FollowCubeManager>();
        
        //followPoint2 = GameObject.FindGameObjectWithTag("followCube2");
        //followPoint2Manager = followPoint1.GetComponent<FollowCubeManager>();
        
        //followPoint3 = GameObject.FindGameObjectWithTag("followCube3");
        //followPoint3Manager = followPoint1.GetComponent<FollowCubeManager>();
        
        //followPoint4 = GameObject.FindGameObjectWithTag("followCube4");
        //followPoint4Manager = followPoint1.GetComponent<FollowCubeManager>();
        
        //followPoint5 = GameObject.FindGameObjectWithTag("followCube5");
        //followPoint5Manager = followPoint1.GetComponent<FollowCubeManager>();
        
        //followPoint6 = GameObject.FindGameObjectWithTag("followCube6");
        //followPoint6Manager = followPoint1.GetComponent<FollowCubeManager>();
        
        //followPoint7 = GameObject.FindGameObjectWithTag("followCube7");
        //followPoint7Manager = followPoint1.GetComponent<FollowCubeManager>();
        
        //followPoint8 = GameObject.FindGameObjectWithTag("followCube8");
        //followPoint8Manager = followPoint1.GetComponent<FollowCubeManager>();
        
        //followPoint9 = GameObject.FindGameObjectWithTag("followCube9");
        //followPoint9Manager = followPoint1.GetComponent<FollowCubeManager>();
        
        //followPoint10 = GameObject.FindGameObjectWithTag("followCube10");
        //followPoint10Manager = followPoint1.GetComponent<FollowCubeManager>();
        
        //followPoint11 = GameObject.FindGameObjectWithTag("followCube11");
        //followPoint11Manager = followPoint1.GetComponent<FollowCubeManager>();
        
        //followPoint12 = GameObject.FindGameObjectWithTag("followCube12");
        //followPoint12Manager = followPoint1.GetComponent<FollowCubeManager>();
        
        //followPoint13 = GameObject.FindGameObjectWithTag("followCube13");
        //followPoint13Manager = followPoint1.GetComponent<FollowCubeManager>();

        //followCubeManagers= new List<FollowCubeManager>() { followPoint1Manager, followPoint2Manager, followPoint3Manager, followPoint4Manager, followPoint5Manager, followPoint6Manager, followPoint7Manager, followPoint8Manager, followPoint9Manager, followPoint10Manager, followPoint11Manager, followPoint12Manager, followPoint13Manager };
        
        //followCubeList= new List<GameObject>() { followPoint1, followPoint2, followPoint3, followPoint4, followPoint5, followPoint6, followPoint7, followPoint8, followPoint9, followPoint10, followPoint11, followPoint12, followPoint13 };

        player = GameObject.FindGameObjectWithTag("player");
        trackSetup = GameObject.FindObjectOfType<TrackSetup>();
        randomTrackSetup = GameObject.FindObjectOfType<RandomTrackSetup>();
        hsScript = GameObject.FindObjectOfType<NewHeartRateScript>();
    }

    // Update is called once per frame
    void Update()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("enemy");
        //if (trackSetup.track == 1 || hsScript.currentState == 0)
        bool isRandom = Convert.ToBoolean(PlayerPrefs.GetInt("isRandom"));
        if (!isRandom)
        {
            if (hsScript.currentState == 0 || trackSetup.fastModeOn)
            {
                if (enemies.Length < 8)
                {
                    for (int i = 0; i < (10 - enemies.Length); i++)
                    {
                        List<int> availableSpots = new List<int>();

                        for (int j = 0; j < followPoints.Length; j++)
                        {
                            var followPointManager = followPoints[j].GetComponent<FollowCubeManager>();
                            if (!followPointManager.isOccupied)
                            {
                                availableSpots.Add(j);
                            }
                        }

                        int targetPointIdx = UnityEngine.Random.Range(0, followPoints.Length);
                        GameObject chosenPoint = followPoints[targetPointIdx];
                        var newEnemy = Instantiate(enemy, this.transform.position, transform.rotation * Quaternion.Euler(90f, 0f, 0f));
                        newEnemy.transform.parent = gameObject.transform;
                        newEnemy.GetComponent<EnemyBehaviorScript>().target = chosenPoint;
                        chosenPoint.GetComponent<FollowCubeManager>().isOccupied = true;
                    }
                }
            }
            //else if (trackSetup.track == 0 || hsScript.currentState == 1)
            else if (hsScript.currentState == 1 || !trackSetup.fastModeOn)
            {
                if (enemies != null)
                {
                    foreach (GameObject enemy in enemies)
                    {
                        Destroy(enemy);
                    }
                }
            }
        }
        else
        {
            if (randomTrackSetup.fastModeOn)
            {
                if (enemies.Length < 8)
                {
                    for (int i = 0; i < (10 - enemies.Length); i++)
                    {
                        List<int> availableSpots = new List<int>();

                        for (int j = 0; j < followPoints.Length; j++)
                        {
                            var followPointManager = followPoints[j].GetComponent<FollowCubeManager>();
                            if (!followPointManager.isOccupied)
                            {
                                availableSpots.Add(j);
                            }
                        }

                        int targetPointIdx = UnityEngine.Random.Range(0, followPoints.Length);
                        GameObject chosenPoint = followPoints[targetPointIdx];
                        var newEnemy = Instantiate(enemy, this.transform.position, transform.rotation * Quaternion.Euler(90f, 0f, 0f));
                        newEnemy.transform.parent = gameObject.transform;
                        newEnemy.GetComponent<EnemyBehaviorScript>().target = chosenPoint;
                        chosenPoint.GetComponent<FollowCubeManager>().isOccupied = true;
                    }
                }
            }
            //else if (trackSetup.track == 0 || hsScript.currentState == 1)
            else if (!randomTrackSetup.fastModeOn)
            {
                if (enemies != null)
                {
                    foreach (GameObject enemy in enemies)
                    {
                        Destroy(enemy);
                    }
                }
            }
        }
    }
}
