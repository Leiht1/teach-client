Shader "Island/Weathered Wreck"
{
    Properties { _MainTex ("Palette", 2D) = "white" {} }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        CGPROGRAM
        #pragma surface surf Lambert addshadow
        sampler2D _MainTex;
        struct Input { float2 uv_MainTex; float3 worldPos; };
        void surf(Input IN, inout SurfaceOutput o)
        {
            float3 atlas = tex2D(_MainTex, IN.uv_MainTex).rgb;
            float luminance = dot(atlas, float3(.30,.54,.16));
            float teal = smoothstep(.02,.18,atlas.g-atlas.r) * smoothstep(.01,.13,atlas.b-atlas.r);
            float wood = smoothstep(.006,.07,atlas.r-atlas.b);
            float2 along = float2(dot(IN.worldPos.xz,float2(.51,.86)), IN.worldPos.y);
            float plank = along.y * 1.9 + along.x * .10;
            float seam = smoothstep(.89,.98,frac(plank));
            float grain = sin(along.y * 35 + 1.7*sin(along.x*.86) + .4*sin(along.x*4));
            float3 amber = lerp(float3(.16,.061,.025),float3(.58,.30,.115),saturate(luminance*1.33));
            amber *= 1 + wood * (.09*sin(floor(plank)*2.4) + .027*grain - .19*seam);
            float3 agedTeal = lerp(float3(.09,.20,.16),float3(.28,.42,.29),luminance);
            float3 baseColor = lerp(atlas*.68, amber, wood);
            o.Albedo = lerp(baseColor, agedTeal, teal);
        }
        ENDCG
    }
    Fallback "Diffuse"
}
