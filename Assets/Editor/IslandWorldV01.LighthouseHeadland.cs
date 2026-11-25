using UnityEngine;

public static partial class IslandWorldV01
{
    // The eastern terrace is reached from the existing circuit, outside the tower podium.
    static readonly Vector2 LighthouseTerrace=new(176,158);
    internal static readonly Vector2[] LighthouseTerraceWalk={new(169,153),new(173,156),LighthouseTerrace};

    static float LighthouseTrailDistance(Vector2 p)
    {
        var route=Tracks[0];
        float distance=float.MaxValue;
        for(int i=24;i<route.Length;i++)
            distance=Mathf.Min(distance,DistanceToTrailSegment(p,route[i-1],route[i]));
        return distance;
    }

    static void ShapeLighthouseHeadland(TerrainData data)
    {
        var origin=terrain.transform.position;
        int n=data.heightmapResolution;
        var heights=data.GetHeights(0,0,n,n);
        float summit=Ground(Summit.x,Summit.y);
        for(int row=0;row<n;row++)
        {
            float z=origin.z+row*data.size.z/(n-1);
            if(z<98 || z>183)continue;
            for(int col=0;col<n;col++)
            {
                float x=origin.x+col*data.size.x/(n-1);
                if(x<126 || x>220)continue;
                var point=new Vector2(x,z);
                float height=origin.y+heights[row,col]*data.size.y;
                float land=Mathf.SmoothStep(0,1,Mathf.InverseLerp(1f,27f,height));
                float crown=Feather(new Vector2((x-162)*.83f,(z-153)*1.12f).magnitude,16,27);
                float terrace=Feather(new Vector2((x-175)*.97f,(z-157)*1.06f).magnitude,7.8f,15);
                height=Mathf.Lerp(height,summit,crown*land);
                height=Mathf.Lerp(height,summit-.68f,terrace*land);
                float seaFace=Mathf.SmoothStep(0,1,Mathf.InverseLerp(179,210,x))
                    *Feather(Mathf.Abs(z-150),23,40)*(1-terrace)*land*Feather(x,205,220);
                float shelf=summit*Mathf.SmoothStep(0,1,1-Mathf.InverseLerp(179,213,x));
                height=Mathf.Lerp(height,Mathf.Min(height,shelf),seaFace);
                heights[row,col]=(height-origin.y)/data.size.y;
            }
        }
        data.SetHeights(0,0,heights);
    }

    static void PaintLighthouseHeadland(TerrainData data)
    {
        var origin=terrain.transform.position;
        int n=data.alphamapResolution;
        var map=data.GetAlphamaps(0,0,n,n);
        for(int row=0;row<n;row++)
        {
            float z=origin.z+row*data.size.z/(n-1);
            if(z<95 || z>196)continue;
            for(int col=0;col<n;col++)
            {
                float x=origin.x+col*data.size.x/(n-1);
                if(x<134 || x>227)continue;
                var p=new Vector2(x,z);
                float distance=LighthouseTrailDistance(p);
                float approach=Feather(distance,2.45f,5.8f)*Mathf.SmoothStep(0,1,Mathf.InverseLerp(97,124,z));
                float court=Feather(new Vector2((x-162)*.82f,z-153).magnitude,13,24);
                float terrace=Feather(Vector2.Distance(p,LighthouseTerrace),5.5f,9.5f);
                float paving=Mathf.Max(court*.84f,terrace*.94f);
                float slope=data.GetSteepness((x-origin.x)/data.size.x,(z-origin.z)/data.size.z);
                float cliff=Mathf.SmoothStep(0,1,Mathf.InverseLerp(29,57,slope))
                    *Feather(Vector2.Distance(p,new Vector2(190,147)),28f,53f)*(1-approach)*(1-paving);
                for(int layer=0;layer<map.GetLength(2);layer++)
                {
                    float face=layer==5?.79f:layer==3?.10f:layer==2?.11f:0;
                    float path=layer==0?.34f:layer==3?.48f:layer==5?.18f:0;
                    float platform=layer==5?.47f:layer==0?.29f:layer==3?.18f:layer==2?.06f:0;
                    float value=Mathf.Lerp(map[row,col,layer],face,cliff*.96f);
                    value=Mathf.Lerp(value,path,approach*.95f);
                    map[row,col,layer]=Mathf.Lerp(value,platform,paving);
                }
            }
        }
        data.SetAlphamaps(0,0,map);
    }

    static void DressLighthouseHeadland()
    {
        var tower=Place(Coast+"Lighthouse_complete.prefab",159,154,16,49,21,.25f);
        var podium=BeachMaterial("Lighthouse Headland Cut Stone",new Color(.62f,.59f,.52f),"Island/North Mountain Stone");
        foreach(var r in tower.GetComponentsInChildren<MeshRenderer>())
        {
            if(!r.name.Contains("dressed stone podium"))continue;
            r.sharedMaterial=podium;
            if(!r.TryGetComponent<Collider>(out _))r.gameObject.AddComponent<MeshCollider>();
        }

        var slate=UpdateMountainStone("North Mountain Blue Slate",new Color(.53f,.55f,.54f));
        var warm=UpdateMountainStone("North Mountain Warm Stone",new Color(.57f,.55f,.50f));
        var gray=UpdateMountainStone("North Mountain Gray Stone",new Color(.55f,.56f,.53f));

        // The offshore side is built in buttresses with stepping ledges between the large masses.
        Buttress(2,187,151,31,57,4,29,slate);
        Buttress(3,185,137,30,46,5,52,warm);
        Buttress(1,184,168,29,53,4,109,gray);
        Buttress(2,195,148,25,30,1,41,gray);
        Buttress(3,193,162,25,32,0,79,slate);
        Buttress(1,194,132,24,23,0,-22,warm);
        Buttress(2,188,122,20,18,2,64,gray);
        Buttress(1,200,167,16,13,-1,105,warm);
        Buttress(2,201,151,17,14,-1,67,gray);
        void Buttress(int variant,float x,float z,float width,float height,float baseY,float yaw,Material stone)
        {
            MountainRock(variant,x,z,width,height,yaw,Ground(x,z)-baseY,stone);
        }

        MountainRock(1,181,124,10,6.2f,93,3.65f,gray);
        MountainRock(2,151,121,8.3f,5.4f,116,2.4f,warm);
        MountainRock(1,157,130,7.3f,3.95f,73,1.55f,slate);
        MountainRock(3,178,130,4.8f,2.2f,41,.68f,gray);
        MountainRock(2,150,147,4.6f,1.73f,37,.72f,warm);

        Grove("Pine_3",new(155,124,14,52),new(180,131,10,94),new(147,161,11,24),new(171,173,10,71));
        Grove("Fern_1",new(156,129,2.4f,45),new(177,132,2.45f,81),new(181,151,1.8f,68),
            new(159,166,1.7f,119),new(153,145,1.8f,76),new(180,163,1.7f,44));
        Grove("Grass_Common_Short",new(158,122,2.5f,79),new(157,131,2.4f,46),new(179,133,2.25f,107),
            new(179,151,2.3f,83),new(180,161,1.6f,27),new(157,166,1.9f,113));
        Grove("Bush_Common_Flowers",new(155,127,3.3f,95),new(178,136,2.8f,39),
            new(157,165,2.8f,65),new(179,163,2.4f,21),new(177,150,2.1f,84));
        Grove("Flower_3_Group",new(158,127,2.5f,18),new(177,133,2.45f,41),new(180,154,2.2f,103),
            new(178,162,1.8f,84),new(153,151,1.45f,60));
        Grove("Flower_4_Group",new(157,135,2.15f,86),new(179,135,2.1f,12),new(179,152,2.1f,110),
            new(180,161,1.8f,46),new(152,152,1.35f,31));

        var wood=BeachMaterial("Lighthouse Headland Rail",new Color(.48f,.30f,.17f));
        HeadlandFence(wood,new(179,150),new(180,153),new(180,156),new(180,159),new(179,162),new(177,165),new(175,167));
        HeadlandFence(wood,new(175,167),new(170,169),new(166,169),new(162,169),new(158,169),new(154,168));
        HeadlandFence(wood,new(173,123),new(173,127),new(173,131),new(173,135),new(173,139));
        BuildHeadlandReefs(slate,warm,gray);
    }

    static void HeadlandFence(Material wood,params Vector2[] posts)
    {
        Vector3 previous=default;
        for(int i=0;i<posts.Length;i++)
        {
            Vector2 point=posts[i];
            Vector3 foot=new(point.x,Ground(point.x,point.y),point.y);
            Beam("崖边护栏 · 木桩",foot-Vector3.up*.30f,foot+Vector3.up*1.21f,.25f,.24f);
            if(i>0)
            {
                Beam("崖边护栏 · 扶手",previous+Vector3.up*.96f,foot+Vector3.up*.96f,.18f,.21f);
                Beam("崖边护栏 · 横档",previous+Vector3.up*.40f,foot+Vector3.up*.40f,.13f,.14f);
            }
            previous=foot;
        }

        void Beam(string label,Vector3 start,Vector3 end,float width,float depth)
        {
            var obj=GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name=label;
            obj.transform.SetParent(group);
            obj.transform.SetPositionAndRotation((start+end)*.5f,Quaternion.FromToRotation(Vector3.up,end-start));
            obj.transform.localScale=new Vector3(width,(end-start).magnitude,depth);
            obj.GetComponent<MeshRenderer>().sharedMaterial=wood;
        }
    }

    static void BuildHeadlandReefs(Material slate,Material warm,Material gray)
    {
        float CoastAt(float z)
        {
            float x=180;
            while(x<265 && Ground(x,z)>0)x+=1;
            return x;
        }
        float shore=CoastAt(146);
        BuildShallows("Lighthouse Headland Shallows",new Vector2(shore-19,106),73,85,
            new Vector2(shore+7,146),new Vector2(34,39));
        Reef(3,142,5,7.25f,6f,29,slate);
        Reef(1,147,10,5.2f,3.5f,76,gray);
        Reef(2,150,14,4.7f,2.6f,41,warm);
        Reef(1,161,7,6.7f,4.9f,92,slate);
        Reef(2,163,12,4.4f,3f,32,gray);
        Reef(3,126,6,5.6f,3.8f,63,warm);

        void Reef(int variant,float z,float offset,float width,float height,float yaw,Material material)
        {
            float x=CoastAt(z)+offset;
            float sink=Ground(x,z)+height*.38f;
            MountainRock(variant,x,z,width,height,yaw,sink,material);
        }
    }
}
