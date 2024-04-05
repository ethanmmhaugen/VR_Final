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
    public bool isPartOfShape = false;
    private bool beingGrabbed = false;
    private IEnumerator grabCR;
    private bool grabbableCoroutineRunning = false;
    void Start()
    {
        if(vert1 is null || vert2 is null) {
            Debug.LogError("Vert 1 or Vert 2 is null in Line Object");
        }
        SetupLinerenderer();

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
        throw new System.NotImplementedException();
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
        if(isPartOfShape) {
            gameObject.transform.position = grabbingTrans.transform.position;

        } else {
            shouldSpawnShape = true;
            GameObject dup = new GameObject("DupeLine");
            dup.transform.position = gameObject.transform.position;
            dup.transform.rotation = gameObject.transform.rotation;

            GameObject newLine1 = new GameObject("NewLine1");
            GameObject newLine2 = new GameObject("NewLine2");
            
            GameObject newVert1 = Instantiate(vert1,gameObject.transform);
            GameObject newVert2 = Instantiate(vert2,gameObject.transform);

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
            isPartOfShape = true;
            nl1Line.isPartOfShape = true;
            nl2Line.isPartOfShape = true;
            dupLine.isPartOfShape = true;

        }

        grabCR = ResetGrabbed(shouldSpawnShape);
        StartCoroutine(grabCR);
        
        return gameObject.GetComponent<Line>();
    }
}
