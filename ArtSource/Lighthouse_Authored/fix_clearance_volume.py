from pathlib import Path
p=Path('ArtSource/Lighthouse_Authored/build_dcc.py');s=p.read_text(encoding='utf-8-sig').replace("cutter=mesh('Surveyed trail clearance cutter',cv,cf,stone)","cf=[tuple(reversed(face)) for face in cf]\ncutter=mesh('Surveyed trail clearance cutter',cv,cf,stone)")
p.write_text(s,encoding='utf-8')
p=Path('Assets/Editor/CoastAuthored.cs');s=p.read_text(encoding='utf-8-sig').replace('"Bush_Common.fbx"','"Bush_Common_Flowers.fbx"');p.write_text(s,encoding='utf-8')
