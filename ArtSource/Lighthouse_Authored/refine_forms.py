from pathlib import Path
p=Path('ArtSource/Lighthouse_Authored/build_dcc.py')
s=p.read_text(encoding='utf-8-sig')
s=s.replace("# Three individually drawn rock contours.",'''def soften(pts, passes=1):
 for _ in range(passes):
  pts=[q for a,b in zip(pts,pts[1:]+pts[:1]) for q in [(a[0]*.78+b[0]*.22,a[1]*.78+b[1]*.22),(a[0]*.22+b[0]*.78,a[1]*.22+b[1]*.78)]]
 return pts

# Three individually drawn rock contours.''')
s=s.replace("pts=contours[variant];N=len(pts);v=[]", "pts=contours[variant];N=len(pts);v=[]")
s=s.replace("(z+([.0,.03,-.025,.015,-.01,.02,-.02,.01,.0][i] if j==4 else 0))*scale[2]", "(z+([.0,.03,-.025,.015,-.01,.02,-.02,.01,.0][i] if j==4 else [.0,.055,-.035,.025,-.04,.035,-.025,.04,.0][i]*(.55 if j==0 else 1)))*scale[2]")
s=s.replace("rim=[(x*.73*scale[0]+.04*scale[0],y*.73*scale[1]+.03*scale[1]) for x,y in pts];g=cap(n+' mossy grass crown',rim,scale[2]+.24,.30,grass,.16);g.location=p;g.rotation_euler.z=o.rotation_euler.z", '''rim=soften([(x*.77*scale[0]+.04*scale[0],y*.77*scale[1]+.03*scale[1]) for x,y in pts])
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
    if (edge+j)%3==0:
     k=len(verts);verts.extend([tuple(base-side*.06+inward*.20),tuple(base+side*.06+inward*.20),tuple(base+inward*.16+Vector((0,0,.23))) ]);faces.append((k,k+1,k+2));faces.append((k+2,k+1,k))
  rimobj=mesh(n+' grass fringe',verts,faces,grass);rimobj.location=p;rimobj.rotation_euler.z=o.rotation_euler.z''')
s=s.replace("cap('Beach sand foundation',beach", "beach=soften(beach,2)\ncap('Beach sand foundation',beach")
# Save shore contour for water distance shader, keep actual sea and foam matched.
s=s.replace("# Interior shelves", "import json\nwith open(ROOT+'/ArtSource/Lighthouse_Authored/shore.json','w') as f:json.dump(beach,f)\n# Interior shelves")
p.write_text(s,encoding='utf-8')
