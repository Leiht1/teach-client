#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
namespace IslandSystem {
public static class WorkbenchPlayerBuild {
 public static void Run(){
  string output=Path.GetFullPath("Builds/IslandSystemTrial/IslandSystemTrial.exe");
  Directory.CreateDirectory(Path.GetDirectoryName(output));
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
   scenes=new[]{"Assets/Scenes/Lighthouse_Coast_ArtBenchmark_V01.unity"},
   locationPathName=output,target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development
  });
  if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Trial build failed: "+report.summary.result);
  File.WriteAllText("Logs/IslandSystem-trial-build-success.txt",output);
 }
}
}
#endif
