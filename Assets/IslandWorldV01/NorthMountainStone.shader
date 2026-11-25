Shader "Island/North Mountain Stone"
{
    Properties
    {
        _Color ("Stone", Color) = (.56,.55,.53,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        CGPROGRAM
        #pragma surface surf Lambert addshadow
        fixed4 _Color;
        struct Input { float3 worldPos; float3 worldNormal; };
        void surf(Input i, inout SurfaceOutput o)
        {
            float3 p = i.worldPos;
            float bend = sin(p.x*.21 + p.z*.11)*.19 + sin(p.z*.37 - p.y*.18)*.09;
            float beds = frac((p.y + p.x*.14 - p.z*.09)*.13 + bend*.27);
            float seam = (1-smoothstep(.015,.075,min(beds,1-beds)))*.020;
            float vein = (1-smoothstep(.008,.031,abs(beds-.23)))*.011;
            float faces = sin(p.x*.51+p.z*.75 + sin(p.y*.55))*sin(p.y*.32-p.z*.22);
            float ledge = smoothstep(.54,.90,i.worldNormal.y+faces*.16);
            float moss = ledge*smoothstep(.20,.81,faces+.23)*.52;
            float3 stone = _Color.rgb + faces*.023 + vein - seam;
            stone += float3(.023,.021,.015)*ledge;
            o.Albedo = lerp(stone,float3(.37,.42,.27),moss);
            o.Alpha = 1;
        }
        ENDCG
    }
    Fallback "Diffuse"
}
