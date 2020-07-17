using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Acts as main control hub for grappling hook and all of its subparts
 */

public class GrapplingHookController : MonoBehaviour
{
    //how fast grappling hook should extend outward
    public float extendSpeed;
    //how fast to retract (faster because we need instant acceleration)
    public float retractSpeed;
    //maximum length grappling hook can reach
    public float maxLength;
    //current state of the grappling hook
    public GrapplingHookState State
    {
        get { return currState; }
        private set { currState = value; }
    }
    //whether on retract, grappling hook should bring attached Interactable or not
    public bool dontBring;

    //entire grappling hook object
    private Transform grapplingHook;
    //Cord part of grappling hook
    private Transform hookCord;
    //Actual hook part of grappling hook
    private Transform actualHookTransform;
    //ActualHook script attached to actual hook
    private ActualHook actualHook;
    //Hook launch point
    private Transform hookLaunchPoint;
    //underlying variable for state
    private GrapplingHookState currState;

    //player transform
    private Transform playerTransform;

    //the point that the grappling hook has hooked to (if any)
    public Vector3 anchorPoint = Vector3.zero;

    void Start()
    {
        grapplingHook = transform.Find("GrapplingHook");
        hookCord = grapplingHook.Find("HookCord");
        actualHookTransform = grapplingHook.Find("ActualHook");
        actualHook = actualHookTransform.gameObject.GetComponent<ActualHook>();
        hookLaunchPoint = transform.Find("HookLaunchPoint");
        playerTransform = transform.parent;
        currState = GrapplingHookState.Inactive;
    }




    /**
     * Extends the grappling hook
     */
    public IEnumerator Extend()
    {

        //Shoot a raycast out in the direction the grappling hook would fire 
        //if the raycast happens to hit something, the grappling hook will hit it too
        //store this position as the anchor point
        //this is to ensure that the destination is on the surface of the object
        //as sometimes the grappling hook goes inside objects
        RaycastHit hit;
        if (Physics.Raycast(hookLaunchPoint.position, hookLaunchPoint.forward, out hit, maxLength))
        {
            if (hit.collider.gameObject.GetComponent<Hookable>())
                anchorPoint = hit.point;
        }
        else
        {
            anchorPoint = Vector3.zero;
        }
        grapplingHook.gameObject.SetActive(true);
        actualHook.gameObject.SetActive(true); //actual hook is disabled to not interfere with mech hand grabbing
        State = GrapplingHookState.Extending;
        Vector3 currScale = hookCord.transform.localScale;
        Vector3 currPos = hookCord.transform.position;
        float length = currScale.y * 2; //since scale for a cylinder corresponds to # of units from centre to either side

        while (length < maxLength)
        {
            //Adjust scale
            currScale.y += extendSpeed * Time.deltaTime;
            length = currScale.y * 2;
            hookCord.transform.localScale = currScale;

            //Adjust position to stay "anchored" to mech hand 
            hookCord.transform.position = hookLaunchPoint.position + (hookLaunchPoint.forward * currScale.y);
            //Ensure actual hook stays on far end of hook cord
            AdjustActualHookPos(currScale.y);
            //ActualHook has hooked something, so stop extending
            if (actualHook.Hooked)
            {
                State = GrapplingHookState.Hooked;
                //move actual hook back out to the anchored position which is on surface
                actualHookTransform.position = anchorPoint;
                /* Change length of hook cord to make sure it doesnt overshoot the actual hook */

                length = Vector3.Distance(anchorPoint, hookLaunchPoint.position);
                //Adjust scale
                currScale.y = length/2;
                hookCord.transform.localScale = currScale;
                //Adjust position to stay "anchored" to mech hand 
                hookCord.transform.position = hookLaunchPoint.position + (hookLaunchPoint.forward * currScale.y);

                //If the Hookable is an Interactable
                if (actualHook.target)
                {
                    State = GrapplingHookState.Held;
                    actualHook.AttachInteractable();
                }
                
                yield break;
            }
           
            if (actualHook.obstructed)
            {

                StartCoroutine(Retract());
                yield break;
            }

    
            yield return null;
        }

        //Start retract process after reaching maximum length
        StartCoroutine(Retract()); 

    }

    /**
     * Retracts the grappling hook
     */
    public IEnumerator Retract()
    {


        anchorPoint = Vector3.zero;
        State = GrapplingHookState.Retracting;
        if (dontBring)
        {
            actualHook.DetachInteractable();
            dontBring = false;
        }
            
        Vector3 currScale = hookCord.transform.localScale;
        float length = currScale.y * 2; //since scale for a cylinder corresponds to # of units from centre to either side

        while (length > 0)
        {
            //Adjust scale
            currScale.y -= retractSpeed * Time.deltaTime;
            length = currScale.y * 2;
            hookCord.transform.localScale = currScale;

            //Adjust position to stay "anchored" to anchor point
            hookCord.transform.position = hookLaunchPoint.position + (hookLaunchPoint.forward * currScale.y);
            //Ensure actual hook stays on far end of hook cord
            AdjustActualHookPos(currScale.y);
            yield return null;
        }

        //Prevent scale from going negative
        currScale.y = 0;
        hookCord.transform.localScale = currScale;
        //Interactable will now be controlled by mech hand rather than the grappling hook
        actualHook.SwitchToMechHand();
        //Grappling hook is left disabled unless in use
        //to avoid problems with collisions
        grapplingHook.gameObject.SetActive(false);
        actualHook.gameObject.SetActive(false); //actual hook is disabled to not interfere with mech hand grabbing
        State = GrapplingHookState.Inactive;
    }

    /**
     * Pull player towards hooked location
     */
    public IEnumerator Pull()
    {
        State = GrapplingHookState.Pulling;
        Vector3 currScale = hookCord.transform.localScale;
        float length = currScale.y * 2; //since scale for a cylinder corresponds to # of units from centre to either side
        
        //Direction from player to the hooked point, the direction we will be pulled towards
        Vector3 dirToMove = (anchorPoint - playerTransform.position).normalized;
        //This is to make sure we dont move downwards if we aim lower (or higher!)
        dirToMove.y = 0;


        while (length > 0)
        {
            float oldScale = currScale.y;
            //Adjust scale
            currScale.y -= retractSpeed * Time.deltaTime;
            length = currScale.y * 2;
            hookCord.transform.localScale = currScale;

            /**1. We move the coord to the mech hand, just like in Extend()/Retract()  **/ 

            //Adjust position to stay "anchored" to anchor point
            hookCord.transform.position = hookLaunchPoint.position + (hookLaunchPoint.forward * currScale.y);
            //Ensure actual hook stays on far end of hook cord
            AdjustActualHookPos(currScale.y);

            /** 2. We move the mech hand (and as a result, the hook cord attached to the mech hand)
             *     in the hook cord's up direction (the direction it moves in) by the amount
             *     the hook cord has shrunk multiplied by 2. Mulitiplication because the hook cord
             *     is no longer in the center
             *     (below is pic demonstrating what this horrible explanation is trying to say)
             *     
             *  A)  , = center of hook cord
             *     currently, scale of 4
             *     [M.Hand]----,----[HookedObj]
             *     
             *  B) hook cord scaled down, now scale of 3
             *     4 - 3 = 1 (deltaScale), distance from HookedObj
             *     [M.Hand] ---,--- [HookedObj]
             *     
             *  C) hook cord moved to mech hand, (which is also deltaScale units away)
             *     increasing the distance b/w hook cord and HookedObj to be 2 * deltaScale
             *     [M.Hand]---,---  [HookedObj]
             *   
             *  D) translate mech hand by 2 * deltaScale and the hook cord is shorter and the mech
             *     hand has moved
             *       [M.Hand]---,---[HookedObj]
             *     
             *
             **/
            float deltaScale = oldScale - currScale.y;
            playerTransform.Translate(dirToMove * deltaScale * 2,Space.World);

            yield return null;
        }

        //Prevent scale from going negative
        currScale.y = 0;
        hookCord.transform.localScale = currScale;

        //Grappling hook is left disabled unless in use
        //to avoid problems with collisions
        grapplingHook.gameObject.SetActive(false);
        State = GrapplingHookState.Inactive;
    }

    /**
     * Ensures actual hook stays at far end of hook cord
     * @param   cordScale   the y value of hookCord's scale
     */ 
    private void AdjustActualHookPos(float cordScale)
    {
        actualHookTransform.transform.position = hookCord.transform.position + (hookCord.up * cordScale);
    }
}

/**
 * Different states the grappling hook can be in
 */ 
public enum GrapplingHookState
{
    Inactive, //hook is unshot and inactive
    Extending, //hook is in process of extending
    Retracting, //hook is in process of retracting
    Pulling, //hook is pulling player to location
    Hooked, //hook is attached to wall/floor etc.
    Held, //hook is attached to an Interactable
    InAir //in the case where hook is attached to an interactable object that is currently
          //off the ground 
}
