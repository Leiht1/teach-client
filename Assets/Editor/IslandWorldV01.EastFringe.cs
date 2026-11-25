using UnityEngine;

public static partial class IslandWorldV01
{
    // From the meadow entrance, the three groups alternate which side frames the path.
    static readonly Vector2 FlowerSlope=new(91,-37);
    static readonly Vector2 StoneShelf=new(131,-19);
    static readonly Vector2 UpperSpur=new(88,61);

    static float FringeTrailDistance(Vector2 point)
    {
        var route=Tracks[0];
        float closest=float.MaxValue;
        // Main-route segments from the meadow junction to the north shoulder.
        for(int i=13;i<=21;i++)
        {
            Vector2 a=route[i-1], segment=route[i]-a;
            Vector2 offset=point-a;
            float t=Mathf.Clamp01(Vector2.Dot(offset,segment)/segment.sqrMagnitude);
            closest=Mathf.Min(closest,(offset-segment*t).magnitude);
        }
        return closest;
    }

    static void ShapeEastFringe(TerrainData data)
    {
        var origin=terrain.transform.position;
        int n=data.heightmapResolution;
        var heights=data.GetHeights(0,0,n,n);
        float shelf=Ground(StoneShelf.x,StoneShelf.y)+1.8f;
        for(int row=0;row<n;row++)
        {
            float z=origin.z+row*data.size.z/(n-1);
            if(z< -73 || z>80)continue;
            for(int col=0;col<n;col++)
            {
                float x=origin.x+col*data.size.x/(n-1);
                if(x<55 || x>156)continue;
                var p=new Vector2(x,z);
                float clear=1-Feather(FringeTrailDistance(p),5f,12f);
                float flower=Feather(Vector2.Distance(p,FlowerSlope),2f,22f);
                float stone=Feather(Vector2.Distance(p,StoneShelf),6f,21f);
                float spur=Feather(Vector2.Distance(p,UpperSpur),3f,17f);
                float height=origin.y+heights[row,col]*data.size.y;
                if(height<8f)continue;
                height+=clear*(flower*2.1f+spur*2.35f);
                height=Mathf.Lerp(height,shelf,clear*stone*.72f);
                heights[row,col]=(height-origin.y)/data.size.y;
            }
        }
        data.SetHeights(0,0,heights);
    }

    static void PaintEastFringe(TerrainData data)
    {
        var origin=terrain.transform.position;
        int n=data.alphamapResolution;
        var map=data.GetAlphamaps(0,0,n,n);
        for(int row=0;row<n;row++)
        {
            float z=origin.z+row*data.size.z/(n-1);
            if(z< -77 || z>76)continue;
            for(int col=0;col<n;col++)
            {
                float x=origin.x+col*data.size.x/(n-1);
                if(x<56 || x>155)continue;
                var p=new Vector2(x,z);
                float edge=Feather(Mathf.Abs(z-1),44f,77f)*Feather(x,127f,155f)*Mathf.SmoothStep(0,1,Mathf.InverseLerp(56,77,x));
                float trail=Feather(FringeTrailDistance(p),2.35f,5.8f)*edge;
                float blooms=Feather(Vector2.Distance(p,FlowerSlope),7f,24f)*.65f*(1-trail);
                float scree=Mathf.Max(Feather(Vector2.Distance(p,StoneShelf),5f,19f),
                    Feather(Vector2.Distance(p,UpperSpur),4f,14f))*(1-trail)*.72f;
                for(int layer=0;layer<map.GetLength(2);layer++)
                {
                    float grass=layer==2?.88f:layer==0?.12f:0;
                    float worn=layer==3?.55f:layer==0?.38f:layer==5?.07f:0;
                    float gravel=layer==5?.53f:layer==3?.18f:layer==2?.29f:0;
                    float value=Mathf.Lerp(map[row,col,layer],grass,blooms);
                    value=Mathf.Lerp(value,gravel,scree);
                    map[row,col,layer]=Mathf.Lerp(value,worn,trail*.93f);
                }
            }
        }
        data.SetAlphamaps(0,0,map);
    }

    static void DressEastFringe()
    {
        var warm=BeachMaterial("East Fringe Granite Warm",new Color(.58f,.55f,.48f),"Island/East Fringe Stone");
        var cool=BeachMaterial("East Fringe Granite Cool",new Color(.48f,.53f,.58f),"Island/East Fringe Stone");

        // Open meadow shoulder.
        Grove("CommonTree_2",new(77,-73,15,32),new(81,-25,16,119));
        ForestTreeTint("CommonTree_3",98,-48,14,66,"East Fringe Honey Crown",new Color(1f,.87f,.49f));
        FringeRock(1,92,-46,5.8f,2.4f,45,.64f,warm);
        FringeRock(2,89,-44,3.1f,1.6f,97,.40f,cool);
        FringeRock(3,97,-42,2.8f,1.2f,18,.36f,warm);
        Grove("Bush_Common_Flowers",new(89,-45,3.3f,43),new(97,-37,3.1f,63),new(93,-23,3.2f,113));
        Grove("Flower_3_Group",new(86,-46,2.3f,75),new(90,-41,2.5f,12),new(97,-45,1.8f,90),
            new(96,-32,2.4f,80),new(95,-22,2.3f,25),new(78,-70,2.15f,92));
        Grove("Flower_4_Group",new(87,-43,2.2f,71),new(93,-40,2.4f,18),new(97,-35,2,104),
            new(104,-20,2.25f,71),new(96,-25,2.6f,45));

        // Low stone terrace at the outside of the uphill curve. Leave the top and the inside bend open.
        Grove("CommonTree_4",new(140,-13,16,78),new(146,-31,14,103),new(97,14,18,31));
        FringeRock(2,127,-21,12f,5.7f,27,1.75f,warm);
        FringeRock(1,133,-24,10.4f,4.25f,109,1.5f,cool);
        FringeRock(3,136,-17,8.5f,2.9f,59,1.08f,warm);
        FringeRock(1,124,-15,5.7f,2.35f,78,.53f,cool);
        FringeRock(3,123,-26,4.1f,1.6f,35,.42f,warm);
        FringeRock(2,128,-13,3.35f,1.38f,61,.33f,warm);
        FringeRock(3,132,-10,2.2f,1.03f,89,.29f,cool);
        Grove("Fern_1",new(122,-21,2.5f,65),new(126,-16,2.7f,84),new(133,-14,2.2f,37),new(141,-19,3.1f,119));
        Grove("Bush_Common_Flowers",new(123,-20,3.55f,21),new(132,-15,3.2f,62),new(136,-26,4.1f,72));
        Grove("Flower_3_Group",new(121,-17,2.7f,38),new(125,-28,2.3f,91),new(130,-14,2.3f,104));
        Grove("Flower_4_Group",new(122,-18,2.4f,39),new(128,-12,1.85f,109),new(134,-28,2.8f,123));

        // Rock strata become more pronounced here, with one silhouette tree by the upper shoulder.
        Grove("CommonTree_3",new(104,45,17,54),new(125,33,15,135));
        FringeRock(3,91,55,10.9f,4.2f,76,1.71f,cool);
        FringeRock(1,91,63,12f,5.1f,124,1.65f,warm);
        FringeRock(2,87,60,7.6f,3.05f,54,.85f,cool);
        FringeRock(3,85,66,5.35f,2.2f,25,.58f,warm);
        FringeRock(1,84,57,3.4f,1.31f,103,.41f,cool);
        FringeRock(2,69,48,6.9f,2.48f,44,.78f,warm);
        FringeRock(3,67,53,3.8f,1.65f,106,.52f,cool);
        Grove("Grass_Wispy_Tall",new(87,54,2.12f,75),new(89,67,2.15f,69),new(68,52,2.45f,58),new(123,-19,2.1f,46));
        Grove("Grass_Common_Short",new(83,57,2.2f,51),new(88,52,2.3f,105),new(67,50,2.6f,83),
            new(124,-12,2.3f,43),new(88,-40,2.25f,117),new(95,-36,2.6f,41));
        Grove("Fern_1",new(89,57,2.6f,110),new(86,67,2.2f,27),new(70,48,2.0f,38));
        Grove("Flower_3_Group",new(84,56,2.4f,39),new(68,50,1.95f,91),new(91,68,2,76));
        Grove("Flower_3_Group",new(103,-13,1.8f,30),new(104,-9,1.6f,54),new(105,-7,2.1f,96),new(100,2,1.9f,78));
        Grove("Flower_4_Group",new(89,51,2.1f,91),new(84,63,1.9f,31),new(66,50,1.8f,28));
    }

    static void FringeRock(int variant,float x,float z,float width,float height,float yaw,float sink,Material material)
    {
        var rock=Place(Nature+"Rock_Medium_"+variant+".fbx",x,z,width,height,yaw,sink);
        foreach(var renderer in rock.GetComponentsInChildren<MeshRenderer>())
        {
            renderer.sharedMaterial=material;
            if(width>=7f && !renderer.TryGetComponent<Collider>(out _))
                renderer.gameObject.AddComponent<MeshCollider>();
        }
    }
}
