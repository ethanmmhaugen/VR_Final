using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabberExample : MonoBehaviour
{
    // Start is called before the first frame update
    public Grabbable objToGrab;
    private Grabbable currGrabbed;
    public Grabbable theLine;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.G)) {
            StartCoroutine(GrabObj(objToGrab));
        }

        if (Input.GetKeyDown(KeyCode.P)) {
            currGrabbed = objToGrab.Pinch();
            StartCoroutine(GrabObj(currGrabbed));
        }
        if(Input.GetKeyDown(KeyCode.L)) {
            Grabbable movableLine = theLine.Grab(gameObject.transform.position);
            StartCoroutine(GrabLine(movableLine));
        }
    }

    IEnumerator GrabObj(Grabbable theObj) {
        for(int i=0; i < 10; i++) {
            Vector3 currPos = gameObject.transform.position;
            currPos.x += i / 10f;
            currPos.y += i /20f;
            currPos.z += i/25f;
            gameObject.transform.position = currPos;
            theObj.Grab(gameObject.transform.position);
            yield return new WaitForSeconds(0.1f);
        }
    }
    IEnumerator GrabLine(Grabbable line) {
        for(int i=-10; i < 0; i ++) {
            Vector3 currPos = gameObject.transform.position;
            currPos.x += i / 25f;
            currPos.y += i / 5f;
            currPos.z += i/ 15f;
            gameObject.transform.position = currPos;
            line.Grab(gameObject.transform.position);
            yield return new WaitForSeconds(0.1f);
        }
    }
    
}
