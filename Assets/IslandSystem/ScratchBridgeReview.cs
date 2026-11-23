#if UNITY_EDITOR
using System;using System.IO;using System.Linq;using System.Collections;using System.Diagnostics;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
namespace IslandSystem {
public class ScratchBridgeReview:MonoBehaviour {
 IEnumerator Start(){yield return null;var walkers=FindObjectsByType<LighthouseWalker>();var states=walkers.Select(w=>w.enabled).ToArray();float time=Time.timeScale;bool background=Application.runInBackground;var root=new GameObject("Background Scratch bridge");root.SetActive(false);var host=root.AddComponent<ScratchWorkbenchHost>();host.BackgroundReview=true;host.SavePathOverride=Path.GetFullPath("Logs/scratch-review-v2.json");root.SetActive(true);string url=host.ReviewStartBridge();host.ReviewToggle(true);if(walkers.Any(w=>w.enabled))throw new Exception("Input was not blocked");var process=Process.Start(new ProcessStartInfo("node", "\""+Path.GetFullPath("Tools/IslandScratchWeb/unity-review.mjs")+"\" "+url){UseShellExecute=false,CreateNoWindow=true,RedirectStandardError=true,RedirectStandardOutput=true});var output=process.StandardOutput.ReadToEndAsync();var err=process.StandardError.ReadToEndAsync();float deadline=Time.realtimeSinceStartup+90;while(!process.HasExited&&Time.realtimeSinceStartup<deadline)yield return null;bool good=process.HasExited&&process.ExitCode==0&&Application.runInBackground==background&&Time.timeScale==time&&walkers.Select((w,i)=>w.enabled==states[i]).All(x=>x);File.WriteAllText("Logs/ScratchBridgeProcess.txt",output.Result+err.Result);if(!good){UnityEngine.Debug.LogError("Scratch bridge review failed: "+err.Result);EditorApplication.Exit(2);yield break;}File.AppendAllText("Logs/ScratchBridgeReview.txt","PASS Original walker enabled states restored\nPASS World timeScale unchanged\n");EditorApplication.isPlaying=false;}
}
[InitializeOnLoad]public static class ScratchBridgeReviewEntry{
 static ScratchBridgeReviewEntry(){if(SessionState.GetBool("ScratchBridgeReview",false))EditorApplication.playModeStateChanged+=Attach;}
 public static void Run(){if(!Application.isBatchMode)throw new Exception("Background only");EditorSceneManager.OpenScene("Assets/Scenes/Lighthouse_Coast_ArtBenchmark_V01.unity");SessionState.SetBool("ScratchBridgeReview",true);EditorApplication.playModeStateChanged+=Attach;EditorApplication.isPlaying=true;}
 static void Attach(PlayModeStateChange s){if(s==PlayModeStateChange.EnteredPlayMode)new GameObject("Scratch background review").AddComponent<ScratchBridgeReview>();if(s==PlayModeStateChange.EnteredEditMode){SessionState.SetBool("ScratchBridgeReview",false);EditorApplication.Exit(0);}}
}
}
#endif

