using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/**
 * Player logic
 * 
 */
public class PlayerController : MonoBehaviour
{
    public bool colliding;
    //How fast the camera moves in first person mode (horizontally)
    public float rotationSpeedX;
    //How fast the camera moves in first person mode (vertically)
    public float rotationSpeedY;
    //How much the mouse has to be moved for camera to move in first person mode
    public float horizontalSensitivity;
    public float verticalSensitivity;
    //mech hand
    public MechHandController mechHandController;

    private Camera cam;

   
    
    
    //Last position of the mouse
    private Vector3 lastMousePos;
    

    //Stores the pitch of the camera
    private float pitch;
    public bool paused = false;


    void Start()
    {

        lastMousePos = Input.mousePosition;
        horizontalSensitivity = 1;
        verticalSensitivity = 1;
        pitch = 0;
    }


    void Update()
    {
 
    }

    void LateUpdate()
    {
        CameraMovement();
    }

    private void CameraMovement()
    {
        //Calculate how much the mouse has moved since last frame
        Vector3 mouseDelta = Input.mousePosition - lastMousePos;
        lastMousePos = Input.mousePosition;

        /* 
		 * Change in mouseDelta is checked against
		 * verticalSensitivity and horizontalSensitivity
		 * 
		 * If mouseDelta's x/y >= vertical/horizontalSensitivity,
		 * camera movement will take place
		 */

        //Vertical camera movement
        if (mouseDelta.y >= verticalSensitivity)
        {
            transform.Rotate(new Vector3(-rotationSpeedY * Math.Abs(mouseDelta.y), 0, 0));
            pitch -= rotationSpeedY * Math.Abs(mouseDelta.y);
        }
        else if (mouseDelta.y <= -verticalSensitivity)
        {
            transform.Rotate(new Vector3(rotationSpeedY * Math.Abs(mouseDelta.y), 0, 0));
            pitch += rotationSpeedY * Math.Abs(mouseDelta.y);
        }

        //Stops camera from rotating more than 90 degrees upward
        //(-90 degrees) or past 90 degrees if moving downward
        if (pitch <= -90)
        {
            Vector3 currRotation = transform.eulerAngles;
            currRotation.x = 270;
            transform.eulerAngles = currRotation;
            pitch = -90;
        }
        else if (pitch >= 90)
        {
            Vector3 currRotation = transform.eulerAngles;
            currRotation.x = 90;
            transform.eulerAngles = currRotation;
            pitch = 90;
        }


        //Horizontal camera movement

        
        if (mouseDelta.x >= horizontalSensitivity)
            transform.RotateAround(transform.position, Vector3.up, rotationSpeedX * Math.Abs(mouseDelta.x));
        else if (mouseDelta.x <= -horizontalSensitivity)
            transform.RotateAround(transform.position, Vector3.up, -rotationSpeedX * Math.Abs(mouseDelta.x));
            

    }

    void OnTriggerEnter(Collider other)
    {

    }
}
