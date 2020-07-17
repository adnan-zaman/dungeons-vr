using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Grappling hook can hook to all Hookables
 */ 

[RequireComponent(typeof(Rigidbody))]
public class Hookable : MonoBehaviour
{
  //literally nothing happens. its just like a tag that allows the grappling hook
  //to identify between what it can and can't attach to
  //also it's a superclass
}
