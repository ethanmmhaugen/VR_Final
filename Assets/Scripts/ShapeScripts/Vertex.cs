using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;

public class Vertex : Grabbable
{

    // Start is called before the first frame update
    void Start()
    {
        hitbox = gameObject.GetComponent<SphereCollider>();
    }

    // Update is called once per frame
    public override Grabbable Grab(Transform grabbingTrans)
    {
        gameObject.transform.position = grabbingTrans.position;
        return gameObject.GetComponent<Vertex>();
    }

    public override Grabbable Pinch()
    {
        GameObject newVert = Instantiate(gameObject);
        Vertex grbComponent = newVert.GetComponent<Vertex>();
        GameObject lineObj = new GameObject("NewLine");

        Line lineComponent = lineObj.GetOrAddComponent<Line>();
        lineComponent.vert1 = gameObject;
        lineComponent.vert2 = newVert;

        return grbComponent;
    }

}
