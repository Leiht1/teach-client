Shader "Island/East Fringe Stone"
{
    Properties
    {
        _Color ("Stone", Color) = (.55,.53,.49,1)
        _Moss ("Moss", Color) = (.37,.49,.16,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        CGPROGRAM
        #pragma surface surf Lambert addshadow
        fixed4 _Color, _Moss;
        struct Input { float3 worldPos; float3 worldNormal; };
        void surf(Input i, inout SurfaceOutput o)
        {
            float3 p=i.worldPos;
            float breakup=sin(p.x*1.7+p.z*.91)*sin(p.z*2.31+p.y);
            float grain=sin(p.x*6.2+p.z*4.3+breakup)*sin(p.y*6.6-p.z*3.6);
            float moss=smoothstep(.64,.96,i.worldNormal.y+breakup*.17+grain*.045);
            float strata=sin(p.y*3.1+p.x*.49+p.z*.69)*.025;
            o.Albedo=lerp(_Color.rgb+strata+grain*.007,_Moss.rgb+breakup*.04,moss);
            o.Alpha=1;
        }
        ENDCG
    }
    Fallback "Diffuse"
}
