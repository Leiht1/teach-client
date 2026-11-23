using System.Collections;
using UnityEngine;
namespace IslandSystem {
// Real action adapter on the existing lighthouse Explorer; the original walker source is unchanged.
public class ActionMotor : MonoBehaviour {
 public Camera view; public Animator animator; public float chopDuration=1; public Transform axe;
 public Vector2 ReviewAxes;public bool ReviewSprint;
 public bool ReviewMode, InputBlocked; public bool Busy {get;private set;}
 float chopStarted;public float ChopPhase=>Busy?(Time.time-chopStarted)/chopDuration:0;
 public int Jumps {get;private set;} public int Landings {get;private set;} public int Chops {get;private set;}
 public float Distance=5.5f, Yaw, Pitch=22; public string Status="靠近前方练习树，按 E 砍伐；Tab 编程";
 public Vector3 Spawn; public float WalkSpeed=3.3f,RunSpeed=6;
 CharacterController body; float vertical; bool airborne; string state; Vector3 command; bool program; float landedAt; Coroutine chopping;
 bool OnGround(){if(body.isGrounded)return true;foreach(var hit in Physics.SphereCastAll(transform.position+Vector3.up*.22f,.2f,Vector3.down,.16f))if(!hit.collider.transform.IsChildOf(transform)&&!hit.collider.isTrigger&&hit.normal.y>.65f)return true;return false;}
 public bool Grounded => body && !airborne && vertical<=0 && OnGround();
 void Awake(){body=GetComponent<CharacterController>();body.minMoveDistance=0;if(!animator)animator=GetComponentInChildren<Animator>();Spawn=transform.position;Yaw=transform.eulerAngles.y;}
 void Update(){var host=FindAnyObjectByType<ScratchWorkbenchHost>();if(host&&host.IsOpen)InputBlocked=true;if(!body)return;float dt=Time.deltaTime;
  if(!ReviewMode&&!InputBlocked){if(Input.GetKeyDown(KeyCode.Escape)){ReleasePointer();FindAnyObjectByType<ActionProgramRunner>()?.Cancel("已停止程序");}
   if(Input.GetMouseButtonDown(0)&&Input.mousePosition.y<Screen.height-70&&!RelicCore.PointerOverEntry){Cursor.lockState=CursorLockMode.Locked;Cursor.visible=false;}
   if(Cursor.lockState==CursorLockMode.Locked)Orbit(Input.GetAxis("Mouse X")*2.2f,-Input.GetAxis("Mouse Y")*1.8f,Input.mouseScrollDelta.y);
   else Orbit(0,0,Input.mouseScrollDelta.y);
   if(!program){Drive(new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical")),Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.RightShift));if(Input.GetKeyDown(KeyCode.Space))Jump();if(Input.GetKeyDown(KeyCode.E))BeginChop();}
  }else if(!program){command=Vector3.zero;if(ReviewMode&&!InputBlocked)Drive(ReviewAxes,ReviewSprint);}
  if(!airborne&&OnGround()&&vertical<0)vertical=-2;
  vertical-=22*dt;var contacts=body.Move(((Busy?Vector3.zero:command)+Vector3.up*vertical)*dt);
  if((contacts&CollisionFlags.Above)!=0&&vertical>0)vertical=0;
  if((contacts&CollisionFlags.Below)!=0&&vertical<=0){vertical=-2;if(airborne){airborne=false;Landings++;landedAt=Time.time;Play("Land");}}
  if(!Busy&&!airborne&&Time.time-landedAt>.16f)Play(command.sqrMagnitude>.05f?(command.magnitude>WalkSpeed+.1f?"Run":"Walk"):"Idle");
  if(transform.position.y<Spawn.y-8){FindAnyObjectByType<ActionProgramRunner>()?.Cancel("离开可通行区域，已返回起点");Teleport(Spawn);}
 }
 public void Drive(Vector2 axes,bool sprint){if(InputBlocked||Busy||program)return;axes=Vector2.ClampMagnitude(axes,1);var q=Quaternion.Euler(0,Yaw,0);command=q*new Vector3(axes.x,0,axes.y)*(sprint?RunSpeed:WalkSpeed);if(command.sqrMagnitude>.01f)transform.rotation=Quaternion.RotateTowards(transform.rotation,Quaternion.LookRotation(command),720*Time.deltaTime);}
 public void Orbit(float x,float y,float wheel){if(InputBlocked)return;Yaw+=x;Pitch=Mathf.Clamp(Pitch+y,-5,65);Distance=Mathf.Clamp(Distance-wheel*.7f,2.2f,11);}
 public bool Jump(){if(InputBlocked||Busy||!Grounded)return false;vertical=7;airborne=true;Jumps++;Play("Jump");return true;}
 public void Teleport(Vector3 p){body.enabled=false;transform.position=p;body.enabled=true;vertical=-2;airborne=false;command=Vector3.zero;Physics.SyncTransforms();}
 public void SetProgram(bool active){program=active;command=Vector3.zero;if(!active&&chopping!=null){StopCoroutine(chopping);chopping=null;Busy=false;if(axe)axe.gameObject.SetActive(false);Play("Idle");}}
 public IEnumerator Walk(float meters){var start=transform.position;var direction=transform.forward;float progress=0,deadline=Time.time+Mathf.Abs(meters)/WalkSpeed+3;command=direction*Mathf.Sign(meters)*WalkSpeed;
  while(progress<Mathf.Abs(meters)-.06f){if(Time.time>deadline){command=Vector3.zero;throw new System.InvalidOperationException("移动被障碍阻挡，程序已停止");}progress=Vector3.Dot(transform.position-start,direction)*Mathf.Sign(meters);yield return null;}command=Vector3.zero;
 }
 public IEnumerator Turn(float degrees){float remaining=degrees;while(Mathf.Abs(remaining)>.1f){float step=Mathf.Sign(remaining)*Mathf.Min(Mathf.Abs(remaining),180*Time.deltaTime);transform.Rotate(0,step,0);remaining-=step;yield return null;}}
 public bool BeginChop(){if(InputBlocked||Busy||!Grounded){Status="当前不能砍伐：请先落地并结束其他动作";return false;}var tree=FindTree();if(!tree){Status="没有可砍伐目标：走近练习树，并面向树干（1.8米内）";return false;}chopping=StartCoroutine(Chop(tree));return true;}
 public ChopTarget FindTree(){ChopTarget best=null;float distance=1.8f;foreach(var t in FindObjectsByType<ChopTarget>(FindObjectsSortMode.None)){var delta=t.transform.position-transform.position;delta.y=0;float d=delta.magnitude;if(!t.Felled&&d<distance&&Vector3.Dot(transform.forward,delta.normalized)>.55f){distance=d;best=t;}}return best;}
 IEnumerator Chop(ChopTarget target){Busy=true;chopStarted=Time.time;command=Vector3.zero;if(axe)axe.gameObject.SetActive(true);Play("Chop");yield return new WaitForSeconds(chopDuration*.47f);if(target&&!target.Felled&&Vector3.Distance(new Vector3(target.transform.position.x,transform.position.y,target.transform.position.z),transform.position)<1.9f){target.Hit();Chops++;Status="砍伐命中 "+target.Hits+" / 3";}yield return new WaitForSeconds(chopDuration*.53f);Busy=false;chopping=null;if(axe)axe.gameObject.SetActive(false);Play("Idle");}
 void Play(string next){if(state==next)return;state=next;if(animator)animator.CrossFadeInFixedTime(next,.08f);}
 public static void ReleasePointer(){Cursor.lockState=CursorLockMode.None;Cursor.visible=true;}
 void LateUpdate(){if(axe&&Busy&&animator){var hand=animator.GetBoneTransform(HumanBodyBones.RightHand);var index=animator.GetBoneTransform(HumanBodyBones.RightIndexProximal);var little=animator.GetBoneTransform(HumanBodyBones.RightLittleProximal);var middle=animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal);if(hand&&index&&little&&middle){var shaft=(index.position-little.position).normalized;var fingers=(middle.position-hand.position).normalized;var normal=Vector3.Cross(fingers,shaft).normalized;axe.rotation=Quaternion.LookRotation(normal,shaft);axe.position=Vector3.Lerp(hand.position,middle.position,.65f)-shaft*.16f;}}if(!view)return;var target=transform.position+Vector3.up*1.3f;var delta=Quaternion.Euler(Pitch,Yaw,0)*Vector3.back*Distance;float length=Distance;foreach(var hit in Physics.SphereCastAll(target,.18f,delta.normalized,Distance))if(!hit.collider.transform.IsChildOf(transform)&&!hit.collider.isTrigger)length=Mathf.Min(length,Mathf.Max(.6f,hit.distance-.1f));view.transform.position=target+delta.normalized*length;view.transform.LookAt(target);}
 void OnApplicationFocus(bool focus){if(!focus&&!ReviewMode)ReleasePointer();}
 void OnDisable(){if(!ReviewMode)ReleasePointer();}
 void OnGUI(){if(ReviewMode)return;var runner=GetComponent<ActionProgramRunner>();if(runner&&runner.Running&&GUI.Button(new Rect(18,18,150,36),"停止程序"))runner.Cancel("已手动停止");}
}
}
