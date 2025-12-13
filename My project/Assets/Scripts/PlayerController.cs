using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.UI.Image;
using Cursor = UnityEngine.Cursor;

//COntrolls how the player character functions.
public class PlayerController : MonoBehaviour
{
    private PlayerInput playerInput; //Used to access the controls

    [Header("Move Settings")]
    public float moveSpeed = 5.0f; //How fast the player moves
    private float _currentMove = 1f; //Runtime modifiers that change the player's movespeed.
    public float rotateSpeed = 100.0f; //How fast the player rotates
    private Vector2 moveDirection; //Two vectors for controling the camera and player movement
    private Vector2 lookDirection;
    private bool _sneak; //Is the player currently sneaking?

    [Header("Jump Settings")]
    [SerializeField] private float jumpHeight = 2f;   // How high the player jumps
    [SerializeField] Transform groundCheckPoint; // empty GameObject at feet
    [SerializeField] float groundRadius = 0.3f;
    [SerializeField] LayerMask groundMask;
    private Vector3 _jumpVector;
    private bool _wasGrounded = true;

    [Header("Noise Settings")]
    [SerializeField] private bool _drawNoise = true;
    [SerializeField] private float _runNoiseRange = 4f;
    [SerializeField] private float _walkNoiseRange = 2f;
    [SerializeField] private float _landingNoiseRange = 5f;
    [SerializeField] private float clapRange = 10.0f; //The range at which enemies can hear the "clap" noise the player emits

    [Header("Controls")]
    public InputActionReference move; //Input actions to allow access to parts of the control scheme. MUST be set in the inspector
    public InputActionReference look;
    public InputActionReference attack;
    public InputActionReference sprint;
    public InputActionReference crouch;
    public InputActionReference jump;
    public CharacterController characterController; //Allows the player to move using Unity prebuilt collision detection. MUST be set in the inspector

    [Header("Camera Settings")]
    public bool controlCamera = true; //A debug option that turns off the camera controls for testing purposes
    public Camera cam; //Allows access to the main camera so the player can use it.
    public Transform camTarget; //An external target used to help adjust camera aim.
    public float lookSpeed = 2f; // Four floats for adjusting how the camera moves.
    public float minPitch = -30f;
    public float maxPitch = 60f;
    private float currentPitch = 0f;

    [Header("Animation")]
    [SerializeField] private Animator _animator; //Controls animation state

    private enum AnimID //Used as a key for dictionary to intuitively get the animation hash
    {
        Jump,
        Sneak,
        MoveSpeed,
        Attack,
        Die,
        Death
    }
    private Dictionary<AnimID, int> _animHash = new(); //Holds hashes used to identify which animation a controller should switch to.

    [Header("UI")]
    [SerializeField] private HP_Bar _hpUI;
    [SerializeField] private EndUI _endUI;
    [SerializeField] private String _gameOverMessage;
    [SerializeField] private String _victoryMessage;

    private enum SoundID //Used as a key for dictionary to intuitively retrieve sound clips
    {
        Walk,
        Stab
    }
    [System.Serializable]
    private struct SoundEntry
    {
        public SoundID id;
        public AudioClip clip;
    }
    [Header("Sound")]
    [SerializeField] private List<SoundEntry> soundEntries;
    private Dictionary<SoundID,AudioClip> _audioClips;
    private AudioSource _audioPlayer;

    [Header("Combat")]
    [SerializeField] private float _maxHealth = 100;
    [SerializeField] private float _HP;
    [SerializeField] private float knifeRange = 3.0f; //The range at which the enemy detects knife swings.
    [SerializeField] private Transform knife;
    [SerializeField] private LayerMask enemyLayer = 1 << 6; //Used to make raycasts that only detect guards.
    private bool _isDead = false;

    public float HP { get => _HP; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerInput = GetComponent<PlayerInput>();

        _animator = GetComponent<Animator>();
        _animHash.Add(AnimID.Sneak, Animator.StringToHash("Sneak"));
        _animHash.Add(AnimID.MoveSpeed, Animator.StringToHash("MoveSpeed"));
        _animHash.Add(AnimID.Jump, Animator.StringToHash("Jump"));
        _animHash.Add(AnimID.Attack, Animator.StringToHash("Attack"));
        _animHash.Add(AnimID.Die, Animator.StringToHash("Die"));
        _animHash.Add(AnimID.Death, Animator.StringToHash("Death"));

        _audioPlayer = GetComponent<AudioSource>();
        _audioClips = new Dictionary<SoundID, AudioClip>();

        foreach (var entry in soundEntries)
            _audioClips[entry.id] = entry.clip;

        Cursor.lockState = CursorLockMode.Locked;
        _HP = _maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        MoveRelativeToCamera();
        if (transform.position.z >= 0)
            Victory();
    }

    private void OnEnable()
    {
        sprint.action.performed += OnSprintPerformed;
        sprint.action.canceled += OnSprintCanceled;
        crouch.action.performed += OnCrouchPerformed;
        crouch.action.canceled += OnCrouchCanceled;
        jump.action.performed += OnJumpPerformed;
        ExitCaller.OnPlayerExit += Victory;
        attack.action.performed += Attack;
    }

    private void OnDisable()
    {
        sprint.action.performed -= OnSprintPerformed;
        sprint.action.canceled -= OnSprintCanceled;
        crouch.action.performed -= OnCrouchPerformed;
        crouch.action.canceled -= OnCrouchCanceled;
        jump.action.performed -= OnJumpPerformed;
        ExitCaller.OnPlayerExit -= Victory;
        attack.action.performed -= Attack;
    }

    //Controls player movement, including moving the camera and moving relative to the camera's positon
    void MoveRelativeToCamera()
    {
        if(_isDead) return;

        moveDirection = move.action.ReadValue<Vector2>();
        lookDirection = look.action.ReadValue<Vector2>();

        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward = cameraForward.normalized;
        cameraRight = cameraRight.normalized;

        Vector3 relativeForward = cameraForward * moveDirection.y;
        Vector3 relativeRight = cameraRight * moveDirection.x;

        Vector3 relativeMove = relativeForward + relativeRight;

        MakeMoveNoise(relativeMove);

        if(relativeMove.sqrMagnitude > 0f) 
            _animator.SetFloat(_animHash[AnimID.MoveSpeed], _currentMove);
        else 
            _animator.SetFloat(_animHash[AnimID.MoveSpeed], 0);

        //if (moveDirection != Vector2.zero)
        //{
        //    Quaternion rotateTo = Quaternion.LookRotation(relativeMove, Vector3.up);
        //    transform.rotation = Quaternion.RotateTowards(transform.rotation, rotateTo, rotateSpeed * Time.deltaTime);
        //}
        if (characterController.isGrounded)
        {
            if (_jumpVector.y < 0)
                _jumpVector.y = -2f; // keep grounded

        }
        _jumpVector.y += Physics.gravity.y * Time.deltaTime;

        relativeMove.y = _jumpVector.y;
       
        characterController.Move(relativeMove * Time.deltaTime * moveSpeed * _currentMove);

        bool groundedNow = IsGrounded();

        if (!_wasGrounded && groundedNow)
        {
            // Player just landed
            JumpLandingNoise();
        }

        _wasGrounded = groundedNow;

        if (controlCamera)
        {
            float yaw = lookDirection.x * lookSpeed;
            transform.Rotate(0f, yaw, 0f);

            currentPitch -= lookDirection.y * lookSpeed;
            currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);
            camTarget.localRotation = Quaternion.Euler(currentPitch, 0f, 0f);

            cam.transform.position = camTarget.position - camTarget.forward * 3f + Vector3.up * 1.5f; // Adjust distance/height
            cam.transform.LookAt(camTarget);
        }
    }

    private void MakeMoveNoise(Vector3 move)
    {
        if(!IsGrounded() || _sneak || move == Vector3.zero ||_isDead) { return; }

        if(_currentMove > 1)
            NoiseHandler.InvokeNoise(NoiseHandler.NoiseID.Run, transform, _runNoiseRange);
        else
            NoiseHandler.InvokeNoise(NoiseHandler.NoiseID.Walk, transform, _walkNoiseRange);
    }

    //Executes when the "Clap" button is pressed and makes a noise
    private void OnClap(InputValue value)
    {
        if (_isDead) return;

        Debug.Log("Clap!");
        NoiseHandler.InvokeNoise(NoiseHandler.NoiseID.Clap, this.transform, clapRange);
    }

    //Used when the Knife button is pressed and processes the player's side of a melee attack. Vurrently only backstabs.
    /*private void OnKnife(InputValue value)
    {
        if (_isDead) return;

        Debug.Log("Knife!");

        if(Physics.Raycast(this.transform.position, this.transform.forward,out RaycastHit hit, knifeRange, enemyLayer))
        {
            Debug.Log("Hit " + hit.collider.name);
            
            var enemy = hit.collider.GetComponent<EnemyStateMachineController>();
            if (enemy != null)
            {
                if(Vector3.Dot(enemy.Trans.forward, (this.transform.position - enemy.Trans.position).normalized) < 0f)
                {
                    enemy.getBackstabbed();
                }
                else
                {
                    Debug.Log("Not a Backstab!");
                }
            }
        }
        // Draw ray in Scene view
        Debug.DrawRay(this.transform.position, this.transform.forward * knifeRange, Color.red, 100f);
    }*/

    private void OnSprintPerformed(InputAction.CallbackContext ctx)
    {
        if (_isDead) return;

        //Begin running by canceling sneaking
        _sneak = false;
        _animator.SetBool(_animHash[AnimID.Sneak], false); //Update sneak in animator.

        //Increase movement speed modifier
        if(_currentMove < 2)
            _currentMove = 2;
    }

    private void OnSprintCanceled(InputAction.CallbackContext ctx)
    {
        if (_isDead) return;

        //Decrease movespeed modifier.
        if (_currentMove > 1)
            _currentMove = 1;
    }
    private void OnCrouchPerformed(InputAction.CallbackContext ctx)
    {
        if (_isDead) return;

        //Begin running by sneaking
        _sneak = true;
        _animator.SetBool(_animHash[AnimID.Sneak], true); //Update sneak in animator.

        //Decrease movement speed modifier
        if(_currentMove > .5f)
            _currentMove = .5f;
    }

    private void OnCrouchCanceled(InputAction.CallbackContext ctx)
    {
        if (_isDead) return;

        //Begin running by canceling sneak
        _sneak = false;
        _animator.SetBool(_animHash[AnimID.Sneak], false); //Update sneak in animator.

        //Increase movement speed modifier
        if (_currentMove < 1)
            _currentMove = 1;
    }

    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        if (_isDead) return;

        Debug.Log(IsGrounded());
        if (IsGrounded())
        {
            _animator.SetTrigger(_animHash[AnimID.Jump]);
        }
    }

    //Function for taking damage
    public void Damage(float damage)
    {
        if(_isDead) return;

        _HP -= damage;
        _hpUI.SetHP(_HP, _maxHealth);

        if (_HP <= 0)
            Die();
    }

    public void Die()
    {
        if(_isDead) return;

        Cursor.lockState = CursorLockMode.None;
        _isDead = true;
        _endUI.Show(false, _gameOverMessage);
        _animator.SetTrigger(_animHash[AnimID.Die]);
        _animator.SetBool(_animHash[AnimID.Death], true);
    }

    bool IsGrounded()
    {
        return Physics.CheckSphere(
            groundCheckPoint.position,
            groundRadius,
            groundMask
        );
    }

    public void Victory()
    {
        if(_isDead) return;

        _animator.SetFloat(_animHash[AnimID.MoveSpeed], 0);
        _animator.SetBool(_animHash[AnimID.Sneak], false);
        Cursor.lockState = CursorLockMode.None;
        _isDead = true;
        _endUI.Show(true, _victoryMessage);
    }

    public void AttackTrigger()
    {
        // 1. Find all enemies within knifeRange
        Collider[] hits = Physics.OverlapSphere(knife.position, knifeRange, enemyLayer);
        EnemyStateMachineController closestEnemy = null;
        float minDistance = float.MaxValue;

        foreach (var col in hits)
        {
            var enemy = col.GetComponent<EnemyStateMachineController>();
            if (enemy == null || enemy.MyState == EnemyStateType.Dead)
                continue;

            // 2. Raycast from knife tip to enemy to check if walls block
            Vector3 dir = (enemy.Trans.position - knife.position).normalized;
            float distance = Vector3.Distance(knife.position, enemy.Trans.position);

            if (Physics.Raycast(knife.position, dir, out RaycastHit hit, knifeRange))
            {
                if (hit.collider.GetComponent<EnemyStateMachineController>() != enemy)
                {
                    // Something else (like wall) is blocking
                    continue;
                }

                // 3. Choose nearest enemy along the ray
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestEnemy = enemy;
                }
            }
        }

        // 4. Apply damage to the closest valid enemy
        if (closestEnemy != null)
        {
            Vector3 toKnife = (transform.position - closestEnemy.Trans.position).normalized;
            float dot = Vector3.Dot(closestEnemy.Trans.forward, toKnife);

            bool isBackstab = dot < 0f;
            closestEnemy.Damage(10, isBackstab);

            Debug.Log($"{closestEnemy.name} hit! Backstab: {isBackstab}");
        }
    }

    public void RestartScene()
    {
        // Get the currently active scene and reload it
        Debug.Log("Resetting");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void Attack(InputAction.CallbackContext ctx)
    {
        if (_isDead)
            { return; }
        _animator.SetTrigger(_animHash[AnimID.Attack]);
    }

    private void ApplyJump()
    {
            _jumpVector.y = jumpHeight;
    }

    private void WalkNoise()
    {
        //if(!_audioPlayer.isPlaying)
            _audioPlayer.PlayOneShot(_audioClips[SoundID.Walk]);
    }

    private void JumpLandingNoise()
    {
        NoiseHandler.InvokeNoise(NoiseHandler.NoiseID.Landing, transform, _landingNoiseRange);
    }

    private void OnDrawGizmos()
    {
        if (_drawNoise)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, _walkNoiseRange);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _runNoiseRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _landingNoiseRange); 
        }
    }
}
