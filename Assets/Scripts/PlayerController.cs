using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float jumpHeight = 2f;
    public float groundDistance = 0.2f;
    public float DashDistance = 5f;
    public LayerMask Ground;

    private Rigidbody rb;
    private Vector3 inputs = Vector3.zero;
    public bool isGrounded = true;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    private void Update()
    {
        Physics.Raycast(transform.position, Vector3.down, groundDistance, Ground);

        inputs = Vector3.zero;
        inputs.x = Input.GetAxis("Horizontal");
        inputs.y = Input.GetAxis("Vertical");

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector3.up * Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y), ForceMode.VelocityChange);
        }    

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Vector3 dashVelocity = Vector3.Scale(transform.forward, DashDistance * new Vector3((Mathf.Log(1f / (Time.deltaTime * rb.drag + 1)) / -Time.deltaTime), 0, (Mathf.Log(1f / (Time.deltaTime * rb.drag + 1)) / -Time.deltaTime)));
            rb.AddForce(dashVelocity, ForceMode.VelocityChange);
        }
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + inputs * speed * Time.fixedDeltaTime);
    }
}
