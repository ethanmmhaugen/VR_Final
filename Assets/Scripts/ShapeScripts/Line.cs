using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Mathematics;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class Line : Grabbable
{
    // Start is called before the first frame update
    public GameObject vert1;
    public GameObject vert2;
    private LineRenderer lr;
    private Vector3[] vertPositions = new Vector3[2];
    private float lineWidth = 0.03f;
    private CapsuleCollider localHitbox;
    public bool isPartOfFace = false;
    private bool beingGrabbed = false;
    private IEnumerator grabCR;
    private bool grabbableCoroutineRunning = false;
    void Start()
    {
        if(vert1 is null || vert2 is null) {
            Debug.LogError("Vert 1 or Vert 2 is null in Line Object");
        }
        SetupLinerenderer();
        gameObject.tag = "Drawable";
        DebugText.log("Setting Tag to drawable in line");
        localHitbox = gameObject.AddComponent<CapsuleCollider>();

    }

    private void SetupLinerenderer() {
        lr = gameObject.AddComponent<LineRenderer>();
        lr.widthMultiplier = lineWidth;
        lr.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        lr.material.color = Color.green;
        
        lr.positionCount = 2;

        lr.enabled = true;
    }

    private void UpdateLinePositions() {
        vertPositions[0] = vert1.transform.position;
        vertPositions[1] = vert2.transform.position;
        lr.SetPositions(vertPositions);
    }
    // Update is called once per frame
    void Update()
    {
        if(vert1.GetInstanceID() == vert2.GetInstanceID() || vert1 == null || vert2 == null) {
            Destroy(gameObject);
        }
        UpdateLinePositions();
        UpdateCollider();

    }
    private void UpdateCollider() {
        
        Vector3 v1Position = vert1.transform.position;
        Vector3 v2Position = vert2.transform.position;
        

        localHitbox.center = Vector3.zero;

        if(!beingGrabbed) {
            gameObject.transform.position = (v1Position + v2Position) / 2f;

        }
        // beingGrabbed = false;
        gameObject.transform.right = (v2Position - v1Position);

        float height = math.sqrt(math.pow(v1Position.x - v2Position.x,2) + math.pow(v1Position.y - v2Position.y,2)+ math.pow(v1Position.z - v2Position.z,2));
        localHitbox.height = height * 0.75f;
        localHitbox.radius = lineWidth * 0.85f;
        localHitbox.direction = 0;

        hitbox = localHitbox;
    }

    public override Grabbable Pinch()
    {
        GameObject newVert = Instantiate(vert1);
        GameObject newLine1 = new GameObject("NewLine1");
        GameObject newLine2 = new GameObject("NewLine2");
        Line nl1Line = newLine1.AddComponent<Line>();

        nl1Line.vert1 = vert1;
        nl1Line.vert2 = newVert;

        Line nl2Line = newLine2.AddComponent<Line>();
        nl2Line.vert1 = vert2;
        nl2Line.vert2 = newVert;
        nl2Line.isPartOfFace = true;
        nl1Line.isPartOfFace = true;
        isPartOfFace = true;

        //In theory, adds the lines to the new vertex's list
        Vertex newVertComp = newVert.GetComponent<Vertex>();
        newVertComp.lines.Clear();
        newVertComp.lines.Add(nl1Line);
        newVertComp.lines.Add(nl2Line);

        //In theory, adds the new lines to the existing vertices' lists
        vert1.GetComponent<Vertex>().lines.Add(nl1Line);
        vert2.GetComponent<Vertex>().lines.Add(nl2Line);
        
        return newVert.GetComponent<Vertex>();
    }

    IEnumerator ResetGrabbed(bool spawnShape) {
        grabbableCoroutineRunning = true;
        yield return new WaitForSeconds(0.2f);
        grabbableCoroutineRunning = false;
        beingGrabbed = false;
    }
    public override Grabbable Grab(Transform grabbingTrans)
    {   
        if(grabCR != null && grabbableCoroutineRunning) {
            StopCoroutine(grabCR);
        }

        
        beingGrabbed = true;
        
        bool shouldSpawnShape = false;
        if(isPartOfFace) {
            Vector3 direction = grabbingTrans.position - gameObject.transform.position;
            vert1.transform.position += direction;
            vert2.transform.position += direction;
            gameObject.transform.position = grabbingTrans.transform.position;

        } else {
            shouldSpawnShape = true;
            GameObject dup = new GameObject("DupeLine");
            dup.transform.position = gameObject.transform.position;
            dup.transform.rotation = gameObject.transform.rotation;

            GameObject newLine1 = new GameObject("NewLine1");
            GameObject newLine2 = new GameObject("NewLine2");
            
            GameObject newVert1 = Instantiate(vert1);
            GameObject newVert2 = Instantiate(vert2);

            newVert1.transform.position = vert1.transform.position;
            newVert2.transform.position = vert2.transform.position;

            Line dupLine = dup.AddComponent<Line>();
            dupLine.vert1 = vert1;
            dupLine.vert2 = vert2;
            vert1 = newVert1;
            vert2 = newVert2;


            Line nl1Line = newLine1.AddComponent<Line>();

            nl1Line.vert1 = dupLine.vert1;
            nl1Line.vert2 = newVert1;

            Line nl2Line = newLine2.AddComponent<Line>();
            nl2Line.vert1 = dupLine.vert2;
            nl2Line.vert2 = newVert2;

            gameObject.transform.position = grabbingTrans.transform.position;
            isPartOfFace = true;
            nl1Line.isPartOfFace = true;
            nl2Line.isPartOfFace = true;
            dupLine.isPartOfFace = true;

            //Add lines to new vertices
            Vertex newVert1Comp = newVert1.GetComponent<Vertex>();
            Vertex newVert2Comp = newVert2.GetComponent<Vertex>();
            newVert1Comp.lines.Clear();
            newVert2Comp.lines.Clear();
            newVert1Comp.lines.Add(gameObject.GetComponent<Line>());
            newVert2Comp.lines.Add(gameObject.GetComponent<Line>());
            newVert1Comp.lines.Add(nl1Line);
            newVert2Comp.lines.Add(nl2Line);

            //Add lines to old vertices
            Vertex dupVert1Comp = dupLine.vert1.GetComponent<Vertex>();
            Vertex dupVert2Comp = dupLine.vert2.GetComponent<Vertex>();
            dupVert1Comp.lines.Clear();
            dupVert2Comp.lines.Clear();
            dupVert1Comp.lines.Add(dupLine.GetComponent<Line>());
            dupVert2Comp.lines.Add(dupLine.GetComponent<Line>());
            dupVert1Comp.lines.Add(nl1Line);
            dupVert2Comp.lines.Add(nl2Line);
        }

        grabCR = ResetGrabbed(shouldSpawnShape);
        StartCoroutine(grabCR);
        
        return gameObject.GetComponent<Line>();
    }
    void OnDestroy() {
        ClearSelfFromVert(vert2.GetComponent<Vertex>());
        ClearSelfFromVert(vert1.GetComponent<Vertex>());
    }

    void ClearSelfFromVert(Vertex vert) {
        
        for(int i=0; i < vert.lines.Count; i ++) {
            if(vert.lines[i].GetInstanceID() == GetInstanceID()) {
                vert.lines.RemoveAt(i);
                break;
            }
        }
    }
    
}
