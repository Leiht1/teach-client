using UnityEngine;

public static partial class IslandWorldV01
{
    static float MeadowRiverWeight(float z) => Feather(Mathf.Abs(z+58),39f,84f);
    static float MeadowRiverHalfWidth(float z) =>
        4f+MeadowRiverWeight(z)*(1.1f+.75f*Mathf.Sin((z+76)*.095f));

    static void PaintCentralMeadow(float[,,] map, Vector3 origin, Vector3 size)
    {
        int n=map.GetLength(0);
        var lawns=new[] {new Vector2(42,-34),new Vector2(51,-91),new Vector2(-26,-76)};
        for(int z=0;z<n;z++)
        for(int x=0;x<n;x++)
        {
            var p=new Vector2(origin.x+x*size.x/(n-1),origin.z+z*size.z/(n-1));
            if(p.x< -47 || p.x>103 || p.y< -125 || p.y>28) continue;
            float grass=0;
            foreach(var lawn in lawns)
                grass=Mathf.Max(grass,Feather(Vector2.Distance(p,lawn),9f,23f)*.38f);
            grass*=1-Mathf.SmoothStep(.13f,.55f,map[z,x,3]);
            float bank=Feather(Mathf.Abs(p.x-RiverX(p.y)),3.4f,9f)*MeadowRiverWeight(p.y)*.65f;
            float warmPath=Feather(Vector2.Distance(p,Junction),19f,52f)*
                Mathf.SmoothStep(0,1,Mathf.InverseLerp(.35f,.81f,map[z,x,3]))*.36f;
            for(int layer=0;layer<map.GetLength(2);layer++)
            {
                float lawn=layer==2?.89f:layer==0?.11f:0;
                float shore=layer==1?.64f:layer==4?.36f:0;
                float dirt=layer==3?.69f:layer==0?.31f:0;
                float value=Mathf.Lerp(map[z,x,layer],lawn,grass);
                value=Mathf.Lerp(value,shore,bank);
                map[z,x,layer]=Mathf.Lerp(value,dirt,warmPath);
            }
        }
    }

    static void DressCentralMeadow()
    {
        ForestTreeTint("CommonTree_2",35,-25,16,105,"Meadow Sunlit Crown",new Color(1f,.85f,.56f));
        Grove("CommonTree_3",new(-26,-76,15,16),new(64,-30,16,119),new(81,-80,16,82));
        Rock(1,57,-100,9.5f,3.4f,62,.8f);
        Rock(0,62,-103,4.7f,2.4f,31,.55f);
        Rock(2,78,-29,8f,3.8f,105,.85f);
        Rock(0,17,-30,4.5f,2.1f,33,.46f);
        BankRock(-1,-72,4.4f,2.0f);
        BankRock(1,-69,2.6f,1.18f);
        BankRock(-1,-47,3.2f,1.33f);
        BankRock(1,-44,3.8f,1.62f);
        BankRock(-1,-22,3.6f,1.28f);
        BankRock(1,-91,4.0f,1.52f);
        BankRock(-1,5,7.4f,3.7f);
        BankRock(1,17,8.1f,4.7f);
        BankRock(-1,30,9.1f,5.2f);
        Rock(2,RiverX(42)+3,43,11,7.2f,61,2.1f);
        Rock(0,RiverX(38)-8,38,9,6.3f,24,1.3f);
        Grove("Grass_Wispy_Tall",new(RiverX(-74)-6,-74,2,37),new(RiverX(-70)+6,-70,2.4f,76),
            new(RiverX(-47)-6,-47,2.3f,110),new(RiverX(-43)+7,-43,1.8f,42),
            new(RiverX(-32)-7,-32,2.15f,78),new(RiverX(-92)+6,-92,2.8f,122));
        Grove("Fern_1",new(-21,-64,2.15f,51),new(12,-48,2.4f,109),new(38,-34,2.8f,73),new(75,-44,2.9f,28));
        Grove("Bush_Common_Flowers",new(-23,-70,3.8f,30),new(12,-39,3.6f,53),new(37,-28,4.1f,54),
            new(61,-37,3.3f,80),new(55,-98,3.4f,61),new(81,-73,3.6f,142));
        Grove("Flower_3_Group",new(-18,-63,2.1f,39),new(-23,-47,1.9f,52),new(8,-49,2.22f,91),
            new(34,-34,2.5f,18),new(58,-46,2.3f,93),new(70,-75,2.1f,101),new(41,-91,2,48));
        Grove("Flower_4_Group",new(-20,-61,2.22f,52),new(11,-50,1.98f,31),new(33,-32,2.4f,101),
            new(57,-44,2.4f,24),new(67,-78,2.1f,64),new(41,-89,2.3f,72));
        MeadowTrailPost();
        Grove("Grass_Common_Short",new(30,-37,2.95f,30),new(59,-43,2.6f,80),new(40,-89,2.15f,46),
            new(-24,-68,2.85f,32),new(11,-49,2.7f,42),new(74,-43,2.6f,151));
    }

    static void MeadowTrailPost()
    {
        Vector3 foot=new(25,Ground(25,-56),-56);
        Material trim=BeachMaterial("Bridge Worn Boards",new Color(.47f,.28f,.15f));
        BeachBeam("路口木桩",foot-Vector3.up*.22f,foot+Vector3.up*2.9f,.14f,trim);
        Sign("往西 · 森林与木桥",2.64f,-1);
        Sign("往东 · 林缘与北部山路",2.24f,1);
        Sign("往南 · 河口",1.84f,-1,34);
        void Sign(string name,float height,int direction,float yaw=0)
        {
            var sign=GameObject.CreatePrimitive(PrimitiveType.Cube);
            sign.name=name;
            sign.transform.SetParent(group);
            var turn=Quaternion.Euler(0,yaw,0);
            sign.transform.SetPositionAndRotation(foot+Vector3.up*height+turn*new Vector3(direction*.52f,0,0),turn);
            sign.transform.localScale=new Vector3(1.46f,.30f,.14f);
            sign.GetComponent<MeshRenderer>().sharedMaterial=trim;
            Object.DestroyImmediate(sign.GetComponent<Collider>());
        }
    }

    static void BankRock(int side,float z,float width,float height)
    {
        float edge=MeadowRiverHalfWidth(z);
        float x=RiverX(z)+side*(edge+.32f);
        Rock(side>0?1:0,x,z,width,height,z*.8f,.32f);
        Rock(2,x+side*1.25f,z+1.22f,width*.44f,height*.5f,z*1.4f,.15f);
    }
}

