using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;
public static class CoastShots {
 public static void Save(){EditorSceneManager.OpenScene(CoastAuthored.Scene);var c=Camera.main;var pos=new[]{new Vector3(-35,25,-39),new Vector3(-11,3,-20),new Vector3(9,6,-5),new Vector3(-12,10,9),new Vector3(12,17,13),new Vector3(-15,17,18),new Vector3(-27,2.5f,-22),new Vector3(-23,4,-6),new Vector3(1,2,-14)};var look=new[]{new Vector3(0,9,8),new Vector3(3,6,4),new Vector3(-4,7,8),new Vector3(1,13,23),new Vector3(-2,17.5f,25),new Vector3(-2,17,25),new Vector3(-8,2.5f,-9),new Vector3(-11,4,0),new Vector3(6,1.6f,-7)};for(int i=0;i<pos.Length;i++){c.transform.position=pos[i];c.transform.LookAt(look[i]);var rt=new RenderTexture(1800,1125,24);c.targetTexture=rt;c.Render();RenderTexture.active=rt;var tex=new Texture2D(1800,1125,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1800,1125),0,0);tex.Apply();File.WriteAllBytes("Docs/Lighthouse_Authored/"+i.ToString("00")+".png",tex.EncodeToPNG());c.targetTexture=null;RenderTexture.active=null;Object.DestroyImmediate(tex);Object.DestroyImmediate(rt);}Debug.Log("COAST_FINAL_SHOTS_SAVED");}
}
