from pathlib import Path
p=Path('ArtSource/Lighthouse_Authored/build_dcc.py');s=p.read_text(encoding='utf-8-sig')
insert='''# Two coastal palms, with continuous swept fronds instead of angular kit foliage.
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
'''
s=s.replace('# Flatten/apply export modifiers.',insert+'\n# Flatten/apply export modifiers.')
p.write_text(s,encoding='utf-8')
