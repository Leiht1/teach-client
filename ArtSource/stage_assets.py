from pathlib import Path
import shutil,json,hashlib
root=Path(__file__).resolve().parent.parent
packs={'StylizedNatureMegaKit':'Quaternius','UltimateModularRuins':'Quaternius','UniversalBaseCharacters':'Quaternius','UniversalAnimationLibrary':'Quaternius','PirateKit':'Kenney','WatercraftKit':'Kenney','ForestNature':'KayKit','ResourceBits':'KayKit','DungeonRemastered':'KayKit'}
for d in ['Characters','Environment','Buildings','Props','Materials','Textures','Animations','VFX','Prefabs']:(root/'Assets/Art'/d).mkdir(parents=True,exist_ok=True)
for d in ['ArtPreview/Scenes','ArtPreview/Prefabs','ArtPreview/Materials','Scenes/ArtTest','Editor']:(root/'Assets'/d).mkdir(parents=True,exist_ok=True)
records=[]; hashes={}
for name,author in packs.items():
 dest=root/'Assets/Art/External'/author/name;dest.mkdir(parents=True,exist_ok=True)
 source=root/'ArtSource/OriginalFiles'/name
 if name=='UniversalAnimationLibrary':source=root/'ArtSource/OriginalFiles/Universal Animation Library[Standard]'
 files=[]
 if source.exists():
  for f in source.rglob('*'):
   if not f.is_file():continue
   rel=f.relative_to(source); s=str(rel).replace('\\','/');low=s.lower();ext=f.suffix.lower()
   if ext in ('.txt','.md') and ('licen' in f.name.lower() or 'readme' in f.name.lower()):
    lp=root/'ArtSource/Licenses'/name/rel;lp.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(f,lp)
   chosen=False
   if ext=='.fbx':
    if name=='StylizedNatureMegaKit':chosen=low.startswith('fbx (unity)/')
    elif name in ('PirateKit','WatercraftKit'):chosen='fbx format/' in low
    elif name=='UniversalBaseCharacters':chosen='/unity/' in low or '/origin at 0/fbx (unity)/' in low
    elif name=='UniversalAnimationLibrary':chosen=low.startswith('unity/')
    elif name in ('ForestNature','ResourceBits','DungeonRemastered'):chosen='/fbx(unity)/' in low or '/fbx (unity)/' in low
    else:chosen=True
   if ext in ('.png','.jpg','.jpeg','.tga') and ('texture' in low or name=='UltimateModularRuins') and not any(x in low for x in ['preview','sample','setup','gltf','unreal','godot','fbx format/textures' if name not in ('PirateKit','WatercraftKit') else 'never-match']):chosen=True
   if chosen:
    data=f.read_bytes();sha=hashlib.sha256(data).hexdigest(); target=dest/f.name if name=='UniversalAnimationLibrary' else dest/rel
    if sha in hashes:
     files.append({'source':str(f),'asset':hashes[sha],'duplicate_reused':True});continue
    target.parent.mkdir(parents=True,exist_ok=True)
    if not target.exists():shutil.copy2(f,target)
    asset=str(target.relative_to(root)).replace('\\','/');hashes[sha]=asset
    files.append({'source':str(f),'asset':asset,'sha256':sha,'bytes':len(data)})
 records.append(dict(name=name,author=author,source=str(source),files=files))
(root/'Docs/Art/FILE_MANIFEST.json').write_text(json.dumps(records,ensure_ascii=False,indent=2),encoding='utf8')
print([(r['name'],len(r['files'])) for r in records])
