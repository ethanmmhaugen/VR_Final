using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

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
        
        DebugText.log(_confidence.ToString());
        DebugText.log($"{(isRightHand ? "Right": "Left")} hand: {_isIndexFingerPinching}");
        if (_isIndexFingerPinching || wasFingerPinching) {
                if(isRightHand) {
                    DebugText.log("Got Here");
                    if(!spawningVertex && currGrabbable == null) {
                        DebugText.log("Got Here as well");
                        vertSpawner = spawnVertex();
                        StartCoroutine(vertSpawner);

                    } else if(currGrabbable != null) {
                        StopCoroutine(vertSpawner);
                        DebugText.log("Stopping Coroutine");
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

            currGrabbable.Grab(domHand.PointerPose);


            

        } else {
            // DebugText.log("Not pinching");
            if(_confidence == OVRHand.TrackingConfidence.High) {
                DestroyHitbox();
                currGrabbable = null;
                if(isRightHand) {

                    DebugText.log("Resetting Curr Grabbable to Null");
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

    void OnTriggerEnter(Collider obj) {
        Grabbable grb = obj.gameObject.GetComponent<Grabbable>();
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
