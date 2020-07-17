using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * This class represents items that can be picked up and thrown
 */ 

[RequireComponent(typeof(Rigidbody))]
public class Interactable : Hookable
{

    protected Controller controller;
    public AudioSource attach;

    public virtual void AttachController(Controller cont)
    {
        controller = cont;
        transform.SetParent(cont.gameObject.transform);
        attach.Play();
        GetComponent<Rigidbody>().useGravity = false;
        GetComponent<Rigidbody>().isKinematic = true;
    }

    public virtual void DetachController()
    {
       
        controller = null;
        transform.SetParent(null);
        GetComponent<Rigidbody>().useGravity = true;
        GetComponent<Rigidbody>().isKinematic = false;
    }

    /**
     * Switching is kept seperate from Attach (even though they do the same thing)
     * just in case there needs to be a distinction later on (e.g an animation affect plays on Attach
     * but we dont want it to play a second time when it switches from grappling hook to mech hand)
     */ 
    public virtual void SwitchController(Controller cont)
    {
        controller = cont;
        transform.SetParent(cont.gameObject.transform);
    }


    /**
     * All Interactables have this for the sake of polymorphism, but only
     * Controllables really use it. Interactables will be moved kinematically
     * by being set as a child of the Controller, Controllable's will, every frame,
     * check the Controller's deltaTranslate and deltaRotation and move within
     * the bounds of its limited movement
     */
    public void UpdateTransform()
    {
        //nothing happens woo
    }

}
