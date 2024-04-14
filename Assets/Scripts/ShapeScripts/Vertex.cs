using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;

public class Vertex : Grabbable
{
    public List<Line> lines = new List<Line>();
    public bool allowOverwrite = true;

    // Start is called before the first frame update
    void Start()
    {
        hitbox = gameObject.GetComponent<SphereCollider>();
        gameObject.tag = "Drawable";
        DebugText.log("Setting Tag to drawable in vertex");
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
        grbComponent.lines.Clear();

        GameObject lineObj = new GameObject("NewLine");

        Line lineComponent = lineObj.GetOrAddComponent<Line>();
        lines.Add(lineComponent);
        lineComponent.vert1 = gameObject;
        lineComponent.vert2 = newVert;
        newVert.GetComponent<Vertex>().lines.Add(lineComponent);

        return grbComponent;
    }
    void OnCollisionEnter(Collision other) {

        // Vertex otherVert = other.gameObject.GetComponent<Vertex>();
        // if(otherVert != null) {
        //     otherVert.allowOverwrite = false;
        // } else {
        //     // DebugText.log("Not a vertex in vert collision");
        // }

        // if(allowOverwrite && otherVert != null) {
                
        //     DebugText.log("Entered if statement for 2 vertices");
        //     List<Line> otherLines = otherVert.lines;
        //     for(int i = 0; i < otherLines.Count; i++){
        //         Line currLine = otherLines[i];
        //         if(currLine.vert1 == otherVert.gameObject){
        //             currLine.vert1 = gameObject;
        //             lines.Add(currLine);
        //         }
        //         else if(currLine.vert2 == otherVert.gameObject){
        //             currLine.vert2 = gameObject;
        //             lines.Add(currLine);
        //         }
        //         else{
        //             DebugText.log("Unexplained Error: Vertex Line 58");
        //         }

        //     }

        //     Destroy(other.gameObject);
        //     // for(int i = 0; i<currLines.Count; i++){
        //     //     Line currline = currLines[i];
            
        //     // }

        //         // //delete vertex
        //         // Destroy(grb.gameObject);

            
        // }
        // DebugText.log();
        
    }

}
