using UnityEngine;

namespace IslandSystem
{
    [RequireComponent(typeof(BeachSurvival))]
    public sealed class BeachGuideHud : MonoBehaviour
    {
        static readonly string[] Titles = {
            "陌生的海岸", "先活下来", "临时落脚点", "沉船里的残留物资", "继续深入岛屿", "海滩探索完成"
        };

        static readonly Color Ink = new(.30f,.20f,.12f);
        static readonly Color Paper = new(.98f,.92f,.77f);
        static readonly Color Wood = new(.52f,.32f,.17f);
        static readonly Color Green = new(.28f,.40f,.22f);
        BeachSurvival guide;
        ActionMotor motor;
        Texture2D rounded;
        Font font;
        Texture2D glow;
        readonly System.Collections.Generic.List<(Vector3 origin, float time)> flights = new();

        void Launch(Vector3 origin) => flights.Add((origin,Time.unscaledTime));

        void Update()
        {
            if (!motor) motor = FindAnyObjectByType<ActionMotor>();
            if (!motor || motor.InputBlocked || motor.FreeCameraActive)
                guide.BagOpen = false;
            else if (guide.Chapter && guide.Chapter.Teaching)
                guide.BagOpen = false;
            else if (Input.GetKeyDown(KeyCode.B))
                guide.BagOpen = !guide.BagOpen;
            else if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab))
                guide.BagOpen = false;

            flights.RemoveAll(flight => Time.unscaledTime-flight.time > .72f);
        }

        void Awake()
        {
            guide = GetComponent<BeachSurvival>();
            guide.PickedUp += Launch;
            glow = new Texture2D(48,48);
            for (int y=0;y<48;y++)
            for (int x=0;x<48;x++)
            {
                float radius = Vector2.Distance(new Vector2(x,y),new Vector2(23.5f,23.5f))/23.5f;
                glow.SetPixel(x,y,new Color(1,1,1,Mathf.Pow(Mathf.Clamp01(1-radius),2)));
            }
            glow.Apply();
            font = Font.CreateDynamicFontFromOSFont(new[] { "Microsoft YaHei", "Noto Sans CJK SC" }, 18);
        }

        void OnDestroy()
        {
            guide.PickedUp -= Launch;
            Destroy(glow);
            if (font) Destroy(font);
        }

        void OnGUI()
        {
            if (Event.current.type != EventType.Repaint) return;
            if (!motor) motor = FindAnyObjectByType<ActionMotor>();
            if (!motor || motor.FreeCameraActive || motor.InputBlocked) return;

            float scale = Mathf.Min(Screen.width/1040f, Screen.height/650f, 1.22f);
            float w = Screen.width/scale, h = Screen.height/scale;
            var previousMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale,scale,1));
            if (guide.Chapter)
            {
                DrawChapter(guide.Chapter,w,h);
                if (guide.Chapter.Cinematic)
                {
                    GUI.matrix = previousMatrix;
                    return;
                }
            }
            else
            {
            int stage = guide.Stage;
            Panel(new Rect(24,25,280,stage == 5 ? 103 : 122));
            Rect header = new(24,25,280,31);
            Fill(header,Wood);
            Label(new Rect(38,25,245,31),stage == 5 ? "探索记录" : $"当前任务   ·   {stage+1} / 5",14,Paper,true);
            Label(new Rect(39,62,250,28),Titles[stage],22,Ink,true);
            Label(new Rect(39,94,252,42),Objectives(stage),15,Ink);

            string lesson = stage switch {
                0 => guide.Moved ? "移动鼠标观察周围。" : "使用 WASD 移动。",
                1 => "靠近漂流木、木桶或石头，按 E 回收。",
                2 => "布棚旁留有空地，可以在这里落脚。",
                3 => "沿海边走到残骸旁，留意背风处。",
                4 => "沿内陆方向的沙土小径继续前进。",
                _ => null
            };
            float age = Time.unscaledTime-guide.StageTime;
            if (lesson != null)
            {
                GUI.color = new Color(1,1,1,Mathf.Lerp(.78f,1,Mathf.Clamp01(age*6)));
                Panel(new Rect(w-254,124,230,87));
                Label(new Rect(w-239,130,200,23),"生存手记",15,Green,true);
                Label(new Rect(w-239,156,200,44),lesson,15,Ink);
                GUI.color = Color.white;
            }

            if (age < 2.8f)
            {
                GUI.color = new Color(1,1,1,Mathf.Min(Mathf.Lerp(.78f,1,Mathf.Clamp01(age*9)),Mathf.Clamp01((2.8f-age)*3)));
                float y = 22 + 10*(1-Mathf.Lerp(.78f,1,Mathf.Clamp01(age*6)));
                Panel(new Rect(w*.5f-158,y,316,67));
                Label(new Rect(w*.5f-145,y+6,290,24),
                    stage == 0 ? "初到海岸" : "◆  已完成 · "+Titles[stage-1],14,Green,true,true);
                Label(new Rect(w*.5f-146,y+29,292,31),
                    stage == 5 ? Titles[stage] : "第 "+(stage+1)+" 步 · "+Titles[stage],20,Ink,true,true);
                GUI.color = Color.white;
            }

            }
            Rect bag = new(w-174,h-63,150,41);
            Panel(bag);
            Fill(new Rect(bag.x+10,bag.y+8,26,25),Wood);
            Label(new Rect(bag.x+10,bag.y+7,26,27),"B",15,Paper,true,true);
            Label(new Rect(bag.x+46,bag.y,94,41),guide.BagOpen ? "关闭背包" : "背包",16,Ink,true);
            DrawGuide(w,h,scale,bag.center);
            if (guide.BagOpen)
            {
                Rect menu = new(w*.5f-143,h*.5f-83,286,166);
                Panel(menu);
                Fill(new Rect(menu.x,menu.y,menu.width,36),Wood);
                Label(new Rect(menu.x+16,menu.y,250,36),"随身背包",18,Paper,true);
                Label(new Rect(menu.x+17,menu.y+46,250,27),"▰   木材       ×"+guide.Wood,17,Ink);
                Label(new Rect(menu.x+17,menu.y+80,250,27),"◆   石头       ×"+guide.Stone,17,Ink);
                Label(new Rect(menu.x+17,menu.y+122,250,25),"B / Esc  关闭",14,Green);
            }
            if (guide.Focus && !guide.BagOpen)
            {
                Panel(new Rect(w*.5f-113,h-116,226,40));
                Fill(new Rect(w*.5f-102,h-109,27,26),Wood);
                Label(new Rect(w*.5f-102,h-110,27,27),"E",16,Paper,true,true);
                Label(new Rect(w*.5f-64,h-115,167,38),guide.Describe(guide.Focus),16,Ink,true);
            }

            float lootAge = Time.unscaledTime-guide.LootTime;
            if (guide.Loot != null && lootAge < 2.35f)
            {
                GUI.color = new Color(1,1,1,Mathf.Clamp01((2.35f-lootAge)*2.5f));
                float y = h-170-lootAge*12;
                Panel(new Rect(w*.5f-106,y,212,40));
                Fill(new Rect(w*.5f-98,y+6,29,28),Green);
                Label(new Rect(w*.5f-98,y+5,29,29),guide.Loot.StartsWith("石") ? "◆" : "▰",16,Paper,true,true);
                Label(new Rect(w*.5f-59,y,152,40),"获得  "+guide.Loot,16,Ink,true);
                GUI.color = Color.white;
            }
            GUI.matrix = previousMatrix;
        }

        void DrawChapter(BeachChapter chapter,float width,float height)
        {
            if (chapter.Storm >= 0)
            {
                float progress=chapter.Storm;
                float blackout=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.78f,1,progress));
                Fill(new Rect(-12,-12,width+24,height+24),new Color(.055f,.15f,.21f,.57f+blackout*.43f));
                if(progress<.96f)
                {
                    for (int i=0;i<54;i++)
                    {
                        float x=Mathf.Repeat(i*141.62f-Time.unscaledTime*145,width);
                        float y=Mathf.Repeat(i*83.47f+Time.unscaledTime*(770+i*3),height);
                        GUI.DrawTexture(new Rect(x,y,1.4f,19+i%6),Texture2D.whiteTexture,
                            ScaleMode.StretchToFill,true,0,new Color(.64f,.84f,.94f,.42f*(1-blackout)),0,0);
                    }
                    float surge=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.67f,.88f,progress));
                    Fill(new Rect(-15,height*(1-surge),width+30,height*surge+20),new Color(.11f,.33f,.39f,surge*.77f));
                }
                return;
            }
            if (!chapter.Cinematic && !string.IsNullOrEmpty(chapter.Objective))
            {
                Panel(new Rect(24,25,328,72));
                Label(new Rect(38,31,295,24),"第一章   ·   海难之后",14,Green,true);
                Label(new Rect(38,54,299,35),chapter.Objective,17,Ink,true);
            }
            if (chapter.Step==1 || chapter.Step==6)
            {
                Panel(new Rect(width*.5f-111,25,222,40));
                Label(new Rect(width*.5f-111,25,222,40),chapter.Step==1?"陌生的海岸":"系统觉醒",20,Ink,true,true);
            }
            if (chapter.Step==6)
            {
                Panel(new Rect(width-235,114,211,104));
                Label(new Rect(width-223,119,187,25),"◇  环境扫描",15,Green,true);
                Label(new Rect(width-223,144,187,66),"船体结构受损\n残骸内检测到物资\n附近地形  ·  沿海沙地",14,Ink);
            }
            if (chapter.CompletionAge is >= 0 and < 5f)
            {
                float t=chapter.CompletionAge;
                GUI.color=new Color(1,1,1,Mathf.Clamp01(Mathf.Min(t*5,(5-t)*2)));
                Rect banner=new(width*.5f-170,36-10*(1-Mathf.Clamp01(t*4)),340,90);
                Panel(banner);
                Label(new Rect(banner.x,banner.y+9,banner.width,26),"◆  海难之后",16,Green,true,true);
                Label(new Rect(banner.x,banner.y+35,banner.width,45),"第一章完成",27,Ink,true,true);
                GUI.color=Color.white;
            }
            if (chapter.Dialogue==null && string.IsNullOrEmpty(chapter.Feedback)) return;
            float span=Mathf.Min(610,width-80);
            Rect card=new(width*.5f-span*.5f,height-221,span,85);
            if (chapter.SystemSpeaker)
            {
                Fill(card,new Color(.09f,.30f,.35f,.95f));
                Fill(new Rect(card.x+2,card.y+2,card.width-4,card.height-4),new Color(.18f,.47f,.52f,.92f));
            }
            else Panel(card);
            Color color=chapter.SystemSpeaker?new Color(.87f,.99f,1):Ink;
            Label(new Rect(card.x+17,card.y+7,card.width-34,23),chapter.SystemSpeaker?"◇  荒岛智能系统":"主角",14,color,true);
            Label(new Rect(card.x+17,card.y+32,card.width-34,43),chapter.Dialogue??chapter.Feedback,17,color);
        }
        void DrawGuide(float width,float height,float scale,Vector2 bag)
        {
            if (!motor.view) return;
            Vector2 Project(Vector3 world, out bool visible)
            {
                var point = motor.view.WorldToScreenPoint(world);
                Vector2 raw = new(point.x/scale,height-point.y/scale);
                if (point.z <= 0) raw = new Vector2(width,height)-raw;
                Vector2 clamped = new(Mathf.Clamp(raw.x,45,width-45),Mathf.Clamp(raw.y,115,height-145));
                visible = point.z > 0 && (raw-clamped).sqrMagnitude < .1f;
                return clamped;
            }

            var target = guide.Chapter ? guide.Chapter.Goal : guide.HintTarget(motor.transform.position,Time.unscaledTime);
            if (target && !guide.BagOpen)
            {
                Vector2 marker = Project(target.position+Vector3.up*.65f,out bool visible);
                float pulse = .5f+.5f*Mathf.Sin(Time.unscaledTime*4);
                Shine(marker,64+20*pulse,new Color(1,.81f,.34f,.75f));
                Fill(new Rect(marker.x-9,marker.y-9,18,18),Wood);
                Label(new Rect(marker.x-42,marker.y-15,84,30),visible ? "◆" : "✦",16,Paper,true,true);
                if (!visible)
                {
                    Panel(new Rect(marker.x-38,marker.y+12,76,23));
                    Label(new Rect(marker.x-38,marker.y+12,76,23),"任务目标",12,Ink,true,true);
                }
            }

            foreach (var flight in flights)
            {
                float t = Mathf.Clamp01((Time.unscaledTime-flight.time)/.72f);
                var from = Project(flight.origin+Vector3.up*.25f,out _);
                var point = Vector2.Lerp(from,bag,Mathf.SmoothStep(0,1,t));
                point.y -= Mathf.Sin(Mathf.PI*t)*53;
                Shine(point,42-17*t,new Color(1,.79f,.25f,1));
                Shine(point,18,new Color(1,1,.83f,1));
            }
        }

        void Shine(Vector2 center,float size,Color color) =>
            GUI.DrawTexture(new Rect(center.x-size*.5f,center.y-size*.5f,size,size),glow,
                ScaleMode.StretchToFill,true,0,color,0,0);
        string Objectives(int stage) => stage switch {
            0 => Mark(guide.Looked)+" 观察四周\n"+Mark(guide.Moved)+" 向前移动",
            1 => Mark(guide.GatheredWood>=3)+"  收集三份木材\n"+Mark(guide.Stone>=2)+"  收集两块石头",
            2 => Mark(guide.CampVisited)+" 前往营火\n"+Mark(guide.CampChecked)+" 检查布棚",
            3 => Mark(guide.Searched)+" 搜索沉船物资\n"+Mark(guide.WreckChecked)+" 检查沉船残骸",
            4 => "•  前往通向森林的小路入口",
            _ => ""
        };

        static string Mark(bool done) => done ? "✓" : "•";

        void Panel(Rect rect)
        {
            var shadow = rect;
            shadow.position += new Vector2(0,4);
            Fill(shadow,new Color(.23f,.15f,.08f,.32f));
            Fill(rect,new Color(.71f,.52f,.31f));
            rect.xMin+=2; rect.xMax-=2; rect.yMin+=2; rect.yMax-=2;
            Fill(rect,Paper);
        }

        void Fill(Rect rect, Color color) =>
            GUI.DrawTexture(rect,Texture2D.whiteTexture,ScaleMode.StretchToFill,true,0,
                new Color(color.r,color.g,color.b,color.a*GUI.color.a),0,8);

        void Label(Rect rect,string text,int size,Color color,bool bold=false,bool center=false)
        {
            var style = new GUIStyle(GUI.skin.label) {
                font = font ? font : GUI.skin.font,
                fontSize = size, fontStyle = bold ? FontStyle.Bold : FontStyle.Normal,
                alignment = center ? TextAnchor.MiddleCenter : TextAnchor.MiddleLeft,
                wordWrap = true, normal = { textColor = color }
            };
            GUI.Label(rect,text,style);
        }
    }
}
