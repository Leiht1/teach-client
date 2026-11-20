from pathlib import Path
p=Path('ArtSource/Lighthouse_Authored/build_dcc.py');s=p.read_text(encoding='utf-8-sig')
s=s.replace("f.append(tuple(range((len(profile)-1)*N,len(profile)*N)));o=mesh", "f.append(tuple(reversed(range(N))));f.append(tuple(range((len(profile)-1)*N,len(profile)*N)));o=mesh")
insert='''# Cut a continuous, surveyed clearance channel into every intersecting land module.
# The road shoulder then covers the cut, so no platform face can slice across the playable road.
cv=[];cf=[]
for i,p in enumerate(road):
 d=road[min(i+1,len(road)-1)]-road[max(0,i-1)];side=Vector((-d.y,d.x,0)).normalized()
 for z in [-.16,35]:
  for sign in [-1,1]:cv.append(tuple(p+side*2.74*sign+Vector((0,0,z))))
for i in range(len(road)-1):
 a=i*4;b=a+4;cf.extend([(a,b,b+1,a+1),(a+2,a+3,b+3,b+2),(a,a+2,b+2,b),(a+1,b+1,b+3,a+3)])
cf.extend([(0,1,3,2),(len(cv)-4,len(cv)-2,len(cv)-1,len(cv)-3)])
cutter=mesh('Surveyed trail clearance cutter',cv,cf,stone)
for obj in list(bpy.context.scene.objects):
 if obj.type!='MESH' or 'fringe' in obj.name:continue
 if not obj.name.startswith(('Cliff','Beach sand','Lower meadow','Middle meadow','Upper meadow','Trail ','Split ','Upper broken','Cottage footing')):continue
 mod=obj.modifiers.new('Continuous road clearance','BOOLEAN');mod.operation='DIFFERENCE';mod.solver='EXACT';mod.object=cutter
 bpy.context.view_layer.objects.active=obj
 try:bpy.ops.object.modifier_apply(modifier=mod.name)
 except Exception as err:print('CLEARANCE_ERROR',obj.name,err)
bpy.data.objects.remove(cutter,do_unlink=True)
'''
s=s.replace('# Export route with Unity coordinate convention',insert+'\n# Export route with Unity coordinate convention')
p.write_text(s,encoding='utf-8')
