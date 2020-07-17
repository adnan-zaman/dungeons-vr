using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
 * Controls all logic for mechanical hand
 * Including user input, sending messages to grappling hook, interaction etc.
 */ 

public class MechHandController : Controller
{

    //if the mech hand is colliding with something
    public bool colliding;
    //win text
    public Text winText;
    //current state of the mech hand
    public MechHandState state;
    //how strong the throw is
    public float throwForce;
    //material of sphere
    public Material sphereMat;
    public Color defaultColor;
    [Header("Sounds")]
    //grappling hook shoot sound
    public AudioSource shootSound;
    public AudioSource pullSound;
    public AudioSource throwSound;
    //Reference to grappling hook controller
    private GrapplingHookController grapHookController;
    //held Interactable
    private Interactable heldItem;
    //potential held Interactable
    private Interactable potentialHeld;

    //references to IK targets
    private Transform indexTarget;
    private Transform middleTarget;
    private Transform ringTarget;
    private Transform littleTarget;
    private Transform thumbTarget;


    //original IK positions
    private Vector3 indexOrig;
    private Vector3 middleOrig;
    private Vector3 ringOrig;
    private Vector3 littleOrig;
    private Vector3 thumbOrig;


    //reticle transform
    private Transform reticleTransform;

    void Start()
    {
        sphereMat.SetColor("_EmissionColor", defaultColor);
        grapHookController = GetComponent<GrapplingHookController>();
        state = MechHandState.Empty;
        //IK setup stuff
        indexTarget = transform.Find("TargetPoles").Find("IndexTarget");
        middleTarget = transform.Find("TargetPoles").Find("MiddleTarget");
        ringTarget = transform.Find("TargetPoles").Find("RingTarget");
        littleTarget = transform.Find("TargetPoles").Find("LittleTarget");
        thumbTarget = transform.Find("TargetPoles").Find("ThumbTarget");
        //Original positions
        indexOrig = indexTarget.localPosition;
        middleOrig = middleTarget.localPosition;
        ringOrig = ringTarget.localPosition;
        littleOrig = littleTarget.localPosition;
        thumbOrig = thumbTarget.localPosition;
    }

    void Update()
    {
        //For Controller
        CalculateDeltas();

        //Empty hand interactions (grappling hook)
        if (state == MechHandState.Empty)
        {
 
            //Extend from inactive
            if (Input.GetMouseButtonDown(0) && grapHookController.State == GrapplingHookState.Inactive)
            {
                
                StartCoroutine(grapHookController.Extend());
                shootSound.Play();
            }

            //Retract from hooked
            if (Input.GetMouseButtonDown(0) && (grapHookController.State == GrapplingHookState.Hooked
                                            || grapHookController.State == GrapplingHookState.InAir
                                            || grapHookController.State == GrapplingHookState.Held))
            {
                grapHookController.dontBring = true;
                StartCoroutine(grapHookController.Retract());
                pullSound.Play();
            }

            //Retract and bring object
            if (Input.GetMouseButtonDown(2) && grapHookController.State == GrapplingHookState.Held)
            {

                StartCoroutine(grapHookController.Retract());
                pullSound.Play();
            }

            //Pull towards hooked position
            if (Input.GetMouseButtonDown(1) && grapHookController.State == GrapplingHookState.Hooked)
            {
 
                StartCoroutine(grapHookController.Pull());
                pullSound.Play();
            }
        }

        //Held item interactions
        if (state == MechHandState.Holding)
        {
            if (Input.GetMouseButtonDown(0))
            {
                throwSound.Play();
                LaunchInteractable();
            }
                
            if (Input.GetMouseButtonDown(2))
            {

                DetachInteractable();
            }
                
        }

        //In reach interactions
        if (state == MechHandState.InReach)
        {
            if (Input.GetMouseButtonDown(0))
            {

                AttachInteractable();
            }
                
        }
    }

    public override void Detach()
    {
        heldItem = null;
        state = MechHandState.Empty;
        IKGrab(false);
    }

   public void TakeFromActualHook(Interactable i)
    {
        heldItem = i;
        state = MechHandState.Holding;
        i.SwitchController(this);
        IKGrab(true);

    }

  

    private void LaunchInteractable()
    {
        heldItem.DetachController();
        heldItem.gameObject.GetComponent<Rigidbody>().AddForce(transform.forward * throwForce, ForceMode.Impulse);
        heldItem = null;
        state = MechHandState.Empty;
        IKGrab(false);
    }

    private void AttachInteractable()
    {

        heldItem = potentialHeld;
        heldItem.AttachController(this);
        potentialHeld = null;
        state = MechHandState.Holding;
        IKGrab(true);
    }

    private void DetachInteractable()
    {
        heldItem.DetachController();
        heldItem = null;
        state = MechHandState.Empty;
        IKGrab(false);
    }


    /*
     * Takes care of the IK animation for grabbing
     * @param grab true if grabbing, false if releasing grab
     */ 
    private void IKGrab(bool grab)
    {
     
        //Sets the new IK target to be approximately on the surface of the Interactable
        if (grab)
        {
            var distance = heldItem.gameObject.transform.position - indexTarget.position;
            distance.x = 0; distance.z = 0;
            distance.y = distance.y + heldItem.gameObject.transform.localScale.y / 2;
            indexTarget.position += distance;
            middleTarget.position += distance;
            ringTarget.position += distance;
            littleTarget.position += distance;
        }
        //letting go
        else
        {
            indexTarget.localPosition = indexOrig;
            middleTarget.localPosition = middleOrig;
            ringTarget.localPosition = ringOrig;
            littleTarget.localPosition = littleOrig;
        }
       
    }

    void OnTriggerEnter(Collider other)
    {
        colliding = true;


        //Mech hand's collider includes the grappling hook's collider
        //and I don't know how to get it to stop so this'll have to do
        if (grapHookController.State == GrapplingHookState.Inactive)
        {
            if (other.gameObject.GetComponent<Interactable>())
            {

                state = MechHandState.InReach;
                potentialHeld = other.gameObject.GetComponent<Interactable>();
            }
        }

        //a winner is you
        if (other.gameObject.name == "WinRoom")
        {
            winText.text = "You reached the last room! \n            You Win!";
                    
        }
        
            
    }

    void OnTriggerExit(Collider other)
    {
        //Mech hand's collider includes the grappling hook's collider
        //and I don't know how to get it to stop so this'll have to do
        if (grapHookController.State == GrapplingHookState.Inactive)
        {
            if (other.gameObject.GetComponent<Interactable>())
            {
                state = MechHandState.Empty;
                potentialHeld = null;
            }
        }
            
    }
}

public enum MechHandState
{
    Empty, //hand is empty, will shoot grappling hook
    Holding, //hand is holding Interactable, will not shoot hook, will throw/drop Interactable
    InReach //hand is in reach of Interactable, will not shoot hook, will grab Interactable
}
