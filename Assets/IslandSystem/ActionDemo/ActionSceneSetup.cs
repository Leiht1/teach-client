using UnityEngine;
namespace IslandSystem {
public class ActionSceneSetup:MonoBehaviour {
 internal static bool SupportsScene(string name) => name=="Lighthouse_Coast_ArtBenchmark_V01" || name=="Island_World_V01";
 public RuntimeAnimatorController controller;public GameObject treePrefab,axePrefab;public float chopDuration=1;
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Install(){if(!SupportsScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name))return;var p=Resources.Load<GameObject>("IslandActionSetup");if(p)Instantiate(p);}
 void Start(){var original=FindAnyObjectByType<LighthouseWalker>();if(!original||FindAnyObjectByType<ActionMotor>())return;original.enabled=false;var motor=original.gameObject.AddComponent<ActionMotor>();motor.view=original.view;motor.animator=original.GetComponentInChildren<Animator>();motor.animator.runtimeAnimatorController=controller;motor.animator.applyRootMotion=false;motor.chopDuration=chopDuration;
  motor.ReviewMode=Application.isBatchMode||System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-scratch-window-review")>=0;
  original.gameObject.AddComponent<ActionProgramRunner>();
  var hand=motor.animator.GetBoneTransform(HumanBodyBones.RightHand);if(hand&&axePrefab){var axe=Instantiate(axePrefab,hand);axe.transform.localPosition=new Vector3(0,.02f,0);axe.transform.localRotation=Quaternion.Euler(0,0,90);motor.axe=axe.transform;axe.SetActive(false);}
  bool island=gameObject.scene.name=="Island_World_V01";
  if(island && motor.view && !motor.ReviewMode)
   gameObject.AddComponent<IslandFreeCamera>().Initialize(motor,System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-island-free-camera")>=0);
  if(island)return;
  for(int i=0;i<3;i++){var point=original.transform.position+(island?new Vector3(-5-i*2,0,2+i*3):new Vector3(i==0?0:i==1?3.8f:-4,0,i==0?1.15f:3.5f));var origin=point+Vector3.up*2;foreach(var hit in Physics.RaycastAll(origin,Vector3.down,6))if(!hit.collider.transform.IsChildOf(original.transform)&&hit.point.y<=point.y+.5f&&hit.point.y>point.y-2){point.y=hit.point.y;break;}var tree=Instantiate(treePrefab,point,Quaternion.identity);tree.name="可砍伐练习树 "+(i+1);}
 }
}
}
