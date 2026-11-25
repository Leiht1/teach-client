using System.Collections;
using UnityEngine;
namespace IslandSystem {
public class ChopTarget:MonoBehaviour {
 public Transform crown; public GameObject logPrefab; public int Hits{get;private set;} public bool Felled{get;private set;} public int DroppedLogs{get;private set;}
 readonly System.Collections.Generic.List<GameObject> drops=new System.Collections.Generic.List<GameObject>();
 Vector3 crownStart;Quaternion rotation;float resetAt;
 void Awake(){if(!crown)crown=transform;crownStart=crown.localPosition;rotation=crown.localRotation;}
 public void Hit(){if(Felled)return;StopAllCoroutines();Hits++;if(Hits>=3){Felled=true;StartCoroutine(Fall());}else StartCoroutine(Shake());}
 IEnumerator Shake(){for(float t=0;t<.25f;t+=Time.deltaTime){crown.localRotation=rotation*Quaternion.Euler(0,0,Mathf.Sin(t*48)*3*(1-t/.25f));yield return null;}if(!Felled)crown.localRotation=rotation;}
 IEnumerator Fall(){foreach(var c in GetComponentsInChildren<Collider>())c.enabled=false;for(float t=0;t<.8f;t+=Time.deltaTime){crown.localRotation=rotation*Quaternion.Euler(0,0,Mathf.SmoothStep(0,82,t/.8f));yield return null;}crown.gameObject.SetActive(false);if(logPrefab)for(int i=0;i<3;i++){var g=Instantiate(logPrefab,transform.position+new Vector3((i-1)*.45f,.12f,.35f),Quaternion.Euler(0,i*38,0));g.name="砍伐掉落木材";g.transform.localScale=Vector3.one*.45f;drops.Add(g);DroppedLogs++;}resetAt=Time.time+20;}
 void OnGUI(){if(gameObject.scene.name=="Island_World_V01")return;var camera=Camera.main;if(!camera||Felled)return;var host=FindAnyObjectByType<ScratchWorkbenchHost>();if(host&&host.IsOpen)return;var p=camera.WorldToScreenPoint(transform.position+Vector3.up*2.8f);if(p.z>0&&Vector3.Distance(camera.transform.position,transform.position)<14)GUI.Box(new Rect(p.x-55,Screen.height-p.y,110,24),"练习树 "+Hits+" / 3");}
 void Update(){if(Felled&&resetAt>0&&Time.time>=resetAt){foreach(var g in drops)if(g)Destroy(g);drops.Clear();DroppedLogs=0;Hits=0;Felled=false;resetAt=0;crown.gameObject.SetActive(true);crown.localPosition=crownStart;crown.localRotation=rotation;foreach(var c in GetComponentsInChildren<Collider>())c.enabled=true;}}
}
}
