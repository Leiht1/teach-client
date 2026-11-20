using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public static class ArtLibraryBuilder {
 static List<string> models; static List<string> audit=new List<string>();
 static Dictionary<string,Material> materials=new Dictionary<string,Material>();
 static string Cat(string p) {
  string n=Path.GetFileNameWithoutExtension(p).ToLowerInvariant();
  if(p.Contains("UniversalAnimationLibrary"))return "08 Animation";
  if(p.Contains("UniversalBaseCharacters"))return "07 Characters";
  if(p.Contains("ResourceBits"))return "06 Resources";
  if(p.Contains("UltimateModularRuins")||p.Contains("DungeonRemastered"))return "04 Ruins";
  if(p.Contains("PirateKit")||p.Contains("WatercraftKit"))return "05 Beach and Boats";
  if(n.Contains("rock")||n.Contains("stone"))return "03 Rocks";
  if(n.Contains("tree")||n.Contains("palm"))return "01 Trees";
  return "02 Plants";
 }
 static Bounds Bounds(GameObject g){var rr=g.GetComponentsInChildren<Renderer>(); if(rr.Length==0)return new Bounds(g.transform.position,Vector3.one);var b=rr[0].bounds;foreach(var r in rr)b.Encapsulate(r.bounds);return b;}
 static Material Mat(string name,Color c){string p="Assets/ArtPreview/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(p);if(!m){m=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(m,p);}m.color=c;m.SetFloat("_Glossiness",0.12f);return m;}
 static GameObject Spawn(string path,Vector3 at,float height=0,float yaw=0){
  var asset=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(!asset)return null;
  var g=(GameObject)PrefabUtility.InstantiatePrefab(asset);g.name=Path.GetFileNameWithoutExtension(path);g.transform.rotation=Quaternion.Euler(0,yaw,0);
  foreach(var r in g.GetComponentsInChildren<Renderer>()){
   var mm=r.sharedMaterials;
   for(int k=0;k<mm.Length;k++)if(mm[k]){
    var original=mm[k];if(AssetDatabase.GetAssetPath(original).StartsWith("Assets/Art/Materials/"))continue; string key=AssetDatabase.GetAssetPath(original)+"/"+original.name;
    if(!materials.TryGetValue(key,out var material)){
     string id=Hash128.Compute(key).ToString();string mp="Assets/Art/Materials/"+id+".mat";
     material=AssetDatabase.LoadAssetAtPath<Material>(mp);if(!material){material=new Material(original);AssetDatabase.CreateAsset(material,mp);}
     material.SetFloat("_Glossiness",.08f);material.enableInstancing=true;
     string n=original.name.ToLowerInvariant();if(original.mainTexture&&(n.Contains("lea")||n.Contains("flower")||n.Contains("grass")||n.Contains("fern"))){material.SetFloat("_Mode",1);material.SetInt("_SrcBlend",1);material.SetInt("_DstBlend",0);material.SetInt("_ZWrite",1);material.EnableKeyword("_ALPHATEST_ON");material.DisableKeyword("_ALPHABLEND_ON");material.SetFloat("_Cutoff",.45f);material.renderQueue=2450;material.SetOverrideTag("RenderType","TransparentCutout");}
     EditorUtility.SetDirty(material);materials[key]=material;
    }mm[k]=material;
   }r.sharedMaterials=mm;
  }
  var b=Bounds(g);if(height>0&&b.size.y>0.001f)g.transform.localScale*=height/b.size.y;
  b=Bounds(g);g.transform.position+=at-new Vector3(b.center.x,b.min.y,b.center.z);
  return g;
 }
 static void Label(string s,Vector3 p,float size=1){var g=new GameObject(s);g.transform.position=p;g.transform.rotation=Quaternion.Euler(90,0,0);var t=g.AddComponent<TextMesh>();t.text=s;t.fontSize=56;t.characterSize=0.15f*size;t.anchor=TextAnchor.MiddleCenter;t.color=new Color(.15f,.24f,.28f);}
 static Camera Lighting(Vector3 pos,Vector3 look){RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.72f,.83f,.89f);RenderSettings.ambientEquatorColor=new Color(.52f,.62f,.55f);RenderSettings.ambientGroundColor=new Color(.28f,.31f,.27f);RenderSettings.fog=false;
  var sun=new GameObject("Soft afternoon sun").AddComponent<Light>();sun.type=LightType.Directional;sun.intensity=1.15f;sun.color=new Color(1,.94f,.81f);sun.shadows=LightShadows.Soft;sun.transform.rotation=Quaternion.Euler(48,-32,0);QualitySettings.shadowDistance=180;QualitySettings.shadows=ShadowQuality.All;QualitySettings.shadowResolution=ShadowResolution.High;QualitySettings.antiAliasing=4;
  var c=new GameObject("Preview Camera").AddComponent<Camera>();c.tag="MainCamera";c.transform.position=pos;c.transform.LookAt(look);c.nearClipPlane=.1f;c.farClipPlane=1000;c.fieldOfView=52;c.clearFlags=CameraClearFlags.SolidColor;c.backgroundColor=new Color(.64f,.82f,.89f);return c;
 }
 static void Shot(Camera c,string file,int w=1600,int h=1000){Directory.CreateDirectory("Docs/Art/Previews");var rt=new RenderTexture(w,h,24);c.targetTexture=rt;c.Render();RenderTexture.active=rt;var t=new Texture2D(w,h,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,w,h),0,0);t.Apply();File.WriteAllBytes("Docs/Art/Previews/"+file+".png",t.EncodeToPNG());c.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(t);UnityEngine.Object.DestroyImmediate(rt);}
 static void Ground(string name,Vector3 pos,Vector3 scale,Material m){var g=GameObject.CreatePrimitive(PrimitiveType.Plane);g.name=name;g.transform.position=pos;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=m;}
 [MenuItem("Art/Build library and beach forest test")]
 public static void Build(){try{
  PlayerSettings.colorSpace=ColorSpace.Linear;AssetDatabase.Refresh();models=AssetDatabase.FindAssets("t:Model",new[]{"Assets/Art/External"}).Select(AssetDatabase.GUIDToAssetPath).Where(p=>p.EndsWith(".fbx",StringComparison.OrdinalIgnoreCase)).OrderBy(p=>p).ToList();
  foreach(var p in models){var i=(ModelImporter)AssetImporter.GetAtPath(p);bool humanoid=p.Contains("UniversalAnimationLibrary")||(p.Contains("UniversalBaseCharacters")&&p.Contains("FullBody"));if(humanoid&&i.animationType!=ModelImporterAnimationType.Human){i.animationType=ModelImporterAnimationType.Human;i.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;i.SaveAndReimport();}}
  AssetDatabase.SaveAssets();
  audit.Add("asset\tcategory\trenderers\ttriangles\tsize_m\tmissing_materials\tuntextured_material_slots\thumanoid_valid\tclips");
  foreach(var p in models){var g=Spawn(p,Vector3.zero);if(!g){audit.Add(p+"\tIMPORT FAILED");continue;}var rs=g.GetComponentsInChildren<Renderer>();long tris=0;foreach(var f in g.GetComponentsInChildren<MeshFilter>())if(f.sharedMesh)tris+=f.sharedMesh.triangles.Length/3;foreach(var f in g.GetComponentsInChildren<SkinnedMeshRenderer>())if(f.sharedMesh)tris+=f.sharedMesh.triangles.Length/3;var mats=rs.SelectMany(r=>r.sharedMaterials).ToArray();int missing=mats.Count(m=>!m||!m.shader||m.shader.name.Contains("Error"));int mt=mats.Count(m=>m&&m.HasProperty("_MainTex")&&!m.mainTexture);var a=g.GetComponent<Animator>();var clips=AssetDatabase.LoadAllAssetsAtPath(p).OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__preview")).ToArray();audit.Add(p+"\t"+Cat(p)+"\t"+rs.Length+"\t"+tris+"\t"+Bounds(g).size.ToString("F2")+"\t"+missing+"\t"+mt+"\t"+(a&&a.avatar?a.avatar.isValid&&a.avatar.isHuman:false)+"\t"+clips.Length);UnityEngine.Object.DestroyImmediate(g);}
  File.WriteAllLines("Docs/Art/UNITY_IMPORT_AUDIT.tsv",audit);
  foreach(var p in models){var importer=(ModelImporter)AssetImporter.GetAtPath(p);bool changed=false;foreach(var m in AssetDatabase.LoadAllAssetsAtPath(p).OfType<Material>()){string key=p+"/"+m.name;if(materials.TryGetValue(key,out var mapped)){importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material),m.name),mapped);changed=true;}}if(changed)importer.SaveAndReimport();}
  Library();Beach();AnimationPreview();AssetDatabase.SaveAssets();Debug.Log("ART_LIBRARY_BUILD_SUCCESS models="+models.Count);
 }catch(Exception e){Debug.LogException(e);throw;}}
 static void Library(){EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);float z=0;var zones=new List<Tuple<string,Vector3,float>>();
  foreach(var group in models.GroupBy(Cat).OrderBy(g=>g.Key)){float x=0,rowDepth=0,start=z;Label(group.Key,new Vector3(35,.04f,z-4),2);foreach(var p in group){var g=Spawn(p,new Vector3(x,0,z));if(!g)continue;var b=Bounds(g);float width=Mathf.Max(b.size.x+2.5f,4);if(x+width>90&&x>0){x=0;z+=rowDepth+4;rowDepth=0;g.transform.position+=new Vector3(-g.transform.position.x,0,z-b.center.z);}b=Bounds(g);g.transform.position+=new Vector3(x+b.extents.x-b.center.x,0,z+b.extents.z-b.center.z);rowDepth=Mathf.Max(rowDepth,b.size.z);Label(g.name,new Vector3(x+width/2,.03f,z-1),.45f);x+=width;}
   z+=rowDepth+12;zones.Add(Tuple.Create(group.Key,new Vector3(43,0,(start+z-10)/2),Mathf.Max(25,(z-start)*.75f)));}
  Ground("Gallery floor",new Vector3(45,-.03f,z/2),new Vector3(11,1,(z+20)/10),Mat("Gallery floor",new Color(.83f,.87f,.80f)));
  var c=Lighting(new Vector3(110,160,-75),new Vector3(42,0,z/2));c.orthographic=true;c.orthographicSize=Mathf.Max(65,z*.56f);Shot(c,"ArtLibraryPreview");
  foreach(var zone in zones){c.transform.position=zone.Item2+new Vector3(32,40,-38);c.transform.LookAt(zone.Item2);c.orthographicSize=Mathf.Max(36,zone.Item3);Shot(c,zone.Item1.Replace(' ','_'));}c.transform.position=new Vector3(110,160,-75);c.transform.LookAt(new Vector3(42,0,z/2));c.orthographicSize=Mathf.Max(65,z*.56f);
  EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),"Assets/ArtPreview/Scenes/ArtLibraryPreview.unity");
 }
 static string Find(string pack,params string[] terms){return models.FirstOrDefault(p=>p.Contains("/"+pack+"/")&&terms.Any(t=>Path.GetFileNameWithoutExtension(p).Equals(t,StringComparison.OrdinalIgnoreCase)))??models.FirstOrDefault(p=>p.Contains("/"+pack+"/")&&terms.Any(t=>Path.GetFileNameWithoutExtension(p).IndexOf(t,StringComparison.OrdinalIgnoreCase)>=0));}
 static float Y(float x,float z){return .2f+Mathf.SmoothStep(0,1,Mathf.InverseLerp(5,48,z))*2.6f+Mathf.Sin(x*.12f)*Mathf.Sin(z*.09f)*.24f;}
 static float PathX(float z){return Mathf.Sin(z*.075f)*3;}
 static void AnimationPreview(){
  EditorSceneManager.OpenScene("Assets/ArtPreview/Scenes/ArtLibraryPreview.unity");var lines=new List<string>{"model\tclip\tavatar_valid\thuman\tduration_seconds"};
  var source=models.First(p=>p.Contains("UniversalAnimationLibrary")&&!p.Contains("_RM"));var clips=AssetDatabase.LoadAllAssetsAtPath(source).OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__preview")).ToArray();
  File.WriteAllLines("Docs/Art/ANIMATION_CLIPS.txt",clips.Select(c=>c.name+"\t"+c.length));
  var wanted=new[]{"|Idle_Loop","|Walk_Loop","|Sprint_Loop"};var selected=wanted.Select(w=>clips.FirstOrDefault(c=>c.name.IndexOf(w,StringComparison.OrdinalIgnoreCase)>=0)).Where(c=>c!=null).ToArray();
  string controllerPath="Assets/Art/Animations/LocomotionPreview.controller";var controller=AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(controllerPath);
  if(!controller)controller=UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(controllerPath);foreach(var st in controller.layers[0].stateMachine.states)controller.layers[0].stateMachine.RemoveState(st.state);{UnityEditor.Animations.AnimatorState last=null,first=null;foreach(var clip in selected){var state=controller.AddMotion(clip);if(first==null)first=state;if(last!=null){var transition=last.AddTransition(state);transition.hasExitTime=true;transition.exitTime=1;transition.duration=.2f;}last=state;}if(last!=null&&first!=last){var transition=last.AddTransition(first);transition.hasExitTime=true;transition.exitTime=1;transition.duration=.2f;}}
  var character=models.First(p=>p.Contains("UniversalBaseCharacters")&&p.Contains("Male_FullBody"));var baseGo=GameObject.Find("07 Characters");float z=10;Ground("Animation preview floor",new Vector3(114,-.03f,16),new Vector3(3,1,3),Mat("Animation floor",new Color(.65f,.72f,.67f)));
  for(int i=0;i<selected.Length;i++){var clip=selected[i];var g=Spawn(character,new Vector3(110+i*4,0,z+4),0,180);g.name="Retarget preview - "+clip.name;var animator=g.GetComponent<Animator>();animator.runtimeAnimatorController=controller;animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;animator.Rebind();animator.Play(clip.name,0,.35f);animator.Update(0);lines.Add(character+"\t"+clip.name+"\t"+animator.avatar.isValid+"\t"+animator.avatar.isHuman+"\t"+clip.length);Label(clip.name.Split('|').Last(),new Vector3(110+i*4,.02f,z+2),.35f);}
  File.WriteAllLines("Docs/Art/ANIMATION_PREVIEW.tsv",lines);var c=UnityEngine.Object.FindFirstObjectByType<Camera>();c.orthographic=false;c.transform.position=new Vector3(114,3.5f,z-7);c.transform.LookAt(new Vector3(114,1,z+4));Shot(c,"Humanoid_animation_preview");EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
  EditorSceneManager.OpenScene("Assets/Scenes/ArtTest/BeachForest_ArtTest.unity");
  Physics.SyncTransforms();int hits=0;float maxStep=0;float previous=0;for(int i=0;i<=150;i++){float zz=-12+i*.48f;var ray=new Ray(new Vector3(PathX(zz),20,zz),Vector3.down);if(Physics.Raycast(ray,out var hit,40)&&hit.collider.gameObject.name=="Continuous walkable beach and forest"){hits++;if(i>0)maxStep=Mathf.Max(maxStep,Mathf.Abs(hit.point.y-previous));previous=hit.point.y;}}
  File.WriteAllText("Docs/Art/SCENE_VALIDATION.txt","Ground path raycasts: "+hits+"/151\nMaximum height difference per 0.48 m sample: "+maxStep+" m\nModels: "+models.Count+"\nPipeline: Built-in\nUnity: "+Application.unityVersion+"\n");
  EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/ArtTest/BeachForest_ArtTest.unity",true),new EditorBuildSettingsScene("Assets/ArtPreview/Scenes/ArtLibraryPreview.unity",true)};
 }
 static void Beach(){EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);UnityEngine.Random.InitState(4921);
  var sand=Mat("Warm sand",new Color(.87f,.73f,.43f));var grass=Mat("Forest ground",new Color(.31f,.46f,.18f));var dirt=Mat("Earth path",new Color(.56f,.39f,.22f));var water=Mat("Turquoise sea",new Color(.13f,.64f,.72f));
  // A single indexed surface carries both biomes and their walkable transition.
  int nx=100,nz=120;float xmin=-42,zmin=-18,dx=.84f,dz=.72f;var vv=new List<Vector3>();var uv=new List<Vector2>();var triangles=new[]{new List<int>(),new List<int>(),new List<int>()};for(int j=0;j<=nz;j++)for(int i=0;i<=nx;i++){float x=xmin+i*dx,z=zmin+j*dz;vv.Add(new Vector3(x,Y(x,z),z));uv.Add(new Vector2(x*.1f,z*.1f));}
  for(int j=0;j<nz;j++)for(int i=0;i<nx;i++){float x=xmin+(i+.5f)*dx,z=zmin+(j+.5f)*dz;float edge=9+Mathf.Sin(x*.22f)*2+Mathf.Cos(x*.4f);int sub=z<edge?0:Mathf.Abs(x-PathX(z))<2.1f?2:1;int k=j*(nx+1)+i;triangles[sub].AddRange(new[]{k,k+nx+1,k+1,k+1,k+nx+1,k+nx+2});}
  var tex=new Texture2D(512,512,TextureFormat.RGB24,false);for(int j=0;j<512;j++)for(int i=0;i<512;i++){float x=xmin+i/511f*nx*dx,z=zmin+j/511f*nz*dz;float noise=Mathf.PerlinNoise(x*.29f+30,z*.29f+30);float edge=10+Mathf.Sin(x*.22f)*2+Mathf.Cos(x*.4f)+(noise-.5f)*3;float blend=Mathf.SmoothStep(0,1,Mathf.InverseLerp(edge-2,edge+3,z));Color col=Color.Lerp(sand.color,grass.color,blend);float pathWidth=1.6f+(noise-.5f)*.6f;float track=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(pathWidth,pathWidth+1,Mathf.Abs(x-PathX(z))));col=Color.Lerp(col,dirt.color,track*Mathf.SmoothStep(0,1,Mathf.InverseLerp(6,18,z)));col*=.94f+noise*.12f;tex.SetPixel(i,j,col);}tex.Apply();File.WriteAllBytes("Assets/ArtPreview/Materials/GroundColor.png",tex.EncodeToPNG());AssetDatabase.ImportAsset("Assets/ArtPreview/Materials/GroundColor.png");var gm=Mat("Continuous ground",Color.white);gm.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/ArtPreview/Materials/GroundColor.png");uv.Clear();for(int j=0;j<=nz;j++)for(int i=0;i<=nx;i++)uv.Add(new Vector2(i/(float)nx,j/(float)nz));
  var mesh=new Mesh();mesh.name="Continuous beach to forest surface";mesh.SetVertices(vv);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles.SelectMany(t=>t).ToArray(),0);mesh.RecalculateNormals();mesh.RecalculateBounds();string meshPath="Assets/ArtPreview/ContinuousGround.asset";var stored=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);if(stored){stored.Clear();stored.vertices=mesh.vertices;stored.uv=mesh.uv;stored.triangles=mesh.triangles;stored.RecalculateNormals();stored.RecalculateBounds();EditorUtility.SetDirty(stored);mesh=stored;}else AssetDatabase.CreateAsset(mesh,meshPath);var ground=new GameObject("Continuous walkable beach and forest");ground.AddComponent<MeshFilter>().sharedMesh=mesh;ground.AddComponent<MeshRenderer>().sharedMaterial=gm;ground.AddComponent<MeshCollider>().sharedMesh=mesh;
  Ground("Sea",new Vector3(0,.03f,-78),new Vector3(38,1,12),water);
  Action<string,float,float,float,float> place=(p,x,z,h,yaw)=>{if(p==null)return;var g=Spawn(p,new Vector3(x,Y(x,z)-.035f,z),h,yaw);if(g){string pp="Assets/Art/Prefabs/"+p.Split('/')[4]+"_"+Path.GetFileNameWithoutExtension(p)+".prefab";if(!File.Exists(pp)){var original=Spawn(p,Vector3.zero);PrefabUtility.SaveAsPrefabAsset(original,pp);UnityEngine.Object.DestroyImmediate(original);}}};
  var palms=new[]{Find("PirateKit","palm-detailed-bend"),Find("PirateKit","palm-detailed-straight"),Find("PirateKit","palm-bend")}.Where(p=>p!=null).ToArray();
  for(int i=0;i<12&&palms.Length>0;i++){float x=(i%2==0?-1:1)*UnityEngine.Random.Range(8f,29f),z=UnityEngine.Random.Range(-9f,11f);place(palms[i%palms.Length],x,z,UnityEngine.Random.Range(5.5f,8f),UnityEngine.Random.Range(0,360));}
  var trees=models.Where(p=>(p.Contains("StylizedNatureMegaKit")||p.Contains("ForestNature"))&&(Path.GetFileName(p).StartsWith("CommonTree")||Path.GetFileName(p).ToLower().Contains("tree_oak"))).ToArray();if(trees.Length==0)trees=models.Where(p=>p.Contains("StylizedNatureMegaKit")&&Path.GetFileName(p).StartsWith("CommonTree")).ToArray();
  for(int i=0;i<112&&trees.Length>0;i++){float z=UnityEngine.Random.Range(12,65),x=UnityEngine.Random.Range(-36,36);if(Mathf.Abs(x-PathX(z))<4)continue;place(trees[i%trees.Length],x,z,UnityEngine.Random.Range(7.5f,12f),UnityEngine.Random.Range(0,360));}
  var plants=models.Where(p=>(p.Contains("StylizedNatureMegaKit")||p.Contains("ForestNature"))&&Cat(p)=="02 Plants"&&!Path.GetFileName(p).ToLower().Contains("dead")).ToArray();for(int i=0;i<180&&plants.Length>0;i++){float x=UnityEngine.Random.Range(-32,32),z=UnityEngine.Random.Range(9,65);if(Mathf.Abs(x-PathX(z))<2.8f)continue;place(plants[i%plants.Length],x,z,UnityEngine.Random.Range(.35f,1.3f),UnityEngine.Random.Range(0,360));}
  var rocks=models.Where(p=>p.Contains("StylizedNatureMegaKit")&&Path.GetFileName(p).StartsWith("Rock_Medium")).ToArray();for(int i=0;i<28&&rocks.Length>0;i++){float x=UnityEngine.Random.Range(-32,32),z=UnityEngine.Random.Range(-12,56);if(Mathf.Abs(x-PathX(z))<4)continue;place(rocks[i%rocks.Length],x,z,UnityEngine.Random.Range(.5f,1.65f),UnityEngine.Random.Range(0,360));}
  place(Find("PirateKit","ship-wreck","ship-wreckage","ship-small"),-13,-9,4.5f,32);place(Find("WatercraftKit","boat-row-small"),-6,-13,.9f,-22);
  for(int i=0;i<5;i++)place(Find("PirateKit",i%2==0?"crate":"barrel"),-9+i*1.2f,-5+(i%2)*1.4f,.65f,i*37);
  for(int i=0;i<5;i++)place(Find("PirateKit","planks","plank","wood"),-15+i*1.4f,-7+i*.4f,.2f,i*27);
  var c=Lighting(new Vector3(35,28,-39),new Vector3(0,2,17));Shot(c,"BeachForest_ArtTest");c.transform.position=new Vector3(0,4.3f,-7);c.transform.LookAt(new Vector3(0,3,29));Shot(c,"Beach_to_forest_path");c.transform.position=new Vector3(24,17,27);c.transform.LookAt(new Vector3(-7,1,-8));Shot(c,"Forest_to_beach");c.transform.position=new Vector3(35,28,-39);c.transform.LookAt(new Vector3(0,2,17));EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),"Assets/Scenes/ArtTest/BeachForest_ArtTest.unity");
 }
}


