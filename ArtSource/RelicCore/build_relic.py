import bpy, math, os
from mathutils import Vector
bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False)
D='D:/codex项目/teach/ArtSource/RelicCore'; OUT='D:/codex项目/teach/Assets/IslandSystem/Relic/RelicCore.fbx'
def mat(n,c,metal=0):
 m=bpy.data.materials.new(n);m.diffuse_color=(*c,1);m.use_nodes=True;p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=(*c,1);p.inputs['Metallic'].default_value=metal;p.inputs['Roughness'].default_value=.36;return m
ivory=mat('Relic_Ivory',(.89,.84,.67));gold=mat('Relic_Gold',(.92,.64,.20),.5);edge=mat('Relic_DeepGold',(.46,.27,.07),.4);leather=mat('Relic_Leather',(.20,.105,.045));cyan=mat('Relic_Energy',(.05,.8,.92));gem=mat('Relic_Crystal',(.05,.65,.86));light=mat('Relic_FacetLight',(.34,.95,1));dark=mat('Relic_FacetBlue',(.02,.30,.62))
# Blender Z is the outward surface normal, Y runs along the forearm.
def mesh(n,verts,faces,m,bevel=0):
 me=bpy.data.meshes.new(n);me.from_pydata(verts,[],faces);me.update();o=bpy.data.objects.new(n,me);bpy.context.collection.objects.link(o);o.data.materials.append(m)
 if bevel:
  mod=o.modifiers.new('Soft crafted edges','BEVEL');mod.width=bevel;mod.segments=3
  mod=o.modifiers.new('Weighted normals','WEIGHTED_NORMAL');mod.keep_sharp=True
 return o
def plate(n,outline,z,thickness,m,bevel=.001):
 k=len(outline);v=[(x,y,h) for h in (z,z+thickness) for x,y in outline];f=[tuple(reversed(range(k))),tuple(range(k,2*k))]+[(i,(i+1)%k,(i+1)%k+k,i+k) for i in range(k)];return mesh(n,v,f,m,bevel)
def ring(n,r0,r1,z,thick,m,start=0,end=math.tau,N=64):
 a=[start+(end-start)*i/N for i in range(N+1)];v=[(r*math.cos(t),r*math.sin(t),h) for h in (z,z+thick) for r in (r0,r1) for t in a];k=N+1;f=[]
 for i in range(N):f.extend([(i,i+1,k+i+1,k+i),(2*k+i,3*k+i,3*k+i+1,2*k+i+1),(i,2*k+i,2*k+i+1,i+1),(k+i,k+i+1,3*k+i+1,3*k+i)])
 f.extend([(0,k,3*k,2*k),(N,2*k+N,3*k+N,k+N)]);return mesh(n,v,f,m,.001)
def line(n,points,m,r=.0009):
 c=bpy.data.curves.new(n,'CURVE');c.dimensions='3D';c.bevel_depth=r;c.bevel_resolution=2;s=c.splines.new('POLY');s.points.add(len(points)-1)
 for p,co in zip(s.points,points):p.co=(*co,1)
 o=bpy.data.objects.new(n,c);bpy.context.collection.objects.link(o);o.data.materials.append(m);return o
outline=[(0,-.123),(.045,-.078),(.065,-.030),(.053,.048),(.025,.091),(0,.113),(-.025,.091),(-.053,.048),(-.065,-.030),(-.045,-.078)]
plate('01_GildedShield',outline,0,.012,gold,.003)
plate('02_IvoryShell',[(x*.86,y*.92) for x,y in outline],.010,.008,ivory,.0025)
plate('03_CoreRecess',[(0,-.055),(.043,-.027),(.043,.027),(0,.06),(-.043,.027),(-.043,-.027)],.019,.006,edge,.002)
# Two curved, substantial straps encircling the forearm, rather than floating flat strips.
for y in (-.069,.063):
 v=[];N=56
 for yy in (y-.013,y+.013):
  for scale in (1,1.12):
   for i in range(N):
    t=i*math.tau/N;v.append((.047*scale*math.cos(t),yy,-.038+.044*scale*math.sin(t)))
 f=[]
 for i in range(N):
  j=(i+1)%N;f.extend([(i,j,N+j,N+i),(2*N+i,3*N+i,3*N+j,2*N+j),(i,2*N+i,2*N+j,j),(N+i,N+j,3*N+j,3*N+i)])
 mesh('04_LeatherBand_'+str(y),v,f,leather,.0015)
 plate('05_GoldBuckle_'+str(y),[(.043,y-.016),(.057,y-.016),(.057,y+.016),(.043,y+.016)],-.032,.012,gold,.0015)
# Four independently articulated curved ring petals with pale inlay and glow seams.
for i in range(4):
 a=i*math.pi/2+.10;b=(i+1)*math.pi/2-.10
 root=bpy.data.objects.new('Petal_'+str(i),None);bpy.context.collection.objects.link(root)
 for o in [ring('PetalGold_'+str(i),.041,.055,.025,.009,gold,a,b,18),ring('PetalIvory_'+str(i),.044,.052,.034,.003,ivory,a+.035,b-.035,18),ring('PetalGlow_'+str(i),.041,.042,.035,.0012,cyan,a,b,18)]:o.parent=root
# Faceted elongated sapphire with readable bright/dark hand-chosen facets.
v=[(0,0,.081),(0,-.027,.036),(.021,-.013,.036),(.023,.013,.036),(0,.033,.036),(-.023,.013,.036),(-.021,-.013,.036),(0,0,.024)]
f=[(0,1+i,1+(i+1)%6) for i in range(6)]+[(7,1+(i+1)%6,1+i) for i in range(6)]
o=mesh('Crystal',v,f,gem);o.data.materials.append(light);o.data.materials.append(dark)
for i,p in enumerate(o.data.polygons):p.material_index=[1,0,2,0,1,0][i%6]
# Gold talons frame the crystal, while thin energy channels lead into the tapered shield.
for sign in (-1,1):
 plate('CrystalClaw_'+str(sign),[(sign*.028,-.037),(sign*.037,-.026),(sign*.031,.024),(sign*.016,.045),(sign*.022,.015)],.030,.005,gold,.001)
 line('EnergyVein_'+str(sign),[(sign*.014,-.107,.020),(sign*.027,-.073,.021),(sign*.030,-.060,.021)],cyan,.0012)
 line('IvoryEdge_'+str(sign),[(sign*.009,-.109,.021),(sign*.036,-.071,.022),(sign*.049,-.026,.022),(sign*.041,.039,.022)],gold,.001)
for y in (-.077,.077):
 line('RuneDiamond_'+str(y),[(0,y-.006,.021),(.004,y,.021),(0,y+.006,.021),(-.004,y,.021),(0,y-.006,.021)],gold,.001)
 line('RuneMark_'+str(y),[(-.005,y-.010,.021),(0,y-.013,.021),(.005,y-.010,.021)],cyan,.0008)
# Small inset studs catch light without elaborate metal decoration.
for x in (-.038,.038):
 for y in (-.04,.042):
  bpy.ops.mesh.primitive_uv_sphere_add(segments=12,ring_count=6,radius=.002,location=(x,y,.023));bpy.context.object.name='GoldStud';bpy.context.object.data.materials.append(gold)
# Convert decorative curves so Unity imports every visible detail.
for o in list(bpy.context.scene.objects):
 if o.type=='CURVE':bpy.ops.object.select_all(action='DESELECT');o.select_set(True);bpy.context.view_layer.objects.active=o;bpy.ops.object.convert(target='MESH')
bpy.ops.wm.save_as_mainfile(filepath=D+'/RelicCore.blend')
bpy.ops.export_scene.fbx(filepath=OUT,use_selection=False,object_types={'MESH','EMPTY'},apply_unit_scale=True,axis_forward='-Z',axis_up='Y',bake_anim=False,add_leaf_bones=False)
print('RELIC_MESH_EXPORTED')
