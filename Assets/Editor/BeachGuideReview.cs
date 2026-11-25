using System;
using System.IO;
using System.Reflection;
using IslandSystem;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class BeachGuideReview
{
    const string Key = "BeachGuideReview";
    const string Folder = "Docs/IslandSystem/BeachChapter";
    const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    static BeachGuideReview() => EditorApplication.playModeStateChanged += State;

    public static void Run()
    {

        EditorSceneManager.OpenScene("Assets/Scenes/Island_World_V01.unity");
        SessionState.SetBool(Key,true);
        EditorApplication.isPlaying = true;
    }

    static void State(PlayModeStateChange state)
    {
        if (!SessionState.GetBool(Key,false) || state != PlayModeStateChange.EnteredPlayMode) return;
        double ready = EditorApplication.timeSinceStartup + 1;
        void Check()
        {
            if (EditorApplication.timeSinceStartup < ready) return;
            EditorApplication.update -= Check;
            var chapter = UnityEngine.Object.FindAnyObjectByType<BeachChapter>();
            chapter.StartCoroutine(RunReview());
        }
        EditorApplication.update += Check;
    }

    static System.Collections.IEnumerator RunReview()
    {
        var stack=new System.Collections.Generic.Stack<System.Collections.IEnumerator>();
        stack.Push(Verify());
        while(stack.Count>0)
        {
        var sequence=stack.Peek();

            bool more;
            try { more=sequence.MoveNext(); }
            catch(Exception error)
            {
                Debug.LogException(error);
                EditorApplication.Exit(2);
                yield break;
            }
            if(!more) { stack.Pop(); continue; }
            if(sequence.Current is System.Collections.IEnumerator nested) stack.Push(nested);
            else yield return sequence.Current;
        }
        Debug.Log("BEACH_CHAPTER_PASS");
        EditorApplication.Exit(0);
    }

    static System.Collections.IEnumerator Verify()
    {
        var legacy = UnityEngine.Object.FindAnyObjectByType<Workbench>();
        if (legacy) legacy.enabled=false;
        var guide = UnityEngine.Object.FindAnyObjectByType<BeachSurvival>();
        var motor = UnityEngine.Object.FindAnyObjectByType<ActionMotor>();
        var controller = motor.GetComponent<CharacterController>();
        var type = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
        var view = EditorWindow.GetWindow(type);
        Directory.CreateDirectory(Folder);
        var chapter=guide.Chapter;
        Shot("01-Storm",1280,720);
        yield return WaitStep(1);
        yield return new WaitForSecondsRealtime(.2f);
        Shot("02-Wake",1280,720);
        yield return new WaitForSecondsRealtime(1.25f);
        Shot("02b-Getting-Up",1280,720);
        yield return WaitStep(2);
        motor.Orbit(24,0,0);
        yield return WaitStep(3);
        controller.Move(Vector3.forward*2.4f);
        yield return WaitStep(4);
        Shot("03-Wreck-Goal",1280,720);
        motor.Teleport(chapter.approach.position);
        yield return WaitStep(5);
        motor.Teleport(guide.wreckMarker.position);
        Require(guide.TryInteract(motor) && guide.WreckChecked,"inspect wreck");
        yield return WaitStep(6);
        Shot("04-Awakening",1280,720);
        yield return new WaitForSecondsRealtime(9);
        Shot("05-Scan",1280,720);
        yield return WaitStep(7);
        yield return new WaitForSecondsRealtime(.65f);
        Shot("05b-Blocked-Supplies",1280,720);
        motor.Teleport(chapter.launch.position + Vector3.back * .8f);
        yield return WaitStep(8);
        Shot("06-First-Program",1280,720);
        Require(chapter.ManualLocked && !chapter.ManualJumpAvailable &&
            Vector3.Dot(motor.transform.forward,Vector3.right)>.99f,"guided facing and input lock");
        var runner=motor.GetComponent<ActionProgramRunner>();
        var draft=new Draft();
        draft.blocks.Add(new Block{kind=0});
        draft.blocks.Add(new Block{kind=13});
        draft.blocks.Add(new Block{kind=12});
        draft.blocks.Add(new Block{kind=13});
        draft.blocks[2].kind=8;
        Require(chapter.LessonIssue(draft)!=null,"vertical jump is not the lesson action");
        draft.blocks[2].kind=12;
        runner.Execute(draft);
        yield return WaitStep(9);
        while(runner.Running) yield return null;
        Shot("07-Supply-Arrival",1280,720);
        Require(guide.TryInteract(motor) && chapter.Step==10 && guide.Wood==3,"claim supply at "+motor.transform.position+" target "+chapter.supplies.position+" step "+chapter.Step+" wood "+guide.Wood+" blocked "+motor.InputBlocked);
        Require(chapter.ManualJumpAvailable && !chapter.ManualLocked,"jump unlocked");
        var originalPosition=motor.transform.position;
        var verticalJump=new Draft();
        verticalJump.blocks.Add(new Block{kind=0});
        verticalJump.blocks.Add(new Block{kind=8});
        runner.Execute(verticalJump);
        while(runner.Running) yield return null;
        var delta=motor.transform.position-originalPosition;
        Require(new Vector2(delta.x,delta.z).magnitude<.12f && string.IsNullOrEmpty(runner.LastError),
            "standard jump stays vertical");
        guide.BagOpen=true;
        Shot("08-Supplies",1280,720);
        guide.BagOpen=false;
        yield return WaitStep(11);
        Require(chapter.Goal==null,"camp hint waits");
        yield return new WaitForSecondsRealtime(BeachChapter.CampHintDelay+.2f);
        Require(chapter.Goal==guide.hearth.transform,"campfire hint");
        motor.Teleport(guide.hearth.transform.position);
        yield return null;
        Shot("09-Campfire-Hint",1280,720);
        Require(guide.TryInteract(motor) && guide.FireStarted && guide.Wood==2,"campfire uses one wood");
        yield return WaitStep(12);
        motor.Teleport(guide.shelter.position);
        Require(guide.TryInteract(motor) && guide.CampChecked,"check camp shelter");
        yield return WaitStep(13);
        motor.Teleport(guide.exit.position);
        yield return WaitStep(14);
        yield return new WaitForSecondsRealtime(.45f);
        Shot("10-Chapter-Complete",1280,720);

        System.Collections.IEnumerator WaitStep(int step)
        {
            float deadline=Time.realtimeSinceStartup+19;
            while(chapter.Step<step && Time.realtimeSinceStartup<deadline) yield return null;
            Require(chapter.Step==step,"chapter step "+step+" actual "+chapter.Step+" "+chapter.Feedback+" at "+motor.transform.position);
        }
        void Shot(string file,int width,int height)
        {
            view.position = new Rect(0,0,width,height+21);
            type.GetProperty("targetSize",Flags).SetValue(view,new Vector2(width,height));
            view.SendEvent(new Event { type = EventType.Repaint });
            var frame = (RenderTexture)type.GetMethod("RenderView",Flags)
                .Invoke(view,new object[] { Vector2.zero,false });
            Require(frame && frame.width == width && frame.height == height,"game view dimensions");
            var old = RenderTexture.active;
            RenderTexture.active = frame;
            var image = new Texture2D(width,height,TextureFormat.RGB24,false);
            image.ReadPixels(new Rect(0,0,width,height),0,0);
            var pixels = image.GetPixels32();
            var flipped = new Color32[pixels.Length];
            for (int row=0; row<height; row++)
                Array.Copy(pixels,row*width,flipped,(height-row-1)*width,width);
            image.SetPixels32(flipped);
            image.Apply();
            File.WriteAllBytes(Path.Combine(Folder,file+".png"),image.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(image);
            RenderTexture.active = old;
        }
    }

    static void Require(bool pass,string message)
    {
        if (!pass) throw new InvalidOperationException(message);
    }
}
