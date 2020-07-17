using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Parent class for all "Controllers", things that can manipulate Interactables.
 * This includes the ActualHook of the Grappling Hook and the Mech Hand.
 */ 
public abstract class Controller : MonoBehaviour
{
    //Change in translate since last frame
    public Vector3 deltaTranslate;
    //Change in rotation since last frame
    public Vector3 deltaRotate;

    //For calculating delta translate/rotation
    protected Vector3 prevPos;
    protected Vector3 prevRot;

    //Subclasses implement their own detach behaviour
    public abstract void Detach();


    //All controllers need to calculate their deltas
    protected void CalculateDeltas()
    {
        deltaTranslate = transform.position - prevPos;
        deltaRotate = transform.eulerAngles - prevRot;
        prevPos = transform.position;
        prevRot = transform.eulerAngles;
    }
}
