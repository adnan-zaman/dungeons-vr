using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**
 * Handles the IK for the fingers in the mech hand
 * This script is based off the tutorial in this video: https://www.youtube.com/watch?v=qqOAzn05fvk
 */

public class IK : MonoBehaviour
{

    //how many links in the chain
    public int chainLength;
    //the target this kinematic chain is aiming for
    public Transform target;
    //pole to act as the "elbow"
    public Transform pole;
    //how many times the algorithm is run
    public int iterations;
    //stop the algorithm early if it's this close
    public float closeEnough;
    //list of the bones (technically referring to the joint, or where the bone begins)
    private Transform[] bones;
    //positions of bones
    private Vector3[] bonePositions;
    //length of each bone
    private float[] boneLengths;
    //total length of entire chain
    private float totalLength;

    private Vector3[] startDirSucc;
    private Quaternion[] startRotBone;
    private Quaternion startRotTarget;
    private Quaternion startRotRoot;

    void Start()
    {
        Init();
    }

    private void Init()
    {
        bones = new Transform[chainLength + 1];
        bonePositions = new Vector3[chainLength + 1];
        boneLengths = new float[chainLength];
        startDirSucc = new Vector3[chainLength + 1];
        startRotBone = new Quaternion[chainLength + 1];
        totalLength = 0;

        Transform curr = transform;


        for (int i = bones.Length - 1; i >= 0; i--)
        {
            bones[i] = curr;
            startRotBone[i] = curr.rotation;
            //end bone
            if (i == bones.Length - 1)
                startDirSucc[i] = target.position - curr.position;
            else
            {
                startDirSucc[i] = bones[i + 1].position - curr.position;
                boneLengths[i] = startDirSucc[i] .magnitude;
                totalLength += boneLengths[i];
            }
            
            curr = curr.parent;
        }
    }

    void LateUpdate()
    {
        Resolve();
    }

    void Resolve()
    {
        if (target)
        {
            //get
            for (int i = 0; i < bones.Length; i++)
                bonePositions[i] = bones[i].position;

            var rootRot = (bones[0].parent != null) ? bones[0].parent.rotation : Quaternion.identity;
            var rootRotDiff = rootRot * Quaternion.Inverse(startRotRoot);
            

            //if target is out of chain's reach
            if ((target.position - bones[0].position).sqrMagnitude >= totalLength * totalLength)
            {
                Vector3 dir = (target.position - bonePositions[0]).normalized;
                //each bone will still be the same distance away from the bone before it
                //but now in the direction towards the target
                for (int i = 1; i < bonePositions.Length; i++)
                    bonePositions[i] = bonePositions[i - 1] + (boneLengths[i - 1] * dir);
            }
            else
            {
                //run the algorithm iteration # of times
                for (int i = 0; i < iterations; i++)
                {
                    //backwards part of the algorithm
                    //set end effector to target, readjust all other bones
                    for (int j = bonePositions.Length - 1; j > 0; j--)
                    {
                        //set end effector to target position
                        if (j == bonePositions.Length - 1)
                            bonePositions[j] = target.position;
                        //set jth bone to be the same distance away from the bone ahead of it, but now 
                        //in the new position that would make sense (since all the bones are changing starting
                        //from the end effector)
                        else
                            bonePositions[j] = bonePositions[j + 1] + (bonePositions[j] - bonePositions[j + 1]).normalized * boneLengths[j];
                    }

                    //forward part
                    //since backward part ends up moving the root bone away from original space
                    //move root bone back, and move all other bones back accordingly
                    for (int j = 1; j < bonePositions.Length; j++)
                        bonePositions[j] = bonePositions[j - 1] + (bonePositions[j] - bonePositions[j - 1]).normalized * boneLengths[j - 1];



                    //by following the above algorithm the root bone would be moved and then moved back
                    //the shortcut is the for loops don't involve the root bone (bonePositions[0] is always skipped)
                    //because it never really moves anyway

                    //the last bone is the the end effector, if it's close enough, break
                    if ((bonePositions[bonePositions.Length - 1] - target.position).sqrMagnitude < closeEnough * closeEnough)
                        break;
                }
            }

            //all bones move towards pole
            for (int i = 1; i < bonePositions.Length - 1; i++)
            {
                var plane = new Plane(bonePositions[i + 1] - bonePositions[i - 1], bonePositions[i - 1]);
                var projectedPole = plane.ClosestPointOnPlane(pole.position);
                var projectedBone = plane.ClosestPointOnPlane(bonePositions[i]);
                var angle = Vector3.SignedAngle(projectedBone - bonePositions[i - 1], projectedPole - bonePositions[i - 1], plane.normal);
                bonePositions[i] = Quaternion.AngleAxis(angle, plane.normal) * (bonePositions[i] - bonePositions[i - 1]) + bonePositions[i - 1];
            }

            //set
            for (int i = 0; i < bonePositions.Length; i++)
            {
                if (i == bonePositions.Length - 1)
                    bones[i].rotation = target.rotation * Quaternion.Inverse(startRotTarget) * startRotBone[i];
                else
                    bones[i].rotation = Quaternion.FromToRotation(startDirSucc[i], bonePositions[i + 1] - bonePositions[i]) * startRotBone[i];
                bones[i].position = bonePositions[i];
            }
                
 
        }
    }
}
