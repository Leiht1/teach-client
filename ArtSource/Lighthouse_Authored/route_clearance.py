from pathlib import Path
p=Path('ArtSource/Lighthouse_Authored/build_dcc.py');s=p.read_text(encoding='utf-8-sig');edits={"('Foreland right',(14,-1,-.8)":"('Foreland right',(18.5,-.2,-.8)","('Trail switchback toe',(12,-.5,-.2)":"('Trail switchback toe',(16,-.5,-.2)","('Mid center',(-5,8,1.6)":"('Mid center',(-3,10.5,1.6)","('Upper east',(9,22,5.6)":"('Upper east',(13,24.5,5.6)","('Trail upper ledge',(7,16,5.3)":"('Trail upper ledge',(11,16,5.3)","('Trail upper corner',(6,22,7)":"('Trail upper corner',(11,22,7)"}
for a,b in edits.items():s=s.replace(a,b)
s=s.replace("mesh('Grass fringe blades'","mesh('Grass fringe blades'")
# remove ruler-like blade strips; retain the authored ivy groups
start=s.index(' for i in range(9):\n  xx=x+(i-4)');end=s.index(' # draped vines',start);s=s[:start]+s[end:]
p.write_text(s,encoding='utf-8')
p=Path('Assets/Lighthouse_Authored/Painted.shader');s=p.read_text();s=s.replace('o.Albedo=col*_Color.rgb*(1+macro);','if(_Kind>0.5&&_Kind<1.5){float wetline=smoothstep(.7,1.8,IN.worldPos.y+macro*4);float2 su=frac(IN.worldPos.xz/9)*.46+.02;float3 sand=tex2Dgrad(_Atlas,su+float2(0,.5),ddx(IN.worldPos.xz/9)*.46,ddy(IN.worldPos.xz/9)*.46).rgb;col=lerp(sand,col,wetline);}o.Albedo=col*_Color.rgb*(1+macro);');p.write_text(s)
