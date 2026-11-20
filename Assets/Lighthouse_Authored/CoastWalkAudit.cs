using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
public class CoastWalkAudit:MonoBehaviour {
 public Vector3[] waypoints;CharacterController cc;Camera cam;List<string> rows=new List<string>();int index=1;float total,stuck;Vector3 previous;int shots;bool done;
 void Start(){cc=GetComponent<CharacterController>();GetComponent<LighthouseWalker>().enabled=false;cam=Camera.main;var anim=GetComponentInChildren<Animator>();if(anim)anim.SetFloat("Speed",1);previous=transform.position;rows.Add("time,index,x,y,z,grounded");Time.captureDeltaTime=1f/30;}
 void Update(){if(done)return;float dt=1f/30;total+=dt;if(index>=waypoints.Length){Finish(true,"Reached lighthouse courtyard");return;}var target=waypoints[index];var d=target-transform.position;d.y=0;if(d.magnitude<.25f){index++;return;}var dir=d.normalized;transform.rotation=Quaternion.LookRotation(dir);cc.Move((dir*4+Vector3.down*7)*dt);GetComponent<LighthouseWalker>().FollowCamera();if(Vector3.Distance(previous,transform.position)<.005f)stuck+=dt;else stuck=0;var anim=GetComponentInChildren<Animator>();if(anim)anim.SetFloat("Speed",1);previous=transform.position;rows.Add(total+","+index+","+transform.position.x+","+transform.position.y+","+transform.position.z+","+cc.isGrounded);if(index>=waypoints.Length*(shots+1)/5&&shots<4){shots++;Capture("ThirdPerson_"+shots);}if(stuck>3||total>90||transform.position.y< -3)Finish(false,"Blocked or timeout at waypoint "+index);}
 void Capture(string name){var rt=new RenderTexture(1800,1125,24);cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;var tx=new Texture2D(1800,1125,TextureFormat.RGB24,false);tx.ReadPixels(new Rect(0,0,1800,1125),0,0);tx.Apply();File.WriteAllBytes("Docs/Lighthouse_Authored/"+name+".png",tx.EncodeToPNG());cam.targetTexture=null;RenderTexture.active=null;Destroy(rt);Destroy(tx);}
 void Finish(bool pass,string message){done=true;Capture(pass?"ReachedLighthouse":"BlockedRoute");File.WriteAllLines("Docs/Lighthouse_Authored/PLAY_WALK.csv",rows);File.WriteAllText("Docs/Lighthouse_Authored/PLAY_RESULT.txt",(pass?"PASS":"FAIL")+"\n"+message+"\nActual CharacterController.Move in Unity Play Mode; no teleport between waypoints.\nTime: "+total+"\nPosition: "+transform.position);Debug.Log("COAST_PLAY_AUDIT "+pass+" "+message);
#if UNITY_EDITOR
 UnityEditor.EditorApplication.isPlaying=false;
#endif
 }
}

