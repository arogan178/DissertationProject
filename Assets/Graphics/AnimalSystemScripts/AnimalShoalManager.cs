using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Original refence for flocking system was
/// https://www.youtube.com/watch?v=eMpI1eCsIyM&t=373s
/// This script determines the container size and generates the shoal
/// 
/// </summary>
//CONTAINER SCRIPT

public class AnimalShoalManager : MonoBehaviour
{
    //Land Animal Variables
    bool LandAnimal;


    [Tooltip("Overide number of Animals")]
    public int _numAnimalsOveride;//ScriptableObject
    int _numAnimals;//ScriptableObject
                    //should the container be paused?
    [Header("Pause the container")]
    public float DeactivateDistance; //pause container if Cart too far.
    public bool ShoalPaused = true;
    public bool onAlert = false;
    public int seekLocationFrequency = 1000;
    public GameObject _avoidThis;
    public GameObject _showSeekTarget; //show's fish target

    [Space(20)]
    [Expandable]
    public AnimalShoalManager_ScriptableObject shoalManager;
 
    BoxCollider _container;
    [HideInInspector] public Vector3 _containerSize;
    [HideInInspector] public Vector3 _containerCentre;
    [HideInInspector] public Vector3 _containerDepth;
    GameObject _animalPrefab;//ScriptableObject
    float _padding; //ScriptableObject
    [Tooltip("Fish scale variety X= min, Y=Max")] //ScriptableObject?
    Vector2 _AnimalSizeMinMax; //move to ScriptableObject?


    [HideInInspector] public Vector3 _seekLocation; //fish target
    [HideInInspector]
    public GameObject[] _allAnimals;
    //float _urgency;
    


    Vector3 _avoidLocation;

    [Space(20)]
    [TextArea(3, 10)]
    public string Notes;

    // Start is called before the first frame update
    void Start()
    {
        //import variables from data object
        _animalPrefab = shoalManager._animalPrefab;
        _padding = shoalManager.shoalGeneral._padding;
        _AnimalSizeMinMax = shoalManager.shoalGeneral._AnimalSizeMinMax;
        LandAnimal = shoalManager.shoalGeneral.LandAnimal;

        if (_numAnimalsOveride < 1)
        {
            _numAnimals = shoalManager.shoalGeneral._numAnimals;
        }
        else
        {
            _numAnimals = _numAnimalsOveride;
        }

     
        // Setting the container size and boundary
        _container = gameObject.GetComponent<BoxCollider>();
        _containerSize = _container.size;
        _containerCentre = _container.center + gameObject.transform.position;
        Vector3 _vPadding = new Vector3(_padding, _padding, _padding);
        _containerDepth =  _containerSize/2 - _vPadding;

        //Debug.Log("container size is " + _containerDepth);

        _allAnimals = new GameObject[_numAnimals];

        // populate the container
        for (int i = 0; i < _numAnimals; i++)
        {
            shoalManager.ChoosePrefab(out _animalPrefab);

            if(LandAnimal != true)
            {
                //Air & Water. WIll generate in volume.
                Vector3 pos = new Vector3(Random.Range(-_containerDepth.x, _containerDepth.x), Random.Range(-_containerDepth.y, _containerDepth.y), Random.Range(-_containerDepth.z, _containerDepth.z)) + _containerCentre;
                _allAnimals[i] = (GameObject)Instantiate(_animalPrefab, pos, Quaternion.identity, transform);

                _allAnimals[i].transform.localScale = new Vector3(1, 1, 1) * (Random.Range(_AnimalSizeMinMax.x, _AnimalSizeMinMax.y));
                _allAnimals[i].GetComponent<AnimalShoal>().shoalManager = shoalManager;
                _allAnimals[i].GetComponent<AnimalShoal>().Init(this);
                _allAnimals[i].SetActive(false);
            }
            else
            {
                //if land animal generate towards the top of the box and let drop.

                Vector3 pos = new Vector3(
                    Random.Range(-_containerDepth.x, _containerDepth.x), 
                    _containerDepth.y + _padding, 
                    Random.Range(-_containerDepth.z, _containerDepth.z))
                    + _containerCentre;
                _allAnimals[i] = (GameObject)Instantiate(_animalPrefab, pos, Quaternion.identity, transform);

                _allAnimals[i].transform.localScale = new Vector3(1, 1, 1) * (Random.Range(_AnimalSizeMinMax.x, _AnimalSizeMinMax.y));
                _allAnimals[i].GetComponent<AnimalShoal>().shoalManager = shoalManager;
                _allAnimals[i].GetComponent<AnimalShoal>().Init(this);
                _allAnimals[i].SetActive(false);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

        //Set the cart location variable
        _avoidLocation = _avoidThis.transform.position;

        //Should shoal start paused or playing?
        float distanceToCart = (_containerCentre - _avoidLocation).magnitude;
        //Debug.Log(distanceToCart);

        //if Greater then set to Paused true
        if (distanceToCart > DeactivateDistance)
        {
            if (ShoalPaused == false)
            {
                ShoalPaused = true;
                //Debug.Log("PAUSED");
                for (int i = 0; i < _numAnimals; i++)
                {
                    _allAnimals[i].GetComponent<AnimalShoal>().InitRT(this);
                    _allAnimals[i].SetActive(false);

                }
            }
        }
        else
        {
            //Unpause
            if (ShoalPaused == true)
            {
                ShoalPaused = false;
                //Debug.Log("ACTIVE");
                for (int i = 0; i < _numAnimals; i++)
                {
                    _allAnimals[i].SetActive(true);
                    _allAnimals[i].GetComponent<AnimalShoal>().InitRT(this);
                }
            }

            // set the seek target
            if (Random.Range(0, seekLocationFrequency) < 1)
            {
                _seekLocation = new Vector3(Random.Range(-_containerDepth.x, _containerDepth.x), Random.Range(-_containerDepth.y, _containerDepth.y), Random.Range(-_containerDepth.z, _containerDepth.z)) + _containerCentre;

                if (_showSeekTarget != null)
                {
                    _showSeekTarget.transform.position = _seekLocation;
                }   
            }


            //Set on ALert
            if ((_avoidLocation.x >= -_containerDepth.x + _containerCentre.x) && (_avoidLocation.x <= _containerDepth.x + _containerCentre.x) &&
                (_avoidLocation.y >= -_containerDepth.y + _containerCentre.y) && (_avoidLocation.y <= _containerDepth.y + _containerCentre.y) &&
                (_avoidLocation.z >= -_containerDepth.z + _containerCentre.z) && (_avoidLocation.z <= _containerDepth.z + _containerCentre.z))
            {
                onAlert = true;
                //Debug.Log(onAlert);
                for (int i = 0; i < _numAnimals; i++)
                {
                    _allAnimals[i].GetComponent<AnimalShoal>().InitRT(this);
                }
            }
            else
            {
                onAlert = false;
                //Debug.Log("False" + onAlert);
                for (int i = 0; i < _numAnimals; i++)
                {
                    _allAnimals[i].GetComponent<AnimalShoal>().InitRT(this);
                }
            }
        }
    }
}