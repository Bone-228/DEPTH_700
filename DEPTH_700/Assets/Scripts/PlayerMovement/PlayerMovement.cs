using UnityEngine;
using Unity.Netcode;
public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement")]
    public float moveSpeed;

    [Header("Ground Drag")]
    public float groundDrag;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool _grounded;

    public Transform orientation;

    float _horizontalInput;
    float _verticalInput;

    Vector3 _moveDirection;

    Rigidbody _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        //check if player is grounded
        _grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround); 

        MyInput();

        if (_grounded)
        {
            _rb.linearDamping = groundDrag;
        }
        else 
        { 
            _rb.linearDamping = 0;
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner)
            return;

        MovePlayer();
    }

    private void MyInput() 
    { 
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        _verticalInput = Input.GetAxisRaw("Vertical");
    }

    private void MovePlayer() 
    { 
        _moveDirection = orientation.forward * _verticalInput + orientation.right * _horizontalInput;
        _rb.AddForce(_moveDirection.normalized * moveSpeed * 10f , ForceMode.Force);
    }
}
