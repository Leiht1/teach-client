using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static partial class IslandWorldV01
{
    const string Folder = "Assets/IslandWorldV01";
    const string Nature = "Assets/Art/External/Quaternius/StylizedNatureMegaKit/FBX (Unity)/";
    const string Pirate = "Assets/Art/External/Kenney/PirateKit/Models/FBX format/";
    const string Coast = "Assets/Lighthouse_Authored/Prefabs/";
    const string Ruins = "Assets/Art/External/Quaternius/UltimateModularRuins/FBX/";
    static Terrain terrain;
    static Transform group;

    // World coordinates chosen individually against the layout. Height snaps to the existing landform.
    static GameObject Place(string path, float x, float z, float width, float height, float yaw, float sink = 0)
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (!asset) throw new FileNotFoundException(path);
        var obj = (GameObject)PrefabUtility.InstantiatePrefab(asset, group);
        obj.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        var bounds = BoundsOf(obj);
        var scale = new Vector3(width / bounds.size.x, height / bounds.size.y, width / bounds.size.x);
        obj.transform.localScale = Vector3.Scale(obj.transform.localScale, scale);
        bounds = BoundsOf(obj);
        obj.transform.position -= new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        var pivot = obj.transform.position;
        var rotation = Quaternion.Euler(0, yaw, 0);
        float y = Ground(x,z);
        obj.transform.SetPositionAndRotation(new Vector3(x,y-sink,z)+rotation*pivot,rotation);
        return obj;
    }

    static Bounds BoundsOf(GameObject obj)
    {
        var renderers = obj.GetComponentsInChildren<Renderer>();
        var bounds = renderers[0].bounds;
        foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
        return bounds;
    }

    static float Ground(float x,float z) => terrain.SampleHeight(new Vector3(x,0,z))+terrain.transform.position.y;
    static void Region(string name) => group = new GameObject(name).transform;
    static void Grove(string asset, params Vector4[] points)
    {
        foreach (var p in points) Place(Nature+asset+".fbx",p.x,p.y,p.z,p.z*.95f,p.w);
    }
    static void Rock(int variant,float x,float z,float width,float height,float yaw,float sink)
    {
        Place(Nature+"Rock_Medium_"+(1+variant%3)+".fbx",x,z,width,height,yaw,sink);
    }
    static void Palm(float x,float z,float height,float yaw) =>
        Place(Coast+"Coastal_palm_left.prefab",x,z,height*.67f,height,yaw);
    static void Shot(Camera cam,string name,Vector3 from,Vector3 to,float ortho=0)
    {
        cam.orthographic=ortho>0; cam.orthographicSize=Mathf.Max(ortho,1);
        cam.fieldOfView=57; cam.transform.position=from; cam.transform.LookAt(to);
        var rt=new RenderTexture(1280,800,24);
        var previous=RenderTexture.active;
        cam.targetTexture=rt; cam.Render(); RenderTexture.active=rt;
        var tex=new Texture2D(1280,800,TextureFormat.RGB24,false);
        tex.ReadPixels(new Rect(0,0,1280,800),0,0); tex.Apply();
        Directory.CreateDirectory("Docs/Art/IslandWorldV01");
        File.WriteAllBytes("Docs/Art/IslandWorldV01/"+name+".png",tex.EncodeToPNG());
        cam.targetTexture=null; RenderTexture.active=previous;
        Object.DestroyImmediate(tex); Object.DestroyImmediate(rt);
    }

    static void CampShelter(float x,float z)
    {
        var p=new Vector3(x,Ground(x,z)+.32f,z);
        var obj=new GameObject("Beach camp · canvas shelter");
        obj.transform.SetParent(group);
        obj.transform.position=p;
        obj.transform.rotation=Quaternion.Euler(0,-23,0);
        var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(Folder+"/CampCanvas.asset");
        if(!mesh)
        {
            mesh=new Mesh();
            AssetDatabase.CreateAsset(mesh,Folder+"/CampCanvas.asset");
        }
        mesh.vertices=new[]{new Vector3(-2.2f,.25f,-2),new Vector3(0,2.65f,-2.1f),new Vector3(2.1f,.38f,-2),new Vector3(-2.2f,.32f,1.8f),new Vector3(0,2.47f,2),new Vector3(2.1f,.46f,1.8f)};
        mesh.triangles=new[]{0,3,1,1,3,4,1,4,2,2,4,5,3,5,4};
        mesh.RecalculateNormals();
        EditorUtility.SetDirty(mesh);
        var canvas=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/CampCanvas.mat");
        if(!canvas)
        {
            canvas=new Material(Shader.Find("Standard")){color=new Color(.84f,.68f,.43f)};
            AssetDatabase.CreateAsset(canvas,Folder+"/CampCanvas.mat");
        }
        canvas.color=new Color(.90f,.57f,.32f);
        EditorUtility.SetDirty(canvas);
        obj.AddComponent<MeshFilter>().sharedMesh=mesh;
        obj.AddComponent<MeshRenderer>().sharedMaterial=canvas;
        obj.AddComponent<MeshCollider>().sharedMesh=mesh;
        var timber=BeachMaterial("Shelter Timber",new Color(.38f,.21f,.10f));
        void Pole(string name, Vector3 start, Vector3 end, float radius)
        {
            var beam=GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            beam.name=name;
            beam.transform.SetParent(obj.transform,false);
            beam.transform.localPosition=(start+end)*.5f;
            beam.transform.localRotation=Quaternion.FromToRotation(Vector3.up,end-start);
            beam.transform.localScale=new Vector3(radius*2,(end-start).magnitude*.5f,radius*2);
            beam.GetComponent<MeshRenderer>().sharedMaterial=timber;
            Object.DestroyImmediate(beam.GetComponent<Collider>());
        }
        foreach(float depth in new[]{-2.05f,1.82f})
        {
            float ridge=depth<0?2.66f:2.47f;
            Pole("篷杆",new(-2.23f,-.55f,depth),new(0,ridge+.10f,depth),.075f);
            Pole("篷杆",new(2.18f,-.55f,depth),new(0,ridge+.10f,depth),.075f);
        }
        Pole("脊杆",new(0,2.66f,-2.23f),new(0,2.47f,2.09f),.07f);
    }

    const float CrossingZ=-56;
    static readonly Vector2 WestBank=new(-24,CrossingZ);
    static readonly Vector2 EastBank=new(5,CrossingZ);
    static readonly Vector2 Junction=new(28,-62);
    static readonly Vector2 Summit=new(159,154);
    internal static readonly Vector2[][] Tracks={
        new Vector2[]{new(-145,-191),new(-142,-165),new(-148,-142),new(-132,-121),new(-118,-91),new(-111,-60),new(-94,-42),new(-74,-34),new(-54,-42),new(-39,-53),WestBank,EastBank,Junction,new(58,-68),new(88,-55),new(108,-31),new(115,5),new(101,27),new(83,47),new(70,69),new(72,88),new(94,91),new(117,83),new(138,71),new(157,91),new(148,111),new(168,122),new(169,146),new(169,153)},
        new Vector2[]{Junction,new(17,-87),new(12,-113),new(17,-143),new(32,-172),new(33,-192),new(29,-211)},
        new Vector2[]{new(70,69),new(55,61),new(37,57),new(23,58),new(15,61)},
        new Vector2[]{new(169,153),new(164,164),new(153,164),new(148,153),new(153,144),new(163,144)}
    };

    // The bridge follows the graded shoulders and leaves the water channel open.
    static void BuildBridge()
    {
        var from=new Vector3(WestBank.x,Ground(WestBank.x,CrossingZ)+.08f,CrossingZ);
        var to=new Vector3(EastBank.x,Ground(EastBank.x,CrossingZ)+.08f,CrossingZ);
        var direction=(to-from).normalized;
        var rotation=Quaternion.LookRotation(Vector3.forward,Vector3.Cross(Vector3.forward,direction));
        var wood=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/BridgeWood.mat");
        if(!wood)
        {
            wood=new Material(Shader.Find("Standard")){color=new Color(.49f,.30f,.15f)};
            AssetDatabase.CreateAsset(wood,Folder+"/BridgeWood.mat");
        }
        var bridge=new GameObject("中央木桥");
        bridge.transform.SetParent(group);
        bridge.transform.SetPositionAndRotation((from+to)*.5f,rotation);
        float span=Vector3.Distance(from,to);
        var deck=bridge.AddComponent<BoxCollider>();
        deck.center=new Vector3(0,-.17f,0);
        deck.size=new Vector3(span+1,.34f,4.4f);
        var honey=BeachMaterial("Bridge Honey Boards",new Color(.58f,.34f,.17f));
        var worn=BeachMaterial("Bridge Worn Boards",new Color(.47f,.28f,.15f));
        void Timber(string name,Vector3 pos,Vector3 size,Material finish=null)
        {
            var piece=GameObject.CreatePrimitive(PrimitiveType.Cube);
            piece.name=name;
            piece.transform.SetParent(bridge.transform,false);
            piece.transform.localPosition=pos;
            piece.transform.localScale=size;
            Object.DestroyImmediate(piece.GetComponent<Collider>());
            piece.GetComponent<Renderer>().sharedMaterial=finish?finish:wood;
        }
        const int planks=36;
        for(int i=0;i<planks;i++)
        {
            float width=(span+1)/planks;
            Timber("桥面木板",new(-span*.5f-.5f+(i+.5f)*width,-.13f,0),new(width-.035f,.26f,4.4f),i%7==2?worn:honey);
        }
        foreach(float side in new[]{-2.14f,2.14f})
        {
            Timber("纵向承梁",new(0,-.44f,side*.79f),new(span+1,.47f,.35f));
            Timber("扶栏",new(0,.98f,side),new(span-1,.17f,.18f));
            Timber("中栏",new(0,.52f,side),new(span-1,.115f,.14f),worn);
            for(int i=0;i<6;i++)
            {
                float x=Mathf.Lerp(-span*.46f,span*.46f,i/5f);
                Timber("立柱",new(x,.49f,side),new(.22f,1.25f,.23f));
                Timber("柱帽",new(x,1.15f,side),new(.31f,.13f,.31f),honey);
                if(i==0||i==5) Timber("桥头桩",new(x,-.72f,side),new(.33f,1.45f,.34f),worn);
            }
        }
    }

    static void ShapeActivityAreas(TerrainData data)
    {
        var origin=terrain.transform.position;
        int n=data.heightmapResolution;
        var heights=data.GetHeights(0,0,n,n);
        var areas=new[]{
            (center:new Vector2(-145,-185),radius:19f,falloff:9f,strength:.78f),
            (center:new Vector2(49,-83),radius:26f,falloff:14f,strength:.72f),
            (center:new Vector2(28,-211),radius:12f,falloff:7f,strength:.70f),
            (center:ForestClearing,radius:13f,falloff:9f,strength:.93f),
            (center:Summit,radius:16f,falloff:13f,strength:1f)};
        foreach(var area in areas)
        {
            float target=(Ground(area.center.x,area.center.y)-origin.y)/data.size.y;
            for(int z=0;z<n;z++)for(int x=0;x<n;x++)
            {
                var point=new Vector2(origin.x+x*data.size.x/(n-1),origin.z+z*data.size.z/(n-1));
                float d=Vector2.Distance(point,area.center);
                if(d>area.radius+area.falloff || Mathf.Abs(point.x-RiverX(point.y))<8 && point.y<44)continue;
                float weight=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(area.radius,area.radius+area.falloff,d));
                if(area.center!=Summit && origin.y+heights[z,x]*data.size.y<1.5f)continue;
                heights[z,x]=Mathf.Lerp(heights[z,x],target,weight*area.strength);
            }
        }
        data.SetHeights(0,0,heights);
    }

    static void GradeRoutes(TerrainData data)
    {
        int count=data.heightmapResolution;
        var heights=data.GetHeights(0,0,count,count);
        var origin=terrain.transform.position;
        var distance=new float[count,count];
        for(int z=0;z<count;z++)for(int x=0;x<count;x++)distance[z,x]=10;
        var original=(float[,])heights.Clone();
        foreach(var route in Tracks)
        {
            var levels=System.Array.ConvertAll(route,p=>Ground(p.x,p.y));
            for(int i=1;i<route.Length;i++)
            {
                float rise=Vector2.Distance(route[i-1],route[i])*.48f;
                levels[i]=Mathf.Clamp(levels[i],levels[i-1]-rise,levels[i-1]+rise);
            }
            if(route==Tracks[0])
            {
                float summitHeight=Ground(Summit.x,Summit.y);
                levels[levels.Length-1]=summitHeight;
                levels[levels.Length-2]=summitHeight;
                for(int i=levels.Length-3;i>=0;i--)
                {
                    float rise=Vector2.Distance(route[i],route[i+1])*.48f;
                    levels[i]=Mathf.Clamp(levels[i],levels[i+1]-rise,levels[i+1]+rise);
                }
            }
            for(int i=1;i<route.Length;i++)
            {
                var start=route[i-1];var end=route[i];var v=end-start;
                float ToIndex(float value,float offset,float size)=>(value-offset)*(count-1)/size;
                int x0=Mathf.Clamp(Mathf.FloorToInt(ToIndex(Mathf.Min(start.x,end.x)-10,origin.x,data.size.x)),0,count-1);
                int x1=Mathf.Clamp(Mathf.CeilToInt(ToIndex(Mathf.Max(start.x,end.x)+10,origin.x,data.size.x)),0,count-1);
                int z0=Mathf.Clamp(Mathf.FloorToInt(ToIndex(Mathf.Min(start.y,end.y)-10,origin.z,data.size.z)),0,count-1);
                int z1=Mathf.Clamp(Mathf.CeilToInt(ToIndex(Mathf.Max(start.y,end.y)+10,origin.z,data.size.z)),0,count-1);
                for(int z=z0;z<=z1;z++)for(int x=x0;x<=x1;x++)
                {
                    var p=new Vector2(origin.x+x*data.size.x/(count-1),origin.z+z*data.size.z/(count-1));
                    float t=Mathf.Clamp01(Vector2.Dot(p-start,v)/v.sqrMagnitude);
                    float d=Vector2.Distance(p,start+v*t);
                    if(d>=distance[z,x] || Vector2.Distance(p,Summit)<12)continue;
                    if(Mathf.Abs(p.y-CrossingZ)<10 && Mathf.Abs(p.x-RiverX(p.y))<6)continue;
                    distance[z,x]=d;
                    float blend=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(2.4f,9,d));
                    heights[z,x]=Mathf.Lerp(original[z,x],(Mathf.Lerp(levels[i-1],levels[i],t)-origin.y)/data.size.y,blend);
                }
            }
            data.SetHeights(0,0,heights);
        }
    }

    static void PaintTrails()
    {
        Directory.CreateDirectory(Folder);
        AssetDatabase.Refresh();
        var data = AssetDatabase.LoadAssetAtPath<TerrainData>(Folder+"/Terrain.asset");
        if (!data)
        {
            data=Object.Instantiate(terrain.terrainData);
            AssetDatabase.CreateAsset(data,Folder+"/Terrain.asset");
        }
        var source=terrain.terrainData;
        terrain.terrainData=data;
        terrain.GetComponent<TerrainCollider>().terrainData=data;
        int n=data.alphamapResolution;
        var map=source.GetAlphamaps(0,0,n,n);
        var origin=terrain.transform.position;
        var tracks=Tracks;
        for(int y=0;y<n;y++) for(int x=0;x<n;x++)
        {
            var p=new Vector2(origin.x+x*data.size.x/(n-1),origin.z+y*data.size.z/(n-1));
            float w=0;
            for(int track=0;track<tracks.Length;track++)
            for(int i=1;i<tracks[track].Length;i++)
            {
                var a=tracks[track][i-1];
                var segment=tracks[track][i]-a;
                float d=Vector2.Distance(p,a+segment*Mathf.Clamp01(Vector2.Dot(p-a,segment)/segment.sqrMagnitude));
                float width=track==0?3.8f:2.4f;
                w=Mathf.Max(w,1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(width*.45f,width+1,d)));
            }
            float height=Ground(p.x,p.y);
            if(height<.35f)continue;
            if(height<9) w=Mathf.Max(w,(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(3,9,height)))*.78f);
            float sand=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(6,14,height));
            float beach=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(16,34,Vector2.Distance(p,new Vector2(-145,-185))));
            sand=Mathf.Max(sand,beach);
            w=Mathf.Max(w,beach*.95f);
            float wet=(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.35f,3.2f,height))) * (1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(28,54,Vector2.Distance(p,new Vector2(-145,-209)))));
            float coast=CoconutCoastWeight(p);
            float coastalSand=coast*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(5,17,height)));
            sand=Mathf.Max(sand,Mathf.Max(coastalSand,coast*.64f));
            w=Mathf.Max(w,coastalSand*.8f);
            wet=Mathf.Max(wet,coast*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.35f,3.2f,height))));
            if(p.x<-140 && p.y< -123) sand=Mathf.Max(sand,.6f);
            sand*=1-wet*.48f;
            w=Mathf.Max(w,wet*.75f);
            float gravel=Mathf.SmoothStep(0,1,Mathf.InverseLerp(56,100,height))*.33f;
            for(int k=0;k<map.GetLength(2);k++)map[y,x,k]=Mathf.Lerp(map[y,x,k],k==0?sand:k==1?wet*.48f:k==3?(1-sand-wet*.48f)*(1-gravel):k==5?(1-sand-wet*.48f)*gravel:0,w*.91f);
        }
        PaintWestForest(map,origin,data.size);
        PaintCentralMeadow(map,origin,data.size);
        data.terrainLayers=source.terrainLayers;
        data.SetAlphamaps(0,0,map);
        PaintStartBeachSand(data);
        ShapeRiver(source,data);
        ShapeActivityAreas(data);
        GradeStartBeach(data);
        ShapeEastFringe(data);
        ShapeNorthMountain(data);
        ShapeLighthouseHeadland(data);
        GradeRoutes(data);
        PaintEstuary(data);
        PaintEastFringe(data);
        PaintNorthMountain(data);
        PaintLighthouseHeadland(data);
        EditorUtility.SetDirty(data);
        AssetDatabase.SaveAssets();
    }

    static float RiverX(float z) => 15+25*Mathf.Sin(z*.026f);

    static float RiverSurface(float x,float z,TerrainData source)
    {
        var origin=terrain.transform.position;
        float height=source.GetInterpolatedHeight((x-origin.x)/source.size.x,(z-origin.z)/source.size.z)+origin.y;
        float surface=Mathf.Max(.08f,height-.42f);
        if(z>-210f)return surface;
        float headZ=-210f;
        float head=source.GetInterpolatedHeight((RiverX(headZ)-origin.x)/source.size.x,(headZ-origin.z)/source.size.z)+origin.y-.42f;
        float channel=Mathf.Lerp(.08f,head,Mathf.InverseLerp(-248f,headZ,z));
        return Mathf.Min(surface,channel);
    }

    static void ShapeRiver(TerrainData source, TerrainData data)
    {
        const float south=-260f;
        const int sections=153;
        const int columns=9;
        var profile=new float[sections];
        var origin=terrain.transform.position;
        var vertices=new Vector3[sections*columns];
        var uv=new Vector2[vertices.Length];
        var triangles=new System.Collections.Generic.List<int>((sections-1)*(columns-1)*6);
        for(int i=0;i<sections;i++)
        {
            float z=south+2*i, x=RiverX(z);
            profile[i]=RiverSurface(x,z,source);
            float half=RiverHalfWidth(z);
            for(int j=0;j<columns;j++)
            {
                float u=j/(float)(columns-1);
                int k=i*columns+j;
                vertices[k]=new Vector3(x+(u*2-1)*half,profile[i],z);
                uv[k]=new Vector2(u,Mathf.Max(MeadowRiverWeight(z),EstuaryWeight(z)));
                if(i<sections-1 && j<columns-1)
                    triangles.AddRange(new[]{k,k+columns,k+1,k+1,k+columns,k+columns+1});
            }
        }
        int n=data.heightmapResolution;
        var heights=source.GetHeights(0,0,n,n);
        for(int row=0;row<n;row++)
        {
            float z=origin.z+row*data.size.z/(n-1);
            if(z<south || z>44)continue;
            float index=(z-south)*.5f;
            int i=Mathf.Min(sections-2,Mathf.FloorToInt(index));
            float water=Mathf.Lerp(profile[i],profile[i+1],index-i);
            float estuary=EstuaryWeight(z);
            float half=RiverHalfWidth(z);
            float bank=Mathf.Max(9f+MeadowRiverWeight(z)*2.1f,half+2f+estuary*6f);
            for(int col=0;col<n;col++)
            {
                float x=origin.x+col*data.size.x/(n-1);
                float d=Mathf.Abs(x-RiverX(z));
                if(d>bank)continue;
                float dryRoute=Feather(EstuaryTrailDistance(new Vector2(x,z)),2.7f,6f)*estuary;
                float shoal=Mathf.SmoothStep(0,1,Mathf.InverseLerp(half*.4f,half,d));
                float depth=Mathf.Lerp(1.25f,Mathf.Lerp(1.55f,.16f,shoal),estuary);
                float sandbar=EstuarySandbar(x,z);
                float bed=(water-depth-origin.y)/data.size.y;
                float floor=Mathf.Min(heights[row,col],bed);
                floor=Mathf.Lerp(floor,(water+.34f-origin.y)/data.size.y,sandbar);
                float blend=Feather(d,half+.5f,bank)*(1-dryRoute);
                if(estuary>0 && d>half)
                {
                    float shelf=water-.16f+(d-half)*.58f;
                    float cap=(shelf-origin.y)/data.size.y;
                    float taper=Feather(d,half+2.5f,bank);
                    floor=Mathf.Lerp(floor,Mathf.Min(heights[row,col],cap),estuary);
                    blend=Mathf.Max(blend,taper*estuary*(1-dryRoute));
                }
                heights[row,col]=Mathf.Lerp(heights[row,col],floor,blend);
            }
        }
        data.SetHeights(0,0,heights);
        var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(Folder+"/River.asset");
        if(!mesh){mesh=new Mesh();AssetDatabase.CreateAsset(mesh,Folder+"/River.asset");}
        mesh.Clear();mesh.vertices=vertices;mesh.uv=uv;mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
        EditorUtility.SetDirty(mesh);
        var river=GameObject.Find("River water above sculpted bed");
        river.GetComponent<MeshFilter>().sharedMesh=mesh;
        var renderer=river.GetComponent<MeshRenderer>();
        var original=renderer.sharedMaterial.color;
        renderer.sharedMaterial=BeachMaterial("Meadow River",original,"Island/Meadow River");
    }

    public static void BuildTrial()
    {
        var build=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
            scenes=new[]{"Assets/Scenes/Island_World_V01.unity"},
            locationPathName="Builds/IslandWorldTrial/IslandWorldTrial.exe",
            target=BuildTarget.StandaloneWindows64,
            options=BuildOptions.Development
        });
        if(build.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)
            throw new System.InvalidOperationException("Island world build failed");
    }

    public static void Build()
    {
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/Island_Surface_V02.unity");
        terrain=Object.FindFirstObjectByType<Terrain>();
        PaintTrails();
        BuildStartBeach();

        BuildCoconutCoast();

        Region("03 · 西部森林");
        Grove("CommonTree_1",new(-141,-81,33,15),new(-104,-81,36,72),new(-71,-90,31,111),new(-94,-58,29,181),new(-131,-26,32,241),new(-102,-2,42,36),new(-53,-28,32,48),new(-100,35,41,87),new(-84,10,38,30));
        Grove("CommonTree_4",new(-167,-65,34,50),new(-122,-66,38,79),new(-83,-64,39,102),new(-62,-57,27,140),new(-147,-11,33,210),new(-110,17,34,11),new(-54,12,35,60),new(-44,42,41,12),new(-125,57,45,31));
        Grove("CommonTree_3",new(-79,-111,31,42),new(-42,-118,28,76),new(-28,-80,25,115),new(-66,-8,32,180),new(-77,49,37,51),new(-160,15,44,71),new(-183,-34,34,91),new(-123,-93,30,34));
        Grove("Bush_Common_Flowers",new(-148,-63,7,0),new(-140,-76,6,35),new(-128,-63,5,74),new(-113,-34,6,11),new(-94,-52,9,50),new(-72,-91,8,9),new(-32,-114,6,21));

        // Western headland closes the forest silhouette without filling the footpath.
        Grove("CommonTree_5",new(-193,35,40,53),new(-172,57,36,117),new(-144,83,39,25),
            new(-160,103,43,20),new(-127,118,37,105),new(-114,100,30,37),new(-117,150,45,77),
            new(-151,135,38,31),new(-179,81,35,70),new(-200,69,38,82),new(-128,171,40,19),
            new(-93,172,31,130),new(-99,137,34,70),new(-209,22,30,40));

        DressWestForest();

        Region("04 · 中央草地与河道");
        BuildBridge();
        DressCentralMeadow();
        Region("05 · 南部河口与河岸");
        DressEstuary();
        // Southern banks split the long exposed slope into distinct tree and rock masses.
        Grove("CommonTree_1",new(74,-134,34,17),new(88,-159,36,110),new(106,-174,31,78),
            new(116,-147,35,19),new(145,-127,38,25),new(157,-105,38,55),new(172,-84,29,90),
            new(181,-53,34,82),new(151,-64,30,40),new(105,-109,30,87),new(80,-183,32,17),
            new(-52,-174,38,34),new(-69,-200,35,91),new(-84,-166,37,57),
            new(-107,-189,25,25),new(-100,-214,26,42),new(-52,-221,23,29));
        Grove("CommonTree_5",new(66,-201,31,72),new(129,-158,28,47),new(154,-139,24,18),
            new(175,-119,28,84),new(188,-76,29,21),new(185,-27,30,63),new(165,7,23,14),
            new(150,36,23,21),new(125,60,20,64),new(103,106,36,81),
            new(105,138,32,92),new(121,157,35,12));

        Region("07 · 北部岩石山地");
        DressNorthMountain();

        Region("07b · 山地洞口");
        var caveMouth=Place(Ruins+"Arch_Round_RoundColumn.fbx",15,66,12,9,22,.6f);
        var caveStone=BeachMaterial("North Mountain Cave Stone",new Color(.48f,.47f,.42f),"Island/North Mountain Stone");
        foreach(var renderer in caveMouth.GetComponentsInChildren<MeshRenderer>())
            renderer.sharedMaterial=caveStone;
        Rock(3,7,67,14,14,-32,5); Rock(11,23,71,17,14,25,5);
        Rock(1,15,77,15,18,60,9);
        Grove("Pine_2",new(-3,66,17,44),new(29,73,19,22),new(37,85,20,93));

        Region("06 · 东部林缘");
        DressEastFringe();

        Region("08 · 东北灯塔崖");
        DressLighthouseHeadland();

        var cam=GameObject.Find("Overview angled").GetComponent<Camera>();
        Shot(cam,"00-Overview",new(280,520,-530),new(0,19,0),294);
        Shot(cam,"09-Top",new(0,650,-1),new(0,0,0),270);
        var sites=new[]{new Vector4(-150,-185,13,-42),new Vector4(-154,-134,-19,-35),
            new Vector4(-106,-55,-10,-27),new Vector4(-8,-53,-31,-27),new Vector4(29,-211,13,-24),
            new Vector4(115,5,-28,-33),new Vector4(72,88,32,-29)};
        for(int i=0;i<sites.Length;i++){
            var p=sites[i];float x=p.x+p.z,z=p.y+p.w;
            Shot(cam,(i+1).ToString("00"),new(x,Mathf.Max(2,Ground(x,z))+5,z),new(p.x,Mathf.Max(0,Ground(p.x,p.y))+4,p.y));
        }
        Shot(cam,"05",new(33,Ground(33,-209)+4.5f,-209),new(25,Ground(25,-229)+.6f,-229));
        Shot(cam,"05-Estuary-Overview",new(51,52,-261),new(17,3,-207));
        Shot(cam,"05-Estuary-Banks",new(35,Ground(35,-232)+3.25f,-232),new(26,Ground(26,-219)+.9f,-219));
        Shot(cam,"06",new(103,Ground(103,-36)+3.25f,-36),new(125,Ground(125,-17)+1.1f,-17));
        Shot(cam,"06-East-Fringe-Overview",new(181,Ground(181,-58)+54,-58),new(104,Ground(104,18),18));
        Shot(cam,"06-East-Fringe-Uphill",new(97,Ground(97,33)+2.8f,33),new(77,Ground(77,63)+3.2f,63));
        Shot(cam,"07",new(72,Ground(72,67)+3.1f,67),new(76,Ground(76,87)+4,87));
        Shot(cam,"07-North-Mountain-Overview",new(178,Ground(125,96)+98,32),new(44,Ground(44,105)+18,105));
        Shot(cam,"07-North-Mountain-Overlook",new(62,Ground(62,72)+3.25f,72),new(176,48,47));
        float summitHeight=Ground(159,154);
        Shot(cam,"08",new(231,summitHeight+41,107),new(166,summitHeight+10,152));
        Shot(cam,"08-Lighthouse-Ascent",new(169,Ground(169,132)+2.9f,132),new(161,summitHeight+17,153));
        Shot(cam,"08-Lighthouse-Terrace",new(174,Ground(174,156)+2.9f,156),new(227,43,177));
        Shot(cam,"08-Lighthouse-Seaward",new(262,87,225),new(175,62,154));
        Shot(cam,"01-Beach-Shallows",new(-139,5,-225),new(-149,1,-208));
        Shot(cam,"01-Beach-Overview",new(-146,25,-224),new(-149,7,-181));
        Shot(cam,"01-Beach-Camp",new(-143,Ground(-143,-192)+3.2f,-192),new(-131,Ground(-131,-183)+.8f,-183));
        Shot(cam,"01-Beach-Route",new(-146,Ground(-146,-193)+2.2f,-193),new(-143,Ground(-143,-167)+.8f,-167));
        float coveX=CoconutWaterline(-151);
        Shot(cam,"02-Coconut-Cove",new(coveX-9,5.5f,-174),new(coveX+12,4,-136));
        Shot(cam,"02-Coconut-Trail",new(-149,Ground(-149,-146)+2.25f,-146),new(-135,Ground(-135,-123)+.6f,-123));
        Shot(cam,"03-Forest-Clearing",new(-109,Ground(-109,-58)+2.7f,-58),new(-128,Ground(-128,-44)+2.5f,-44));
        Shot(cam,"03-Forest-Exit",new(-104,Ground(-104,-53)+2.3f,-53),new(-79,Ground(-79,-36)+1.1f,-36));
        Shot(cam,"04-Meadow-Overview",new(-46,Ground(-46,-75)+43,-75),new(29,Ground(29,-46)+1,-46));
        Shot(cam,"04-Meadow-Junction",new(9,Ground(9,-57)+2.7f,-57),new(34,Ground(34,-63)+1,-63));
        var benchmark=EditorSceneManager.OpenScene("Assets/Scenes/Lighthouse_Coast_ArtBenchmark_V01.unity",OpenSceneMode.Additive);
        var explorer=System.Array.Find(benchmark.GetRootGameObjects(),obj=>obj.name=="Explorer");
        if (!explorer) foreach (var root in benchmark.GetRootGameObjects())
        {
            var walker=root.GetComponentInChildren<LighthouseWalker>();
            if (walker) { explorer=walker.gameObject; break; }
        }
        if (!explorer) throw new System.InvalidOperationException("Missing benchmark Explorer");
        var player=Object.Instantiate(explorer);
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(player,scene);
        player.name="Explorer";
        player.transform.SetPositionAndRotation(new Vector3(-145,Ground(-145,-191)+.2f,-191),Quaternion.Euler(0,-36,0));
        var control=player.GetComponent<LighthouseWalker>();
        control.view=cam;
        control.speed=6;
        EditorSceneManager.CloseScene(benchmark,true);
        cam.orthographic=false;cam.fieldOfView=60;control.FollowCamera();
        EditorSceneManager.SaveScene(scene,"Assets/Scenes/Island_World_V01.unity");
        Debug.Log("ISLAND_WORLD_SAVED");
    }
}

[InitializeOnLoad]
public static class IslandWorldPlayCheck
{
    static IslandWorldPlayCheck() => EditorApplication.playModeStateChanged += OnState;

    public static void Run()
    {
        IslandWorldV01.Build();
        EditorSceneManager.OpenScene("Assets/Scenes/Island_World_V01.unity");
        SessionState.SetBool("IslandWorldPlayCheck",true);
        EditorApplication.isPlaying=true;
    }

    static void OnState(PlayModeStateChange state)
    {
        if(!SessionState.GetBool("IslandWorldPlayCheck",false))return;
        if(state==PlayModeStateChange.EnteredPlayMode)
        {
            double start=EditorApplication.timeSinceStartup;
            void Check()
            {
                if(EditorApplication.timeSinceStartup-start<2)return;
                EditorApplication.update-=Check;
                var walker=Object.FindFirstObjectByType<LighthouseWalker>();
                var controller=walker ? walker.GetComponent<CharacterController>() : null;
                var ground=Object.FindFirstObjectByType<Terrain>();
                bool ok=walker && walker.view && controller && ground &&
                    Mathf.Abs(walker.transform.position.y-(ground.SampleHeight(walker.transform.position)+ground.transform.position.y))<.35f;
                if(walker)Debug.Log("Island player at "+walker.transform.position+" grounded="+(controller && controller.isGrounded));
                var motor=Object.FindFirstObjectByType<IslandSystem.ActionMotor>();
                var relic=IslandSystem.RelicCore.Instance;
                Debug.Log($"ISLAND_COMPONENTS motor={!!motor} relic={!!relic} device={relic && relic.Device}");
                ok=ok && motor && relic && relic.Device && motor.GetComponent<IslandSystem.ActionProgramRunner>();
                var survival=Object.FindFirstObjectByType<IslandSystem.BeachSurvival>();
                bool beachOk=survival && !survival.Use(survival.hearth.transform.position,survival.hearth.transform)
                    && !survival.Use(survival.wreckCrate.position+Vector3.right*5,survival.wreckCrate)
                    && survival.Use(survival.driftwood.position,survival.driftwood)
                    && !survival.Use(survival.driftwood.position,survival.driftwood)
                    && survival.Use(survival.wreckCrate.position,survival.wreckCrate)
                    && survival.Use(survival.salvageBarrel.position,survival.salvageBarrel)
                    && survival.Wood==6
                    && survival.Use(survival.hearth.transform.position,survival.hearth.transform)
                    && survival.Lit && survival.embers.activeSelf && survival.Wood==5;
                Debug.Log($"BEACH_SURVIVAL_OK={beachOk}");
                ok &= beachOk;
                var reviewRoutes=new System.Collections.Generic.List<Vector2[]>(IslandWorldV01.Tracks)
                    { IslandWorldV01.ForestClearingWalk, IslandWorldV01.MountainViewWalk, IslandWorldV01.LighthouseTerraceWalk };
                foreach(var route in reviewRoutes)
                {
                    float maxSlope=0,minHeight=float.MaxValue;
                    Vector2 steepest=default;
                    for(int i=1;i<route.Length;i++)
                    {
                        float length=Vector2.Distance(route[i-1],route[i]);
                        int steps=Mathf.CeilToInt(length);
                        float previous=Height(route[i-1]);
                        for(int j=1;j<=steps;j++)
                        {
                            float height=Height(Vector2.Lerp(route[i-1],route[i],j/(float)steps));
                            float slope=Mathf.Abs(height-previous)*steps/length;
                            if(slope>maxSlope) { maxSlope=slope; steepest=Vector2.Lerp(route[i-1],route[i],j/(float)steps); }
                            minHeight=Mathf.Min(minHeight,height);
                            previous=height;
                        }
                    }
                    Debug.Log($"ISLAND_ROUTE grade={maxSlope:F2} steepest={steepest} minimumHeight={minHeight:F2}");
                    ok &= minHeight>0 && maxSlope<1;
                }
                if(ok)
                {
                    foreach(var route in reviewRoutes)
                    {
                        var start=route[0];
                        motor.Teleport(new Vector3(start.x,Height(start)+.15f,start.y));
                        controller.Move(Vector3.down*.5f);
                        bool clear=true;
                        for(int i=1;i<route.Length&&clear;i++)
                        {
                            int steps=Mathf.CeilToInt(Vector2.Distance(route[i-1],route[i])/.25f);
                            for(int j=1;j<=steps;j++)
                            {
                                var point=Vector2.Lerp(route[i-1],route[i],j/(float)steps);
                                var delta=new Vector3(point.x-motor.transform.position.x,0,point.y-motor.transform.position.z);
                                controller.Move(delta+Vector3.down*.025f);
                                controller.Move(Vector3.down*.24f);
                                var actual=new Vector2(motor.transform.position.x,motor.transform.position.z);
                                if(Vector2.Distance(actual,point)<.55f)continue;
                                Debug.LogError($"ISLAND_ROUTE_OBSTRUCTION at {point} actual={actual}");
                                clear=false;
                                break;
                            }
                        }
                        Debug.Log($"ISLAND_ROUTE_CONTROLLER reached={clear} target={route[route.Length-1]}");
                        ok &= clear;
                    }
                }
                float Height(Vector2 p)
                {
                    float floor=ground.SampleHeight(new Vector3(p.x,0,p.y))+ground.transform.position.y;
                    if(Mathf.Abs(p.y+56)<2 && p.x>=-24 && p.x<=5 &&
                        Physics.Raycast(new Vector3(p.x,180,p.y),Vector3.down,out var hit,180) &&
                        hit.collider.gameObject.name=="中央木桥")
                        return Mathf.Max(floor,hit.point.y);
                    return floor;
                }
                SessionState.SetInt("IslandWorldPlayExit",ok?0:2);
                Debug.Log(ok?"ISLAND_WORLD_PLAY_ROUTE_AND_COMPONENTS_OK":"ISLAND_WORLD_PLAY_FAILED");
                EditorApplication.isPlaying=false;
            }
            EditorApplication.update+=Check;
        }
        if(state==PlayModeStateChange.EnteredEditMode)
        {
            SessionState.SetBool("IslandWorldPlayCheck",false);
            EditorApplication.Exit(SessionState.GetInt("IslandWorldPlayExit",2));
        }
    }
}
