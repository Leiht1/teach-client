from pathlib import Path
p=Path('ArtSource/Lighthouse_Authored/build_dcc.py');s=p.read_text(encoding='utf-8-sig')
s=s.replace('if j==4 else [.0,.055','*.2 if j==4 else [.0,.055')
s=s.replace('x*.77*scale[0]','x*.83*scale[0]').replace('y*.77*scale[1]','y*.83*scale[1]')
s=s.replace('if (edge+j)%3==0:', 'if False:')
p.write_text(s,encoding='utf-8')
p=Path('Assets/Lighthouse_Authored/DaySky.shader');s=p.read_text(encoding='utf-8-sig').replace('d.y*1.4','d.y*3.5').replace('(d.y-.25)*6','(d.y-.08)*10').replace('(d.y-.42)*7','(d.y-.18)*10').replace('(d.y-.20)*7','(d.y-.10)*10');p.write_text(s,encoding='utf-8')
p=Path('Assets/Editor/CoastAuthored.cs');s=p.read_text(encoding='utf-8-sig').replace('Asset(i%2==0?"CommonTree_1.fbx":"CommonTree_2.fbx",new Vector3(p.x,p.y,p.z),p.w,i*37);','var tree=Asset(i%2==0?"CommonTree_1.fbx":"CommonTree_2.fbx",new Vector3(p.x,p.y,p.z),p.w,i*37);tree.transform.localScale=Vector3.Scale(tree.transform.localScale,new Vector3(1.45f,1,1.45f));');p.write_text(s,encoding='utf-8')
