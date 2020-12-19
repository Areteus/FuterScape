using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockDragandDrop : MonoBehaviour
{
    [SerializeField]
    private float gravityRate;
    Vector3 verticalVector;
    Rigidbody rb;

    //Mouse properties
    Vector3 mouseOffset;
    float mouseCordination;
    private void OnMouseDrag() //when object is draged with the mouse
    {
        transform.position = GetMousePosition() + mouseOffset; //position cursor and adds a certian offcet to the object
    }

    private Vector3 GetMousePosition() //gets mouse implemeneted to the world 
    {
        //(x,y) coordinates
        Vector3 mousePoint = Input.mousePosition;
        //world coordinates
        mousePoint.z = mouseCordination; // z coordanate of the gameobject on screen

        return Camera.main.ScreenToWorldPoint(mousePoint); //returns the vector as a result
    }

    private void OnMouseDown()
    {
        mouseCordination = Camera.main.WorldToScreenPoint(gameObject.transform.position).z; //to get mouse to screen point
        mouseOffset = gameObject.transform.position - GetMousePosition();  //the offset position of the gameobject and the mouse cursor
    }

    private void Update()
    {
        // applying gravity 
        verticalVector = Vector3.zero;
        verticalVector.y -= gravityRate * Time.fixedDeltaTime;

        Vector3 targetPosition = rb.position += verticalVector;

        rb.MovePosition(targetPosition);
    }

}
