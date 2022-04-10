using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class Movement : MonoBehaviour
{
    private float speed;
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 1.5f;
    [SerializeField] private bool usePhysics = true;
    // [SerializeField] private CinemachineImpulseSource _impulseSource;
    // private float jumpSpeed = 5000f;
    private bool canJump;

    private Camera _mainCamera;
    private Rigidbody _rb;
    private Controls _controls;
    private Animator _animator;
    private static readonly int IsWalking = Animator.StringToHash("isWalking");
    private static readonly int IsRunning = Animator.StringToHash("isRunning");
    private static readonly int isJumping = Animator.StringToHash("isJumping");
    private static readonly int isDancing = Animator.StringToHash("isDancing");

    [SerializeField] GameObject stepRayUpper;
    [SerializeField] GameObject stepRayLower;
    [SerializeField] float stepHeight = 0.3f;
    [SerializeField] float stepSmooth = 2f;

    public static bool GameIsPaused = false;   

    private void Awake()
    {
        _controls = new Controls();
        _rb = GetComponent<Rigidbody>();

        //_impulseSource = GetComponent<CinemachineImpulseSource>();
        stepRayUpper.transform.position = new Vector3(stepRayUpper.transform.position.x, stepHeight, stepRayUpper.transform.position.z);
    }

    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        _controls.Enable();
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        _controls.Disable();
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Pause()
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("MenuScene");
    }

    private void Start()
    {
        speed = walkSpeed;
        _mainCamera = Camera.main;
        _rb = gameObject.GetComponent<Rigidbody>();
        _animator = gameObject.GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (!_controls.Player.Move.IsPressed()) return;
        Vector2 input = _controls.Player.Move.ReadValue<Vector2>();
        Vector3 target = HandleInput(input);
        RotateCharacter(target);
    }
    
    private void FixedUpdate()
    {
        if (!usePhysics)
        {
            return;
        }

        if (_controls.Player.Pause.IsPressed())
        {
            if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        if (_controls.Player.Dance.IsPressed())
        {
            _animator.SetBool(isDancing, true);
        }
        else
        {
            _animator.SetBool(isDancing, false);
        }

        if (_controls.Player.Jump.IsPressed() & canJump)
        {
            // _rb.AddForce(1f, jumpSpeed * Time.deltaTime, 2f);
            if (!_animator.GetBool(isJumping))
            {
                _animator.SetBool(isJumping, false);
            }

            Jump();
        }

        // if (_controls.Player.Dance.IsPressed())
        // {
        //     _animator.SetBool(isDancing, true);
        // }
        // else
        // {
        //     _animator.SetBool(IsDancing, false);
        // }

        if (_controls.Player.Run.IsPressed())
        {
            _animator.SetBool(IsRunning, true);
            CamShake.Instance.ShakeCam(5f,1f);
            speed = walkSpeed * runSpeed;
        }
        else
        {
            _animator.SetBool(IsRunning, false);
            speed = walkSpeed;
        }

        if (_controls.Player.Move.IsPressed())
        {
            _animator.SetBool(IsWalking, true);
            Vector2 input = _controls.Player.Move.ReadValue<Vector2>();
            Vector3 target = HandleInput(input);
            Move(target);
        }
        else
        {
            _animator.SetBool(IsWalking, false);
        }

        stepClimb();
    }

    private Vector3 HandleInput(Vector2 input)
    {
        Vector3 forward = _mainCamera.transform.forward;
        Vector3 right = _mainCamera.transform.right;

        forward.y = 0;
        right.y = 0;
        
        forward.Normalize();
        right.Normalize();

        Vector3 direction = right * input.x + forward * input.y;
        
        return transform.position + direction * speed * Time.deltaTime;
    }

    private void Move(Vector3 target)
    {
        _rb.MovePosition(target); 
        // transform.position = target;
    }

    private void RotateCharacter(Vector3 target)
    {
        Vector3 lookAt = target - transform.position;
        lookAt.y = 0;
        transform.rotation = Quaternion.LookRotation(lookAt);
    }

    public void Jump()
    {
        _rb.AddForce(Vector3.up * 200);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            canJump = true;
            _animator.SetBool(isJumping, false);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            canJump = false;
            _animator.SetBool(isJumping, true);
        }
    }

    // private void MovePhysics(Vector3 target)
    // {
    //     _animator.SetBool(isJumping, false);
    // }

    void stepClimb()
    {
        RaycastHit hitLower;
        if (Physics.Raycast(stepRayLower.transform.position, transform.TransformDirection(Vector3.forward), out hitLower, 0.1f))
        {
            RaycastHit hitUpper;
            if (!Physics.Raycast(stepRayUpper.transform.position, transform.TransformDirection(Vector3.forward), out hitUpper, 0.2f))
            {
                _rb.position -= new Vector3(0f, -stepSmooth * Time.deltaTime, 0f);
            }
        }

         RaycastHit hitLower45;
        if (Physics.Raycast(stepRayLower.transform.position, transform.TransformDirection(1.5f,0,1), out hitLower45, 0.1f))
        {

            RaycastHit hitUpper45;
            if (!Physics.Raycast(stepRayUpper.transform.position, transform.TransformDirection(1.5f,0,1), out hitUpper45, 0.2f))
            {
                _rb.position -= new Vector3(0f, -stepSmooth * Time.deltaTime, 0f);
            }
        }

        RaycastHit hitLowerMinus45;
        if (Physics.Raycast(stepRayLower.transform.position, transform.TransformDirection(-1.5f,0,1), out hitLowerMinus45, 0.1f))
        {

            RaycastHit hitUpperMinus45;
            if (!Physics.Raycast(stepRayUpper.transform.position, transform.TransformDirection(-1.5f,0,1), out hitUpperMinus45, 0.2f))
            {
                _rb.position -= new Vector3(0f, -stepSmooth * Time.deltaTime, 0f);
            }
        }
    }
}
