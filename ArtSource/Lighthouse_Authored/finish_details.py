from pathlib import Path
p=Path('Assets/Lighthouse_Authored/Painted.shader');s=p.read_text();s=s.replace('tex2D(_Atlas,tile+off)','tex2Dgrad(_Atlas,tile+off,ddx(p/_Scale)*.46,ddy(p/_Scale)*.46)');p.write_text(s)
p=Path('ArtSource/Lighthouse_Authored/build_dcc.py');s=p.read_text(encoding='utf-8-sig');s=s.replace('return mesh(n,v,f,m,bevel)','''o=mesh(n,v,f,m,bevel)
 if m==grass:
  o.data.materials.append(stone)
  for face in o.data.polygons:
   if face.normal.z<.6:face.material_index=1
 return o''')
s=s.replace("ribbon('Natural grass shoulders',road,3.5,grass,-.08);ribbon('Sandy mountain footpath',road,1.6,sand,.025)","""shoulder=ribbon('Natural grass shoulders',road,2.8,grass,-.08)
shoulder.data.materials.append(stone);solid=shoulder.modifiers.new('Rock supported trail bed','SOLIDIFY');solid.thickness=2.6;solid.offset=-1;solid.material_offset=1;solid.material_offset_rim=1
ribbon('Sandy mountain footpath',road,1.4,sand,.025)""")
s=s.replace("('Mid left',(-13,7,1.8)","('Mid left',(-17,8,1.8)")
s=s.replace("[(-16,9),(-11,5),(-4,6),(3,8)","[(-16,12),(-10,12),(-7,10),(-4,8),(3,8)")
# create deliberately grouped border details rather than distribute throughout the whole region
idx=s.index('# Flatten/apply export modifiers.')
s=s[:idx]+'''
# Small sculpted leaves, grass fringe and flowering groups conceal transitions at specific focal corners.
def leafmesh(n,p,s,angle,m):
 verts=[(-.5,0,0),(-.30,-.20,.10),(.15,-.22,.13),(.55,0,0),(.15,.22,.13),(-.30,.20,.10),(0,0,.21)]
 c=math.cos(angle);si=math.sin(angle);v=[(p[0]+(x*c-y*si)*s,p[1]+(x*si+y*c)*s,p[2]+z*s) for x,y,z in verts];return mesh(n,v,[(i,(i+1)%6,6) for i in range(6)],m)
fringes=[(-16,-5.9,4.1),(-13,-6.4,4),(-7,-5.3,4.2),(16,-3.0,4),(-18,5,7.9),(-8,3.9,7.9),(-6,17,12.6),(3,16.2,12.6),(10,18,12.5),(-13,12,7.7)]
for gi,(x,y,z) in enumerate(fringes):
 for i in range(9):
  xx=x+(i-4)*.16;hh=[.28,.42,.24,.37,.52,.30,.43,.26,.34][i];mesh('Grass fringe blades',[(xx-.06,y,z-.07),(xx+.06,y,z-.07),(xx+.09,y+.05,z+hh),(xx+.02,y+.08,z+hh*.8)],[(0,1,2,3)],grassLight)
 # draped vines, the trailing leaves sit in front of the rock surface
 if gi%2==0:
  for i in range(7):
   zz=z-i*.25;xx=x+.09*math.sin(i);beam('Trailing vine stem',(xx,y-.10,zz),(xx+.06,y-.12,zz-.26),.018,leaf);leafmesh('Trailing ivy leaf',(xx+(.13 if i%2==0 else -.13),y-.16,zz-.15),.32,i*.8,leaf)
flowergroups=[(-15,-11,.34),(-16,-9,.34),(-14,-2,3.65),(16,6,3.65),(-12,15,7.65),(10,17,7.65),(-7,23,12.45),(7,24,12.45)]
for gi,p in enumerate(flowergroups):
 for i,(dx,dy,h) in enumerate([(-.35,.1,.38),(0,0,.5),(.3,.16,.43),(.05,.35,.34)]):
  x,y,z=p[0]+dx,p[1]+dy,p[2];beam('Flower curved stalk',(x,y,z),(x+.04,y,z+h),.022,leaf)
  for k in range(5):
   a=k*math.pi*2/5;leafmesh('Hibiscus soft petals',(x+.13*math.cos(a),y+.13*math.sin(a),z+h),.30,a,pink if gi%2==0 else cream)
  bpy.ops.mesh.primitive_uv_sphere_add(segments=10,ring_count=6,radius=.065,location=(x,y,z+h+.03));bpy.context.object.name='Flower golden heart';bpy.context.object.data.materials.append(gold)
  leafmesh('Flower basal leaf',(x+.12,y,z+.1),.45,.4,leaf)
# Broad broken outcrops interrupt the large cliff silhouette.
for n,p,s,v,a in [('Split face left',(-18,4,2),(1.8,2.0,3.3),2,-24),('Split middle face',(-3,3.3,3.6),(2,1.4,3.4),1,25),('Upper broken ledge',(-7,15,8.5),(2.3,1.8,2.8),2,-8),('Cottage footing',(10,25,8),(2.0,2.2,3.5),0,20)]:cliff(n,p,s,v,a,False)
''' +s[idx:]
p.write_text(s,encoding='utf-8')
