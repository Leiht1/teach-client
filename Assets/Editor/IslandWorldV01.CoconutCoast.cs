using UnityEditor;
using UnityEngine;

public static partial class IslandWorldV01
{
    static float CoconutCoastWeight(Vector2 p)
    {
        float radius = new Vector2((p.x + 172) / 55f, (p.y + 142) / 43f).magnitude;
        return 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.48f, 1f, radius));
    }

    static float CoconutWaterline(float z)
    {
        float water = -260f;
        float land = -154f;
        for (int i = 0; i < 16; i++)
        {
            float x = (water + land) * .5f;
            if (Ground(x,z) < .12f) water = x;
            else land = x;
        }
        return land;
    }

    static void BuildCoconutCoast()
    {
        Region("02 · 椰林海岸");
        // Open cove first, paired crowns at the forest approach.
        CoconutPalm(-168,-165,13,35,false);
        CoconutPalm(-184,-150,16,111,true);
        CoconutPalm(-174,-139,11,263,false);
        CoconutPalm(-130,-150,14,-22,true);
        CoconutPalm(-162,-122,18,162,true);
        CoconutPalm(-151,-116,12,50,false);
        CoconutPalm(-119,-120,16,84,false);
        CoconutPalm(-132,-103,17,250,true);
        CoconutPalm(-149,-102,12,35,false);
        Grove("CommonTree_2",new(-157,-96,23,75),new(-142,-90,25,33),new(-111,-106,19,121));

        Rock(0,-182,-142,8.2f,3.7f,56,.9f);
        Rock(1,-178,-145,3.6f,1.8f,19,.43f);
        Rock(2,-155,-123,4.7f,2.4f,23,.55f);
        Rock(3,-128,-138,5.9f,2.8f,74,.55f);
        Grove("Plant_1_Big",new(-167,-163,3.3f,28),new(-129,-153,3.5f,35),new(-157,-122,3.15f,91),new(-116,-121,3.8f,232));
        Grove("Plant_7",new(-166,-161,2.2f,33),new(-177,-140,2.4f,66),new(-128,-149,2.6f,92),new(-146,-116,2.2f,96));
        Grove("Fern_1",new(-162,-153,2.5f,45),new(-131,-146,2.4f,60),new(-154,-125,2.5f,112),
            new(-143,-108,2.8f,192),new(-124,-106,2.2f,12),new(-116,-121,2.5f,65));
        Grove("Bush_Common_Flowers",new(-163,-148,4.2f,15),new(-155,-129,3.5f,75),new(-122,-137,4.8f,117),new(-140,-109,3.5f,170));
        Grove("Flower_3_Group",new(-134,-155,2.8f,61),new(-157,-143,2.5f,112),new(-128,-135,2.4f,15),new(-146,-112,2.9f,127));
        Grove("Flower_4_Group",new(-132,-153,2.5f,24),new(-159,-142,2.4f,65),new(-127,-133,2.2f,251),new(-143,-110,2.1f,81));

        BeachReef(CoconutWaterline(-162)-.8f,-162,4.1f,1.35f,42);
        BeachReef(CoconutWaterline(-158)-2.5f,-158,2.5f,1.05f,64);
        BeachReef(CoconutWaterline(-137)-.6f,-137,3.8f,1.6f,35);
        BeachReef(CoconutWaterline(-133)+.5f,-133,2.9f,1.1f,119);
        Rock(1,CoconutWaterline(-148)+4,-148,2.5f,1.2f,57,.16f);
        float bank=CoconutWaterline(-143);
        Grove("Plant_1",new(bank+9,-142,2.05f,63),new(bank+11,-139,2.35f,197));
        Grove("Fern_1",new(bank+10,-143,1.9f,42),new(bank+12,-139,1.7f,130));
        Grove("Flower_3_Group",new Vector4(bank+11,-142,1.65f,15));
        Grove("Flower_4_Group",new Vector4(bank+12,-141,1.35f,70));
        BeachDetail(CoconutWaterline(-151)+3,-151,false,131);
        BeachDetail(CoconutWaterline(-147)+2,-147,true,93);
        var drift = BeachMaterial("Cove Driftwood",new Color(.57f,.36f,.20f));
        Vector3 Sand(float x,float z,float lift) => new Vector3(x,Ground(x,z)+lift,z);
        float driftX=CoconutWaterline(-150)+4.8f;
        var joint=Sand(driftX,-150,.22f);
        BeachBeam("潮线漂木", Sand(driftX-1.4f,-151,.12f), joint,.17f, drift);
        BeachBeam("潮线漂木", joint,Sand(driftX+1.2f,-150.6f,.15f),.12f,drift);
        BeachBeam("折枝", joint,Sand(driftX+.45f,-149.1f,.29f),.068f,drift);
        BuildShallows("Coconut Cove Shallows", new Vector2(-251,-183),111,81,
            new Vector2(CoconutWaterline(-145)-8,-143),new Vector2(43,40));
    }

    static void CoconutPalm(float x,float z,float height,float yaw,bool right)
    {
        string palm=right?"Coastal_palm_right.prefab":"Coastal_palm_left.prefab";
        var tree=Place(Coast+palm,x,z,height*.72f,height,yaw,.16f);
        if (!right) return;
        var youngLeaf=BeachMaterial("Cove Lime Fronds",new Color(.49f,.64f,.20f));
        foreach(var renderer in tree.GetComponentsInChildren<MeshRenderer>())
        {
            string name=renderer.gameObject.name;
            if (name.EndsWith("frond 0") || name.EndsWith("frond 3") || name.EndsWith("frond 6"))
                renderer.sharedMaterial=youngLeaf;
        }
    }
}

