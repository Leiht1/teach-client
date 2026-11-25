Shader "Island/Meadow River"
{
    Properties { _Color ("Original river color", Color) = (.12,.57,.67,1) }
    SubShader
    {
        Tags { "Queue"="Transparent-5" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            fixed4 _Color;
            struct App { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct V { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; float3 world:TEXCOORD1; };
            V vert(App v) { V o; o.pos=UnityObjectToClipPos(v.vertex); o.uv=v.uv; o.world=mul(unity_ObjectToWorld,v.vertex).xzy; return o; }
            fixed4 frag(V i):SV_Target
            {
                float phase=i.world.y*.93+_Time.y*1.8;
                float wave=sin(phase+sin(i.world.x*1.4))*.55+sin(i.world.x*1.8-phase*.54)*.45;
                float glint=pow(saturate(wave),7)*.28;
                float edge=smoothstep(.24,1,abs(i.uv.x*2-1));
                float3 color=lerp(float3(.035,.41,.54),float3(.26,.67,.58),edge);
                float pebbles=smoothstep(.69,.93,sin(i.world.x*6+sin(i.world.y*3))*sin(i.world.y*5));
                color-=pebbles*edge*.11;
                color+=glint+sin(phase*.3)*.028;
                float foam=smoothstep(.955,.995,abs(i.uv.x*2-1))*(.14+glint);
                color=lerp(color,float3(.83,.95,.89),foam);
                float estuary=1-smoothstep(-193,-142,i.world.y);
                float openWater=saturate(smoothstep(-260,-238,i.world.y)*.7+smoothstep(.07,1.7,i.world.z)*.3);
                float3 tidal=lerp(float3(.06,.58,.65),float3(.49,.74,.60),edge);
                tidal+=glint*.65;
                color=lerp(color,tidal,estuary);
                float bankFade=1-smoothstep(.80,1,abs(i.uv.x*2-1));
                float alpha=lerp(1,(.9-.57*edge)*openWater*bankFade,estuary);
                return fixed4(lerp(_Color.rgb,color,i.uv.y),alpha);
            }
            ENDCG
        }
    }
}
