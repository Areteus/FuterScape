using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneAi : MonoBehaviour
{
    private GameObject target;
    public GameObject patrolPosition;

    [SerializeField]
    private float moveSpeed = 250;
    [SerializeField]
    private float minDistance = 50.0f;
    [SerializeField]
    private float maxDistance = 100.0f; 
    [SerializeField]
    private float rotationDrag = 0.75f;
    [SerializeField]
    private bool canAttack = true;
    [SerializeField]
    private float brakeForce = 5f;

    public LayerMask layer;

    private bool isAttacking = false;
    private Vector3 direction;
    private float distance = 0.0f;

    Rigidbody rb;
    Vector2 input;

    public enum CurrentState { Idle, Following, Attacking, Seeking };
    public CurrentState currentState;
    public bool debugGizmo = true;

    public float DistanceToPlayer { get { return distance; } }


    void Start()
    {
        currentState = CurrentState.Idle;
        isAttacking = false;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
       // float horizontal = Input.GetAxis("Horizontal");
        //float vertical = Input.GetAxis("Vertical");

        input = new Vector3();
        // input.x = horizontal;
        //input.y = vertical;

        //Find the distance to the player
     

    }

    private void FixedUpdate()
    {
        //If the player is in range move towards
        if (distance > minDistance && distance < maxDistance)
        {
            currentState = CurrentState.Following;
        }

        else if (distance <= minDistance && distance > 0.5 )
        {
            currentState = CurrentState.Idle;

            if (canAttack)
            {
                currentState = CurrentState.Attacking;
            }
        }

        StateMachine();
    }


    void StateMachine()
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
        else if (currentState == CurrentState.Attacking)
        {

        }
    }

    void MovementHandler()
    {
        rb.rotation = Quaternion.LookRotation(direction, Vector3.up);
        rb.angularDrag = rotationDrag;

        // Calculate horizontal movement
        //Vector3 movement = Vector3.right * moveSpeed * Time.fixedDeltaTime;
        Vector3 movement = transform.forward * moveSpeed * Time.fixedDeltaTime;
        movement.y = 0.0f;
        Vector3 targetPosition = rb.position + movement;

        rb.MovePosition(targetPosition);
    }

    void SeekHandler()
    {
        if (target)
        {
            distance = Vector3.Distance(target.transform.position, this.transform.position);

            //Face the drone to the player
            direction = (target.transform.position - this.transform.position);
            direction.Normalize();
            currentState = CurrentState.Following;
        }
        if (!target)
        {
            RaycastHit hit;
            Physics.SphereCast(transform.position, maxDistance, transform.forward, out hit, layer);
            Collider colliders = hit.collider;
            if (colliders)
            {
                target = colliders.gameObject;
            }
            else
            {
                Vector3 position = new Vector3(Random.Range(-maxDistance, maxDistance), transform.position.y, Random.Range(-maxDistance, maxDistance));
                patrolPosition.transform.position = position;
                target = patrolPosition; 
            }
        }
    }

   /* private void DroneStopsMoving()
    {
        isAttacking = false;
        rb.drag = (brakeForce);
    }

    private void DroneMovesToPlayer()
    {
        isAttacking = true;
        rb.AddRelativeForce(Vector3.forward * moveSpeed * Time.deltaTime);
    }
   */
    private void OnDrawGizmosSelected()
    {

        if (debugGizmo)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(this.transform.position, maxDistance);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(this.transform.position, minDistance);
        }
    }
}
