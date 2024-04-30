using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;

public class Vertex : Grabbable
{
    public List<Line> lines = new List<Line>();
    public List<Face> faces = new List<Face>();
    public bool allowOverwrite = true;
    public bool isFace = false;

    // Start is called before the first frame update
    void Start()
    {
        hitbox = gameObject.GetComponent<SphereCollider>();
        gameObject.tag = "Drawable";
        // DebugText.log("Setting Tag to drawable in vertex");
    }

    // Update is called once per frame
    public override Grabbable Grab(Vector3 grabbingTrans)
    {
        // Vector3 newPos = new Vector3((float)Math.Round(grabbingTrans.x,2), (float)Math.Round(grabbingTrans.y,2), (float)Math.Round(grabbingTrans.z,2));
        gameObject.transform.position = grabbingTrans;
        return gameObject.GetComponent<Vertex>();
        
    }

    public override Grabbable Pinch()
    {
        GameObject newVert = Instantiate(gameObject);
        Vertex grbComponent = newVert.GetComponent<Vertex>();
        grbComponent.lines.Clear();
        grbComponent.faces.Clear();
        
        GameObject lineObj = new GameObject("NewLine");

        Line lineComponent = lineObj.GetOrAddComponent<Line>();
        lines.Add(lineComponent);
        lineComponent.vert1 = gameObject;
        lineComponent.vert2 = newVert;
        newVert.GetComponent<Vertex>().lines.Add(lineComponent);

        return grbComponent;
    }
    public void CullFaces() {
        List<Face> facesToCull = new List<Face>();
        List<Line> validLines = new List<Line>();
        foreach (Face face in faces) {
            for(int i=0; i < faces.Count; i ++) {
                if(face == faces[i]) continue; //skip over myself
                if(face.IsSubFace(faces[i]) && !facesToCull.Contains(faces[i])) {
                    facesToCull.Add(face);
                } else {
                    foreach(Line line in faces[i].lines) {
                        validLines.Add(line);
                    }
                }
            }
        }
        foreach(Face face in facesToCull) {
            foreach(Line line in face.lines) {
                if(!validLines.Contains(line)) {
                    line.vert1.GetComponent<Vertex>().lines.Remove(line);
                    line.vert2.GetComponent<Vertex>().lines.Remove(line);
                    Destroy(line);
                }
            }
            Destroy(face);
        }
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
    public void StealFaces(Vertex old) {
        Vertex active = gameObject.GetComponent<Vertex>(); // I am the active vertex, stealing faces from old vertex
        List<int> faceIdxToRemove = new List<int>();
        for(int i=0; i < old.faces.Count; i ++) { //For Each face
            for(int j=0; j < old.faces[i].vertices.Count; j++) { //For each vertex in each face of old vert
                if(old.faces[i].vertices[j] == old) {
                    // if(old.faces[i].vertices.Contains(active) && old.faces[i] != null) {
                    //     // Destroy(old.faces[i].gameObject);
                    // } else {
                        old.faces[i].vertices[j] = active;
                        active.faces.Add(old.faces[i]);
                        DebugText.log("Fixed Face");

                    // }
                    
                }
            }

        }

    }

    public void SpawnNewFaces() {
        Vertex theVert = gameObject.GetComponent<Vertex>(); // Get my vertex component
        List<Vertex> vertPath = new List<Vertex>{theVert};
        List<Line> linePath = new List<Line>();
        List<Vertex> visited = new List<Vertex>();
        List<List<Vertex>> faceVerts = new List<List<Vertex>>();
        List<List<Line>> faceLines = new List<List<Line>>();

        DFSUtil(theVert,theVert,visited,vertPath,linePath,faceVerts,faceLines);

        int fewestVerts = 10000; // Should be infinity but idk how to do infinity here
        for(int i =0; i < faceVerts.Count; i ++) {
            if (faceVerts[i].Count < fewestVerts) {
                fewestVerts = faceVerts[i].Count;
            }
        }

        List<List<Vertex>> actualFaceVerts = new List<List<Vertex>>();
        List<List<Line>> actualFaceLines = new List<List<Line>>();
        for(int i =0; i < faceVerts.Count; i ++) {
            if (faceVerts[i].Count ==  fewestVerts) {
                actualFaceLines.Add(faceLines[i]);
                actualFaceVerts.Add(faceVerts[i]);
            }
        }

        faceVerts= actualFaceVerts;
        faceLines = actualFaceLines;



        for(int i=0; i < faceVerts.Count; i ++) {
            DebugText.log($"Face Verts[{i}]: {faceVerts[i].Count}");
            DebugText.log($"Face Lines[{i}]: {faceLines[i].Count}");
            GameObject go = new GameObject("NewFace");
            Face face = go.AddComponent<Face>();
            DebugText.log("Looking for new face to spawn");
            for(int j =0; j < faceVerts[i].Count; j ++) {
                face.vertices.Add(faceVerts[i][j]);
                face.lines.Add(faceLines[i][j]);
            }
            if(!ShouldAddFace(theVert,face)) {
                DebugText.log("Destroying face");
                Destroy(go);
            } else {
                for(int j =0; j < faceVerts[i].Count; j ++) {
                    faceVerts[i][j].faces.Add(face);
                    faceLines[i][j].faces.Add(face);
                    faceLines[i][j].isPartOfFace = true;
                }
            }
        }
        
    }
    bool ShouldAddFace(Vertex v, Face newFace) {
        foreach(Face f in v.faces) {
            if(CheckFaceEquivalance(f,newFace)) {
                return false;
            }
        }
        return true;
    }
    void DFSUtil(Vertex start, Vertex end, List<Vertex> visited, List<Vertex> vertPath, List<Line> linePath, List<List<Vertex>> faceVerts,List<List<Line>> faceLines) {
        if(!visited.Contains(start)) {
            visited.Add(start);
        }
        List<Vertex> adjVertices = GetConnectedVerts(start);
        for(int i=0; i < adjVertices.Count; i ++) {
            if(!visited.Contains(adjVertices[i])) {
                vertPath.Add(adjVertices[i]);
                linePath.Add(start.lines[i]);
                DFSUtil(adjVertices[i],end,visited,vertPath,linePath,faceVerts,faceLines);
                linePath.Remove(start.lines[i]);
                vertPath.Remove(adjVertices[i]);
            } else if(vertPath.Count >= 3 && adjVertices[i] == end) {
                DebugText.log("WE FOUND ONE REAL");
                List<Vertex> foundVerts = new List<Vertex> ();
                List<Line> foundLines = new List<Line> ();
                for(int z=0; z<vertPath.Count; z++) {
                    foundVerts.Add(vertPath[z]);
                    
                }
                for(int z=0; z<linePath.Count; z++) {
                    // foundVerts.Add(vertPath[z]);
                    foundLines.Add(linePath[z]);
                    
                }
                foundLines.Add(start.lines[i]);
                faceVerts.Add(foundVerts);
                faceLines.Add(foundLines);
            }
        }
        visited.Remove(start);

    }

    List<Vertex> GetConnectedVerts(Vertex v) {
        List<Vertex> vertices = new List<Vertex>();
        for(int i=0; i < v.lines.Count; i++) {
            if(v.lines[i].vert1.GetComponent<Vertex>() == v) {
                vertices.Add(v.lines[i].vert2.GetComponent<Vertex>());
            } else {
                vertices.Add(v.lines[i].vert1.GetComponent<Vertex>());
            }
        }
        return vertices;
    }
    bool CheckFaceEquivalance(Face f1, Face f2) {
        bool toReturn = true;
        for(int i=0; i < f1.vertices.Count; i ++) {
            if(!f2.vertices.Contains(f1.vertices[i])) { // If the f2 does not contain a vert from f1, then return false else return true if all verts in f1 are in f2
            DebugText.log("Face Duplicate found====================================");
                toReturn = false;
                break;
            }
        }
        for(int i=0; i < f2.vertices.Count; i ++) {
            if(!f1.vertices.Contains(f2.vertices[i])) { // If the f1 does not contain a vert from f2, then return false else return true if all verts in f1 are in f2
            DebugText.log("Face Duplicate found====================================");
                toReturn = false;
                break;
            }
        }

        return toReturn;
    }


}
