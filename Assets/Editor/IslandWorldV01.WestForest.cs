using UnityEngine;

public static partial class IslandWorldV01
{
    static readonly Vector2 ForestClearing = new(-125,-47);
    internal static readonly Vector2[] ForestClearingWalk =
        { new(-102.5f,-51),new(-112,-48),ForestClearing,new(-130,-42),new(-122,-40),ForestClearing };

    static void PaintWestForest(float[,,] map, Vector3 origin, Vector3 size)
    {
        var groves = new[] { new Vector2(-139,-70),new Vector2(-147,-32),
            new Vector2(-105,-24),new Vector2(-94,-69),new Vector2(-129,-85) };
        var trail = Tracks[0];
        int n = map.GetLength(0);
        for (int z = 0; z < n; z++)
        for (int x = 0; x < n; x++)
        {
            var p = new Vector2(origin.x+x*size.x/(n-1),origin.z+z*size.z/(n-1));
            if (p.x < -173 || p.x > -52 || p.y < -108 || p.y > -12) continue;
            float litter = 0;
            foreach (var grove in groves)
                litter = Mathf.Max(litter,Feather(Vector2.Distance(p,grove),6f,16f)*.48f);
            float road = 0;
            for (int i = 4; i <= 8; i++)
            {
                Vector2 v = trail[i]-trail[i-1];
                float t = Mathf.Clamp01(Vector2.Dot(p-trail[i-1],v)/v.sqrMagnitude);
                float distance = Vector2.Distance(p,trail[i-1]+v*t);
                road = Mathf.Max(road,Feather(distance,1.55f,4.65f));
            }
            road *= Feather(Mathf.Abs(p.y+65),24f,40f) * Feather(Mathf.Abs(p.x+102),25f,49f);
            float glade = Feather(Vector2.Distance(p,ForestClearing),10f,16f)*.56f;
            for (int layer=0;layer<map.GetLength(2);layer++)
            {
                float value = Mathf.Lerp(map[z,x,layer],layer==4?1f:0f,litter);
                value = Mathf.Lerp(value,layer==2?1f:0f,glade);
                float pathColor = layer==3?.84f:layer==0?.16f:0f;
                map[z,x,layer] = Mathf.Lerp(value,pathColor,road*.92f);
            }
        }
    }

    static float Feather(float distance,float inner,float outer) =>
        1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(inner,outer,distance));

    static void DressWestForest()
    {
        // Frame the sunlit clearing, leaving the eastern opening and path unobstructed.
        Grove("CommonTree_2",new(-149,-51,22,31),new(-143,-31,21,139),new(-113,-26,19,81));
        Grove("CommonTree_3",new(-145,-66,19,101),new(-104,-71,21,61),new(-93,-24,19,54));
        Grove("CommonTree_5",new(-151,-78,17,176),new(-132,-72,13,117),new(-70,-46,18,201));

        Grove("CommonTree_4",new(-166,-57,24,121),new(-173,-38,22,26),new(-159,-28,23,177));
        Grove("CommonTree_1",new(-164,-72,23,63),new(-176,-53,21,86),new(-153,-61,18,209));
        ForestTreeTint("CommonTree_2",-144,-42,21,61,"Clearing Amber",new Color(1f,.77f,.48f));
        ForestTreeTint("CommonTree_1",-154,-43,27,45,"Forest Deep",new Color(.72f,.87f,.71f));

        Rock(0,-141,-54,4.4f,2.8f,35,.52f);
        Rock(1,-140,-52,2.6f,1.22f,71,.31f);
        Rock(2,-130,-31,4.1f,2.05f,87,.46f);
        Rock(0,-105,-65,3.8f,2.1f,27,.41f);
        Rock(1,-94,-35,2.8f,1.4f,29,.24f);
        Rock(2,-80,-43,2.7f,1.25f,105,.28f);

        Grove("Fern_1",new(-139,-54,2.7f,38),new(-136,-57,2.35f,91),new(-131,-33,2.8f,80),
            new(-119,-34,2.6f,75),new(-106,-66,2.1f,77),new(-98,-39,1.8f,66),new(-80,-42,2.4f,172));
        Grove("Plant_1",new(-140,-47,2.7f,33),new(-133,-61,1.8f,61),new(-111,-41,1.9f,88),new(-86,-48,2.45f,18));
        Grove("Grass_Common_Short",new(-135,-58,2.7f,41),new(-119,-60,2.32f,93),new(-139,-43,2.3f,15),
            new(-128,-34,2.6f,32),new(-108,-43,1.7f,113),new(-100,-50,1.8f,10),new(-74,-42,2.1f,83));
        Grove("Flower_3_Group",new(-137,-55,2.3f,38),new(-118,-34,2.35f,110),new(-107,-67,2.25f,30),
            new(-103,-46,1.9f,102),new(-89,-51,2.3f,88),new(-79,-44,1.8f,54));
        Grove("Flower_4_Group",new(-135,-56,2.1f,71),new(-140,-47,1.65f,27),new(-130,-35,1.85f,14),
            new(-109,-42,2.1f,55),new(-91,-50,2.05f,193),new(-68,-41,1.8f,42));

        var bark = BeachMaterial("Forest Deadwood Bark", new Color(.31f,.16f,.077f));
        var end = BeachMaterial("Forest Cut End", new Color(.67f,.40f,.18f));
        Vector3 stump = new(-134,Ground(-134,-59),-59);
        BeachBeam("林缘断桩",stump-Vector3.up*.20f,stump+Vector3.up*.71f,.44f,bark);
        BeachBeam("断桩木芯",stump+Vector3.up*.70f,stump+Vector3.up*.735f,.36f,end);
        Vector3 root = new(-137,Ground(-137,-61)+.29f,-61);
        Vector3 tip = new(-134,Ground(-134,-62)+.27f,-62);
        BeachBeam("林缘枯木",root,tip,.27f,bark);
        BeachBeam("枯木断面",tip,tip+(tip-root).normalized*.036f,.22f,end);
    }
    static void ForestTreeTint(string asset,float x,float z,float size,float yaw,string variant,Color tint)
    {
        var tree=Place(Nature+asset+".fbx",x,z,size,size*.95f,yaw);
        int index=0;
        foreach(var renderer in tree.GetComponentsInChildren<MeshRenderer>())
        {
            var materials=renderer.sharedMaterials;
            for(int slot=0;slot<materials.Length;slot++)
            {
                var source=materials[slot];
                if(!source || !source.HasProperty("_Color")) continue;
                string path=Folder+"/"+variant+" "+index++ +".mat";
                var copy=UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(path);
                if(!copy)
                {
                    copy=new Material(source){color=source.color*tint};
                    UnityEditor.AssetDatabase.CreateAsset(copy,path);
                }
                materials[slot]=copy;
            }
            renderer.sharedMaterials=materials;
        }
    }
}
