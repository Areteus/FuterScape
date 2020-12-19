using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverEnemy : MonoBehaviour
{
    private GameObject target;
    public GameObject patrolPosition;

    [SerializeField]
    private float moveSpeed = 5;

    Vector3 movementVector;
    Vector3 verticalVector;

    [SerializeField]
    private float gravityRate;
    [SerializeField]
    private float minDistance = 50.0f;
    [SerializeField]
    private float maxDistance = 100.0f;
    [SerializeField]
    private float rotationDrag = 0.75f;
    [SerializeField]
    private float rotationSpeed = 0.75f;
    [SerializeField]
    private float minimumAvoidanceDistance = 20.0f;
    [SerializeField]
    private float avoidanceForce;
    [SerializeField]
    private float toleranceRadius = 3.0f;
    [SerializeField]
    private bool canAttack = true;
    [SerializeField]
    private float brakeForce = 5f;

    public LayerMask CoverLayer;
    public LayerMask obstacleLayer;

    private bool isAttacking = false;
    private bool isGrounded;
    public float groundCheckLength;
    public LayerMask groundLayer;

    private Vector3 direction;
    private Vector3 targetPoint;
    private float distance = 0.0f;

    Rigidbody rb;
    //Vector2 input;

    Grid grid;

    public enum CurrentState { Idle, Following, Attacking, Seeking, Falling };
    private CurrentState currentState;
    public bool isDebugging = true;

    public float DistanceToPlayer { get { return distance; } }

    void Start()
    {
        currentState = CurrentState.Idle;
        Random.InitState(Random.Range(0, 600));
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

        if (!isGrounded)
        {
            currentState = CurrentState.Falling;
        }
        else
        {
            if (currentState == CurrentState.Falling)
                currentState = CurrentState.Idle;
        }
    }

    private void FixedUpdate()
    {
        StateMachine();
    }

    void StateMachine()
    {
        switch (currentState)
        {
            case CurrentState.Following:
                Movement();
                return;

            case CurrentState.Idle:
                currentState = CurrentState.Seeking;
                return;

            case CurrentState.Seeking:
                Seek();
                return;

            case CurrentState.Attacking:
                currentState = CurrentState.Idle;
                return;

            case CurrentState.Falling:
                GravityHandler();
                return;
        }
    }

    void Movement()
    {
        rb.rotation = Quaternion.LookRotation(direction, Vector3.up);
        rb.angularDrag = rotationDrag;

        movementVector = transform.forward * moveSpeed * Time.fixedDeltaTime;
        movementVector.y = 0.0f;
        Vector3 targetPosition = rb.position + movementVector;

        rb.MovePosition(targetPosition);

        if (Vector3.Distance(transform.position, target.transform.position) <= minDistance)
        {
            target = null;
            //direction = new Vector3();
            currentState = CurrentState.Attacking;
        }
        else if (Vector3.Distance(transform.position, target.transform.position) >= maxDistance)
        {
            target = null;
            currentState = CurrentState.Seeking;
        }
    }

    void Seek()
    {
        if (target)
        {
            //Face the drone to the player
            direction = (target.transform.position - this.transform.position);
            direction.Normalize();
            currentState = CurrentState.Following;
        }
        if (!target)
        {
            SeekRotate();
            Collider[] colliders = Physics.OverlapSphere(transform.position, maxDistance, CoverLayer);
            if (colliders.Length >= 1)
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

    /*
        private void ApplyAvoidance(ref Vector3 direction)
        {

        }
    */
    void GravityHandler()
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
        if (isDebugging)
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
}
