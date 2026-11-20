using UnityEngine;
public class LighthouseWalker : MonoBehaviour {
 public Camera view;public float speed=4;CharacterController body;float fall;float yaw;
 void Start(){body=GetComponent<CharacterController>();yaw=transform.eulerAngles.y;}
 void Update(){float h=Input.GetAxis("Horizontal"),v=Input.GetAxis("Vertical");var anim=GetComponentInChildren<Animator>();if(anim)anim.SetFloat("Speed",Mathf.Abs(v));yaw+=h*95*Time.deltaTime;transform.rotation=Quaternion.Euler(0,yaw,0);fall=body.isGrounded?-2:fall-20*Time.deltaTime;body.Move((transform.forward*v*speed+Vector3.up*fall)*Time.deltaTime);FollowCamera();}
 public void FollowCamera(){if(!view)return;var target=transform.position+Vector3.up*1.4f;var desired=transform.position-transform.forward*5+Vector3.up*3;var ray=desired-target;float distance=ray.magnitude;foreach(var hit in Physics.SphereCastAll(target,.18f,ray.normalized,distance,~0,QueryTriggerInteraction.Ignore)){if(hit.collider.gameObject==gameObject||hit.collider.transform.IsChildOf(transform))continue;distance=Mathf.Min(distance,Mathf.Max(.6f,hit.distance-.1f));}view.transform.position=target+ray.normalized*distance;view.transform.LookAt(target);}
}
