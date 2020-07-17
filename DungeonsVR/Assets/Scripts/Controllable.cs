using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

/**
 * Class that represents all Controllables (levers, buttons etc)
 */

public class Controllable : Interactable
{

    void Update()
    {
        //update orientation depending on attached controller
        if (controller)
        {
            transform.position += controller.deltaTranslate;
            transform.eulerAngles += controller.deltaRotate;
        }
    }

    public override void AttachController(Controller cont)
    {
        controller = cont;
    }

    public override void DetachController()
    {
        controller = null;
    }

    public override void SwitchController(Controller cont)
    {
        controller = cont;
    }

    /**
     * Called when controller tries moving controllable past its limited range
     */ 
    private void Break()
    {
        controller.Detach();
        controller = null;
    }

    /*
    * Function for checking equality of floats
    */
    public bool FloatEqual(float value, float equalTo, float precision = 0.001f)
    {
        return ((value <= equalTo + precision) && (value >= equalTo - precision));
    }
}

[Serializable] public class FloatEvent : UnityEvent<float> { }