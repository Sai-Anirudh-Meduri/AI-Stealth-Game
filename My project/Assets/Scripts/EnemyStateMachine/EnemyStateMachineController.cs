using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static NoiseHandler;
using Random = UnityEngine.Random;

//Used to help track enemy states in the inspector.
public enum EnemyStateType
{
    Idle,
    Patrol,
    MoveToNoise,
    Knifed,
    Dead,
    SoloPursuit,
    SpawnPursuit
}

//Controls the enemy, including holding variables for use by enemy states, and handling functions from outside scripts that need to be intelligently routed to states.
public class EnemyStateMachineController : MonoBehaviour
{
    [Header("Pathfinding")]
    private NavMeshAgent _agent; //Navmesh agent that controls movement in scene.
    [SerializeField] private Vector3 _goal; //A goal that can be set to pass to the navmesh agent in certain states (like moving to a noise)
    [SerializeField] private List<Vector3> _patrolPoints; //A list of coordinates to move to when patroling.
    [SerializeField] private double _patrolWait = 2.0; //How long the enemy waits before moving to the next patrol point.

    //State Machine variables for controlling behaviour
    [Header("StateMachine")]
    [SerializeField] private BaseState _currentState; //Used to see what state we are in when viewed in the inspector
    private EnemyStateFactory _states; //Used by states to switch to new states.
    [SerializeField] private EnemyStateType _defaultState = EnemyStateType.Idle; //Set in the inspector to change what state an enemy starts in when the scene starts.
    [SerializeField] private EnemyStateType _myState; //Used to see current state in the inspector.

    //Vision/player detection cone variables
    [Header("Vision")]
    [SerializeField] private float _visionAngle = 90.0f; //Angle of their vision
    [SerializeField] private float _visionRange = 10.0f; //How far away they can see the player
    [SerializeField] private float _attackRange = 5.0f; //How close the enemy has to be to start shooting.
    [SerializeField] private LayerMask _wallMask = 1 << 8; //Allows raycasts to intersect walls
    private Transform _playerTrans; //Allows the enemy to track the player's actual position.
    private PlayerController _playerController; //Allows access to public functions on the player's controller (ie damage)

    //Combat variable
    [Header("Combat")]
    [SerializeField] private float _maxHealth = 100.0f; //Highest possible HP for enemy
    [SerializeField] private float _HP; //Current HP of enemy
    [SerializeField] private float _baseAccuracy = 0.25f; //From 0 to 1. Base percent chance to hit. Actual chance goes up longer spent attacking
    private float _attackAccuracy; //Chance to hit on each shot. Goes up every time we shoot. Resets on entering state.
    [SerializeField] private float _attackDelay = 1.0f; //Delay from start of shooting to shot firing
    [SerializeField] private float _damageValue = 10.0f; //How much damage a shot does to the player on hit
    private bool _shooting = false;
    [SerializeField] private float _attackTimer = 2.0f;

    [Header("Animation")]
    [SerializeField] private Animator _animator; //Controls animation state

    public enum AnimID //Used as a key for dictionary to intuitively get the animation hash
    {
        Attack,
        Speed,
        Dead
    }
    private Dictionary<AnimID, int> _animHash = new(); //Holds hashes used to identify which animation a controller should switch to.

    public enum SoundID //Used as a key for dictionary to intuitively retrieve sound clips
    {
        Walk,
        Gunshot
    }
    [System.Serializable]
    private struct SoundEntry
    {
        public SoundID id;
        public AudioClip clip;
    }
    [Header("Sound")]
    [SerializeField] private List<SoundEntry> soundEntries;
    private Dictionary<SoundID, AudioClip> _audioClips;
    private AudioSource _audioPlayer;

    //OTHER
    private Transform _trans; //Our transform. Keeping a reference is slightly more CPU efficient than using this.transform
    //[SerializeField] private List<Material> _skinMaterial; //A list of materials for states to switch to. Mostly used to debug in-progress behavior
    //private Renderer _renderer; //Our renderer. Ued to switch materials.
    public bool spawnedBySpawner = true; //Set to false in inspector to use default state


    //Getters and Setters
    public BaseState CurrentState { get => _currentState; set => _currentState = value; }
    public NavMeshAgent Agent { get => _agent; }
    public Vector3 Goal { get => _goal; set => _goal = value; }
    public Transform Trans { get => _trans; }
    public List<Vector3> patrolPoints { get => _patrolPoints; set => _patrolPoints = value; }
    public double patrolWait { get => _patrolWait; set => _patrolWait = value; }
    //public List<Material> SkinMaterial { get => _skinMaterial; set => _skinMaterial = value; }
    //public Renderer Renderer { get => _renderer; }
    public EnemyStateType MyState { get => _myState; set => _myState = value; }
    public Transform Player { get => _playerTrans; }
    public PlayerController PlayerController { get => _playerController; }
    public float AttackRange { get => _attackRange; }
    public EnemyStateType DefaultState { get => _defaultState; }
    public float BaseAccuracy { get => _baseAccuracy; }
    public float AttackDelay { get => _attackDelay; }
    public float DamageValue { get => _damageValue; }
    public EnemyStateFactory StateFactory { get => _states; }

    public Animator Anim { get => _animator; }

    public Dictionary<AnimID, int> AnimHash { get => _animHash; }
    
    public AudioSource AudioPlayer { get => _audioPlayer; }
    public Dictionary<SoundID, AudioClip> AudioClips { get => _audioClips; }

    public float Accuracy { get => _attackAccuracy; set => _attackAccuracy = value; }
    public bool Shooting { get => _shooting; }

    public float AttackTimer { get => _attackTimer; }

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _states = new EnemyStateFactory(this);
        _trans = this.transform;
        _goal = _trans.position; //Default goal to where we are to stop movement in relevant states
        _playerTrans = GameObject.FindGameObjectWithTag("Player").transform; //Find the player and track it's transform
        _playerController = Player.GetComponent<PlayerController>(); //Get the player's controller for later use
        
        _animator = GetComponent<Animator>();
        _animHash.Add(AnimID.Attack, Animator.StringToHash("Attack"));
        _animHash.Add(AnimID.Speed, Animator.StringToHash("Speed"));
        _animHash.Add(AnimID.Dead, Animator.StringToHash("Dead"));

        _audioPlayer = GetComponent<AudioSource>();
        _audioClips = new Dictionary<SoundID, AudioClip>();

        foreach (var entry in soundEntries)
            _audioClips[entry.id] = entry.clip;

        //_renderer = this.GetComponent<Renderer>();
        //_renderer.material = SkinMaterial[0]; //Start our renderer on the default material.

        _HP = _maxHealth; //Default health to be full

        if (!spawnedBySpawner) { InitializeState(_defaultState); }
    }

    // Update is called once per frame
    void Update()
    {
        _currentState.UpdateState(); //Run update logic based on what state we are in.
    }

    //Function for internal states which is used to perform vision checks
    public bool playerVision()
    {
        Vector3 playerDir = (_playerTrans.position - _trans.position).normalized; //Get a vector between player and self
        float playerDist = Vector3.Distance(_trans.position, _playerTrans.position);

        //Check that the player is in the FoV
        if (Vector3.Angle(_trans.forward, playerDir) < _visionAngle / 2f && 
            playerDist < _visionRange)
        {
            Debug.DrawRay(transform.position, playerDir * playerDist, Color.red);
            //Check if something blocks vision
            if (!Physics.Raycast(_trans.position, playerDir, playerDist, _wallMask)) {
                return true;
            }
            else {
                return false;
            }
        }
        else {
            return false;
        }
    }

    //Function to determine if the agent's destintion is reached
    public bool HasReachedDestination()
    {
        if (_agent.pathStatus == NavMeshPathStatus.PathInvalid) return false; // Cannot reach target
        // No path yet
        if (!_agent.pathPending)
        {
            // Close enough to the destination
            if (_agent.remainingDistance <= _agent.stoppingDistance)
            {
                // Either stopped moving or has no path left
                if (!_agent.hasPath || _agent.velocity.sqrMagnitude <= 0.1f)
                {
                    return true;
                }
            }
        }
        return false;
    }

    //Public function to allow other scripts to cause an enemy to hear a noise. Respects the enemy's state's ability to hear that noise.
    public void HearNoise(NoiseID id, Transform origin, double range)
    {
        if (_currentState is ICanHear listener)
        {
            listener.HearNoise(id, origin, range);
        }
        else
        {
            return;
        }
        
    }

    //Public function to backstab the enemy. Respects whether the enemy can currently be stabbed (if they are dying for example, they cannot be stabbed)
    public void getBackstabbed()
    {
        Debug.Log("DIE!");
        _currentState.ExitState();
        _currentState = StateFactory.DieState();
        _currentState.EnterState();
    }

    public void StartShooting()
    {
        _shooting = true;
    }

    public void StopShooting()
    {
        _shooting = false;
    }

    public void Shoot()
    {
        _audioPlayer.PlayOneShot(_audioClips[SoundID.Gunshot]);
        if (Random.value <= _attackAccuracy)
        {
            if(playerVision())
                PlayerController.Damage(DamageValue); //Do damage
        }

        _attackAccuracy += 0.25f; //Increment accuracy so shots are more likely to hit next time
        if (_attackAccuracy > 1.0f)
        {
            _attackAccuracy = 1.0f; //Accuracy cannot be greater than 100%
        }
    }

    public void WalkSound()
    {
        _audioPlayer.PlayOneShot(_audioClips[SoundID.Walk]);
    }
    
    //Draws debug information using Unity Gizmos
    private void OnDrawGizmosSelected()
    {
        //Draw Vision Cone
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _visionRange);

        Vector3 leftBoundary = Quaternion.Euler(0, -_visionAngle / 2f, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, _visionAngle / 2f, 0) * transform.forward;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * _visionRange);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * _visionRange);
    }

    // Initialize method for external spawners
    public void InitializeState(EnemyStateType state)
    {
        switch (state)
        {
            case EnemyStateType.Idle:
                _currentState = _states.Idle();
                break;
            case EnemyStateType.Patrol:
                _currentState = _states.Patrol();
                break;
            case EnemyStateType.MoveToNoise:
                _currentState = _states.MoveToNoise();
                break;
            case EnemyStateType.SoloPursuit:
                _currentState = _states.PursuitState();
                break;
        }

        _currentState.EnterState();
    }

    //Initialize as a spawn pursuit state
    public void InitializeSpawnerState(Vector3 pos, EnemySpawnerController spawner)
    {
        _currentState = _states.SpawnPursuitState(pos, spawner);
        _currentState.EnterState();
    }

    //Used to spawn enemies in other scipts.
    public static EnemyStateMachineController SpawnEnemy(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        GameObject go = Instantiate(prefab, pos, rot);
        EnemyStateMachineController controller = go.GetComponent<EnemyStateMachineController>();
        return controller;
    }

    //USed to destroy this object
    public void destroySelf()
    {
        Destroy(gameObject);
    }

    public void Damage(int damage, bool backstab)
    {
        _HP -= damage;

        if(backstab || _HP <= 0)
        {
            getBackstabbed();
        }
    }
}
