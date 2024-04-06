using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shape : Grabbable
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public override Grabbable Pinch()
    {
        throw new System.NotImplementedException();
    }
    public override Grabbable Grab(Transform grabbingTrans)
    {
        throw new System.NotImplementedException();
    }
}
