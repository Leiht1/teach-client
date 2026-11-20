import bpy, math, os
from mathutils import Vector
from mathutils.geometry import tessellate_polygon
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2].as_posix()
OUT=ROOT+'/Assets/Lighthouse_Authored'
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
def mat(n,c):
 m=bpy.data.materials.new(n);m.diffuse_color=(*c,1);m.use_nodes=True;p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=(*c,1);p.inputs['Roughness'].default_value=.83;return m
stone=mat('Warm lavender limestone',(.43,.45,.51));stoneLight=mat('Limestone sunlit strata',(.62,.60,.57));stoneDark=mat('Limestone shaded seams',(.33,.37,.44));grass=mat('Meadow jade',(.25,.48,.075));grassLight=mat('Grass lime tips',(.44,.64,.12));sand=mat('Apricot dune sand',(.82,.64,.38));wet=mat('Wet golden sand',(.65,.48,.28));ivory=mat('Warm ivory plaster',(.93,.86,.69));coral=mat('Vermilion painted wood',(.70,.105,.065));wood=mat('Honey cedar',(.39,.19,.075));gold=mat('Brass ochre',(.68,.40,.08));blue=mat('Sailor blue',(.035,.23,.36));glass=mat('Lantern warm glass',(.94,.64,.23));leaf=mat('Foliage green',(.24,.49,.075));pink=mat('Coral hibiscus',(.95,.18,.22));cream=mat('Daisy petals',(.97,.87,.56))
def mesh(n,v,f,m,bevel=0):
 me=bpy.data.meshes.new(n);me.from_pydata(v,[],f);me.update();o=bpy.data.objects.new(n,me);bpy.context.collection.objects.link(o);o.data.materials.append(m)
 if bevel:
  mod=o.modifiers.new('Soft hand finished edges','BEVEL');mod.width=bevel;mod.segments=3
  mod=o.modifiers.new('Weighted broad faces','WEIGHTED_NORMAL');mod.keep_sharp=True
 return o
def cap(n,pts,z,depth,m,bevel=.12):
 count=len(pts);v=[(x,y,z) for x,y in pts]+[(x,y,z-depth) for x,y in pts];f=[]
 poly=[Vector(p) for p in v[:count]]
 for tri in tessellate_polygon([poly]):f.append(tuple(p if isinstance(p,int) else poly.index(p) for p in tri))
 for i in range(count):j=(i+1)%count;f.append((i,count+i,count+j,j))
 f.append(tuple(reversed(range(count,count*2))))
 o=mesh(n,v,f,m,bevel)
 if m==grass:
  o.data.materials.append(stone)
  for face in o.data.polygons:
   if face.normal.z<.6:face.material_index=1
 return o
def block(n,p,size,m,bev=.08):
 bpy.ops.mesh.primitive_cube_add(size=1,location=p);o=bpy.context.object;o.name=n;o.dimensions=size;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);o.data.materials.append(m)
 if bev:mod=o.modifiers.new('Rounded edges','BEVEL');mod.width=bev;mod.segments=3;o.modifiers.new('Weighted normals','WEIGHTED_NORMAL')
 return o
def beam(n,a,b,r,m):
 d=Vector(b)-Vector(a);bpy.ops.mesh.primitive_cylinder_add(vertices=10,radius=r,depth=d.length,location=(Vector(a)+Vector(b))/2);o=bpy.context.object;o.name=n;o.rotation_euler=d.to_track_quat('Z','Y').to_euler();o.data.materials.append(m);mod=o.modifiers.new('Edge softness','BEVEL');mod.width=r*.25;mod.segments=2;return o
def soften(pts, passes=1):
 for _ in range(passes):
  pts=[q for a,b in zip(pts,pts[1:]+pts[:1]) for q in [(a[0]*.78+b[0]*.22,a[1]*.78+b[1]*.22),(a[0]*.22+b[0]*.78,a[1]*.22+b[1]*.78)]]
 return pts

# Three individually drawn rock contours. Ledges slope back into the land, not vertical walls.
contours=[ [(-.93,-.64),(-.48,-.96),(.27,-.91),(.91,-.52),(1,.13),(.67,.75),(.02,.88),(-.71,.63),(-1,.08)], [(-.88,-.52),(-.35,-.9),(.50,-.83),(.97,-.21),(.77,.53),(.15,.9),(-.6,.71),(-1,.1)], [(-1,-.22),(-.62,-.81),(.14,-.92),(.83,-.52),(1,.32),(.49,.78),(-.27,.92),(-.88,.49)]]
def cliff(n,p,scale,variant,angle=0,topgrass=True):
 pts=contours[variant];N=len(pts);v=[]
 # hand authored broken silhouette profiles
 profiles=[(-.04,.79,.02,-.04),(.16,1.0,-.06,0),(.48,.94,.035,.015),(.76,.88,-.04,.045),(1,.70,.04,.03)]
 for j,(z,r,ox,oy) in enumerate(profiles):
  for i,(x,y) in enumerate(pts):v.append(((x*r+ox)*scale[0],(y*r+oy)*scale[1],(z+([.0,.03,-.025,.015,-.01,.02,-.02,.01,.0][i] *.2 if j==4 else [.0,.055,-.035,.025,-.04,.035,-.025,.04,.0][i]*(.55 if j==0 else 1)))*scale[2]))
 f=[tuple(reversed(range(N)))];f.extend((j*N+i,j*N+(i+1)%N,(j+1)*N+(i+1)%N,(j+1)*N+i) for j in range(4) for i in range(N));f.append(tuple(range(4*N,5*N)));o=mesh(n,v,f,stone,.14);o.location=p;o.rotation_euler.z=math.radians(angle);o.data.materials.append(stoneLight);o.data.materials.append(stoneDark)
 for face in o.data.polygons:face.material_index=1 if face.index>N*3 else (2 if face.index in (2,8,11,17) else 0)
 if topgrass:
  rim=soften([(x*.83*scale[0]+.04*scale[0],y*.83*scale[1]+.03*scale[1]) for x,y in pts])
  g=cap(n+' mossy grass crown',rim,scale[2]+.08,.18,grass,.12)
  for face in g.data.polygons:face.material_index=0
  g.location=p;g.rotation_euler.z=o.rotation_euler.z
  # A single sculpted rim mesh: hanging scallops and tapered blades, integral to this asset.
  verts=[];faces=[]
  for edge,(a,b) in enumerate(zip(rim,rim[1:]+rim[:1])):
   for j in range(6):
    t=j/6; x=a[0]*(1-t)+b[0]*t;y=a[1]*(1-t)+b[1]*t
    inward=Vector((-x,-y,0)).normalized();side=Vector((-inward.y,inward.x,0));base=Vector((x,y,scale[2]+.08))
    width=.10+[.03,.09,.02,.06][(edge+j)%4];drop=[.17,.29,.40,.23,.15,.33][(edge+j)%6]
    k=len(verts);verts.extend([tuple(base-side*width+inward*.08),tuple(base+side*width+inward*.08),tuple(base-inward*.12+Vector((0,0,-drop))) ]);faces.append((k,k+2,k+1))
    if False:
     k=len(verts);verts.extend([tuple(base-side*.06+inward*.20),tuple(base+side*.06+inward*.20),tuple(base+inward*.16+Vector((0,0,.23))) ]);faces.append((k,k+1,k+2));faces.append((k+2,k+1,k))
  rimobj=mesh(n+' grass fringe',verts,faces,grass);rimobj.location=p;rimobj.rotation_euler.z=o.rotation_euler.z
 return o
# Exact artistic layout; no random scattering or formula-generated scene positions.
rocks=[('Foreland left',(-15,-5,-.8),(5,4,4.8),0,-12),('Foreland shoulder',(-8,-4,-.6),(4.8,4.6,4.6),1,10),('Foreland inner',(0,-2,-.8),(4.8,5,4.8),2,-18),('Foreland right',(18.5,-.2,-.8),(5,5,4.6),1,-25),('Eastern toe',(20,5,-1),(4,5,6),0,20),('Mid left',(-17,8,1.8),(4.5,5,6),2,-16),('Mid center',(-3,10.5,1.6),(5,5.5,6.3),0,6),('Mid right',(4,9,1.8),(4.8,5,6.1),1,-12),('Eastern wall',(16,13,2),(4.7,5.4,6.5),2,15),('Upper shoulder',(-10,18,5.5),(5,5,6.3),1,-15),('Upper core',(-1,21,5.8),(5.4,5.8,7.1),0,8),('Upper east',(13,24.5,5.6),(5,6,7),2,-12),('Rear left',(-9,27,4),(5,4.5,8),2,-8),('Rear east',(5,29,5),(5,4.5,7.6),1,15)]
for n,p,s,v,a in rocks:cliff('Cliff • '+n,p,s,v,a)
# Authored shore outline and beach. Curved edge is a drawn contour, not a rectangular terrain.
beach=[(-26,8),(-26,-1),(-23,-10),(-18,-17),(-12,-20),(-5,-19),(2,-16),(10,-12),(18,-8),(24,-1),(26,10),(23,21),(17,31),(5,35),(-8,32),(-19,22)]
beach=soften(beach,2)
cap('Beach sand foundation',beach,.30,2.2,sand,.45)
import json
with open(ROOT+'/ArtSource/Lighthouse_Authored/shore.json','w') as f:json.dump(beach,f)
# Interior shelves are individually drawn footprints.
cap('Lower meadow',[(-18,-3),(-13,-6),(-6,-5),(1,-2),(8,-2),(17,0),(20,7),(17,12),(4,13),(-11,12),(-18,7)],3.55,1.5,grass,.28)
cap('Middle meadow',[(-16,12),(-10,12),(-7,10),(-4,8),(3,8),(11,10),(17,14),(15,22),(2,23),(-11,21),(-17,16)],7.50,1.6,grass,.28)
cap('Upper meadow',[(-12,19),(-6,15),(1,15),(4,17),(4,22),(2,24),(5,27),(9,29),(5,32),(-5,30),(-12,26)],12.30,1.8,grass,.3)
# Route is explicitly authored as a sequence of surveyed points. Shoulder geometry carries it into the rock.
route=[(-13,-16,.34),(-8,-13,.42),(-2,-10,.75),(5,-7,1.45),(11,-3,2.45),(12,1,3.6),(8,3,3.7),(2,3,4.3),(-5,4,5.15),(-10,7,6.2),(-11,11,7.6),(-7,13,7.65),(-1,13,8.3),(5,14,9.15),(8,18,10.25),(7,22,11.3),(4.8,24,12.8),(1,22.5,13.05),(-2,22,13.05)]
# Smooth only between the authored points, with Catmull-Rom to remove angular corners.
def smooth(points,steps=8):
 q=[Vector(points[0])]+[Vector(p) for p in points]+[Vector(points[-1])];out=[]
 for i in range(1,len(q)-2):
  a,b,c,d=q[i-1:i+3]
  for j in range(steps):
   t=j/steps;out.append(.5*((2*b)+(-a+c)*t+(2*a-5*b+4*c-d)*t*t+(-a+3*b-3*c+d)*t*t*t))
 out.append(Vector(points[-1]));return out
road=smooth(route)
def ribbon(n,points,width,m,drop=0):
 v=[];f=[]
 for i,p in enumerate(points):
  tangent=points[min(i+1,len(points)-1)]-points[max(0,i-1)];side=Vector((-tangent.y,tangent.x,0)).normalized()
  for s in (-1,1):v.append(tuple(p+side*width*s+Vector((0,0,drop))))
 for i in range(len(points)-1):k=i*2;f.append((k,k+2,k+3,k+1))
 return mesh(n,v,f,m)
shoulder=ribbon('Natural grass shoulders',road,2.8,grass,-.08)
shoulder.data.materials.append(stone);solid=shoulder.modifiers.new('Rock supported trail bed','SOLIDIFY');solid.thickness=2.6;solid.offset=-1;solid.material_offset=1;solid.material_offset_rim=1
pathmat=sand.copy();pathmat.name='Mountain footpath blend'
pv=[];pf=[];weights=[]
for i,p in enumerate(road):
 d=road[min(i+1,len(road)-1)]-road[max(0,i-1)];side=Vector((-d.y,d.x,0)).normalized()
 for j,k in enumerate([-1,-.82,-.63,0,.63,.82,1]):pv.append(tuple(p+side*(1.65*k)+Vector((0,0,.025))));weights.append([1,.4,0,0,0,.4,1][j] if p.z>1 else 0)
 if i:
  for j in range(6):a=(i-1)*7+j;pf.append((a,a+7,a+8,a+1))
pathmesh=mesh('Sandy mountain footpath',pv,pf,pathmat)
colors=pathmesh.data.color_attributes.new(name='Color',type='FLOAT_COLOR',domain='POINT')
for i,w in enumerate(weights):colors.data[i].color=(w,w,w,1)

# Exposed road shoulders supported with individual small rock modules at hand-picked turns.
for n,p,s,v,a in [('Trail first support',(7,-6,-.5),(4.8,3.3,2.3),1,20),('Trail switchback toe',(16,-.5,-.2),(3,4,3.8),2,15),('Trail west ledge',(-9,5,1.2),(3.5,4,4.6),0,-12),('Trail upper ledge',(11,16,5.3),(3,4,4.8),1,12),('Trail upper corner',(11,22,7),(3,3,4.7),2,-15)]:cliff(n,p,s,v,a,False)
# Landmark architectural asset: tapered plaster courses, arched joinery, individual masonry and balcony.
def lathe(n,p,profile,m):
 N=64;v=[];f=[]
 for z,r in profile:
  for i in range(N):a=2*math.pi*i/N;v.append((p[0]+r*math.cos(a),p[1]+r*math.sin(a),p[2]+z))
 for j in range(len(profile)-1):
  for i in range(N):f.append((j*N+i,j*N+(i+1)%N,(j+1)*N+(i+1)%N,(j+1)*N+i))
 f.append(tuple(reversed(range(N))));f.append(tuple(range((len(profile)-1)*N,len(profile)*N)));o=mesh(n,v,f,m,.035)
 for face in o.data.polygons:face.use_smooth=True
 return o
T=(-2,25,12.55)
lathe('Lighthouse dressed stone podium',T,[(0,5.7),(.38,5.7),(.45,5.5)],stoneLight)
base=(T[0],T[1],T[2]+.45)
lathe('Lighthouse • tapered plaster',base,[(0,1.85),(.25,1.95),(.5,1.82),(6.8,1.35),(7.05,1.55)],ivory)
lathe('Lighthouse • red base course',base,[(.45,1.84),(1.8,1.745)],coral)
lathe('Lighthouse • upper red stripe',base,[(5.0,1.525),(5.75,1.47)],coral)
lathe('Lighthouse • balcony corbels',base,[(6.8,1.4),(7.1,1.95),(7.32,2.02),(7.4,2.02)],coral)
lathe('Lighthouse • lantern glass',base,[(7.4,1.26),(9.35,1.26)],glass)
lathe('Lighthouse • bell roof',base,[(9.32,1.75),(9.5,1.76),(9.75,1.5),(10.15,.95),(10.42,.33),(10.48,0)],coral)
lathe('Lighthouse • finial',base,[(10.4,.14),(10.9,.14),(11.03,0)],gold)
for i in range(12):
 a=i*math.pi/6;x=base[0]+1.29*math.cos(a);y=base[1]+1.29*math.sin(a);beam('Lantern brass mullion',(x,y,base[2]+7.4),(x,y,base[2]+9.4),.045,gold)
for h in (7.75,8.8):lathe('Lantern glazing transom',base,[(h,1.285),(h+.045,1.285)],gold)
for i in range(24):
 a=i*math.pi/12;b=(i+1)*math.pi/12;p=(base[0]+1.85*math.cos(a),base[1]+1.85*math.sin(a),base[2]+7.4);q=(base[0]+1.85*math.cos(b),base[1]+1.85*math.sin(b),base[2]+8.24);beam('Balcony spindle',p,(p[0],p[1],q[2]),.035,coral);beam('Balcony handrail',(p[0],p[1],q[2]),q,.055,coral)
# Front arched door in XZ plane, facing the approach.
def arch(n,x,y,z,w,h,m):
 pts=[(-w/2,0),(w/2,0),(w/2,h-w/2)]+[(math.cos(a)*w/2,h-w/2+math.sin(a)*w/2) for a in [i*math.pi/16 for i in range(17)]]
 return mesh(n,[(x+u,y,z+v) for u,v in pts],[tuple(range(len(pts)))],m)
arch('Door stone arch surround',base[0],base[1]-1.88,base[2],1.42,2.45,stoneLight);arch('Blue oak entrance',base[0],base[1]-1.905,base[2]+.08,1.08,2.21,blue)
for x in (-.35,-.17,0,.17,.35):beam('Door vertical joinery',(base[0]+x,base[1]-1.92,base[2]+.14),(base[0]+x,base[1]-1.92,base[2]+1.75),.012,wood)
for z,r in [(3.05,1.66),(4.25,1.58),(6.0,1.45)]:arch('Window ivory surround',base[0],base[1]-r,base[2]+z,.62,.88,stoneLight);arch('Window blue inset',base[0],base[1]-r-.02,base[2]+z+.08,.40,.65,blue)
# Keeper cottage with true roof thickness and terracotta tiles.
block('Keeper cottage plaster',(3.8,26,14.1),(3.6,3.8,2.8),ivory,.12)
roof=mesh('Keeper cottage pitched roof',[(1.7,23.8,15.5),(5.9,23.8,15.5),(3.8,23.8,17),(1.7,28.2,15.5),(5.9,28.2,15.5),(3.8,28.2,17)],[(0,2,1),(3,4,5),(0,3,5,2),(2,5,4,1)],coral,.08)
for y in [24,24.45,24.9,25.35,25.8,26.25,26.7,27.15,27.6,28]:
 beam('Roof tile seam',(1.75,y,15.55),(3.8,y,17.05),.035,wood);beam('Roof tile seam',(3.8,y,17.05),(5.85,y,15.55),.035,wood)
arch('Cottage blue entrance',3.8,24.085,12.8,1,2.1,blue);block('Cottage chimney',(5,27,16.8),(.55,.6,1.8),ivory,.06)
# Wood guardrails. These authored runs frame the vista without fencing the full path.
def fence(points):
 for p in points:beam('Cedar square-ended post',p,(p[0],p[1],p[2]+1),.105,wood)
 for p,q in zip(points,points[1:]):
  for h in (.45,.88):beam('Cedar guardrail',(p[0],p[1],p[2]+h),(q[0],q[1],q[2]+h),.065,wood)
fence([(-16,-4,3.6),(-13,-5.2,3.6),(-10,-5.1,3.6),(-7,-4.6,3.6)])
fence([(-13,10,7.6),(-13,12.5,7.6),(-11,15,7.6),(-8,15.5,7.6)])
fence([(-7,25,13),(-6.4,28,13),(-3.6,30,13),(0,30.4,13),(3,29.8,13)])
fence([(13.6,-2,2.4),(15,1,3.7),(12.1,4.5,3.8),(9,5,3.8)])
# Natural sea boulders, each deliberately placed.
for n,p,s,v,a in [('Surf sentinel',(-19,-15,-.7),(2.3,1.8,2.6),0,20),('Surf companion',(-22,-12,-.8),(1.5,1.4,1.5),2,-25),('Beach foreground',(-7,-19,.0),(1.8,1.1,1.4),1,12),('Right beach',(18,-8,-.5),(2.5,1.8,2.8),2,-15),('Tide rock',(-24,-6,-1),(2.3,2,2.3),0,30),('Foreground pebble',(-4,-17,.2),(.7,.6,.5),2,15)]:cliff('Shore rock • '+n,p,s,v,a,False)

# Small sculpted leaves, grass fringe and flowering groups conceal transitions at specific focal corners.
def leafmesh(n,p,s,angle,m):
 verts=[(-.5,0,0),(-.30,-.20,.10),(.15,-.22,.13),(.55,0,0),(.15,.22,.13),(-.30,.20,.10),(0,0,.21)]
 c=math.cos(angle);si=math.sin(angle);v=[(p[0]+(x*c-y*si)*s,p[1]+(x*si+y*c)*s,p[2]+z*s) for x,y,z in verts];return mesh(n,v,[(i,(i+1)%6,6) for i in range(6)],m)
fringes=[(-16,-5.9,4.1),(-13,-6.4,4),(-7,-5.3,4.2),(16,-3.0,4),(-18,5,7.9),(-8,3.9,7.9),(-6,17,12.6),(3,16.2,12.6),(10,18,12.5),(-13,12,7.7)]
for gi,(x,y,z) in enumerate(fringes):
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
for n,p,s,v,a in [('Split face left',(-18,4,2),(1.8,2.0,3.3),2,-24),('Split middle face',(-5,9,3.6),(2,1.4,3.4),1,25),('Upper broken ledge',(-7,15,8.5),(2.3,1.8,2.8),2,-8),('Cottage footing',(10,25,8),(2.0,2.2,3.5),0,20)]:cliff(n,p,s,v,a,False)
# Two coastal palms, with continuous swept fronds instead of angular kit foliage.
palmBark=mat('Palm honey bark',(.46,.27,.12));palmRidge=mat('Palm sunlit bark rings',(.56,.35,.17));palmLeaf=mat('Palm emerald leaves',(.12,.43,.12));palmTip=mat('Palm sunlit leaves',(.30,.55,.10))
def palm(n,base,height,rotation):
 bx,by,bz=base;v=[];f=[];segments=20;N=16
 for j in range(segments+1):
  t=j/segments;cx=bx+math.sin(t*1.4)*.65;cy=by+.35*t*t;r=.26*(1-t*.55)*(1+.07*math.cos(t*math.pi*20))
  for k in range(N):a=2*math.pi*k/N;v.append((cx+math.cos(a)*r,cy+math.sin(a)*r,bz+t*height))
 for j in range(segments):
  for k in range(N):f.append((j*N+k,j*N+(k+1)%N,(j+1)*N+(k+1)%N,(j+1)*N+k))
 trunk=mesh(n+' curved trunk',v,f,palmBark)
 for face in trunk.data.polygons:face.use_smooth=True
 top=Vector((bx+math.sin(1.4)*.65,by+.35,bz+height))
 for k in range(10):
  ang=rotation+k*math.pi*.2;forward=Vector((math.cos(ang),math.sin(ang),0));side=Vector((-math.sin(ang),math.cos(ang),0));length=[2.6,2.2,2.9,2.4,2.7][k%5];verts=[];faces=[]
  for j in range(13):
   t=j/12;center=top+forward*length*t+Vector((0,0,math.sin(t*math.pi*.9)*.75-t*t*.95));width=math.sin(t*math.pi)**.8*.39*(.92+.08*math.cos(t*math.pi*12))
   for sign in [-1,0,1]:verts.append(tuple(center+side*width*sign-Vector((0,0,abs(sign)*.075))))
  for j in range(12):
   for h in range(2):a=j*3+h;faces.append((a,a+1,a+4,a+3));faces.append((a+3,a+4,a+1,a))
  frond=mesh(n+' swept frond '+str(k),verts,faces,palmTip if k%3==0 else palmLeaf)
  for face in frond.data.polygons:face.use_smooth=True
palm('Coastal palm left',(-20,-9,.32),5.2,.3)
palm('Coastal palm right',(15,-10,.32),5.7,1.0)

# Flatten/apply export modifiers. Geometry is stored as reusable DCC-authored meshes, not generated at runtime.
for o in list(bpy.context.scene.objects):
 if o.type=='MESH':
  bpy.context.view_layer.objects.active=o;o.select_set(True)
  for mod in list(o.modifiers):
   try:bpy.ops.object.modifier_apply(modifier=mod.name)
   except:pass
  o.select_set(False)
# Cut a continuous, surveyed clearance channel into every intersecting land module.
# The road shoulder then covers the cut, so no platform face can slice across the playable road.
cv=[];cf=[]
for i,p in enumerate(road):
 d=road[min(i+1,len(road)-1)]-road[max(0,i-1)];side=Vector((-d.y,d.x,0)).normalized()
 for z in [-.16,35]:
  for sign in [-1,1]:cv.append(tuple(p+side*2.74*sign+Vector((0,0,z))))
for i in range(len(road)-1):
 a=i*4;b=a+4;cf.extend([(a,b,b+1,a+1),(a+2,a+3,b+3,b+2),(a,a+2,b+2,b),(a+1,b+1,b+3,a+3)])
cf.extend([(0,1,3,2),(len(cv)-4,len(cv)-2,len(cv)-1,len(cv)-3)])
cf=[tuple(reversed(face)) for face in cf]
cutter=mesh('Surveyed trail clearance cutter',cv,cf,stone)
for obj in list(bpy.context.scene.objects):
 if obj.type!='MESH' or 'fringe' in obj.name:continue
 if not obj.name.startswith(('Cliff','Beach sand','Lower meadow','Middle meadow','Upper meadow','Trail ','Split ','Upper broken','Cottage footing')):continue
 mod=obj.modifiers.new('Continuous road clearance','BOOLEAN');mod.operation='DIFFERENCE';mod.solver='EXACT';mod.object=cutter
 bpy.context.view_layer.objects.active=obj
 try:bpy.ops.object.modifier_apply(modifier=mod.name)
 except Exception as err:print('CLEARANCE_ERROR',obj.name,err)
bpy.data.objects.remove(cutter,do_unlink=True)

# Export route with Unity coordinate convention for gameplay check.
import json
with open(ROOT+'/ArtSource/Lighthouse_Authored/route.json','w') as f:json.dump([[p.x,p.z,p.y] for p in road],f)
bpy.ops.wm.save_as_mainfile(filepath=ROOT+'/ArtSource/Lighthouse_Authored/Lighthouse_Coast_Authored.blend')
bpy.ops.export_scene.fbx(filepath=OUT+'/CoastArchitecture.fbx',use_selection=False,apply_scale_options='FBX_SCALE_UNITS',axis_forward='-Z',axis_up='Y',add_leaf_bones=False,bake_anim=False)
print('AUTHORED_COAST_EXPORTED',len(bpy.context.scene.objects))


