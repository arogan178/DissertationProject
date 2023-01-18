using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script controls the fLock movement & Behavior
/// </summary>

public class AnimalShoal : MonoBehaviour
{
    public AnimalShoalManager_ScriptableObject shoalManager; // The Scriptable Object. GET it from the container
    
    //Debugging////////////
    //public Palette_ScriptableObject palette;
    [HideInInspector]
    public Renderer _renderer; //used for debugging colours
    bool _debugOn; //used for debugging colours

    //Variables///////////
    float speed;
    [HideInInspector] public float speedActual;
    float speedVariable = 1.0f;
    float previousSpeedvariable;
    Vector2 _AnimalRotSpeedMinMax;      //copied from Scriptable Object
    //float turnUrgency = 5f;             //EXPOSE in scriptableObject ? <<<<<<<<<<<<<<<
    float _neighbourDistance;           //(Flocking)copied from Scriptable Object
    float _avoidDistance;               //(Flocking)copied from Scriptable Object
    float _weight;                      //(Flocking)copied from Scriptable Object
    
    Vector3 _avoidLocation;             //(Avoidance)copied from Scriptable Object
    Vector3 _seekLocation;
    [HideInInspector] public Vector3 _containerCentre;           //copied from Container Object
    [HideInInspector] public Vector3 _containerDepth;            //copied from Container Object
    GameObject[] _allAnimals;           //copied from Container Object
    AnimalShoalManager _container;      //copied from Container Object
    GameObject _avoidThis;              //copied from Container Object
    bool ShoalPaused;                   //copied from Container Object
    bool OnAlert;                   //copied from Container Object
    float _avoidDist;
    Vector3 LocOff;                     //location offset used in calculations
    AnimationCurve _ReactionTime;       //copied from Scriptable Object
    //Vector3 direction;
    //float testA;
    //public float inputA = 0f;
    //float _urgency = 1f;                //calculated in scriptable object on demand.
    float _reactionMagnitude;           //copied from Scriptable Object
    float _reactionLength;           //copied from Scriptable Object
    //int _id;


    int ProbabilityEnterIdle;
    int ProbabilityEnterFlocking;
    int ProbabilityExitFlocking;
    int ProbabilityEnterSeek;

    //Unique Behavior variables
    [HideInInspector] public bool HasUniqueBehavior = false;
    //public MonoBehaviour UniqueBehaviorScript;
    

    //Land Animal Variables
    bool LandAnimal = false;
    float UpRightFactor;
    protected Animator animator;
    //float varEat;
    //float varEatOld;

    // Declare the float to use with the animation curve
    private float curveDeltaTime = 0.0f;

    float _dangerRadius;
    //Vector3 goalPos;

    //states
    enum State {pause, idle, seek, flock, flee, turn};
    State currentState;
    State previousState;
    //int num = 0;
    //int numB = 0;

    Color c_animalIdle;
    Color c_animalFlee;
    Color c_animalFlocking;
    Color c_animalSeek;
    Color c_animalTurning;
    [HideInInspector]  public Color c_custom1;
    [HideInInspector]  public Color c_custom2;
    [HideInInspector]  public Color c_custom3;

    //Inits called from elsewhere Once
    public void Init(AnimalShoalManager containerScript)
    {
        _container = containerScript;
        _containerCentre = containerScript._containerCentre;
        _containerDepth = containerScript._containerDepth;
        _allAnimals = containerScript._allAnimals;
        _avoidThis = containerScript._avoidThis;
        ShoalPaused = containerScript.ShoalPaused;
        OnAlert = containerScript.onAlert;
    }
    
    //Inits called from elsewhere but for realtime updates
    public void InitRT(AnimalShoalManager containerScript)
    {
        ShoalPaused = containerScript.ShoalPaused;
        OnAlert = containerScript.onAlert;
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        //_id = GetInstanceID();
        animator = GetComponent<Animator>();

        //Change colours based on debugging state
        Debugger();


        //Start Speed
        speed = Random.Range(shoalManager.behaviorDefault._AnimalSpeedMinMax.x, shoalManager.behaviorDefault._AnimalSpeedMinMax.y);
        _renderer = GetComponentInChildren<Renderer>();
        //_renderer.material.SetColor("_Color", palette.c_animalStart);

        //shoalManager is your scriptable object.
        _AnimalRotSpeedMinMax = shoalManager.behaviorDefault._AnimalRotSpeedMinMax;

        _reactionMagnitude = shoalManager.behaviorFlee._reactionMagnitude;
        _reactionLength = shoalManager.behaviorFlee._reactionLength;
        _ReactionTime = shoalManager.behaviorFlee.ReactionTime; //curve
        _dangerRadius = shoalManager.behaviorFlee._dangerRadius;
        _neighbourDistance = shoalManager.behaviorFlocking._neighbourDistance;
        _avoidDistance = shoalManager.behaviorFlocking._avoidDistance;
        _weight = shoalManager.behaviorFlocking._weight;
        ProbabilityEnterIdle = shoalManager.behaviorDefault.ProbabilityEnterIdle;
        ProbabilityEnterFlocking = shoalManager.behaviorFlocking.ProbabilityEnterFlocking;
        ProbabilityExitFlocking = shoalManager.behaviorFlocking.ProbabilityExitFlocking;
        ProbabilityEnterSeek = shoalManager.behaviorSeek.ProbabilityEnterSeek;


        currentState = State.pause; //assign currentState to pause.
        ChangeState(State.idle);

        //Land Animal Settings
        UpRightFactor = shoalManager.shoalGeneral.UpRightFactor;
        LandAnimal = shoalManager.shoalGeneral.LandAnimal;
                
    }

    void realTimevariables()
    {
        //shoalManager is your scriptable object.
        //these are updated EVERY frame
        _avoidLocation = _avoidThis.transform.position;
        _avoidDist = Vector3.Distance(_avoidLocation, this.transform.position);
    }

    //Change colours based on debugging state
    void Debugger()
    {
        _debugOn = shoalManager.DebugSettings._debugOn;
        if (_debugOn == true)
        {
            //palette.debuggerOn();
            c_animalIdle = shoalManager.behaviorDefault.c_animalIdle;
            c_animalFlee = shoalManager.behaviorFlee.c_animalFlee;
            c_animalFlocking = shoalManager.behaviorFlocking.c_animalFlocking;
            c_animalSeek = shoalManager.behaviorSeek.c_animalSeek;
            c_animalTurning = shoalManager.behaviorTurn.c_animalTurning;
            c_custom1 = shoalManager.DebugSettings.c_custom1;
            c_custom2 = shoalManager.DebugSettings.c_custom2;
            c_custom3 = shoalManager.DebugSettings.c_custom3;
        }
        else
        {
            c_animalIdle = shoalManager.shoalGeneral.c_global;
            c_animalFlee = shoalManager.shoalGeneral.c_global;
            c_animalFlocking = shoalManager.shoalGeneral.c_global;
            c_animalSeek = shoalManager.shoalGeneral.c_global;
            c_animalTurning = shoalManager.shoalGeneral.c_global;
            c_custom1 = shoalManager.shoalGeneral.c_global;
            c_custom2 = shoalManager.shoalGeneral.c_global;
            c_custom3 = shoalManager.shoalGeneral.c_global;
        }        
    }

    void Update()
    {
                
        if (ShoalPaused == false)//not paused, go ahead.
        {
            realTimevariables();

            if(LandAnimal != false)
            {
                KeepUpRight();
                
            }
            

            switch (currentState)
            {
                case State.idle:
                    StateWander();
                    break;
                case State.seek:
                    StateSeek();
                    break;
                case State.flock:
                    StateFlock();
                    break;
                case State.flee:
                    StateAvoid();
                    break;
                case State.turn:
                    StateTurn();
                    break;
                case State.pause:
                    speed = 0f;
                    break;
            }
        }

    }

 
    //CASE 1: Wander/Idle
    void StateWander()
    {
        //Behavior//
        //Debug.Log("WANDER");

        //transform.Translate(0, 0, Time.deltaTime * speed *0.1f);
        float speedVarTemp = shoalManager.behaviorDefault.speedVariable;
        speedVariable = Mathf.Lerp(previousSpeedvariable, speedVarTemp, Time.deltaTime);
        Move(speedVariable);

        //if (Random.Range(0, 20) < 1)
        if ((LandAnimal == true) && (HasUniqueBehavior == true))
        {
            //Debug.Log(UniqueBehaviorScript);
            UniqueBehaviorRun();
        }
               

        if (Random.Range(0, 100) < 1)
        {
            RandomLocationOffset(10f, out LocOff);
        }

        Vector3 direction = LocOff - transform.position;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), (Random.Range(_AnimalRotSpeedMinMax.x, _AnimalRotSpeedMinMax.y) * Time.deltaTime));

        //Exit conditions
        ExitIfClose();
        ExitIfEnemyClose();
        ExitIfHungry();
        ExitTimeToFlock();

    }

    //CASE 2: Seek/Forage
    void StateSeek()
    {
        //Behavior//
        //Debug.Log("SEEK");

        _seekLocation = _container._seekLocation;

        if ((LandAnimal == false) && (HasUniqueBehavior == true))
        {
            //Debug.Log(UniqueBehaviorScript);
            UniqueBehaviorRun();
        }

        //transform.Translate(0, 0, Time.deltaTime * speed);
        float speedVarTemp = shoalManager.behaviorSeek.speedVariable;
        speedVariable = Mathf.Lerp(previousSpeedvariable, speedVarTemp, Time.deltaTime);
        Move(speedVariable);

        Vector3 direction = (_seekLocation - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), (Random.Range(_AnimalRotSpeedMinMax.x, _AnimalRotSpeedMinMax.y) * Time.deltaTime));

        //EXIT CONDITIONS//

        //Exit conditions
        ExitIfClose();
        ExitIfEnemyClose();
        ExitTimeToFlock();
        ExitIfTired();

    }


    //CASE 4: Flee/Avoid
    void StateAvoid()
    {
        //Behavior//
        //Debug.Log("Flee");

        float _avoidDist = Vector3.Distance(_avoidLocation, this.transform.position);

        //// Call evaluate on that time
        curveDeltaTime += Time.deltaTime * (1f/(_reactionLength + 0.01f));
        //currentPosition.y = _ReactionTime.Evaluate(curveDeltaTime);
        float TempBB = _ReactionTime.Evaluate(curveDeltaTime) * _reactionMagnitude;
        //Debug.Log(">>>>>>  curveDeltaTime = " + numB + ": " + curveDeltaTime + " TempBB = " + TempBB);
        //numB++;
        //// Update the current position of the sphere
        //transform.position = currentPosition;

        //shoalManager.HowUrgent(_avoidDist, out _urgency);
        //>>>>_urgency = _ReactionTime.Evaluate(speed);
        //Debug.Log("urgency actual" + _urgency);
        //>>>>float tempA = (_urgency - 1) / _reactionMagnitude;
        //Debug.Log("lerp % = " + tempA);




        //transform.Translate(0, 0, Time.deltaTime * speed * _urgency);
        //float speedVarTemp = shoalManager.behaviorFlee.speedVariable;
        float speedVarTemp = TempBB;
        //speedVariable = Mathf.Lerp(previousSpeedvariable, speedVarTemp, Time.deltaTime);
        speedVariable = TempBB;
        Move(speedVariable);


        Color colA = GetDebuggerCol(State.pause);
        Color colB = GetDebuggerCol(State.flee);
        Color lerpedColor = Color.Lerp(colB, colA, curveDeltaTime);
        //Set State Colour
        _renderer.material.SetColor("_Color", lerpedColor);
        //Debug.Log(lerpedColor);


        //Debug.Log("urgency is "+ urgency);
        Vector3 direction = (transform.position - _avoidLocation);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), (Random.Range(_AnimalRotSpeedMinMax.x, _AnimalRotSpeedMinMax.y) * Time.deltaTime) * speedVariable);


        //EXIT CONDITIONS//
        ExitIfClose();
        ExitIfNoDanger();
    }

    //CASE 5: Turn/Too close to border
    void StateTurn()
    {
        Vector3 direction = _containerCentre - transform.position;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Random.Range(_AnimalRotSpeedMinMax.x, _AnimalRotSpeedMinMax.y) * Time.deltaTime);

        //transform.Translate(0, 0, Time.deltaTime * speed);
        float speedVarTemp = shoalManager.behaviorTurn.speedVariable;
        speedVariable = Mathf.Lerp(previousSpeedvariable, speedVarTemp, Time.deltaTime);
        Move(speedVariable);

        ExitIfFarEnough();

    }


    //CASE 3: Flock/Group
    void StateFlock()
    {

        if (Random.Range(0, 4) < 1)
        {
            ApplyRules();
        }

        if ((LandAnimal == false) && (HasUniqueBehavior == true))
        {
            //Debug.Log(UniqueBehaviorScript);
            UniqueBehaviorRun();
        }

        //transform.Translate(0, 0, Time.deltaTime * speed);
        //float speedVarTemp = shoalManager.behaviorFlocking.speedVariable;
        //speedVariable = Mathf.Lerp(previousSpeedvariable, speedVarTemp, Time.deltaTime);
        speedVariable = shoalManager.behaviorFlocking.speedVariable;
        Move(speedVariable);

        if (Random.Range(0, ProbabilityExitFlocking) < 1)
        {
            //EXIT CONDITIONS//
            ExitIfClose();
            ExitIfNoDanger();
        }


    }

    //Boids
    void ApplyRules()
    {
        GameObject[] gos;
        gos = _allAnimals;

        Vector3 vcentre = Vector3.zero;
        Vector3 vavoid = Vector3.zero;
        float gSpeed = 0f;
        float dist;
        int groupSize = 0;
        foreach (GameObject go in gos)
        {
            if (go != this.gameObject)
            {                
                dist = Vector3.Distance(go.transform.position, this.transform.position);
                
                if (dist <= _neighbourDistance)
                {
                    vcentre += go.transform.position;
                    groupSize++;
                    

                    if (dist < _avoidDistance)
                    {
                        vavoid = vavoid + Vector3.Lerp(this.transform.position, go.transform.position, Time.deltaTime);
                        Vector3 direction = (vcentre - transform.position) + (_weight * vavoid);
                        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Random.Range(_AnimalRotSpeedMinMax.x, _AnimalRotSpeedMinMax.y) * Time.deltaTime);
                    }

                    //Flock anotherFlock = go.GetComponent<Flock>();
                    AnimalShoal anotherFlock = go.GetComponent<AnimalShoal>();
                    gSpeed = gSpeed + anotherFlock.speed;
                    //Debug.Log("gSpeed is " + gSpeed);
                }
            }
        }

        if (groupSize > 0)
        {
            //Debug.Log(groupSize);
            //_renderer.material.SetColor("_Color", Color.black);

            vcentre = vcentre / groupSize + (_seekLocation - this.transform.position);
            speed = gSpeed / groupSize;

            Vector3 direction = (_seekLocation - transform.position) + (_weight * vavoid);
            if (direction != Vector3.zero)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Random.Range(_AnimalRotSpeedMinMax.x, _AnimalRotSpeedMinMax.y) * Time.deltaTime);
        }
        else
        {
            //EXIT CONDITIONS//
            ExitIfClose();
            ExitIfNoDanger();
        }

    }

    //////////////////////////////////////
    //////////////////////////////////////
    ////EXIT CONDITIONS//////////////////

    //Bounds too close
    void ExitIfClose()
    {
        CheckTooClose();
    }

    void ExitIfFarEnough()
    {
        CheckFarEnough();
    }

    //Enemy too close
    void ExitIfEnemyClose()
    {
        
        if(OnAlert == true)
        {
            if (_avoidDist < _dangerRadius)
            {
                speed = Random.Range(shoalManager.behaviorFlee._AnimalSpeedMinMax.x, shoalManager.behaviorFlee._AnimalSpeedMinMax.y);
                _AnimalRotSpeedMinMax = shoalManager.behaviorFlee._AnimalRotSpeedMinMax;

                ChangeState(State.flee);
            }
        }

    }

    void ExitTimeToFlock()
    {
        if (Random.Range(0, ProbabilityEnterFlocking) < 1)
        {
            speed = Random.Range(shoalManager.behaviorFlocking._AnimalSpeedMinMax.x, shoalManager.behaviorFlocking._AnimalSpeedMinMax.y);
            _AnimalRotSpeedMinMax = shoalManager.behaviorFlocking._AnimalRotSpeedMinMax;

            ChangeState(State.flock);
        }
    }

    //No more danger. As before.
    void ExitIfNoDanger()
    {
        if (_avoidDist > _dangerRadius)
        {
            if (Random.Range(0, 5) < 1)
            {
                speed = Random.Range(shoalManager.behaviorSeek._AnimalSpeedMinMax.x, shoalManager.behaviorSeek._AnimalSpeedMinMax.y);
                _AnimalRotSpeedMinMax = shoalManager.behaviorSeek._AnimalRotSpeedMinMax;

                ChangeState(previousState);
            }
        }
    }

    //Bored
    void ExitIfHungry()
    {
        if (Random.Range(0, ProbabilityEnterSeek) < 1)
        {
            speed = Random.Range(shoalManager.behaviorSeek._AnimalSpeedMinMax.x, shoalManager.behaviorSeek._AnimalSpeedMinMax.y);
            _AnimalRotSpeedMinMax = shoalManager.behaviorSeek._AnimalRotSpeedMinMax;

            ChangeState(State.seek);
        }
    }

    //Tired
    void ExitIfTired()
    {
        if (Random.Range(0, ProbabilityEnterIdle) < 1)
        {
            speed = Random.Range(shoalManager.behaviorDefault._AnimalSpeedMinMax.x, shoalManager.behaviorDefault._AnimalSpeedMinMax.y);
            _AnimalRotSpeedMinMax = shoalManager.behaviorDefault._AnimalRotSpeedMinMax;

            ChangeState(State.idle);
        }
    }


    //////////////////////////////////////
    //////////////////////////////////////
    //Misc Functions//////////////////////

    public void RandomLocationOffset(in float rangeB, out Vector3 LocOff)
    {
        Vector3 pos = transform.position;
        float rangeA = rangeB;
        //_renderer.material.SetColor("_Color", palette.c_animalDead);
        Vector3 posShift = new Vector3(Random.Range(pos.x - rangeA, pos.x + rangeA), Random.Range(pos.y - rangeA, pos.y + rangeA), Random.Range(pos.z - rangeA, pos.z + rangeA));
        LocOff = posShift;
    }

    //void CheckNeedForAvoidance()
    //{
    //    float _avoidDist = Vector3.Distance(_avoidLocation, this.transform.position);
    //}

    void CheckTooClose()
    {
        if (
                (Mathf.Abs(transform.position.x - _containerCentre.x) >= Mathf.Abs(_containerDepth.x)) ||
                (Mathf.Abs(transform.position.y - _containerCentre.y) >= Mathf.Abs(_containerDepth.y)) ||
                (Mathf.Abs(transform.position.z - _containerCentre.z) >= Mathf.Abs(_containerDepth.z))
                )
        {
            speed = Random.Range(shoalManager.behaviorTurn._AnimalSpeedMinMax.x, shoalManager.behaviorTurn._AnimalSpeedMinMax.y);
            _AnimalRotSpeedMinMax = shoalManager.behaviorTurn._AnimalRotSpeedMinMax;
            ChangeState(State.turn);
        }
    }
    void CheckFarEnough()
    {
        //Percent of depth to remain in turning state. CLoser to 1 emerges from turning immedietly. Closer to 0 remains in turning till it gets to the centre of the container.
        float tempD = 0.75f; //EXPOSE in scriptableObject?/////////////////

        if (
                (Mathf.Abs(transform.position.x - _containerCentre.x) > Mathf.Abs((_containerDepth.x) * tempD)) ||
                (Mathf.Abs(transform.position.y - _containerCentre.y) > Mathf.Abs((_containerDepth.y) * tempD)) ||
                (Mathf.Abs(transform.position.z - _containerCentre.z) > Mathf.Abs((_containerDepth.z) * tempD))
                )
        {
            //Debug.Log("-------------------------Still turning  ----------------------------");
        }
        else
        {
            speed = Random.Range(shoalManager.behaviorSeek._AnimalSpeedMinMax.x, shoalManager.behaviorSeek._AnimalSpeedMinMax.y);
            _AnimalRotSpeedMinMax = shoalManager.behaviorSeek._AnimalRotSpeedMinMax;


            // should have flock too.
            //State state = previousState;
            State state = Random.value < .5 ? State.flock : State.seek;
            ChangeState(state);
        }
    }

    private Color GetDebuggerCol (State currentState)
    {
        switch(currentState)
        {
            case State.pause: return c_custom3;
            case State.idle: return c_animalIdle;
            case State.flee: return c_animalFlee;
            case State.flock: return c_animalFlocking;
            case State.seek: return c_animalSeek;
            case State.turn: return c_animalTurning;
            default: return Color.white;
        }
    }

    private State ChangeState (State newState)
    {
        curveDeltaTime = 0f;
        previousSpeedvariable = speedVariable;
        previousState = currentState;
        currentState = newState;
        //Debug.Log(_id + " No:" + num + " State changed from " + previousState + " from " + currentState);
        //Debug.Log(" No:" + num + " State changed from " + previousState + " from " + currentState);
        //num++;
        //numB = 0;
        Color debugCol = GetDebuggerCol(currentState);
        _renderer.material.SetColor("_Color", debugCol);
        return currentState;
    }

    void Move(in float speedVariable)
    {
        speedActual = speed * speedVariable;
        transform.Translate(0, 0, Time.deltaTime * speedActual);        
    }


    protected void KeepUpRight()
    {
        Quaternion upRot = Quaternion.FromToRotation(transform.up, Vector3.up);
        upRot *= transform.rotation;
        //Debug.Log(_id + "  " + upRot + "  " + UpRightFactor);
        transform.rotation = Quaternion.Slerp(transform.rotation, upRot, Time.deltaTime  * UpRightFactor);       
    }


    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("Collsiion Entered");
        //_renderer.material.SetColor("_Color", Color.black);

        if (collision.relativeVelocity.magnitude > 0.1)
        {
            //Debug.Log("magnitude > 2");
            _renderer.material.SetColor("_Color", c_custom1);

            
            animator.ResetTrigger("Jump");

        }

    }


    private void OnCollisionExit(Collision collision)
    {

        if (collision.relativeVelocity.magnitude > 3)
        {
            _renderer.material.SetColor("_Color", c_custom2);

            animator.SetTrigger("Jump");
            //Debug.Log("Jumped");

        }


    }


    protected virtual void UniqueBehaviorRun()
    {

    }

}
