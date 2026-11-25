using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static partial class IslandWorldV01
{
    static void PaintStartBeachSand(TerrainData data)
    {
        string path = Folder + "/Beach Golden Sand.terrainlayer";
        var layers = data.terrainLayers;
        var sand = AssetDatabase.LoadAssetAtPath<TerrainLayer>(path);
        if (!sand)
        {
            sand = Object.Instantiate(layers[0]);
            sand.normalScale = .24f;
            sand.diffuseRemapMin = new Vector4(.12f,.06f,.025f,0);
            sand.diffuseRemapMax = new Vector4(1f,.96f,.86f,1);
            AssetDatabase.CreateAsset(sand, path);
        }
        int last = System.Array.IndexOf(layers, sand);
        if (last < 0)
        {
            last = layers.Length;
            System.Array.Resize(ref layers, last + 1);
            layers[last] = sand;
            data.terrainLayers = layers;
        }
        int size = data.alphamapResolution;
        var origin = terrain.transform.position;
        var map = data.GetAlphamaps(0, 0, size, size);
        for (int z = 0; z < size; z++)
        for (int x = 0; x < size; x++)
        {
            var point = new Vector2(origin.x + x * data.size.x / (size - 1), origin.z + z * data.size.z / (size - 1));
            float radius = new Vector2((point.x + 148f) / 48f, (point.y + 194f) / 38f).magnitude;
            float weight = 1f - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.56f, 1f, radius));
            map[z,x,last] = map[z,x,0] * weight;
            map[z,x,0] -= map[z,x,last];
        }
        data.SetAlphamaps(0, 0, map);
    }

    static void GradeStartBeach(TerrainData data)
    {
        int resolution = data.heightmapResolution;
        var heights = data.GetHeights(0, 0, resolution, resolution);
        var origin = terrain.transform.position;
        float reach = 36f;
        for (int z = 0; z < resolution; z++)
        for (int x = 0; x < resolution; x++)
        {
            var point = new Vector2(origin.x + x * data.size.x / (resolution - 1),
                                    origin.z + z * data.size.z / (resolution - 1));
            float distance = Vector2.Distance(point, new Vector2(-150, -198));
            if (distance >= reach) continue;
            float elevation = heights[z, x] * data.size.y + origin.y;
            if (elevation <= 0 || elevation >= 16) continue;
            float blend = (1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(20, reach, distance))) * .85f;
            // Keep the waterline fixed and spread the gain over the dry beach.
            float grade = elevation * Mathf.Lerp(.37f, 1f, Mathf.SmoothStep(0, 1, Mathf.InverseLerp(6f, 16f, elevation)));
            heights[z, x] = (Mathf.Lerp(elevation, grade, blend) - origin.y) / data.size.y;
        }
        data.SetHeights(0, 0, heights);
    }

    static void BuildStartBeach()
    {
        Region("01 · 沉船海滩 / 起始营地");
        var wreck = Place(Pirate + "ship-wreck.fbx", -167, -184, 21, 16, -31, 4.1f);
        var grounding = new Vector3(-167, Ground(-167, -184), -184);
        wreck.transform.RotateAround(grounding, Vector3.right, -8f);
        wreck.name = "登陆点 · 搁浅船体";
        foreach (var part in wreck.GetComponentsInChildren<MeshFilter>())
            if (part.sharedMesh && !part.GetComponent<Collider>())
                part.gameObject.AddComponent<MeshCollider>().sharedMesh = part.sharedMesh;
        var timber = BeachMaterial("Weathered Wreck", Color.white, "Island/Weathered Wreck");
        timber.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(Pirate + "Textures/colormap.png");
        EditorUtility.SetDirty(timber);
        foreach (var renderer in wreck.GetComponentsInChildren<MeshRenderer>())
        {
            var materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++) materials[i] = timber;
            renderer.sharedMaterials = materials;
        }
        BuildTornSail(wreck);

        Palm(-176, -185, 13, 39);
        Palm(-124, -190, 12, 149);
        Palm(-158, -171, 14, 108);
        Rock(3, -123, -200, 7.5f, 3.1f, -30, .6f);
        Rock(6, -177, -191, 8, 3.4f, 81, .8f);
        Rock(2, -130, -171, 5, 2.8f, 67, .8f);
        Grove("Bush_Common_Flowers", new(-174,-182,3.8f,52), new(-125,-185,4.9f,46),
            new(-129,-172,3.5f,125), new(-154,-166,4,168), new(-158,-176,2.5f,35));
        Place(Pirate + "grass-plant.fbx", -125, -192, 1.7f, .66f, 110);
        Place(Pirate + "grass-plant.fbx", -155, -169, 1.45f, .59f, 70);

        Grove("Flower_3_Group", new(-127,-188,2.2f,31), new(-132,-170,1.65f,100), new(-158,-173,2.2f,204), new(-126,-199,3.0f,73));
        Grove("Flower_4_Group", new(-125,-186,1.85f,54), new(-130,-171,1.45f,109), new(-174,-182,2.1f,78), new(-124,-198,2.6f,118));
        CampShelter(-128, -183);
        Place(Pirate + "crate.fbx", -126, -185, 1.1f, 1f, 18);
        Place(Pirate + "barrel.fbx", -129.7f, -181, .88f, .92f, -22);
        var salvage = Place(Pirate + "barrel.fbx", -157, -197, 1.05f, 1.15f, 13);
        var drift = Place(Pirate + "platform-planks.fbx", -159, -200, 3.4f, .23f, 52);
        Place(Pirate + "platform-planks.fbx", -166, -202, 2.2f, .15f, -18);
        BuildBeachHearth(-133, -186);
        var cache = Place(Pirate + "crate.fbx", -169.8f, -175.4f, .95f, .82f, 33);
        cache.name = "沉船背风侧 · 干木箱";
        AddBeachSurvival(drift.transform, cache.transform, salvage.transform, wreck.transform);
        Place(Pirate + "mast-ropes.fbx", -171, -189, 3.6f, 1.4f, 64, .24f);
        var tree = Place("Assets/IslandSystem/ActionDemo/PracticeTree.prefab",-154,-163,7.4f,8f,119,.16f);
        tree.name = "岛内入口 · 可砍伐阔叶树";
        Grove("Fern_1",new(-151,-164,1.8f,58),new(-133,-166,1.7f,91));
        BeachDetail(-139.7f, -208.4f, false, 23);
        BeachDetail(-137.5f, -207.8f, true, 42);
        BeachDetail(-151.3f, -201.4f, false, 205);
        BeachReef(-173, -211, 3.7f, 1.1f, 56);
        BeachReef(-158, -221, 3.1f, .87f, 15);
        BeachReef(-136, -220, 2.3f, .77f, 125);
        Rock(1, -138, -206, 1.65f, .63f, 73, .12f);
        Rock(2, -136.8f, -205.8f, .84f, .34f, 106, .09f);
        Place(Pirate + "tool-paddle.fbx", -153.8f, -202, 2.2f, .13f, 38);
        BeachReef(-177.8f,-193.6f,4.2f,1.8f,37);
        BeachReef(-175.1f,-195.5f,2.7f,1.15f,108);
        BuildShallows();
    }

    static void AddBeachSurvival(Transform drift, Transform cache, Transform salvage, Transform wreck)
    {
        const float x = -133, z = -186;
        Vector3 foot = new(x, Ground(x,z), z);
        var wood = BeachMaterial("Camp tripod warm wood", new Color(.36f,.19f,.09f));
        var iron = BeachMaterial("Camp iron", new Color(.25f,.23f,.21f));
        var top = foot + Vector3.up*1.75f;
        BeachBeam("吊锅架 · 西脚", foot+new Vector3(-.71f,.02f,-.51f),top,.074f,wood);
        BeachBeam("吊锅架 · 东脚", foot+new Vector3(.71f,.02f,-.37f),top,.070f,wood);
        BeachBeam("吊锅架 · 后脚", foot+new Vector3(.13f,.02f,.67f),top,.066f,wood);
        BeachBeam("吊索",top,foot+Vector3.up*.97f,.023f,iron);
        var pot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pot.name = "营火 · 吊锅";
        pot.transform.SetParent(group);
        pot.transform.position = foot+Vector3.up*.88f;
        pot.transform.localScale = new Vector3(.60f,.42f,.56f);
        pot.GetComponent<Renderer>().sharedMaterial = iron;
        Object.DestroyImmediate(pot.GetComponent<Collider>());

        var camp = new GameObject("沉船海滩 · 柴火和营火");
        camp.transform.SetParent(group);
        camp.transform.position = foot + Vector3.up*.22f;
        var survival = camp.AddComponent<IslandSystem.BeachSurvival>();
        survival.driftwood = drift;
        survival.wreckCrate = cache;
        survival.salvageBarrel = salvage;
        survival.shelter = Point("检查布棚",-130,-183);
        survival.wreckMarker = Point("检查残骸",-156,-189);
        survival.exit = Point("林间小路",-143,-165);
        survival.stones = new[] { Pebble(-140,-195,32), Pebble(-151,-191,72) };
        camp.AddComponent<IslandSystem.BeachGuideHud>();
        var chapter = camp.AddComponent<IslandSystem.BeachChapter>();
        chapter.wreck = wreck;
        chapter.approach = Point("抵达船骸",-151,-189);
        chapter.launch = Point("断梁前",-156,-195);
        chapter.supplies = Place(Pirate+"crate.fbx",-150.2f,-195,1.1f,.94f,14).transform;
        chapter.supplies.name = "受困的补给箱";
        Barrier("横倒的断梁",-153.55f,-195,new Vector3(.46f,.58f,3.35f),4);
        Barrier("北侧船板残骸",-151.85f,-193.3f,new Vector3(3.2f,1.3f,.46f),4);
        Barrier("南侧船板残骸",-151.85f,-196.7f,new Vector3(3.2f,1.3f,.46f),-3);
        Barrier("后方船板残骸",-149.7f,-195,new Vector3(.55f,1.3f,3.5f),3);

        void Barrier(string label,float px,float pz,Vector3 size,float yaw)
        {
            var timber=GameObject.CreatePrimitive(PrimitiveType.Cube);
            timber.name=label;
            timber.transform.SetParent(group);
            timber.transform.SetPositionAndRotation(new Vector3(px,Ground(px,pz)+size.y*.5f-.08f,pz),
                Quaternion.Euler(0,yaw,0));
            timber.transform.localScale=size;
            timber.GetComponent<Renderer>().sharedMaterial=wood;
        }

        Transform Point(string label,float px,float pz)
        {
            var marker = new GameObject(label).transform;
            marker.SetParent(group);
            marker.position = new Vector3(px,Ground(px,pz),pz);
            return marker;
        }

        Transform Pebble(float px,float pz,float yaw)
        {
            var stone = Place(Nature+"Rock_Medium_1.fbx",px,pz,.47f,.29f,yaw,.025f);
            stone.name = "可拾取石头";
            return stone.transform;
        }
        survival.hearth = camp.AddComponent<Light>();
        survival.hearth.type = LightType.Point;
        survival.hearth.color = new Color(1f,.48f,.18f);
        survival.hearth.range = 5f;
        survival.hearth.intensity = 3f;
        survival.hearth.enabled = false;

        var fire = new GameObject("营火 · 火焰");
        fire.transform.SetParent(camp.transform,false);
        var particles = fire.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.startColor = new Color(1f,.51f,.17f);
        main.startSpeed = .8f;
        main.startSize = new ParticleSystem.MinMaxCurve(.065f,.17f);
        main.startLifetime = .55f;
        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.radius = .23f;
        fire.GetComponent<ParticleSystemRenderer>().sharedMaterial =
            BeachMaterial("Camp flame",Color.white,"Sprites/Default");
        fire.SetActive(false);
        survival.embers = fire;
    }

    static void BeachDetail(float x, float z, bool star, float yaw)
    {
        string name = star ? "Tidepool Star" : "Scallop Shell";
        var center = new Vector3(x, Ground(x, z) + .045f, z);
        int count = star ? 10 : 9;
        var vertices = new Vector3[count + 1];
        var triangles = new int[(star ? count : count - 1) * 3];
        vertices[0] = center + Vector3.up * (star ? .12f : .23f);
        for (int i = 0; i < count; i++)
        {
            float angle = (yaw + (star ? i * 36f : i * 21f)) * Mathf.Deg2Rad;
            float radius = star ? (i % 2 == 0 ? .39f : .14f) : .29f;
            vertices[i + 1] = center + new Vector3(Mathf.Sin(angle) * radius, 0f, Mathf.Cos(angle) * radius);
            if (i * 3 >= triangles.Length) continue;
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = 1 + (i + 1) % count;
        }
        var mesh = new Mesh { vertices = vertices, triangles = triangles };
        mesh.RecalculateNormals();
        SaveBeachMesh(name + " " + yaw, mesh, BeachMaterial(name, star ? new Color(.91f,.39f,.18f) : new Color(1f,.75f,.61f)));
    }

    static void BeachReef(float x, float z, float width, float height, float yaw)
    {
        float surface = Ground(x, z);
        float sink = Mathf.Clamp(surface + height * .62f, -.3f, .18f);
        Rock(2, x, z, width, height, yaw, sink);
    }

    static Material BeachMaterial(string name, Color tint, string shader = "Standard")
    {
        string path = Folder + "/" + name + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material) return material;
        material = new Material(Shader.Find(shader));
        if (material.HasProperty("_Color")) material.color = tint;
        if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", .16f);
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    static void SaveBeachMesh(string name, Mesh source, Material material)
    {
        string path = Folder + "/" + name + ".asset";
        var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (mesh)
        {
            EditorUtility.CopySerialized(source, mesh);
            Object.DestroyImmediate(source);
        }
        else
        {
            mesh = source;
            AssetDatabase.CreateAsset(mesh, path);
        }
        var obj = new GameObject(name);
        obj.transform.SetParent(group);
        obj.AddComponent<MeshFilter>().sharedMesh = mesh;
        var renderer = obj.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        if (material.shader.name == "Island/Beach Shallows")
        {
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }
    }

    static void BuildShallows() => BuildShallows("Beach Shallows",new Vector2(-202,-259),97,78,
        new Vector2(-155,-218),new Vector2(45,37));

    static void BuildShallows(string name,Vector2 origin,int columns,int rows,Vector2 center,Vector2 radius,bool estuary=false)
    {
        var vertices = new Vector3[columns * rows];
        var depths = new Vector2[vertices.Length];
        var triangles = new List<int>((columns - 1) * (rows - 1) * 6);
        for (int z = 0; z < rows; z++)
        for (int x = 0; x < columns; x++)
        {
            int index = x + z * columns;
            float px = origin.x + x;
            float pz = origin.y + z;
            vertices[index] = new Vector3(px, .075f, pz);
            float cove = new Vector2((px-center.x)/radius.x,(pz-center.y)/radius.y).magnitude;
            float fade = Feather(cove,.60f,1f);
            // Keep surf on the sides of the inlet and let the river's surface show through its throat.
            if(estuary)
            {
                float edge=Mathf.Abs(px-RiverX(pz));
                fade*=1f-Feather(edge,5f,12f)*Mathf.SmoothStep(0,1,Mathf.InverseLerp(-254,-233,pz));
            }
            depths[index] = new Vector2(-Ground(px, pz), fade);
            if (x == columns - 1 || z == rows - 1) continue;
            triangles.AddRange(new[] {index, index + columns, index + 1, index + 1, index + columns, index + columns + 1});
        }
        var mesh = new Mesh { vertices = vertices, uv = depths, triangles = triangles.ToArray() };
        mesh.RecalculateBounds();
        SaveBeachMesh(name, mesh, BeachMaterial("Beach Shallows", Color.white, "Island/Beach Shallows"));
    }

    static void BuildTornSail(GameObject wreck)
    {
        var bounds = BoundsOf(wreck);
        var mast = new Vector3(bounds.center.x, bounds.max.y - 8.45f, bounds.center.z);
        Vector3 yard = wreck.transform.right * 3.1f;
        Vector3 bow = -wreck.transform.forward;
        Vector3 Point(float u, float v, float b) => mast + yard * u + Vector3.up * v + bow * b;
        var wood = BeachMaterial("Shelter Timber", new Color(.38f,.21f,.10f));
        BeachBeam("帆桁", mast - yard + Vector3.up * .48f, mast + yard - Vector3.up * .15f, .11f, wood);
        BeachBeam("断桅", mast - Vector3.up * 6f + bow * .55f, mast + Vector3.up * .8f, .19f, wood);
        var rope = BeachMaterial("Wreck Rigging", new Color(.58f,.37f,.19f));
        Vector3 rigStart = mast + Vector3.up * .55f - yard * .84f;
        Vector3 rigEnd = mast - Vector3.up * 5.1f - yard * 1.7f + bow * 1.5f;
        Vector3 bend = Vector3.Lerp(rigStart, rigEnd, .56f) - Vector3.up * .45f;
        BeachBeam("残存索具", rigStart, bend, .062f, rope);
        BeachBeam("残存索具", bend, rigEnd, .062f, rope);
        var mesh = new Mesh();
        mesh.vertices = new[]
        {
            Point(-.88f,.3f,0), Point(-.27f,.16f,.08f), Point(.43f,0,.06f), Point(1,-.22f,0),
            Point(-.81f,-1.24f,.29f), Point(-.15f,-1.38f,.65f), Point(.48f,-1.48f,.73f), Point(.91f,-1.32f,.32f),
            Point(-.60f,-3.02f,.46f), Point(-.34f,-2.41f,.42f), Point(-.06f,-3.35f,.30f),
            Point(.21f,-2.35f,.42f), Point(.50f,-3.06f,.35f), Point(.85f,-1.96f,.32f)
        };
        var face = new[] {0,4,1,1,4,5,1,5,2,2,5,6,2,6,3,3,6,7,4,8,9,4,9,5,5,9,10,5,10,11,6,11,12,6,12,7,7,12,13};
        var triangles = new List<int>(face);
        for (int i = 0; i < face.Length; i += 3)
            triangles.AddRange(new[] {face[i], face[i+2], face[i+1]});
        mesh.triangles = triangles.ToArray();
        // Split the faces so opposite sides retain independent normals.
        var positions = mesh.vertices;
        var corners = triangles.ConvertAll(i => positions[i]);
        mesh.Clear();
        mesh.SetVertices(corners);
        for (int i = 0; i < triangles.Count; i++) triangles[i] = i;
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        SaveBeachMesh("Torn sail", mesh, BeachMaterial("Sunbleached Sailcloth", new Color(.94f,.77f,.50f)));
    }

    static void BeachBeam(string name, Vector3 start, Vector3 end, float radius, Material material)
    {
        var beam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        beam.name = name;
        beam.transform.SetParent(group);
        beam.transform.SetPositionAndRotation((start + end) * .5f, Quaternion.FromToRotation(Vector3.up,end-start));
        beam.transform.localScale = new Vector3(radius * 2, (end - start).magnitude * .5f, radius * 2);
        beam.GetComponent<MeshRenderer>().sharedMaterial = material;
        Object.DestroyImmediate(beam.GetComponent<Collider>());
    }

    static void BuildBeachHearth(float x, float z)
    {
        var stones = new[] { new Vector2(-.79f,.25f), new Vector2(-.39f,.62f), new Vector2(.19f,.66f),
            new Vector2(.69f,.39f), new Vector2(.72f,-.18f), new Vector2(.29f,-.56f), new Vector2(-.29f,-.52f) };
        for (int i = 0; i < stones.Length; i++)
            Rock(i, x + stones[i].x, z + stones[i].y, .57f, .34f, i * 38, .055f);

        var wood = BeachMaterial("Charred Hearth Wood", new Color(.23f,.105f,.054f));
        for (int i = 0; i < 2; i++)
        {
            var log = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            log.name = "营火 · 焦木";
            log.transform.SetParent(group);
            log.transform.SetPositionAndRotation(new Vector3(x, Ground(x, z) + .13f + i * .10f, z),
                Quaternion.Euler(0, 34 + i * 92, 85));
            log.transform.localScale = new Vector3(.18f, .53f, .19f);
            log.GetComponent<MeshRenderer>().sharedMaterial = wood;
            Object.DestroyImmediate(log.GetComponent<Collider>());
        }
    }
}
