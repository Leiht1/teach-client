import bpy,json
from mathutils import Vector
from mathutils.bvhtree import BVHTree
from pathlib import Path
root=Path(__file__).resolve().parents[2].as_posix()
bpy.ops.wm.open_mainfile(filepath=root+'/ArtSource/Lighthouse_Authored/Lighthouse_Coast_Authored.blend')
route=json.load(open(root+'/ArtSource/Lighthouse_Authored/route.json'))
objects=[o for o in bpy.context.scene.objects if o.type=='MESH' and 'fringe' not in o.name and o.name.startswith(('Cliff','Beach sand','Lower meadow','Middle meadow','Upper meadow','Trail ','Split ','Upper broken','Cottage footing'))]
dg=bpy.context.evaluated_depsgraph_get();trees=[(o,BVHTree.FromObject(o,dg)) for o in objects];bad=[]
for i,(x,z,y) in enumerate(route):
 for o,tree in trees:
  inv=o.matrix_world.inverted();loc,n,idx,dist=tree.ray_cast(inv@Vector((x,y,50)),inv.to_3x3()@Vector((0,0,-1)),60)
  if loc is not None:
   h=(o.matrix_world@loc).z
   if h>z-.1:bad.append((i,o.name,round(h-z,3)))
print('ROAD_CLEARANCE_INTRUSIONS',len(bad));print(bad[:30])
