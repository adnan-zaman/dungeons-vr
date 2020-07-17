using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * It's a platform
 * and it moves
 */

public class MovingPlatform : MonoBehaviour
{
    //used for non-analog movement
    public Transform target;
    //how fast this moves for non-analog movement
    public float speed;
    //original position
    private Vector3 orig;
    //direction of orig to target
    private Vector3 origToTarget;
    //whether this moving platform is on or off
    private bool on = false;

    private void Start()
    {
        orig = transform.position;
        origToTarget = target.position - orig;
    }

    //for platforms where you can change position analog-ly (?)
    public void ChangeX(float x)
    {
        Vector3 v = transform.localPosition;
        v.x = x;
        transform.localPosition = v;
    }

    //float doesn't actually do anything, just made this so it's
    //compatible with the controls I made that only had analog movement in mind
    //this is for moving platforms that are either "on" or "off"
    public void Toggle(float x)
    {
        on = !on;
        StartCoroutine(MoveToTarget());
    }

    private IEnumerator MoveToTarget()
    {
        Vector3 dir = (on) ? origToTarget : -origToTarget;
        Vector3 targ = (on) ? target.position : orig;
        while (Vector3.Distance(transform.position,targ) <= 0.1)
        {
            transform.position += dir * speed * Time.deltaTime;
            yield return null;
        }

        //just in case its not exactly on target
        transform.position = targ;



    }
}
