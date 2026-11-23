using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
class HostForm:Form {protected override bool ShowWithoutActivation{get{return true;}}}
class Host {
 [DllImport("user32.dll")] static extern bool SetProcessDpiAwarenessContext(IntPtr context);
 [DllImport("user32.dll",SetLastError=true)] static extern IntPtr SetParent(IntPtr child,IntPtr parent);
 [DllImport("user32.dll")] static extern IntPtr GetParent(IntPtr child);
 [DllImport("user32.dll")] static extern int GetWindowLong(IntPtr h,int n);
 [DllImport("user32.dll")] static extern int SetWindowLong(IntPtr h,int n,int value);
 [DllImport("user32.dll")] static extern bool SetForegroundWindow(IntPtr h);
 [STAThread] static void Main(string[] args){try{Run(args);}catch(Exception e){Console.Error.WriteLine(e);}}
 static void Run(string[] args){try{SetProcessDpiAwarenessContext(new IntPtr(-4));}catch{}Application.EnableVisualStyles();IntPtr parent=new IntPtr(long.Parse(args[0]));bool background=args.Length>3&&args[3]=="background";
 var form=new HostForm{FormBorderStyle=FormBorderStyle.None,ShowInTaskbar=false,AutoScaleMode=AutoScaleMode.None,StartPosition=FormStartPosition.Manual,Width=1120,Height=620};
 // Let WinForms create an actual child control. Changing only WS_CHILD on a
 // top-level Form lets its first Show apply top-level screen positioning.
 if(parent!=IntPtr.Zero)form.TopLevel=false;
 var view=new WebView2{Dock=DockStyle.Fill};form.Controls.Add(view);var handle=form.Handle;
 if(parent!=IntPtr.Zero){SetParent(handle,parent);if(GetParent(handle)!=parent)throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(),"Cannot attach Scratch child window");}
 bool shown=false;int visualVersion=0;var context=new ApplicationContext();form.FormClosed+=(s,e)=>context.ExitThread();
 Action<string> command=async line=>{try{var p=line.Split('|');if(p[0]=="SHOW"){if(shown)return;shown=true;int version=++visualVersion;if(view.CoreWebView2!=null)await view.CoreWebView2.ExecuteScriptAsync("(()=>{window.relicEntrance?.cancel();document.body.style.opacity='0';window.relicEntrance=document.body.animate([{opacity:0,transform:'translateY(12px) scale(.97)'},{opacity:1,transform:'none'}],{duration:180,easing:'ease-out',fill:'both'});window.relicEntrance.pause();window.relicEntrance.currentTime=0;getComputedStyle(document.body).opacity;})()");if(!shown||version!=visualVersion)return;Console.Out.WriteLine("ENTRANCE_PREPARED");Console.Out.Flush();form.Show();if(!background)view.Focus();if(view.CoreWebView2!=null)await view.CoreWebView2.ExecuteScriptAsync("requestAnimationFrame(()=>{document.body.style.opacity='1';window.relicEntrance?.play();})");}if(p[0]=="HIDE"){shown=false;visualVersion++;form.Hide();Console.Out.WriteLine("HIDDEN|"+(p.Length>1?p[1]:"0"));Console.Out.Flush();}if(p[0]=="BOUNDS")form.SetBounds(int.Parse(p[1]),int.Parse(p[2]),int.Parse(p[3]),int.Parse(p[4]));if(p[0]=="QUIT")form.Close();if(p[0]=="SNAP")Capture(view,p[1]);}catch(Exception e){Console.Error.WriteLine(e);}};
 Task.Run(()=>{var input=new StreamReader(Console.OpenStandardInput(),System.Text.Encoding.UTF8);string line;while((line=input.ReadLine())!=null){string value=line;try{form.BeginInvoke(new Action(()=>command(value)));}catch{return;}}try{form.BeginInvoke(new Action(()=>form.Close()));}catch{}});
 form.BeginInvoke(new Action(async()=>{try{var env=await CoreWebView2Environment.CreateAsync(null,args[2]);await view.EnsureCoreWebView2Async(env);view.CoreWebView2.Settings.AreDevToolsEnabled=false;view.CoreWebView2.Settings.AreDefaultContextMenusEnabled=false;view.CoreWebView2.Settings.IsStatusBarEnabled=false;view.CoreWebView2.NewWindowRequested+=(s,e)=>e.Handled=true;view.CoreWebView2.PermissionRequested+=(s,e)=>e.State=CoreWebView2PermissionState.Deny;view.CoreWebView2.NavigationStarting+=(s,e)=>{if(!e.Uri.StartsWith(args[1].Split('?')[0]))e.Cancel=true;};view.Source=new Uri(args[1]);if(shown&&!background)view.Focus();}catch(Exception e){Console.Error.WriteLine(e);}}));Application.Run(context);
 }
 static async void Capture(WebView2 view,string path){try{using(var stream=File.Create(path+".tmp"))await view.CoreWebView2.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png,stream);if(File.Exists(path))File.Delete(path);File.Move(path+".tmp",path);}catch(Exception e){Console.Error.WriteLine(e);}}
}


