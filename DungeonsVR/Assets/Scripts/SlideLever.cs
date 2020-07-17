using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/**
 * Slide levers
 */ 
public class SlideLever : Controllable
{
    //How far until controller breaks off
    public float breakDistance;
    //The value represented by this lever
    public float value;
    //value is represented by how far away the lever moves from origin
    //multipler is used to change the actual value sent to the event
    public float multiplier;

    //The thing this lever will control
    public FloatEvent OnChangeValue;
    //Transform of the lever base
    private Transform baseTransform;
    //Transform of whole lever
    private Transform leverTransform;
    //value from previous frame
    private float lastValue;
   

    void Start()
    {
        leverTransform = transform.parent;
        baseTransform = leverTransform.parent;
        value = leverTransform.localPosition.x;
        lastValue = value;
    }

    
    void Update()
    {
        if (controller)
        {
            Vector3 translate = controller.gameObject.transform.position - baseTransform.position;
            if (translate.magnitude >= breakDistance)
            {
                controller.Detach();
                controller = null;
            }
            else
            {
                translate = leverTransform.InverseTransformVector(controller.deltaTranslate);
                translate.y = 0;
                translate.z = 0;
                if (Math.Abs(leverTransform.localPosition.x + translate.x) <= baseTransform.localScale.x / 2)
                    leverTransform.localPosition += translate;
            }

            lastValue = value;
            value = leverTransform.localPosition.x * multiplier;

            if (!FloatEqual(lastValue, value))
                OnChangeValue.Invoke(value);
        }

    }
}
