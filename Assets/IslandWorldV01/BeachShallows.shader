Shader "Island/Beach Shallows"
{
    SubShader
    {
        Tags { "Queue"="Transparent-10" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct Vertex { float4 position : POSITION; float2 shore : TEXCOORD0; };
            struct Fragment { float4 position : SV_POSITION; float2 shore : TEXCOORD0; float2 world : TEXCOORD1; };
            Fragment vert(Vertex v)
            {
                Fragment o;
                o.position = UnityObjectToClipPos(v.position);
                o.shore = v.shore;
                o.world = mul(unity_ObjectToWorld, v.position).xz;
                return o;
            }
            fixed4 frag(Fragment i) : SV_Target
            {
                float depth = i.shore.x;
                float time = _Time.y;
                float ripple = sin(i.world.x * 1.17 + sin(i.world.y * .8) + time * .9)
                             + sin(i.world.y * 1.43 + sin(i.world.x * .7) - time * .7);
                float glint = pow(saturate(ripple * .32 + .34), 5);
                float tide = .20 + .14 * sin(time * 1.55 + i.world.x * .14) + .086 * sin(i.world.x * .93);
                float breaker = 1 - smoothstep(.06, .18, abs(depth - tide));
                float wash = (1 - smoothstep(.035, .10, abs(depth - tide - .64))) * .28;
                float turquoise = smoothstep(0, 3.7, depth);
                float3 color = lerp(float3(.48,.79,.65), float3(.035,.63,.70), turquoise);
                color = lerp(color + .10 * glint, float3(.94,.99,.90), saturate(breaker * .91 + wash));
                float fade = i.shore.y * (1 - smoothstep(3.5, 10.0, depth));
                return fixed4(color, fade * max(.68 + turquoise * .13, breaker));
            }
            ENDCG
        }
    }
}
