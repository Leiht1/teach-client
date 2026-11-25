using UnityEngine;

namespace IslandSystem
{
    [RequireComponent(typeof(BeachSurvival))]
    public sealed class BeachChapter : MonoBehaviour
    {
        public Transform wreck, approach, launch, supplies;
        public int Step { get; private set; }
        public string Dialogue { get; private set; }
        public bool SystemSpeaker { get; private set; }
        public float Storm => Step == 0 ? Mathf.Clamp01(elapsed/6f) : -1;
        public bool Cinematic => Step == 0 || Step == 1 || Step == 6 || (Step == 7 && elapsed < 2.4f);
        public bool EditorAvailable => Step >= 8 && !(Step == 8 && runner && runner.Running);
        public bool Teaching => Step == 8;
        public bool ManualLocked => Cinematic || aligning || Teaching;
        public bool ManualJumpAvailable => Step >= 10;
        Transform stormShip;
        Vector3 stormOrigin;
        bool aligning;
        public string Feedback { get; private set; }
        public const float CampHintDelay = 8f;
        public Transform PriorityCampTarget => Step switch
        {
            10 when survival.Wood == 0 => survival.NearestWood(motor.transform.position),
            11 when !survival.FireStarted => survival.hearth.transform,
            12 when !survival.CampChecked => survival.shelter,
            13 => survival.exit,
            _ => null
        };
        public Transform Goal => Step switch { 4 => approach, 5 => survival.wreckMarker, 7 or 8 => launch, 9 => supplies, >=10 when elapsed >= CampHintDelay => PriorityCampTarget, _ => null };
        public string Objective => Step switch
        {
            2 => "环顾四周", 3 => "站起来，向前走几步", 4 => "前往船只残骸",
            5 => "靠近船骸，按 E 检查", 7 => "前往断梁前的沙地",
            8 => running ? "正在执行越障程序" : "按 Tab 打开系统，越过断梁",
            9 => "按 E 搜索补给箱", 10 => "拾取 1 个木材，用于点燃营火",
            11 => "按 E 点燃营火（用 1 木材）",
            12 => "靠近布棚，按 E 检查落脚点",
            13 => "沿海滩小路前往岛内", _ => ""
        };

        BeachSurvival survival;
        ActionMotor motor;
        ActionProgramRunner runner;
        float elapsed, yaw, lineEnds;
        Vector3 origin;
        int jumpCount;
        bool running, crossed;
        AudioSource surf;
        AudioClip surfClip;
        Quaternion wreckRotation;
        public float CompletionAge => Step == 14 ? elapsed : -1;

        void Awake()
        {
            survival = GetComponent<BeachSurvival>();
            var samples = new float[44100*3];
            var noise = new System.Random(328);
            float low=0;
            for (int i=0;i<samples.Length;i++)
            {
                low = Mathf.Lerp(low,(float)noise.NextDouble()*2-1,.11f);
                samples[i]=low*(.32f+.22f*Mathf.Sin(i*2*Mathf.PI/samples.Length));
            }
            surfClip=AudioClip.Create("风浪",samples.Length,1,44100,false);
            surfClip.SetData(samples,0);
            surf=gameObject.AddComponent<AudioSource>();
            surf.clip=surfClip;
            surf.loop=true;
            surf.volume=.56f;
            surf.Play();
        }

        void OnDestroy() { if(surfClip) Destroy(surfClip); }

        void Update()
        {
            if (!motor)
            {
                motor = FindAnyObjectByType<ActionMotor>();
                if (!motor) return;
                runner = motor.GetComponent<ActionProgramRunner>();
                origin = motor.transform.position;
                yaw = motor.Yaw;
                stormShip=Instantiate(wreck.gameObject,new Vector3(-620,-.65f,-620),wreck.rotation).transform;
                stormShip.name="海难序幕 · 外海船体";
                stormOrigin=stormShip.position;
                wreckRotation=stormShip.rotation;
                foreach(var collider in stormShip.GetComponentsInChildren<Collider>())
                    collider.enabled=false;

            }
            if (motor.FreeCameraActive) return;
            elapsed += Time.unscaledDeltaTime;
            if (Time.unscaledTime > lineEnds) Dialogue=null;

            if (Step==0)
                {
                stormShip.rotation=Quaternion.Euler(Mathf.Sin(elapsed*3)*6,0,Mathf.Sin(elapsed*2.1f)*9)*wreckRotation;
                stormShip.position=stormOrigin+Vector3.up*(Mathf.Sin(elapsed*2.6f)*1.1f);
            }

            switch (Step)
            {
                case 0 when elapsed >= 6:
                    Destroy(stormShip.gameObject);
                    surf.volume=.16f;
                    Go(1,"咳……咳咳……这里……是哪？",false,3);
                    break;
                case 1 when elapsed >= 3:
                    yaw=motor.Yaw;
                    Go(2);
                    break;
                case 2 when Mathf.Abs(Mathf.DeltaAngle(yaw,motor.Yaw)) >= 15:
                    origin=motor.transform.position;
                    Go(3,"海……还有沙滩。我被冲到什么地方了？");
                    break;
                case 3 when (motor.transform.position-origin).sqrMagnitude >= 4 && Dialogue == null:
                    Go(4,"那边……好像是我们的船。");
                    break;
                case 4 when At(approach,5) && Dialogue == null:
                    Go(5,"船已经毁成这样了……其他人呢？");
                    break;
                case 5 when survival.WreckChecked && Dialogue == null:
                    RelicCore.Instance?.Activate(true);
                    Go(6,"检测到生命体征。宿主状态异常。正在建立连接……",true,3);
                    break;
                case 6:
                    if (elapsed >= 13)
                    {
                        RelicCore.Instance?.Activate(false);
                        Go(7,"断开的船板围住了补给箱。先走近看看。",true);
                    }
                    else if (Dialogue == null)
                    {
                        if (elapsed < 5.5f) Say("什么声音？",false,2);
                        else if (elapsed < 8.4f) Say("连接建立。荒岛智能系统，初始化完成。",true,2.8f);
                        else if (elapsed < 10.5f) Say("你能帮我活下去？",false,2);
                        else Say("此处不适合长期停留。可组合行动指令，搜索物资。",true,2.5f);
                    }
                    break;
                case 7 when (aligning || At(launch,2.2f)) && Dialogue == null:
                    aligning=true;
                    if (motor.AlignForLesson(launch.position, supplies.position))
                    {
                        aligning=false;
                        survival.BagOpen=false;
                        Go(8,"断梁挡住了去路。可以组合动作，从这里越过去。按 Tab 打开系统。",true);
                    }
                    break;
                case 8:
                    if (runner && runner.Running && !running)
                    {
                        running=true;
                        crossed=false;
                        jumpCount=motor.Landings;
                        Feedback=null;
                    }
                    if (running && motor.Landings>jumpCount && OnSupplySide())
                        crossed=true;
                    if (running && !runner.Running)
                    {
                        running=false;
                        if (crossed && At(supplies,2.25f) && string.IsNullOrEmpty(runner.LastError)) Go(9,"找到补给了。先看看里面有什么。");
                        else { motor.Teleport(launch.position); Feedback=string.IsNullOrEmpty(runner.LastError) ?
                            "未能落在补给箱旁。已返回起跳点，按 Tab 重试。" : runner.LastError; }
                    }
                    break;
                case 10 when survival.FireStarted:
                case 11 when survival.FireStarted:
                    Go(12);
                    break;
                case 10 when survival.Wood > 0:
                    Go(11);
                    break;
                case 12 when survival.CampChecked:
                    Go(13,"营地安顿好了。沿着海滩的小路往岛内看看。");
                    break;
                case 13 when At(survival.exit,5):
                    Go(14);
                    break;
            }
        }

        public ProgramIssue LessonIssue(Draft draft)
        {
            if (!Teaching) return null;
            var steps=draft.blocks;
            if (steps.Count == 4 && steps[0].kind == 0 &&
                steps[1].kind == 13 && steps[2].kind == 12 && steps[3].kind == 13)
                return null;
            return new ProgramIssue(null,"请按当前指引连接四块积木：启动、靠近断梁、向前跳、走向补给箱。");
        }

        void LateUpdate()
        {
            if (Step != 1 || !motor || !motor.animator || !motor.animator.isHuman) return;
            var rig=motor.animator;
            var root=motor.transform;
            var hip=rig.GetBoneTransform(HumanBodyBones.Hips);
            if (!hip) return;
            float brace=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.3f,1.3f,elapsed));
            float rise=Mathf.SmoothStep(0,1,Mathf.InverseLerp(1.55f,2.95f,elapsed));
            float weight=1-rise;
            var hipRest=root.InverseTransformPoint(hip.position);
            hip.position=root.TransformPoint(Vector3.Lerp(hipRest,new Vector3(0,Mathf.Lerp(.18f,.54f,brace),-.24f*(1-brace)),weight));
            hip.rotation=Quaternion.AngleAxis(Mathf.Lerp(77,24,brace)*weight,root.right)*hip.rotation;
            Aim(HumanBodyBones.LeftUpperLeg,HumanBodyBones.LeftLowerLeg,new Vector3(-.12f,-.9f,.48f));
            Aim(HumanBodyBones.RightUpperLeg,HumanBodyBones.RightLowerLeg,new Vector3(.12f,-.65f,-.65f));
            Aim(HumanBodyBones.LeftLowerLeg,HumanBodyBones.LeftFoot,new Vector3(0,-1,-.22f));
            Aim(HumanBodyBones.RightLowerLeg,HumanBodyBones.RightFoot,new Vector3(0,-.26f,-1));
            Aim(HumanBodyBones.LeftUpperArm,HumanBodyBones.LeftLowerArm,new Vector3(-.23f,-.8f,.33f));
            Aim(HumanBodyBones.RightUpperArm,HumanBodyBones.RightLowerArm,new Vector3(.23f,-.8f,.33f));
            Aim(HumanBodyBones.LeftLowerArm,HumanBodyBones.LeftHand,new Vector3(0,-.67f,.74f));
            Aim(HumanBodyBones.RightLowerArm,HumanBodyBones.RightHand,new Vector3(0,-.67f,.74f));

            void Aim(HumanBodyBones from,HumanBodyBones to,Vector3 braced)
            {
                var bone=rig.GetBoneTransform(from);
                var child=rig.GetBoneTransform(to);
                if (!bone || !child) return;
                bool leg=from==HumanBodyBones.LeftUpperLeg || from==HumanBodyBones.RightUpperLeg ||
                    from==HumanBodyBones.LeftLowerLeg || from==HumanBodyBones.RightLowerLeg;
                Vector3 lying=leg ? new Vector3(braced.x,-.04f,-1) : new Vector3(braced.x,-.2f,.85f);
                var aim=root.TransformDirection(Vector3.Lerp(lying,braced,brace));
                var turn=Quaternion.FromToRotation(child.position-bone.position,aim);
                bone.rotation=Quaternion.Slerp(Quaternion.identity,turn,weight)*bone.rotation;
            }
        }

        bool OnSupplySide()
        {
            var span=supplies.position-launch.position;
            span.y=0;
            return Vector3.Dot(motor.transform.position-launch.position,span.normalized)>span.magnitude*.56f;
        }

        public bool TakeSupplies(Vector3 player)
        {
            if (Step!=9 || Vector3.Distance(player,supplies.position)>2.25f) return false;
            Go(10,"箱里有木材。拿一份就能点燃沙滩营地的营火。现在也能用空格跳跃了。");
            return true;
        }

        bool At(Transform target,float range) => target && Vector3.Distance(motor.transform.position,target.position)<range;
        void Go(int step,string line=null,bool system=false,float seconds=2.8f)
        {
            Step=step;
            elapsed=0;
            Say(line,system,seconds);
        }

        void Say(string line,bool system,float seconds)
        {
            Dialogue=line;
            SystemSpeaker=system;
            lineEnds=Time.unscaledTime+seconds;
        }

        public bool Frame(Camera camera)
        {
            if (!Cinematic || !motor || !wreck) return false;
            if (Step==0)
            {
                var p=stormShip.position;
                camera.transform.position=p+new Vector3(14,8,14)+Vector3.up*Mathf.Sin(elapsed*3)*1.3f;
                camera.transform.LookAt(p+Vector3.up*4);
                camera.transform.Rotate(0,0,Mathf.Sin(elapsed*2.5f)*10);
            }
            else if (Step==1)
            {
                float t=Mathf.SmoothStep(0,1,elapsed/3);
                camera.transform.position=motor.transform.position+Vector3.Lerp(new Vector3(1.8f,.65f,-1.5f),new Vector3(0,3,-5.3f),t);
                camera.transform.LookAt(motor.transform.position+Vector3.up*Mathf.Lerp(.5f,1.25f,t));
            }
            else if (Step == 7)
            {
                var focus=Vector3.Lerp(launch.position,supplies.position,.67f);
                camera.transform.position=focus+new Vector3(-5.4f,4.3f,-2f);
                camera.transform.LookAt(focus+Vector3.up*.25f);
            }
            else
            {
                var wrist=RelicCore.Instance ? RelicCore.Instance.Device : null;
                if (!wrist) return false;
                camera.transform.position=wrist.position-motor.transform.forward*.8f-motor.transform.right*.85f+Vector3.up*.35f;
                camera.transform.LookAt(wrist.position);
            }
            return true;
        }
    }
}
