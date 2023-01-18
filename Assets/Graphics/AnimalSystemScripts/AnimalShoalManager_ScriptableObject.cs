using UnityEditor;
//using UnityEditor.Experimental.UIElements;
using UnityEngine;

[CreateAssetMenu(fileName = "Animal_", menuName = "Animals/Animal", order = 1)]

/// <summary>
/// >>>>>>>>>>>>>>>>This script CREATES the SCRIPTABLE OBJECT
/// </summary>

public class AnimalShoalManager_ScriptableObject : ScriptableObject
{
    [Header("----------------     Shoal Setup     ----------------", order = 1)]
    [Space(10, order = 2)]


    //    //    //    //    //    //    //    //    //    //    
    [Tooltip("Settings used during Debugging")]
    public Debug DebugSettings;

    [System.Serializable]
    public class Debug
    {
        public bool _debugOn = false; //use debugging colours
        //[Expandable]
        //public Palette_ScriptableObject palette;
        public string custom1;
        public Color c_custom1 = Color.white;
        public string custom2;
        public Color c_custom2 = Color.white;
        public string custom3;
        public Color c_custom3 = Color.white;

        [TextArea(2, 8)]
        public string Comments;
    }

    //    //    //    //    //    //    //    //    //    //    
    [Space(10)]
    [Tooltip("General Shoal Properties")]
    public ShoalGeneral shoalGeneral;

    [System.Serializable]
    public class ShoalGeneral
    {
        public Color c_global = Color.white;
        public bool LandAnimal = false;
        public float UpRightFactor = 1f;

        public GameObject[] _animalPrefabs = new GameObject[1]; //animals to be used in the shoal
        public int _numAnimals = 10; //how many animals in container
        [Tooltip("Turning DIstance")]
        public float _padding = 1.5f; // Margin of safety to container edge
        [Tooltip("Animal scale variety X= min, Y=Max")]
        public Vector2 _AnimalSizeMinMax = new Vector2(0.4f, 1.4f);
        [Tooltip("Object to Avoid or Follow (CART)")]
        //public GameObject _avoidThis; //Thing to move away from
        
        //public float _dangerRadius = 7.0f; // min distance at which to activate 'fleeing'
        public Vector2 _followRadiusMinMax = new Vector2(0.4f, 1.4f); // Range in which to activate 'follow'
    }
    //    //    //    //    //    //    //    //    //    //    

    //Hidden variables
    [HideInInspector]
    public GameObject _animalPrefab;

    [Header("----------------     Animal Behavior     ----------------", order = 1)]

    //    //    //    //    //    //    //    //    //    //
    [Space(10, order = 2)]
    [Tooltip("Creature Behavior: Idle")]
    public CreatureBehaviorDefault behaviorDefault;

    [System.Serializable]
    public class CreatureBehaviorDefault
    {
        public Color c_animalIdle = Color.blue;
        [Tooltip("1 always. Bigger number less likely.")]
        public int ProbabilityEnterIdle = 1;
        [Tooltip("Animal speed X= min, Y=Max")]
        public Vector2 _AnimalSpeedMinMax = new Vector2(0.0f, 0.5f);
        public float speedVariable = 1.0f;
        [Tooltip("Animal Rotation speed X= min, Y=Max")]
        public Vector2 _AnimalRotSpeedMinMax = new Vector2(0.3f, 2.0f);
        
    }
    
    //    //    //    //    //    //    //    //    //    // 
    [Space(10)]
    [Tooltip("Creature Behavior: Seek")]
    public CreatureBehaviorSeek behaviorSeek;

    [System.Serializable]
    public class CreatureBehaviorSeek
    {
        public Color c_animalSeek = Color.green;
        [Tooltip("1 always. Bigger number less likely.")]
        public int ProbabilityEnterSeek = 1;
        [Tooltip("Animal speed X= min, Y=Max")]
        public Vector2 _AnimalSpeedMinMax = new Vector2(1.0f, 4.0f);
        public float speedVariable = 1.0f;
        [Tooltip("Animal Rotation speed X= min, Y=Max")]
        public Vector2 _AnimalRotSpeedMinMax = new Vector2(0.1f, 1.0f);
    }
    
    //    //    //    //    //    //    //    //    //    // 
    [Space(10)]
    [Tooltip("Creature Behavior: Flee")]
    public CreatureBehaviorFlee behaviorFlee;

    [System.Serializable]
    public class CreatureBehaviorFlee
    {
        public Color c_animalFlee = Color.red;
        //[Tooltip("1 always. Bigger number less likely.")]
        //public float ProbabilityEnterFlee = 1;
        //[HideInInspector] public bool onAlert = false; //is the avoidthis object in the zone?
        [Tooltip("Animal speed X= min, Y=Max")]
        public Vector2 _AnimalSpeedMinMax = new Vector2(1.5f, 6.0f);
        public float speedVariable = 1.0f;
        [Tooltip("Animal Rotation speed X= min, Y=Max")]
        public Vector2 _AnimalRotSpeedMinMax = new Vector2(0.1f, 0.7f);
        [Space(10)]
        public float _dangerRadius = 7.0f; // min distance at which to activate 'fleeing'
        public AnimationCurve ReactionTime = AnimationCurve.Linear(0, 0, 1, 1); ///what is this?
        public float _reactionMagnitude = 1.0f;///what is this?
        public float _reactionLength = 5.0f; ///what is this?
    }
    
    //    //    //    //    //    //    //    //    //    // 
    [Space(10)]
    [Tooltip("Creature Behavior: Flocking")]
    public CreatureBehaviorFlocking behaviorFlocking;

    [System.Serializable]
    public class CreatureBehaviorFlocking
    {
        public Color c_animalFlocking = Color.yellow;
        [Tooltip("1 always. Bigger number less likely.")]
        public int ProbabilityEnterFlocking = 1;
        public int ProbabilityExitFlocking = 1;
        [Tooltip("Animal speed X= min, Y=Max")]
        public Vector2 _AnimalSpeedMinMax = new Vector2(0.8f, 3.0f);
        public float speedVariable = 1.0f;
        [Tooltip("Animal Rotation speed X= min, Y=Max")]
        public Vector2 _AnimalRotSpeedMinMax = new Vector2(0.1f, 1.0f);
        public float _neighbourDistance = 12.0f; //flocking setting
        public float _avoidDistance = 0.3f; //flocking setting
        public float _weight = 1.3f; //flocking setting
    }
    
    //    //    //    //    //    //    //    //    //    // 
    [Space(10)]
    [Tooltip("Creature Behavior: Turn")]
    public CreatureBehaviorTurn behaviorTurn;

    [System.Serializable]
    public class CreatureBehaviorTurn
    {
        public Color c_animalTurning = Color.magenta;
        [Tooltip("Animal speed X= min, Y=Max")]
        public Vector2 _AnimalSpeedMinMax = new Vector2(1.5f, 6.0f);
        [Tooltip("Animal Rotation speed X= min, Y=Max")]
        public Vector2 _AnimalRotSpeedMinMax = new Vector2(0.1f, 0.7f);
        public float speedVariable = 1.0f;
    }
    
    //    //    //    //    //    //    //    //    //    // 

    [TextArea(2, 8)]
    public string Comments;

    //    //    //    //    //    //    //    //    //    // 

    public void ChoosePrefab(out GameObject _animalPrefab)
    {
        int index = Random.Range(0, shoalGeneral._animalPrefabs.Length);
        _animalPrefab = shoalGeneral._animalPrefabs[index];        
    }

    public void HowUrgent(in float _avoidDist, out float _urgency)
    {
        float urgencyTemp = (behaviorFlee._reactionMagnitude * Mathf.Pow(2, -(Mathf.Pow(_avoidDist,2)/behaviorFlee._reactionLength))) + 1;
        _urgency = urgencyTemp;
    }



}