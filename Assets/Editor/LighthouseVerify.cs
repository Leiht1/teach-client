using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
public static class LighthouseVerify {
 public static void Run(){EditorSceneManager.OpenScene("Assets/Scenes/Lighthouse_Coast_ArtBenchmark_V01.unity");Physics.SyncTransforms();var rows=new List<string>{"sample,x,z,ground,collider,slope"};int fail=0;for(int i=0;i<=240;i++){float f=i/240f,z=-15+51*f,x=10*Mathf.Sin(f*Mathf.PI*2-.55f)*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(28,36,z)));if(Physics.Raycast(new Vector3(x,60,z),Vector3.down,out var h,100)){float slope=Vector3.Angle(h.normal,Vector3.up);if(slope>48)fail++;rows.Add(i+","+x+","+z+","+h.point.y+","+h.collider.name+","+slope);}else{fail++;rows.Add(i+",MISS");}}File.WriteAllLines("Docs/LighthouseCoastV01/ROUTE_COLLISION.csv",rows);Debug.Log("ROUTE_CHECK failures="+fail);}
}
