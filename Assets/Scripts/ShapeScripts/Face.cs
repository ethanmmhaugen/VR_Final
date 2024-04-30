using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Face : Grabbable
{
    public List<Line> lines = new List<Line>();
    public List<Vertex> vertices = new List<Vertex>();
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private Vector3 midpoint;
    public bool isPartOfShape = false;
    private List<int> triangles;
    public float colliderRadius = 0.05f;
    private Mesh mesh;
    private  GameObject debugSphere;
    //TRIGGERS to make a face is pinch/grab a line 
    private SphereCollider localHitbox;
    // Start is called before the first frame update
    void Start()
    {
        if(lines.Count < 3 || vertices.Count < 3 || lines.Count != vertices.Count) {
            Debug.LogError("illegal face");
        }
        recalcMidpoint();
        SetupMeshRenderer();
        gameObject.tag = "Drawable";
        debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        debugSphere.transform.position = midpoint;
        debugSphere.transform.parent = gameObject.transform;
        debugSphere.GetComponent<MeshRenderer>().material = meshRenderer.material;
        debugSphere.GetComponent<MeshRenderer>().material.color = meshRenderer.material.color;
        debugSphere.transform.localScale = new Vector3(colliderRadius * 2,colliderRadius * 2,colliderRadius * 2);
        localHitbox = gameObject.AddComponent<SphereCollider>();
        localHitbox.radius = colliderRadius;
        // localHitbox.transform.position = debugSphere.transform.position;
        localHitbox.center = midpoint;
    }

    public bool IsSubFace(Face f) {
        if(f == gameObject.GetComponent<Face>()) return false; //self is not a subface of self
        foreach(Vertex v in f.vertices) {
            if(!vertices.Contains(v)) {
                return false;
            }
        }
        return true;
    }
    // Update is called once per frame
    void Update()
    {
        for(int i=0; i < vertices.Count; i ++) {
            if(vertices[i] == null){
                Destroy(gameObject);
            }
        }
        for(int i=0; i < lines.Count; i ++) {
            if(lines[i] == null){
                Destroy(gameObject);
            }
        }
        if(vertices.Count() != vertices.Distinct().Count()) {
            Face me = gameObject.GetComponent<Face>();
            foreach(Vertex vert in vertices) {
                List<int> idxToRemove = new List<int>();
                for(int i=0; i < vert.faces.Count; i ++) {
                    idxToRemove.Add(i);
                }
                foreach(int idx in idxToRemove) {
                    vert.faces.RemoveAt(idx);
                    if(vert.faces.Count == 0) {
                        foreach(Line l in vert.lines) {
                            l.isPartOfFace = false;
                        }
                    }
                }
            }
            // Destroy(gameObject);
        }
        reRender();
        
        debugSphere.transform.position = midpoint;
        localHitbox.center = midpoint;
        // localHitbox.transform.position = debugSphere.transform.position;
    }
    public override Grabbable Pinch()
    {
        GameObject newVert = Instantiate(vertices[0].gameObject);
        Vertex newVertComponent = newVert.GetComponent<Vertex>();
        newVert.transform.position = midpoint;
        newVertComponent.lines.Clear();
        newVertComponent.faces.Clear();
        Dictionary<GameObject,GameObject> seenVerts = new Dictionary<GameObject,GameObject>();

        foreach(Line l in lines) {
            GameObject l1go;
            GameObject l2go;
            Line l1Comp;
            Line l2Comp;
            if(seenVerts.ContainsKey(l.vert1)) {
                l1go = seenVerts[l.vert1];
                l1Comp = seenVerts[l.vert1].GetComponent<Line>();
            } else {
                l1go = new GameObject("NewLine");
                l1Comp = l1go.AddComponent<Line>();
                l1Comp.vert1 = newVert;
                l1Comp.vert2 = l.vert1;
                seenVerts.Add(l.vert1,l1go);
            }

            if(seenVerts.ContainsKey(l.vert2)) {
                l2go = seenVerts[l.vert2];
                l2Comp = seenVerts[l.vert2].GetComponent<Line>();
            } else {
                l2go = new GameObject("NewLine");
                l2Comp = l2go.AddComponent<Line>();
                l2Comp.vert1 = newVert;
                l2Comp.vert2 = l.vert2;
                seenVerts.Add(l.vert2,l2go);
            }
            if(!newVertComponent.lines.Contains(l1Comp)) {
                newVertComponent.lines.Add(l1Comp);

            }
            if(!newVertComponent.lines.Contains(l2Comp)) {
                newVertComponent.lines.Add(l2Comp);

            }

            GameObject faceObj = new GameObject("NewFace");
            // faceObj.transform.position = gameObject.transform.position;
            Face faceComponent = faceObj.GetOrAddComponent<Face>();
            faceComponent.vertices.Add(newVertComponent);
            faceComponent.vertices.Add(l.vert1.GetComponent<Vertex>());
            faceComponent.vertices.Add(l.vert2.GetComponent<Vertex>());
            
            if(!l.vert1.GetComponent<Vertex>().faces.Contains(faceComponent)) {
                l.vert1.GetComponent<Vertex>().faces.Add(faceComponent);
            }
            if(!l.vert2.GetComponent<Vertex>().faces.Contains(faceComponent)) {
                l.vert2.GetComponent<Vertex>().faces.Add(faceComponent);
            }
            if(!l.vert1.GetComponent<Vertex>().lines.Contains(l1Comp)) {
                l.vert1.GetComponent<Vertex>().lines.Add(l1Comp);
            }
            if(!l.vert2.GetComponent<Vertex>().lines.Contains(l2Comp)) {
                l.vert2.GetComponent<Vertex>().lines.Add(l2Comp);
            }
            faceComponent.lines.Add(l);
            faceComponent.lines.Add(l1Comp);
            faceComponent.lines.Add(l2Comp);
            faceComponent.isPartOfShape = true;
            l1Comp.isPartOfFace = true;
            l2Comp.isPartOfFace = true;
            l.isPartOfFace = true;
            newVertComponent.faces.Add(faceComponent);

        }
        DebugText.log("Pinching Face");
        isPartOfShape = true;
        // throw new System.NotImplementedException();
        return newVertComponent;
    }
    
    public override Grabbable Grab(Vector3 grabbingTrans)
    {
        Vector3 direction = grabbingTrans - midpoint;
        if(isPartOfShape) {
            foreach(Vertex v in vertices) {
                v.Grab(v.transform.position + direction);
            }
        } else{
            List<List<int>> lineIndexConnections = new List<List<int>>(); //its about to get wacky and wild in here
            GameObject originalFaceGo = new GameObject("NewFace");
            Face originalFaceComp = originalFaceGo.AddComponent<Face>();
            for(int i=0; i < vertices.Count; i ++) {
                GameObject newVert = Instantiate(vertices[i].gameObject);
                Vertex newVertComp = newVert.GetComponent<Vertex>();
                newVertComp.lines.Clear();
                newVertComp.faces.Clear();
                originalFaceComp.vertices.Add(newVertComp);
            }
            for(int i=0; i < lines.Count; i ++) { // Duplicate myself and leave it where they grabbed initially
                int v1Idx = vertices.IndexOf(lines[i].vert1.GetComponent<Vertex>());
                int v2Idx = vertices.IndexOf(lines[i].vert2.GetComponent<Vertex>());
                GameObject lineGo = new GameObject();
                Line lineComp = lineGo.AddComponent<Line>();
                lineComp.vert1 = originalFaceComp.vertices[v1Idx].gameObject;
                lineComp.vert2 = originalFaceComp.vertices[v2Idx].gameObject;
                lineComp.isPartOfFace = true;
                originalFaceComp.vertices[v1Idx].lines.Add(lineComp);
                originalFaceComp.vertices[v2Idx].lines.Add(lineComp);
                originalFaceComp.lines.Add(lineComp);
                originalFaceComp.isPartOfShape = true;
            }

            Dictionary<GameObject,GameObject> verticalLineMapper = new Dictionary<GameObject,GameObject>();
            for(int i=0; i < vertices.Count; i ++) {
                GameObject newLine = new GameObject("NewLine");
                Line newLineComp = newLine.AddComponent<Line>();
                newLineComp.vert1 = vertices[i].gameObject;
                newLineComp.vert2 = originalFaceComp.vertices[i].gameObject;
                vertices[i].lines.Add(newLineComp);
                originalFaceComp.vertices[i].lines.Add(newLineComp);
                newLineComp.isPartOfFace = true;
                verticalLineMapper.Add(vertices[i].gameObject,newLine);
            }
            for(int i=0; i < lines.Count; i ++) {
                GameObject faceGo = new GameObject("NewFace");
                Face faceComp = faceGo.AddComponent<Face>();
                int v1Idx = vertices.IndexOf(lines[i].vert1.GetComponent<Vertex>());
                int v2Idx = vertices.IndexOf(lines[i].vert2.GetComponent<Vertex>());
                faceComp.vertices.Add(vertices[v1Idx]);
                faceComp.vertices.Add(vertices[v2Idx]);
                faceComp.vertices.Add(originalFaceComp.vertices[v1Idx]);
                faceComp.vertices.Add(originalFaceComp.vertices[v2Idx]);
                faceComp.lines.Add(lines[i]);
                faceComp.lines.Add(originalFaceComp.lines[i]);
                faceComp.lines.Add(verticalLineMapper[lines[i].vert1].GetComponent<Line>());
                faceComp.lines.Add(verticalLineMapper[lines[i].vert2].GetComponent<Line>());
                faceComp.isPartOfShape = true;
                vertices[v1Idx].faces.Add(faceComp);
                vertices[v2Idx].faces.Add(faceComp);
                originalFaceComp.vertices[v1Idx].faces.Add(faceComp);
                originalFaceComp.vertices[v2Idx].faces.Add(faceComp);

            }
            isPartOfShape = true;
            // originalFaceComp.GetComponent<MeshRenderer>().material.color = Color.red; // This line errors but it is the only way that i got it to work... Otherwise the face thats being grabbed is incorrect
            return originalFaceGo.GetComponent<Face>();
        }


        return gameObject.GetComponent<Face>();
    }

    private void SetupMeshRenderer(){
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        meshRenderer.material.color = UnityEngine.Random.ColorHSV();
        meshFilter = gameObject.AddComponent<MeshFilter>();
        reRender();
    }

    public void reRender(){
        recalcMidpoint();
        drawTriangles();
    }

    public void recalcMidpoint(){
        float x = 0;
        float y = 0;
        float z = 0;
        foreach(Vertex vertex in vertices){
            if(vertex != null) {
                x += vertex.transform.position.x;
                y += vertex.transform.position.y;
                z += vertex.transform.position.z;

            }
        }

        midpoint = new Vector3(x/vertices.Count, y/vertices.Count, z/vertices.Count);

    }

    public int findIndex(Vector3[] verts, Vertex vertex){
        for(int i = 0; i<verts.Length; i++){
            if(vertex.transform.position == verts[i]){
                return i;
            }
        }
        DebugText.log("Hitting error Face ln 82");
        return -1;
    }
    public void drawTriangles(){
        triangles = new List<int>();
        Vector3[] vertpos = new Vector3[vertices.Count+1];
        vertpos[0] = midpoint;
        for(int i = 0; i<vertices.Count; i++){
            if(vertices[i] == null) return; //Need this to stop errors idk anymore man
            vertpos[i+1] = vertices[i].transform.position;
        }

        for(int i = 0; i<lines.Count; i++){
            triangles.Add(findIndex(vertpos, lines[i].vert1.GetComponent<Vertex>()));
            triangles.Add(findIndex(vertpos, lines[i].vert2.GetComponent<Vertex>()));
            triangles.Add(0);

            triangles.Add(0);
            triangles.Add(findIndex(vertpos, lines[i].vert2.GetComponent<Vertex>()));
            triangles.Add(findIndex(vertpos, lines[i].vert1.GetComponent<Vertex>()));
            
        }
        mesh = new Mesh();
        mesh.vertices = vertpos;
        mesh.triangles = triangles.ToArray();
        meshFilter.mesh = mesh;
    }
    void OnDestroy() {
        foreach(Vertex vert in vertices) {
            if( vert != null ){
                vert.faces.Remove(gameObject.GetComponent<Face>());
            }
        }
    }

}
