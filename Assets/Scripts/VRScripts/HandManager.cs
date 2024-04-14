using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class HandManager : MonoBehaviour
{
    public OVRHand domHand;

    public GameObject vertexPrefab;
    public float colliderRadius = 0.05f;
    public bool isRightHand = true;
    private bool _hasPinched;
    private bool _isIndexFingerPinching;
    private float _pinchStrength;
    private OVRHand.TrackingConfidence _confidence;

    public Material debugMaterial;
    private SphereCollider hitDetector;
    private GameObject debugSphere;
    private Grabbable currGrabbable;
    private IEnumerator vertSpawner;
    private bool spawningVertex = false;
    private bool isDelayingHitbox = false;
    private bool wasFingerPinching = false;
    // Start is called before the first frame update
    void Start()
    {
        hitDetector = gameObject.AddComponent<SphereCollider>();
        hitDetector.transform.parent = domHand.transform;
        hitDetector.radius = colliderRadius;
        DrawColliders dc = hitDetector.AddComponent<DrawColliders>();
        hitDetector.isTrigger = true;
        Rigidbody rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        _pinchStrength = domHand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
        _isIndexFingerPinching = domHand.GetFingerIsPinching(OVRHand.HandFinger.Index);
        _confidence = domHand.GetFingerConfidence(OVRHand.HandFinger.Index);
        
        // DebugText.log(_confidence.ToString());
        // DebugText.log($"{(isRightHand ? "Right": "Left")} hand: {_isIndexFingerPinching}");
        if (_isIndexFingerPinching || wasFingerPinching) {
                //CREATE VERTEX
                if(isRightHand) {
                    // DebugText.log("Got Here");
                    if(!spawningVertex && currGrabbable == null) {
                        // DebugText.log("Got Here as well");
                        vertSpawner = spawnVertex();
                        StartCoroutine(vertSpawner);

                    } else if(currGrabbable != null) {
                        StopCoroutine(vertSpawner);
                        // DebugText.log("Stopping Coroutine");
                        spawningVertex = false;
                    }

                }
            
            // DebugText.log("Pinching");
            gameObject.transform.position = domHand.PointerPose.position;
            if(debugSphere != null) {
                debugSphere.transform.position = domHand.PointerPose.position;
            }

            if(!hitDetector.enabled) {
                SpawnHitBox(domHand);
            }
            
            if(currGrabbable != null) {
                currGrabbable.Grab(domHand.PointerPose);
            }


            

        } else {
            // DebugText.log("Not pinching");
            if(_confidence == OVRHand.TrackingConfidence.High) {
                DestroyHitbox();
                currGrabbable = null;
                if(isRightHand) {

                    // DebugText.log("Resetting Curr Grabbable to Null");
                }
            }

        }
        wasFingerPinching = _isIndexFingerPinching;
    }
    
    IEnumerator spawnVertex(){
        spawningVertex = true;
        yield return new WaitForSeconds(0.1f);
        GameObject newVert = Instantiate(vertexPrefab);

        newVert.transform.position = domHand.PointerPose.position;
        spawningVertex = false;
        vertSpawner = null;
    }
    void SpawnHitBox(OVRHand theHand) {
        if(!isDelayingHitbox) {
            hitDetector.enabled = true;

            
            if(debugMaterial != null) {

                debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                debugSphere.GetComponent<SphereCollider>().enabled = false;
                debugSphere.transform.localScale = new Vector3(colliderRadius * 2,colliderRadius * 2,colliderRadius * 2);
                debugSphere.GetComponent<MeshRenderer>().material = debugMaterial;
                debugSphere.transform.position = theHand.PointerPose.position;
                debugSphere.transform.parent = theHand.transform;
            }

        }
    
      

        hitDetector.transform.position = theHand.PointerPose.position;
        

    }
    void DestroyHitbox() {
        if(debugSphere != null) {
            Destroy(debugSphere);
            debugSphere = null;
        }
        hitDetector.enabled = false;
    }


    IEnumerator delayHitboxChecking(float time) {
        // DebugText.log("Disabling Hitbox");
        hitDetector.enabled = false;
        isDelayingHitbox = true;
        yield return new WaitForSeconds(time);
        hitDetector.enabled = true;
        isDelayingHitbox = false;
        // DebugText.log("Enabling Hitbox");
    }

    void CleanLines(Vertex theVert) {
        List<int> linesToRemove = new List<int>();
        HashSet<int> seenVerts = new HashSet<int>();
        HashSet<Line> seenLines = new HashSet<Line>();

        for(int i =0; i < theVert.lines.Count; i ++) {
            Line currLine  = theVert.lines[i];
            DebugText.log($"in lines: {currLine.gameObject.name}");
            if(seenLines.Contains(currLine)) {
                linesToRemove.Add(i);
                continue;
            }
            seenLines.Add(currLine);
            
            if (currLine.vert1.GetInstanceID() == currLine.vert2.GetInstanceID()) {
                linesToRemove.Add(i);
            } else if(currLine.vert1.GetInstanceID() == theVert.GetInstanceID()){
                if(seenVerts.Contains(currLine.vert2.GetInstanceID())) {
                    linesToRemove.Add(i);
                } else {
                    seenVerts.Add(currLine.vert2.GetInstanceID());
                }
            } else if(currLine.vert2.GetInstanceID() == theVert.GetInstanceID()) {
                if(seenVerts.Contains(currLine.vert1.GetInstanceID())) {
                    linesToRemove.Add(i);
                } else {
                    seenVerts.Add(currLine.vert1.GetInstanceID());
                }
            } else {
                DebugText.log("we fucked up");
            }
        }

        DebugText.log($"seenVerts size: {seenVerts.Count} linesToRemoveSize: {linesToRemove.Count}");
        for(int i=0; i < linesToRemove.Count; i ++) {
            Destroy(theVert.lines[linesToRemove[i]].gameObject);
            DebugText.log($"Removed line at idx {linesToRemove[i]}. {theVert.lines[linesToRemove[i]].gameObject.name}" );

            // theVert.lines.RemoveAt(linesToRemove[i]);

        }

    }
    bool DoesLineExist(Vertex v1, Vertex v2, List<Line> lines) {
        for(int i=0; i < lines.Count; i++) {
            if((lines[i].vert1 == v1.gameObject && lines[i].vert2 == v2.gameObject) ||(lines[i].vert1 == v2.gameObject && lines[i].vert2 == v1.gameObject)) {
                // DebugText.log("Found Duplicate line");
                // DebugText.log($"V1: {v1.gameObject.name}");
                return true;
            }
        }
        DebugText.log("Did Not Find Duplicate Line");
        return false;
    }
    void OnTriggerEnter(Collider obj) {
        Grabbable grb = obj.gameObject.GetComponent<Grabbable>();
        // DebugText.log($"Entered Collision {obj.gameObject.name}");
        //MIGHT HAVE TO IMPLEMENT != or == for grabbable
        // Debug.Log($"grb: {grb.gameObject}");
        // Debug.Log($"currGrabbable{currGrabbable.gameObject}");
        // DebugText.log($"Grb: {grb.GetType().ToString()}");
        
        if (grb != null && grb.GetType() == typeof(Vertex) && currGrabbable != null && currGrabbable.GetType() == typeof(Vertex) && grb.gameObject.GetInstanceID() != currGrabbable.gameObject.GetInstanceID()) {
            List<Line> otherLines = grb.GetComponent<Vertex>().lines;
            DebugText.log($"{otherLines.Count} {Time.time}");
            for(int i = 0; i < otherLines.Count; i++){
                Line currLine = otherLines[i];
                
                if(currLine.vert1 == grb.gameObject){
                    currLine.vert1 = currGrabbable.gameObject;
                    if(!DoesLineExist(currLine.vert1.GetComponent<Vertex>(),currLine.vert2.GetComponent<Vertex>(),currGrabbable.gameObject.GetComponent<Vertex>().lines)) {
                        currGrabbable.GetComponent<Vertex>().lines.Add(currLine);
                    } else {
                        Destroy(currLine.gameObject);
                    }
                }
                else if(currLine.vert2 == grb.gameObject){
                    currLine.vert2 = currGrabbable.gameObject;
                    if(!DoesLineExist(currLine.vert1.GetComponent<Vertex>(),currLine.vert2.GetComponent<Vertex>(),currGrabbable.gameObject.GetComponent<Vertex>().lines)) {
                        currGrabbable.GetComponent<Vertex>().lines.Add(currLine);
                    } else {
                        Destroy(currLine.gameObject);
                    }
                }
                
                else{
                    DebugText.log("Unexplained Error: Hand Manager Line 217");
                }

            }
            Destroy(grb.gameObject);
            // CleanLines(currGrabbable.gameObject.GetComponent<Vertex>());
            grb = currGrabbable;
        }

        if(grb != null) {
            if(isRightHand) { // pinch for right
                if(currGrabbable == null) {
                    currGrabbable = grb.Pinch();
                    
                }
                currGrabbable.Grab(domHand.PointerPose);

            } else { //Grab for left hand
                grb.Grab(domHand.PointerPose);
                currGrabbable = grb;
            }

            StartCoroutine(delayHitboxChecking(1f));

        } else {
            // DebugText.log("We Cannot Grab this");
        }
    }

    void OnTriggerStay(Collider obj) {

    }
    
    

}
