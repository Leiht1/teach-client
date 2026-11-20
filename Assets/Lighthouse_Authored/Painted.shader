Shader "Coast/Painted" {
Properties {_Atlas("Painted atlas",2D)="white"{} _Color("Tint",Color)=(1,1,1,1) _Kind("Quadrant",Float)=0 _Scale("World tile size",Float)=5}
SubShader {Tags{"RenderType"="Opaque"} CGPROGRAM
#pragma surface surf Standard fullforwardshadows
#pragma target 3.0
sampler2D _Atlas;fixed4 _Color;float _Kind,_Scale;
struct Input {float3 worldPos;float3 worldNormal;float4 color:COLOR;};
float3 paint(float2 p,float2 quadrant,float scale){float2 u=frac(p/scale)*.46+.02+quadrant;return tex2Dgrad(_Atlas,u,ddx(p/scale)*.46,ddy(p/scale)*.46).rgb;}
float3 sandAt(float2 p){return lerp(paint(p,float2(0,.5),9),float3(.78,.61,.36),.38)*float3(.92,.94,.91);}
float3 grassAt(float2 p){return lerp(paint(p,float2(.5,.5),4.5),float3(.28,.47,.085),.48)*float3(.80,.91,.72);}
void surf(Input IN,inout SurfaceOutputStandard o){float3 p=IN.worldPos;float3 col;float macro=sin(p.x*.24+sin(p.z*.19))*sin(p.z*.28)*.05;
if(_Kind<.5)col=sandAt(p.xz);
else if(_Kind<1.5)col=lerp(sandAt(p.xz),grassAt(p.xz),smoothstep(.7,1.8,p.y+macro*4));
else if(_Kind<2.5){float3 n=pow(abs(IN.worldNormal),6);n/=max(.001,n.x+n.y+n.z);col=paint(p.zy,float2(0,0),_Scale)*n.x+paint(p.xz,float2(0,0),_Scale)*n.y+paint(p.xy,float2(0,0),_Scale)*n.z;col=lerp(col,float3(.53,.52,.56),.22)*_Color.rgb;}
else if(_Kind<3.5)col=paint(p.xz,float2(.5,0),4)*_Color.rgb;
else col=lerp(sandAt(p.xz),grassAt(p.xz),smoothstep(0,1,IN.color.r));
o.Albedo=col*(1+macro);o.Smoothness=.04;o.Metallic=0;o.Alpha=1;}
ENDCG }Fallback "Diffuse"}
