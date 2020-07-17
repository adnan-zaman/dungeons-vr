using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**
 * A type of control that's turned on when a specified object is placed in
 */ 
public class HoldActivated : MonoBehaviour
{
    //the object that turns on this control
    public Transform targetObj;
    //whatever this thing controls
    public FloatEvent OnActivated;
    //sound played on activate
    public AudioSource activateSound;
    //how close the target needs to be to consider placed
    public float activateDistance;
    //How much "value" is sent to the connected event when activated
    public float value;
    //if true, control continuously sends value while being pressed
    //else, it only acts as an on/off signal 
    public bool contSend;
    //whether this control is activated or not
    private bool on;
    //last state of this control
    private bool lastState;


    private void Start()
    {
        on = (Vector3.Distance(targetObj.position, transform.position) <= activateDistance);
        //identifier will have same material as target object
        MeshRenderer identifier = transform.parent.parent.Find("Identifier").GetComponent<MeshRenderer>();
        identifier.material = targetObj.gameObject.GetComponent<MeshRenderer>().material;
        lastState = on;
    }
    void Update()
    {

        lastState = on;
        on = (Vector3.Distance(targetObj.position, transform.position) <= activateDistance);
        if (on)
        {
            targetObj.gameObject.GetComponent<Rigidbody>().isKinematic = true;
            targetObj.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
            targetObj.position = transform.position;
            targetObj.gameObject.GetComponent<Rigidbody>().isKinematic = false;
            activateSound.Play();
        }
        //if contsend, continuously send value to event whle button is pressed
        if (on != lastState)
            OnActivated.Invoke(value);
   
    }
}
