using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
public static class CoastPhysicsInspect {
 public static void Run(){EditorSceneManager.OpenScene(CoastAuthored.Scene);Physics.SyncTransforms();var rows=new List<string>();foreach(var f in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None)){if(f.name.Contains("Beach sand")||f.name.Contains("Sandy mountain")){rows.Add(f.name+" bounds "+f.GetComponent<Renderer>().bounds+" collider "+(f.GetComponent<Collider>()!=null));var tri=f.sharedMesh.triangles;var vv=f.sharedMesh.vertices;rows.Add("normal "+Vector3.Cross(vv[tri[1]]-vv[tri[0]],vv[tri[2]]-vv[tri[0]]).normalized);}}foreach(bool back in new[]{false,true}){Physics.queriesHitBackfaces=back;foreach(float x in new[]{-13f,13f})foreach(float z in new[]{-16f,16f}){if(Physics.Raycast(new Vector3(x,50,z),Vector3.down,out var hit,100))rows.Add(back+" "+x+","+z+" hits "+hit.collider.name+" "+hit.point+" normal "+hit.normal);else rows.Add(back+" "+x+","+z+" MISS");}}File.WriteAllLines("Docs/Lighthouse_Authored/PHYSICS_DIAGNOSTIC.txt",rows);}
}
