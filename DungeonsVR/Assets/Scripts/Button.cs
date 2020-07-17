using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**
 * Class representing a button
 */

public class Button : Controllable
{

    //Connected callback event
    public FloatEvent OnButtonPressed;
    //the sound of click
    public AudioSource clickSound;

    [Header("Configuration")]
    //How much "value" is sent to the connected event when pressed
    public float value;
    //if true, button continuously sends value while being pressed
    //else, it only acts as an on/off signal 
    public bool contSend;

    public float breakDistance = .4f;

    //How much the button presses in
    private float pressDistance = 0.2f;

    //Button's unpressed position
    private Vector3 restingPos;
    //Button's pressed position;
    private Vector3 pressedPos;

    //Whether button is pressed or not
    public bool isPressed;
    //last pressed state
    private bool lastPressed;



    void Start()
    {
        restingPos = transform.localPosition;
        pressedPos = transform.localPosition + new Vector3(0, 0, pressDistance);
        isPressed = false;
        lastPressed = false;
    }

    void Update()
    {
        lastPressed = isPressed;

        if (controller)
        {
            isPressed = true;
            Vector3 controllerPos = controller.transform.position;
            if (Vector3.Distance(controllerPos, transform.position) > breakDistance)
            {
                controller.Detach();
                controller = null;
                return;
            }
        }

        else
            isPressed = false;

        

        transform.localPosition = (isPressed) ? pressedPos : restingPos;

        if (isPressed != lastPressed)
            clickSound.Play();

        //if contsend, continuously send value to event whle button is pressed
        if (contSend && isPressed)
            OnButtonPressed.Invoke(value);
        //if not a contsend button, only send value when button is clicked on
        else if (!contSend && isPressed && (isPressed != lastPressed))
            OnButtonPressed.Invoke(value);


    }

}
