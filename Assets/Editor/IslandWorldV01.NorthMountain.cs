using UnityEditor;
using UnityEngine;

public static partial class IslandWorldV01
{
    static readonly Vector2 MountainView=new(61,72);
    internal static readonly Vector2[] MountainViewWalk={new(70,69),new(66,71),MountainView,new(58,71)};

    static float DistanceToTrailSegment(Vector2 point, Vector2 start, Vector2 end)
    {
        Vector2 segment=end-start, offset=point-start;
        float t=Mathf.Clamp01(Vector2.Dot(offset,segment)/segment.sqrMagnitude);
        return (offset-segment*t).magnitude;
    }

    static float MountainTrailDistance(Vector2 point)
    {
        float distance=float.MaxValue;
        var route=Tracks[0];
        for(int i=20;i<=25;i++)
            distance=Mathf.Min(distance,DistanceToTrailSegment(point,route[i-1],route[i]));
        return distance;
    }

    static void ShapeNorthMountain(TerrainData data)
    {
        int n=data.heightmapResolution;
        var heights=data.GetHeights(0,0,n,n);
        var origin=terrain.transform.position;
        float entrance=Ground(MountainViewWalk[0].x,MountainViewWalk[0].y), platform=entrance+1.65f;
        Vector2 access=MountainViewWalk[0], delta=MountainView-access;
        for(int row=0;row<n;row++)
        {
            float z=origin.z+row*data.size.z/(n-1);
            if(z<45 || z>158)continue;
            for(int col=0;col<n;col++)
            {
                float x=origin.x+col*data.size.x/(n-1);
                if(x< -102 || x>149)continue;
                var point=new Vector2(x,z);
                float height=origin.y+heights[row,col]*data.size.y;
                if(height<16)continue;
                float clear=1-Feather(MountainTrailDistance(point),4.25f,10f);
                float foot=Feather(Vector2.Distance(point,new Vector2(52,88)),5f,19f);
                float flank=Feather(Vector2.Distance(point,new Vector2(-23,112)),11f,48f);
                float saddle=Feather(Vector2.Distance(point,new Vector2(66,95)),4f,13f);
                height+=(foot*4.2f+flank*6f-saddle*2.45f)*clear;
                float view=Feather(Vector2.Distance(point,MountainView),6.4f,14.5f);
                float spur=Feather(DistanceToTrailSegment(point,access,MountainView),2.6f,8f);
                float t=Mathf.Clamp01(Vector2.Dot(point-access,delta)/delta.sqrMagnitude);
                height=Mathf.Lerp(height,Mathf.Lerp(entrance,platform,t),spur*clear);
                height=Mathf.Lerp(height,platform,view*clear);
                heights[row,col]=(height-origin.y)/data.size.y;
            }
        }
        data.SetHeights(0,0,heights);
    }

    static void PaintNorthMountain(TerrainData data)
    {
        int n=data.alphamapResolution;
        var map=data.GetAlphamaps(0,0,n,n);
        var origin=terrain.transform.position;
        for(int row=0;row<n;row++)
        {
            float z=origin.z+row*data.size.z/(n-1);
            if(z<46 || z>173)continue;
            for(int col=0;col<n;col++)
            {
                float x=origin.x+col*data.size.x/(n-1);
                if(x< -102 || x>158)continue;
                var point=new Vector2(x,z);
                float ridge=Feather(new Vector2((x-14)*.62f,(z-109)*1.15f).magnitude,24f,69f);
                float trail=Feather(MountainTrailDistance(point),2.45f,5.2f)
                    *Mathf.SmoothStep(0,1,Mathf.InverseLerp(53,86,z))*Feather(x,135,153);
                float view=Feather(Vector2.Distance(point,MountainView),5.4f,10.3f);
                float spur=Feather(DistanceToTrailSegment(point,MountainViewWalk[0],MountainView),1.45f,4.0f);
                float flecks=.83f+.17f*Mathf.Sin(x*.55f+Mathf.Sin(z*.38f))*Mathf.Sin(z*.57f);
                float rock=Mathf.Max(ridge*.75f,view*.92f)*flecks;
                float worn=Mathf.Max(trail,spur*.89f);
                for(int layer=0;layer<map.GetLength(2);layer++)
                {
                    float scree=layer==5?.72f:layer==2?.20f:layer==4?.08f:0;
                    float path=layer==0?.28f:layer==3?.55f:layer==5?.17f:0;
                    float value=Mathf.Lerp(map[row,col,layer],scree,rock);
                    map[row,col,layer]=Mathf.Lerp(value,path,worn*.93f);
                }
            }
        }
        data.SetAlphamaps(0,0,map);
    }

    static void DressNorthMountain()
    {
        var slate=UpdateMountainStone("North Mountain Blue Slate",new Color(.53f,.55f,.54f));
        var warm=UpdateMountainStone("North Mountain Warm Stone",new Color(.57f,.55f,.50f));
        var gray=UpdateMountainStone("North Mountain Gray Stone",new Color(.55f,.56f,.53f));

        // The original eight silhouettes anchor the ridge.
        MountainRock(2,-32,122,89,63,6,31,gray);
        MountainRock(3,9,129,83,76,43,49,slate);
        MountainRock(2,47,145,69,51,105,35,warm);
        MountainRock(1,-60,135,71,54,154,35,slate);
        MountainRock(1,4,165,74,66,-40,43,gray);
        MountainRock(1,-29,94,49,43,-18,24,warm);
        MountainRock(2,36,98,41,33,15,24,gray);
        MountainRock(1,-74,110,45,29,95,23,warm);

        // Smaller bedrock pieces connect the faces to the slope and frame the saddle.
        MountainRock(3,46,120,29,23,83,11.3f,gray);
        MountainRock(3,53,105,27,17,53,9.2f,slate);
        MountainRock(1,53,91,21,14,32,7.4f,warm);
        MountainRock(2,50,79,14.4f,8.8f,47,3.0f,gray);
        MountainRock(2,26,92,24,14.7f,122,8.1f,slate);
        MountainRock(3,-7,92,17,9.7f,70,4f,warm);
        MountainRock(2,-50,104,22,12.8f,77,5.9f,gray);
        MountainRock(1,82,104,13,10.3f,14,2.7f,gray);
        MountainRock(2,86,109,15.1f,9.2f,59,3.5f,warm);
        MountainRock(3,60,84,6.9f,4.1f,81,1.75f,warm);
        MountainRock(2,82,98,5,3.25f,39,1.10f,slate);
        MountainRock(2,92,76,7.6f,3.9f,46,1.75f,warm);
        MountainRock(3,113,75,4.7f,2.3f,139,.95f,gray);

        // Keep the overlook top free, with the rocks partly buried along its far edge.
        MountainRock(1,55,75,8.5f,3.4f,68,2.70f,warm);
        MountainRock(3,53,70,7.5f,3.6f,43,2.45f,slate);
        MountainRock(2,56,66,5.2f,2.7f,116,1.31f,gray);
        Grove("Pine_3",new(-75,87,19,41),new(-62,106,22,57),new(-38,75,15,23),
            new(65,115,13,57),new(73,111,13,15),new(107,101,13,30),new(74,142,15,45),new(-78,150,21,72));
        Grove("Grass_Wispy_Tall",new(56,76,2.45f,62),new(61,84,1.8f,34),new(82,104,1.95f,31),
            new(99,77,2.15f,123),new(125,104,1.9f,117),new(135,98,2.05f,28));
        Grove("Fern_1",new(54,82,2.1f,81),new(57,91,2.35f,119),new(86,103,1.8f,45),new(133,104,1.8f,93));
        Grove("Bush_Common_Flowers",new(53,81,3.25f,114),new(93,76,2.9f,33),new(133,101,2.5f,130));
        Grove("Flower_3_Group",new(59,82,2,72),new(84,99,1.8f,35),new(98,79,1.9f,104),new(132,98,1.45f,42));
        Grove("Flower_4_Group",new(56,81,2.2f,100),new(80,100,1.9f,74),new(101,79,1.8f,35),new(131,104,1.55f,53));
    }

    static Material UpdateMountainStone(string name, Color color)
    {
        var material = BeachMaterial(name, color, "Island/North Mountain Stone");
        material.color = color;
        EditorUtility.SetDirty(material);
        return material;
    }

    static void MountainRock(int variant,float x,float z,float width,float height,float yaw,float sink,Material material)
    {
        var rock=Place(Nature+"Rock_Medium_"+variant+".fbx",x,z,width,height,yaw,sink);
        foreach(var renderer in rock.GetComponentsInChildren<MeshRenderer>())
        {
            renderer.sharedMaterial=material;
            if(!renderer.TryGetComponent<Collider>(out _))
                renderer.gameObject.AddComponent<MeshCollider>();
        }
    }
}
