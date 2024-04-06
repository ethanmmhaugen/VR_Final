using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Grabbable : MonoBehaviour
{

    public abstract Grabbable Grab(Transform grabbingTrans);
    public abstract Grabbable Pinch();
    protected  Collider hitbox;

}
