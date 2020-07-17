using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Controls ActualHook's collision with Hookable objects
 */ 
public class ActualHook : Controller
{
    //Whether or not hook has hooked onto a Hookable object
    public bool Hooked
    {
        get { return hooked; }
        private set { hooked = value; }
    }


    //Coordinate of where actual hook has hooked an object
    public Vector3 anchoredPos;
    //if hook hits a non hookable
    public bool obstructed;
    //transform of Interactable
    public Transform targetTransform;
    //attached interactable
    public Interactable target;

    private bool hooked;

    //Collider of Interactable
    private Collider targetCollider;

   
    //mech hand transform
    private Transform mechHandTransform;

    //grappling hook controller
    private GrapplingHookController grappHookController;

    void Start()
    {
        hooked = false;
        target = null;
        mechHandTransform = transform.parent.parent; //actualHook -> grapplingHook -> mechHands
        grappHookController = mechHandTransform.gameObject.GetComponent<GrapplingHookController>();
        //Controller variables
        prevPos = transform.position;
        prevRot = transform.rotation.eulerAngles;
    }

    void Update()
    {
        CalculateDeltas();
    }

    public override void Detach()
    {
        target = null;

        StartCoroutine(grappHookController.Retract());
        
    }


    public void AttachInteractable()
    {
        if (target)
        {
            target.AttachController(this);
        }

    }

    public void DetachInteractable()
    {
        if (target)
        {
            target.DetachController();
        }
            
        target = null;
    }

    /**
     * Changes Interactable's parent to mech hand. Should be called when actualHook is about to be disabled
     */
    public void SwitchToMechHand()
    {
        if (target)
        {
            mechHandTransform.gameObject.GetComponent<MechHandController>().TakeFromActualHook(target);
        }
            
        target = null;
    }

    void OnTriggerEnter(Collider other)
    {
        //If hook has hit a Hookable object
        if (other.gameObject.GetComponent<Hookable>())
        {
            Hooked = true;
            if (other.gameObject.GetComponent<Hookable>() is Interactable)
            {
                target = other.gameObject.GetComponent<Interactable>();
            }
        }
        //hit a non-hookable
        else if (other.gameObject.name != "HookCord" && other.gameObject.name != "WinRoom")
        {
            obstructed = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        Hooked = false;
        if (target)
        {
            target.DetachController();
        }
        target = null;
    }

    void OnEnable()
    {

    }

    void OnDisable()
    {
        Hooked = false;
        obstructed = false;
        target = null;
        targetTransform = null;
        targetCollider = null;
        
    }

    
}

