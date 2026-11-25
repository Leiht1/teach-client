using UnityEngine;

public static partial class IslandWorldV01
{
    static float EstuaryWeight(float z) => Feather(z,-193,-142);

    static float RiverHalfWidth(float z) => MeadowRiverHalfWidth(z) +
        EstuaryWeight(z)*(2.6f + 6.7f*Feather(z,-230,-188) + 1.2f*Mathf.Sin((z+223)*.17f));

    static float EstuarySandbar(float x,float z) =>
        Feather(new Vector2((x-17f)*1.15f,(z+222f)*.54f).magnitude,1f,4.7f);

    static float EstuaryTrailDistance(Vector2 point)
    {
        var route=Tracks[1];
        float closest=float.MaxValue;
        for(int i=1;i<route.Length;i++)
        {
            var segment=route[i]-route[i-1];
            var offset=point-route[i-1];
            float t=Mathf.Clamp01(Vector2.Dot(offset,segment)/segment.sqrMagnitude);
            closest=Mathf.Min(closest,(offset-segment*t).magnitude);
        }
        return closest;
    }

    static void PaintEstuary(TerrainData data)
    {
        int n=data.alphamapResolution;
        var map=data.GetAlphamaps(0,0,n,n);
        Vector3 origin=terrain.transform.position;
        for(int row=0;row<n;row++)
        {
            float z=origin.z+row*data.size.z/(n-1);
            if(z< -253 || z> -150)continue;
            float center=RiverX(z);
            float half=RiverHalfWidth(z);
            for(int col=0;col<n;col++)
            {
                float x=origin.x+col*data.size.x/(n-1);
                if(Mathf.Abs(x-center)>35)continue;
                float edge=Mathf.Abs(x-center)-half;
                float height=Ground(x,z);
                float boundary=edge+Mathf.Sin(z*.33f+x*.28f)*1.1f;
                float band=Feather(boundary,3f,19f)*EstuaryWeight(z);
                float trail=Feather(EstuaryTrailDistance(new Vector2(x,z)),1.8f,4.5f);
                float moisture=Feather(Mathf.Abs(boundary-1.8f),1.2f,7.3f)*Feather(-height,.6f,2.4f);
                float sand=Mathf.Lerp(.95f,.37f,moisture);
                float earth=Mathf.Lerp(.04f,.37f,moisture);
                float pebble=1-sand-earth;
                float pathSand=Mathf.Lerp(.12f,.68f,Feather(z,-206,-160));
                for(int layer=0;layer<map.GetLength(2);layer++)
                {
                    float bank=layer==0?sand:layer==1?earth:layer==4?pebble:0;
                    float path=layer==0?pathSand:layer==3?1-pathSand:0;
                    float value=Mathf.Lerp(map[row,col,layer],bank,band*.91f*(1-trail));
                    map[row,col,layer]=Mathf.Lerp(value,path,trail*.82f*EstuaryWeight(z));
                }
            }
        }
        data.SetAlphamaps(0,0,map);
    }

    static void DressEstuary()
    {
        BuildShallows("Estuary tidal shallows",new Vector2(-15,-263),79,56,
            new Vector2(25,-239),new Vector2(36,28),true);

        // Plant groups frame three distinct views: the inland bend, the sand spit and the open sea.
        Grove("CommonTree_3",new(-16,-153,22,24),new(58,-175,23,123),new(-17,-213,19,57));
        Grove("Fern_1",new(-17,-166,3,51),new(-11,-181,2.2f,132),new(40,-191,2.7f,88),
            new(39,-210,2.1f,61),new(5,-221,2.5f,21));
        Grove("Grass_Wispy_Tall",new(-9,-176,2.3f,64),new(-8,-179,2.9f,92),new(-6,-181,2.25f,52),
            new(20,-184,2.2f,18),new(23,-187,2.7f,54),new(24,-190,2.2f,95),
            new(8,-223,1.9f,82),new(7,-226,2.8f,21),new(9,-227,2.55f,67),
            new(39,-223,2.9f,93),new(40,-226,2.5f,76));
        Grove("Flower_3_Group",new(-11,-173,2.2f,91),new(22,-182,2.7f,32),new(42,-199,2.6f,64),
            new(40,-214,2.1f,109),new(5,-223,2.25f,73));
        Grove("Flower_4_Group",new(-9,-170,2.15f,20),new(23,-180,1.8f,63),new(43,-202,2.05f,102),
            new(6,-228,2.1f,74));

        Rock(1,-10,-179,5.8f,2.6f,24,.48f);
        Rock(2,-7,-182,3.4f,1.25f,61,.34f);
        Rock(0,19,-178,4.5f,1.8f,87,.32f);
        Rock(1,23,-189,2.8f,1.18f,12,.31f);
        Rock(2,8,-228,4.6f,1.74f,59,.46f);
        Rock(1,11,-230,2.2f,.8f,77,.28f);
        Rock(2,40,-227,5.7f,2.4f,42,.57f);
        Rock(1,37,-229,2.2f,1.13f,11,.41f);
        Rock(0,36,-231,1.7f,.73f,59,.25f);
        Rock(1,17,-222,2.35f,1.15f,104,.48f);
        Rock(0,15,-220,1.5f,.66f,73,.23f);
        Rock(0,6,-208,1.75f,.7f,58,.16f);
        Rock(2,8,-209,1.1f,.44f,67,.13f);
        Rock(1,9,-211,1.4f,.49f,48,.10f);

        Cattails(-8,-177,1.10f);
        Cattails(21,-185,.91f);
        Cattails(7,-226,1.15f);
        Cattails(39,-224,1.27f);
        Cattails(37.8f,-226.4f,1.02f);
        Cattails(38.6f,-225.7f,.82f);
        Cattails(8.9f,-224.2f,1.17f);
        Cattails(10.4f,-228.3f,.94f);
        Cattails(16.2f,-219.4f,.76f);
        Grove("Grass_Common_Short",new(37,-223,2.5f,63),new(38,-220,2.1f,114),
            new(10,-225,3,73),new(8,-224,2.1f,33),new(16,-220,1.8f,121));
        Grove("Flower_3_Group",new(37,-221,1.6f,54),new(35,-224,1.9f,87),new(9,-224,1.6f,78));
        Lily(21,-211,.61f,23,true);
        Lily(20,-212.4f,.47f,132,false);
        Lily(22.1f,-212.8f,.74f,71,true);
        Lily(21.2f,-214,.46f,149,false);
        Lily(28,-222,.66f,71,false);
        Lily(29.1f,-223.2f,.49f,160,true);
        Lily(29.8f,-224.4f,.55f,100,false);
        Lily(30.5f,-225.6f,.39f,19,false);

        var wood=BeachMaterial("Cove Driftwood",new Color(.47f,.31f,.17f));
        Vector3 a=new(41,Ground(41,-218)+.21f,-218);
        Vector3 b=new(44,Ground(44,-215)+.28f,-215);
        BeachBeam("河水冲刷的枯木",a,b,.27f,wood);
        BeachBeam("折枝",Vector3.Lerp(a,b,.47f),new(42.9f,b.y+.62f,-218.0f),.09f,wood);
    }

    static void Cattails(float x,float z,float scale)
    {
        var leaf=BeachMaterial("Estuary Reed Green",new Color(.34f,.48f,.14f));
        var seed=BeachMaterial("Estuary Cattail",new Color(.49f,.28f,.14f));
        var stems=new[]{new Vector3(-.55f,1.44f,.28f),new Vector3(.12f,1.85f,-.33f),new Vector3(.55f,1.35f,.22f)};
        foreach(var stem in stems)
        {
            Vector3 foot=new(x+stem.x,Ground(x+stem.x,z+stem.z)-.08f,z+stem.z);
            Vector3 top=foot+stem*scale;
            BeachBeam("香蒲茎",foot,top,.027f,leaf);
            BeachBeam("香蒲穗",top-Vector3.up*.12f,top+Vector3.up*.17f,.081f,seed);
        }
    }

    static void Lily(float x,float z,float radius,float yaw,bool bloom)
    {
        var river=UnityEditor.AssetDatabase.LoadAssetAtPath<Mesh>(Folder+"/River.asset").vertices;
        float index=(z+260f)*.5f;
        int row=Mathf.FloorToInt(index);
        float y=Mathf.Lerp(river[row*9].y,river[(row+1)*9].y,index-row)+.05f;
        var start=new Vector3(x,y,z);
        if(Ground(x,z)>y-.08f || Mathf.Abs(x-RiverX(z))+radius>RiverHalfWidth(z))return;
        var pad=new GameObject("睡莲 · 缺口叶");
        pad.transform.SetParent(group);
        pad.transform.SetPositionAndRotation(start,Quaternion.Euler(0,yaw,0));
        const int sides=10;
        var verts=new Vector3[sides+2];
        var triangles=new int[sides*3];
        for(int i=0;i<=sides;i++)
        {
            float a=Mathf.Lerp(.27f,Mathf.PI*2-.27f,i/(float)sides);
            verts[i+1]=new Vector3(Mathf.Sin(a)*radius,.02f*Mathf.Cos(a),Mathf.Cos(a)*radius);
            if(i==sides)continue;
            triangles[i*3+1]=i+1;
            triangles[i*3+2]=i+2;
        }
        var mesh=new Mesh{vertices=verts,triangles=triangles};
        mesh.RecalculateNormals();
        string meshName="Estuary Lily "+x.ToString("0.0",System.Globalization.CultureInfo.InvariantCulture)+" "+z.ToString("0.0",System.Globalization.CultureInfo.InvariantCulture);
        var asset=UnityEditor.AssetDatabase.LoadAssetAtPath<Mesh>(Folder+"/"+meshName+".asset");
        if(asset)
        {
            UnityEditor.EditorUtility.CopySerialized(mesh,asset);
            Object.DestroyImmediate(mesh);
        }
        else
        {
            asset=mesh;
            UnityEditor.AssetDatabase.CreateAsset(asset,Folder+"/"+meshName+".asset");
        }
        pad.AddComponent<MeshFilter>().sharedMesh=asset;
        pad.AddComponent<MeshRenderer>().sharedMaterial=BeachMaterial("Estuary Lily Leaf",new Color(.25f,.51f,.22f));
        if(!bloom)return;
        var petals=BeachMaterial("Estuary Lily Petal",new Color(1f,.69f,.78f));
        var heart=BeachMaterial("Estuary Lily Heart",new Color(1f,.78f,.24f));
        for(int i=0;i<5;i++)
        {
            var rotation=Quaternion.Euler(0,i*72f,0);
            var petal=GameObject.CreatePrimitive(PrimitiveType.Sphere);
            petal.name="睡莲花瓣";
            petal.transform.SetParent(pad.transform,false);
            petal.transform.localPosition=rotation*new Vector3(0,.09f,.105f);
            petal.transform.localRotation=rotation*Quaternion.Euler(-20,0,0);
            petal.transform.localScale=new Vector3(.15f,.095f,.25f);
            petal.GetComponent<MeshRenderer>().sharedMaterial=petals;
            Object.DestroyImmediate(petal.GetComponent<Collider>());
        }
        BeachBeam("睡莲花心",start+Vector3.up*.078f,start+Vector3.up*.155f,.075f,heart);
    }
}
