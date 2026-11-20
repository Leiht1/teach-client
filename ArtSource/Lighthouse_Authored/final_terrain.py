from pathlib import Path
p=Path('ArtSource/Lighthouse_Authored/build_dcc.py');s=p.read_text(encoding='utf-8-sig');s=s.replace("('Split middle face',(-3,3.3,3.6)","('Split middle face',(-5,9,3.6)")
s=s.replace("[(-12,19),(-6,15),(1,15),(8,17),(13,22),(12,29),(5,32),(-5,30),(-12,26)]","[(-12,19),(-6,15),(1,15),(4,17),(4,22),(2,24),(5,27),(9,29),(5,32),(-5,30),(-12,26)]")
s=s.replace('(7,22,11.3),(3,25,12.45),(-1,25,12.49)','(7,22,11.3),(4.8,24,12.8),(1,22.5,13.05),(-2,22,13.05)')
# Draw a softer sandy path edge using vertex paint across the ribbon.
s=s.replace("ribbon('Sandy mountain footpath',road,1.4,sand,.025)","""pathmat=sand.copy();pathmat.name='Mountain footpath blend'
pv=[];pf=[];weights=[]
for i,p in enumerate(road):
 d=road[min(i+1,len(road)-1)]-road[max(0,i-1)];side=Vector((-d.y,d.x,0)).normalized()
 for j,k in enumerate([-1,-.82,-.63,0,.63,.82,1]):pv.append(tuple(p+side*(1.65*k)+Vector((0,0,.025))));weights.append([1,.4,0,0,0,.4,1][j] if p.z>1 else 0)
 if i:
  for j in range(6):a=(i-1)*7+j;pf.append((a,a+7,a+8,a+1))
pathmesh=mesh('Sandy mountain footpath',pv,pf,pathmat)
colors=pathmesh.data.color_attributes.new(name='Color',type='FLOAT_COLOR',domain='POINT')
for i,w in enumerate(weights):colors.data[i].color=(w,w,w,1)
""")
p.write_text(s,encoding='utf-8')
p=Path('Assets/Editor/CoastAuthored.cs');s=p.read_text(encoding='utf-8-sig');s=s.replace('low.Contains("limestone")){','low.Contains("limestone")||low.Contains("footpath")){');s=s.replace('m.SetFloat("_Kind",low.Contains("limestone")?2:','m.SetFloat("_Kind",low.Contains("footpath")?4:low.Contains("limestone")?2:');p.write_text(s,encoding='utf-8')
p=Path('Assets/Lighthouse_Authored/Painted.shader');s=p.read_text();s=s.replace('float3 worldNormal;','float3 worldNormal;float4 color:COLOR;');s=s.replace('float2 off=_Kind<.5?','float2 off=(_Kind<.5||_Kind>3.5)?');s=s.replace('o.Albedo=col*_Color.rgb','if(_Kind>3.5){float2 gu=frac(IN.worldPos.xz/4.5)*.46+.02;float3 g=tex2Dgrad(_Atlas,gu+float2(.5,.5),ddx(IN.worldPos.xz/4.5)*.46,ddy(IN.worldPos.xz/4.5)*.46).rgb;col=lerp(col,g*float3(.8,.91,.72),saturate(IN.color.r));}o.Albedo=col*_Color.rgb');p.write_text(s)
