using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneAi : MonoBehaviour
{
    private GameObject target;
    public GameObject patrolPosition;

    public float speed = 5;

    public float gravity;

    public float minDistance = 15.0f;

    public float maxDistance = 20.0f;

    public float rotationDrag = 100.0f;

    public float roationSpeed = 0.75f;

    public float minAvoidanceDistance = 0.5f;

    public float avoidanceForce;

    private float brakeForce = 5f;

    private bool canAttack = true;

    public LayerMask playerLayer;
    public LayerMask obsticleLayer;

    private bool isAttacking = false;
    private bool isGrounded;

    public float groundCheckLength;
    public LayerMask groundLayer;
    private Vector3 direction;
    private float distance = 0.0f;

    Rigidbody rb;
    Vector2 input;

    public enum CurrentState { Idle, Following, Attacking, Seeking, Falling };
    private CurrentState currentState;

    public bool debugGizmo = true;

    public float DistanceToPlayer { get { return distance; } }

    private void Start()
    {
        currentState = CurrentState.Idle;
        isAttacking = false;
        Random.InitState(Random.Range(0, 200));
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        Ray groundRay = new Ray(transform.position, Vector3.down); // sets the isGrounded bool to true if raycast hits ground layer
        isGrounded = Physics.Raycast(groundRay, groundCheckLength, groundLayer);

        if (!isGrounded)
        {
            currentState = CurrentState.Falling;
        }
        else
        {
            if (currentState == CurrentState.Falling) // sets enemy state to idle if the enemy state is falling
            {
                currentState = CurrentState.Idle;     
            }
                
        }
    }

    private void FixedUpdate()
    {
        StateMachine();     // ensures that the enemy is calling and Using StateMachine function

        if (canAttack)
        {
            currentState = CurrentState.Attacking;
        }
    }


    void StateMachine() //checks enemy's STATES
    {
        if (currentState == CurrentState.Following)
        {
            MovementHandler();
        }
        else if (currentState == CurrentState.Idle)
        {
            currentState = CurrentState.Seeking;
        }
        else if (currentState == CurrentState.Seeking)
        {
            SeekHandler();
        }
        else if (currentState == CurrentState.Attacking) //returns attackings
        {
            currentState = CurrentState.Idle; 
        }
        else if (currentState == CurrentState.Falling)
        {
            GravityHandler();
        }
    }

    void MovementHandler() // handles enemy movements
    {
        rb.rotation = Quaternion.LookRotation(direction, Vector3.up);
        rb.angularDrag = rotationDrag;

        Vector3 movement = transform.forward * speed * Time.fixedDeltaTime; //lets the enemy move forward
        movement.y = 0.0f; // has the the movement vector for the y-axis to 0 so it dosent constantly drag
        Vector3 targetPos = rb.position + movement; //moves towards targeted gameobject's position

        RaycastHit obsticleAvoidHit; 

        if (Physics.Raycast(transform.position, transform.forward, out obsticleAvoidHit, minAvoidanceDistance, obsticleLayer))
        {
          /*  Vector3 hit = obsticleAvoidHit.normal;
            hit.y = 0.0f;
          */
        }

        rb.MovePosition(targetPos); // updates and moves to target

        if (Vector3.Distance(transform.position, target.transform.position) <= minDistance) // checks if target is in minimum distance from enemy
        {
            target = null;
            currentState = CurrentState.Idle; 
        }
    }

    void SeekHandler() //handles enemy seeks
    {
        if (target)
        {
            //Face enemy to the player
            direction = (target.transform.position - this.transform.position);
            direction.Normalize();
            currentState = CurrentState.Following;
        }
        if (!target)
        {
            SeekRotate();
            Collider[] colliders = Physics.OverlapSphere(transform.position, maxDistance, playerLayer); //if player overlaps sphere range 
            if (colliders.Length >= 1)
            {
                target = colliders[0].gameObject;
            }
            else
            {
                patrolPosition.transform.parent = gameObject.transform;
                Vector3 position = new Vector3(Random.Range(-maxDistance, maxDistance), transform.position.y, Random.Range(-maxDistance, maxDistance));
                patrolPosition.transform.position = position;
                patrolPosition.transform.parent = null;
                target = patrolPosition;
            }
        }
    }
    void GravityHandler()
    {
        Vector3 vertical = Vector3.zero;
        vertical.y -= gravity * Time.fixedDeltaTime;

        Vector3 targetPosition = rb.position += vertical;

        rb.MovePosition(targetPosition);
    }

    void SeekRotate()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, roationSpeed, 0), Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        if (debugGizmo)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(this.transform.position, maxDistance);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(this.transform.position, minDistance);

            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, new Vector3(transform.position.x, transform.position.y - groundCheckLength, transform.position.z));

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + new Vector3(0, 0, minAvoidanceDistance));
        }
    }

}
