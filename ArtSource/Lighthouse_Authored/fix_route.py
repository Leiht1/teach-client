from pathlib import Path
p=Path('ArtSource/Lighthouse_Authored/build_dcc.py');s=p.read_text(encoding='utf-8-sig');s=s.replace('f.append((k,k+1,k+3,k+2))','f.append((k,k+2,k+3,k+1))');p.write_text(s,encoding='utf-8')
p=Path('Assets/Lighthouse_Authored/Painted.shader');s=p.read_text();s=s.replace('tile=tile*.485+.0075','tile=tile*.46+.02');p.write_text(s)
