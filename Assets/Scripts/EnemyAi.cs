using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAi : MonoBehaviour
{
    private GameObject target; // finds player target
    public GameObject patrolPosition; // finds patrol target 

    [SerializeField]
    private float moveSpeed = 5;

    Vector3 movementVector; // Enemy vector of drone
    Vector3 verticalVector; // vertical Vector for enemy

    [SerializeField]
    private float gravityRate; //gravity flaot for drone
    [SerializeField]
    private float minDistance = 50.0f; // minmiumm distance between drone and partol position
    [SerializeField]
    private float maxDistance = 100.0f; // minmiumm distance between drone and partol position
    [SerializeField]
    private float rotationDrag = 0.75f; // rotation drag for drone
    [SerializeField]
    private float rotationSpeed = 0.75f; // rotation speed for drone
    [SerializeField]
    private float minimumAvoidanceDistance = 20.0f; //avoidance distance for obsticles
    [SerializeField]
    private float avoidanceForce; // force for avoidance
    [SerializeField]
    private float toleranceRadius = 3.0f; // avoidance tolerance
    [SerializeField]
    private bool canAttack = true; // can attack booleon
    [SerializeField]
    private float brakeForce = 5f; //drone brakes

    public LayerMask playerLayer; // gets playerLayer
    public LayerMask obstacleLayer; // gets ObsticleLayer

    private bool isAttacking = false;
    //groundCheck
    private bool isGrounded; //checks if drone is grounded
    public float groundCheckLength; // get groundCheckLength
    public LayerMask groundLayer; // gets groundLayer

    private Vector3 direction; //direction of drone
    private Vector3 targetPoint; 
    private float distance = 0.0f;

    Rigidbody rb; //rb
    //Vector2 input;

    Grid grid; 

    public enum CurrentState { Idle, Following, Attacking, Seeking, Falling }; //drone current states 
    private CurrentState currentState; // setting currentState

    public float DistanceToPlayer { get { return distance; } }

    void Start()
    {
        currentState = CurrentState.Idle;
        Random.InitState(Random.Range(0, 200));
        targetPoint = Vector3.zero;

    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Sets isGrounded bool to true if raycast hits a surface on the ground layer
        Ray groundingRay = new Ray(transform.position, Vector3.down);
        isGrounded = Physics.Raycast(groundingRay, groundCheckLength, groundLayer);

        if (!isGrounded) // if its not grounded
        {
            currentState = CurrentState.Falling; //set the current state to falling
        }
        else
        {
            if (currentState == CurrentState.Falling) 
                currentState = CurrentState.Idle; //set the current state to idle
        }
    }

    private void FixedUpdate()
    {
        StateMachine(); //call state machine every fixed update
    }
        
    void StateMachine() //statemachine 
    {
        switch (currentState) //execute current state
        {
            case CurrentState.Following:
                Movement();
                return;

            case CurrentState.Idle: //idle
                currentState = CurrentState.Seeking; //when in idle start seeking
                return;

            case CurrentState.Seeking: //Seeking
                Seek(); // when in seeking call and run seek
                return;

            case CurrentState.Attacking: // Attacking
                currentState = CurrentState.Idle; //after attack return to idle or player left zone
                return;

            case CurrentState.Falling: //falling
                GravityHandle(); //call and run gravity
                return;
        }
    }

    void Movement() //enemy movement
    {
        rb.rotation = Quaternion.LookRotation(direction, Vector3.up); //rotations
        rb.angularDrag = rotationDrag;

        movementVector = transform.forward * moveSpeed * Time.fixedDeltaTime;
        movementVector.y = 0.0f;
        Vector3 targetPosition = rb.position + movementVector;

        rb.MovePosition(targetPosition); //moving object
                                                                        
        if (Vector3.Distance(transform.position, target.transform.position) <= minDistance) // if distance of drones and targets is less then or equal to minDistance
        {
            target = null; //return            
            currentState = CurrentState.Attacking; // switch to attacking
        }
        else if (Vector3.Distance(transform.position, target.transform.position) >= maxDistance) // if distance of drones and targets is greater then or equal to minDistance
        {
            target = null; //return
            currentState = CurrentState.Seeking; // seek
        }
    }

    void Seek() //seek functions
    {
        if (target) // if drone finds target
        {
            //Face the drone to the player
            direction = (target.transform.position - this.transform.position);
            direction.Normalize();
            currentState = CurrentState.Following; //set state to following
        }
        if (!target) // if its not the target 
        {
            SeekRotate(); //rotate
            Collider[] colliders = Physics.OverlapSphere(transform.position, maxDistance, playerLayer); //detects multiple objects with sphere and checks if they have the playerLayer
            if (colliders.Length >= 1) //colliders array length greater than equal to one
            {
                target = colliders[0].gameObject;
            }
            else
            {
                patrolPosition.transform.parent = gameObject.transform;
                Vector3 position = new Vector3(Random.Range(-maxDistance, maxDistance), transform.position.y, Random.Range(-maxDistance, maxDistance)); //changes patrol position in diffrent random positions
                patrolPosition.transform.position = position;
                patrolPosition.transform.parent = null;
                target = patrolPosition;
            }
        }
    }
    
    void GravityHandle() // handles gravity 
    {
        verticalVector = Vector3.zero;
        verticalVector.y -= gravityRate * Time.fixedDeltaTime;

        Vector3 targetPosition = rb.position += verticalVector;

        rb.MovePosition(targetPosition);
    }

    void SeekRotate()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, rotationSpeed, 0), Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(this.transform.position, maxDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(this.transform.position, minDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x, transform.position.y - groundCheckLength, transform.position.z));

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(0, 0, minimumAvoidanceDistance));
    }

}
