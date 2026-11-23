#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;using System.IO;using System.Collections;using System.Diagnostics;using System.Reflection;using System.Linq;using System.Runtime.InteropServices;using UnityEngine;
namespace IslandSystem {
// Opt-in real-player integration review. Its entire window tree is owned by a
// hidden harness parent; no desktop input, foreground activation or screenshots.
public class ScratchWindowReview:MonoBehaviour {
 const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
 [StructLayout(LayoutKind.Sequential)]struct RectWin{public int left,top,right,bottom;}
 [StructLayout(LayoutKind.Sequential)]struct Point{public int x,y;}
 delegate bool EnumCallback(IntPtr h,IntPtr p);
 [DllImport("user32.dll")]static extern bool EnumChildWindows(IntPtr parent,EnumCallback callback,IntPtr p);
 [DllImport("user32.dll")]static extern uint GetWindowThreadProcessId(IntPtr h,out uint id);
 [DllImport("user32.dll")]static extern IntPtr GetParent(IntPtr h);
 [DllImport("user32.dll")]static extern bool GetWindowRect(IntPtr h,out RectWin r);
 [DllImport("user32.dll")]static extern bool GetClientRect(IntPtr h,out RectWin r);
 [DllImport("user32.dll")]static extern bool ScreenToClient(IntPtr h,ref Point p);
 [DllImport("user32.dll")]static extern int GetWindowLong(IntPtr h,int n);
 [DllImport("user32.dll")]static extern bool SetWindowPos(IntPtr h,IntPtr after,int x,int y,int w,int height,uint flags);
 [StructLayout(LayoutKind.Sequential)]struct ThreadInfo{public int size,flags;public IntPtr active,focus,capture,menuOwner,moveSize,caret;public RectWin caretRect;}
 [DllImport("user32.dll")]static extern bool GetGUIThreadInfo(uint id,ref ThreadInfo info);
 [DllImport("user32.dll")]static extern IntPtr SetFocus(IntPtr h);
 [DllImport("user32.dll")]static extern IntPtr GetForegroundWindow();
 IntPtr ThreadFocus(IntPtr window){uint pid;uint thread=GetWindowThreadProcessId(window,out pid);var info=new ThreadInfo{size=Marshal.SizeOf(typeof(ThreadInfo))};if(!GetGUIThreadInfo(thread,ref info))throw new Exception("Cannot inspect hidden window input focus");return info.focus;}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Install(){if(Array.IndexOf(Environment.GetCommandLineArgs(),"-scratch-window-review")>=0)new GameObject("Hidden player window review").AddComponent<ScratchWindowReview>();}
 T Get<T>(ScratchWorkbenchHost host,string name){return (T)typeof(ScratchWorkbenchHost).GetField(name,Private).GetValue(host);}
 void Call(ScratchWorkbenchHost host,string name,params object[] args){typeof(ScratchWorkbenchHost).GetMethod(name,Private).Invoke(host,args);}
 IEnumerator Start(){yield return null;yield return null;var host=FindAnyObjectByType<ScratchWorkbenchHost>();if(!host){Fail("No Scratch host installed in player");yield break;}var walkers=FindObjectsByType<LighthouseWalker>();var walkerStates=walkers.Select(w=>w.enabled).ToArray();bool originalBackground=Application.runInBackground;float originalTime=Time.timeScale;host.SuppressFocus=true;typeof(ScratchWorkbenchHost).GetField("path",Private).SetValue(host,Path.GetFullPath("Logs/window-review-draft.json"));Call(host,"Toggle",true);if(walkers.Any(w=>w.enabled)){Fail("Walker input was not blocked on open");yield break;}float deadline=Time.realtimeSinceStartup+30;while(!Get<bool>(host,"nativeReady")&&Time.realtimeSinceStartup<deadline)yield return null;if(!Get<bool>(host,"nativeReady")){Fail("Native UI did not become ready: "+Get<string>(host,"error"));yield break;}var main=Get<IntPtr>(host,"nativeWindow");var childProcess=Get<Process>(host,"child");IntPtr panel=IntPtr.Zero;EnumChildWindows(main,(h,p)=>{uint id;GetWindowThreadProcessId(h,out id);if(id==childProcess.Id&&GetParent(h)==main){panel=h;return false;}return true;},IntPtr.Zero);if(panel==IntPtr.Zero){Fail("No native panel attached to Unity window");yield break;}var motor=FindAnyObjectByType<ActionMotor>();if(motor&&!motor.InputBlocked){Fail("Action motor input not blocked while workbench open");yield break;}string report="PASS Official Scratch loaded in actual hidden Unity player\n";int[] sizes={1280,720,960,540,1600,900};for(int i=0;i<sizes.Length;i+=2){SetWindowPos(main,IntPtr.Zero,0,0,sizes[i],sizes[i+1],0x0016);float until=Time.realtimeSinceStartup+1;while(Time.realtimeSinceStartup<until)yield return null;RectWin parentRect,panelRect;GetClientRect(main,out parentRect);GetWindowRect(panel,out panelRect);var p=new Point{x=panelRect.left,y=panelRect.top};ScreenToClient(main,ref p);int w=parentRect.right,h=parentRect.bottom;float scale=Mathf.Min(w/1280f,h/720f);int pw=Mathf.RoundToInt(1120*scale),ph=Mathf.RoundToInt(620*scale);string actual="client="+w+"x"+h+" panel="+p.x+","+p.y+" "+(panelRect.right-panelRect.left)+"x"+(panelRect.bottom-panelRect.top);UnityEngine.Debug.Log("SCRATCH_WINDOW_REVIEW "+actual);if(p.x!=(w-pw)/2||p.y!=(h-ph)/2||panelRect.right-panelRect.left!=pw||panelRect.bottom-panelRect.top!=ph||(GetWindowLong(panel,-16)&0x10000000)==0){Fail("Panel outside client or not shown: "+actual);yield break;}report+="PASS Centered visible child at "+actual+"\n";}
 string capture=Path.GetFullPath("Logs/ScratchPlayerEmbedded.png");if(File.Exists(capture))File.Delete(capture);Call(host,"Send","SNAP|"+capture);deadline=Time.realtimeSinceStartup+8;while(!File.Exists(capture)&&Time.realtimeSinceStartup<deadline)yield return null;if(!File.Exists(capture)){Fail("No native content capture");yield break;}report+="PASS Captured embedded WebView content directly, no desktop screenshot\n";
 // Focus only our invisible child-window queue. Never activate a desktop window
 // or inject physical keyboard/mouse input.
 IntPtr foreground=GetForegroundWindow();SetFocus(panel);if(ThreadFocus(main)!=panel){Fail("Could not reproduce keyboard focus remaining in native panel");yield break;}
 Call(host,"Toggle",false);deadline=Time.realtimeSinceStartup+5;while(Get<bool>(host,"restoringInput")&&Time.realtimeSinceStartup<deadline)yield return null;
 if(Get<bool>(host,"restoringInput")||(GetWindowLong(panel,-16)&0x10000000)!=0||ThreadFocus(main)!=main||!walkers.Select((w,i)=>w.enabled==walkerStates[i]).All(x=>x)||Application.runInBackground!=originalBackground){Fail("Direct close did not restore native focus, walker or background state");yield break;}
 if(motor&&motor.InputBlocked){Fail("Action motor stayed blocked after native close");yield break;}report+="PASS Close waits for native hide acknowledgement and restores Unity keyboard focus\nPASS Original walker enabled states and background setting restored after close\n";
 Call(host,"Toggle",true);deadline=Time.realtimeSinceStartup+5;while((GetWindowLong(panel,-16)&0x10000000)==0&&Time.realtimeSinceStartup<deadline)yield return null;if((GetWindowLong(panel,-16)&0x10000000)==0){Fail("Panel did not reopen");yield break;}
 SetFocus(panel);string endpoint=Get<string>(host,"prefix")+"rpc";var closeRequest=System.Threading.Tasks.Task.Run(()=>{using(var web=new System.Net.WebClient()){web.Headers["Content-Type"]="application/json";return web.UploadString(endpoint,"{\"type\":\"close\"}");}});deadline=Time.realtimeSinceStartup+5;while((!closeRequest.IsCompleted||Get<bool>(host,"restoringInput")||Get<bool>(host,"opened"))&&Time.realtimeSinceStartup<deadline)yield return null;
 if(closeRequest.IsFaulted||!closeRequest.IsCompleted||Get<bool>(host,"opened")||Get<bool>(host,"restoringInput")||ThreadFocus(main)!=main||walkers.Select((w,i)=>w.enabled==walkerStates[i]).Any(x=>!x)||Application.runInBackground!=originalBackground||Time.timeScale!=originalTime){Fail("Browser RPC close did not restore control and keyboard focus");yield break;}
 if(motor&&motor.InputBlocked){Fail("Action motor stayed blocked after browser close");yield break;}if(GetForegroundWindow()!=foreground){Fail("Hidden review changed desktop foreground window");yield break;}
 report+="PASS Reopen then browser RPC close restores native keyboard focus and original input states\nPASS World timeScale unchanged; desktop foreground untouched; no OS key/mouse simulation\n";File.WriteAllText("Logs/ScratchPlayerWindowReview.txt",report);Application.Quit(0);
 }
 void Fail(string error){File.WriteAllText("Logs/ScratchPlayerWindowReview.txt","FAIL "+error);UnityEngine.Debug.LogError(error);Application.Quit(3);}
}
}
#endif


