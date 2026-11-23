#if UNITY_EDITOR
using System;using System.IO;using UnityEditor;using UnityEditor.Build.Reporting;
namespace IslandSystem {public static class ScratchPlayerBuild{public static void Run(){var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/Lighthouse_Coast_ArtBenchmark_V01.unity"},locationPathName="Builds/IslandScratchTrial/IslandScratchTrial.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});if(result.summary.result!=BuildResult.Succeeded)throw new Exception("Scratch trial build failed");File.WriteAllText("Logs/ScratchBuildSuccess.txt",DateTime.UtcNow.ToString("O"));}}}
#endif
