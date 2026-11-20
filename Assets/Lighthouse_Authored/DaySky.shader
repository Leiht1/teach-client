Shader "Coast/DaySky" {SubShader{Tags{"Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox"}Cull Off ZWrite Off Pass{CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
struct a{float4 vertex:POSITION;};struct v{float4 pos:SV_POSITION;float3 dir:TEXCOORD0;};v vert(a x){v o;o.pos=UnityObjectToClipPos(x.vertex);o.dir=x.vertex.xyz;return o;}
float cloud(float2 p){float d=min(length(p/float2(1.1,.30))-1,min(length((p-float2(-.43,.16))/float2(.44,.40))-1,min(length((p-float2(.05,.30))/float2(.48,.48))-1,length((p-float2(.53,.10))/float2(.42,.33))-1)));return 1-smoothstep(-.025,.035,d);}
fixed4 frag(v i):SV_Target{float3 d=normalize(i.dir);float3 color=lerp(float3(.60,.83,.92),float3(.07,.40,.78),saturate(d.y*3.5));float ang=atan2(d.x,d.z);float c=max(cloud(float2((ang+.48)*3,(d.y-.08)*10)),max(cloud(float2((ang-.55)*3.7,(d.y-.18)*10)),cloud(float2((ang-1.6)*3,(d.y-.10)*10))));color=lerp(color,lerp(float3(.83,.89,.94),float3(1,1,.98),saturate(d.y*3)),c*.96);return float4(color,1);}
ENDCG}}}

