using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAi : MonoBehaviour
{
    #region Variables
    private GameObject target;
    public GameObject patrolPosition;

    [SerializeField]
    private float moveSpeed = 5;

    Vector3 _movement;
    Vector3 _verticalVector;

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
    private float minimumAvoidanceDistance = 0.5f;

    [SerializeField]
    private float avoidanceForce;

    [SerializeField]
    private bool canAttack = true;

    [SerializeField]
    private float brakeForce = 5f;

    public LayerMask playerLayer;
    public LayerMask obstacleLayer;

    private bool isAttacking = false;
    private bool _isGrounded;
    public float groundCheckLength;
    public LayerMask groundLayer;
    private Vector3 direction;
    private float distance = 0.0f;

    Rigidbody rb;
    Vector2 input;

    public enum enemyState { Idle, Following, Attacking, Seeking, Falling };
    private enemyState _curState;
    public bool isDebugging = true;

    public float DistanceToPlayer { get { return distance; } }
    #endregion

    void Start()
    {
        _curState = enemyState.Idle;
        isAttacking = false;
        Random.InitState(Random.Range(0, 200));
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Sets _isGrounded Boolean to true if raycast hits a surface on the ground layer
        Ray groundingRay = new Ray(transform.position, Vector3.down);
        _isGrounded = Physics.Raycast(groundingRay, groundCheckLength, groundLayer);

        if (!_isGrounded)
        {
            _curState = enemyState.Falling;
        }
        else
        {
            if (_curState == enemyState.Falling)
                _curState = enemyState.Idle;
        }
    }

    private void FixedUpdate()
    {
        StateMachine();

    }
        
    void StateMachine()
    {
        switch (_curState)
        {
            case enemyState.Following:
                MovementHandler();
                return;

            case enemyState.Idle:
                _curState = enemyState.Seeking;
                return;

            case enemyState.Seeking:
                SeekHandler();
                return;

            case enemyState.Attacking:
                return;

            case enemyState.Falling:
                GravityHandler();
                return;
        }
    }

    void MovementHandler()
    {
        rb.rotation = Quaternion.LookRotation(direction, Vector3.up);
        rb.angularDrag = rotationDrag;

        _movement = transform.forward * moveSpeed * Time.fixedDeltaTime;
        _movement.y = 0.0f;
        Vector3 targetPosition = rb.position + _movement;

        RaycastHit avoidanceHit;
        //Check that the agent hit with the obstacles within it's minimum distance to avoid
        if (Physics.Raycast(transform.position, transform.forward, out avoidanceHit, minimumAvoidanceDistance, obstacleLayer))
        {
            //Get the normal of the hit point to calculate the new direction
            Vector3 hitNormal = avoidanceHit.normal;
            hitNormal.y = 0.0f; //Don't want to move in Y-Space

            //Get the new direction vector by adding force to agent's current forward vector
            direction = transform.forward + hitNormal * avoidanceForce;
        }

        rb.MovePosition(targetPosition);

        if (Vector3.Distance(transform.position, target.transform.position) <= minDistance)
        {
            target = null;
            _curState = enemyState.Idle;
        }
    }

    void SeekHandler()
    {
        if (target)
        {
            //Face the drone to the player
            direction = (target.transform.position - this.transform.position);
            direction.Normalize();
            _curState = enemyState.Following;
        }
        if (!target)
        {
            SeekRotate();
            Collider[] colliders = Physics.OverlapSphere(transform.position, maxDistance, playerLayer);
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
        _verticalVector = Vector3.zero;
        _verticalVector.y -= gravityRate * Time.fixedDeltaTime;

        Vector3 targetPosition = rb.position += _verticalVector;

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
