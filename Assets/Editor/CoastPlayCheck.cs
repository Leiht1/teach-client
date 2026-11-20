using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;
[InitializeOnLoad] public static class CoastPlayCheck { static CoastPlayCheck(){EditorApplication.playModeStateChanged+=state=>{if(state==PlayModeStateChange.EnteredEditMode&&SessionState.GetBool("CoastAudit",false)){SessionState.SetBool("CoastAudit",false);EditorApplication.delayCall+=()=>EditorApplication.Exit(0);}};}
 [System.Serializable]class Route {public Vector3[] points;}
 public static void Begin(){EditorSceneManager.OpenScene(CoastAuthored.Scene);var p=GameObject.Find("Explorer");var points=new List<Vector3>();var raw=File.ReadAllText("ArtSource/Lighthouse_Authored/route.json");var matches=System.Text.RegularExpressions.Regex.Matches(raw,@"\[\s*([-\d.Ee]+),\s*([-\d.Ee]+),\s*([-\d.Ee]+)\s*\]");foreach(System.Text.RegularExpressions.Match m in matches)points.Add(new Vector3(float.Parse(m.Groups[1].Value,System.Globalization.CultureInfo.InvariantCulture),float.Parse(m.Groups[2].Value,System.Globalization.CultureInfo.InvariantCulture),float.Parse(m.Groups[3].Value,System.Globalization.CultureInfo.InvariantCulture)));var a=p.AddComponent<CoastWalkAudit>();a.waypoints=points.ToArray();EditorApplication.playModeStateChanged+=state=>{if(state==PlayModeStateChange.EnteredEditMode)EditorApplication.delayCall+=()=>EditorApplication.Exit(0);};SessionState.SetBool("CoastAudit",true);EditorApplication.isPlaying=true;}
}

