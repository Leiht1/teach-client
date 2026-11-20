import zipfile,pathlib
root=pathlib.Path('ArtSource');src=root/'OriginalFiles'/'Universal Animation Library[Standard]'
with zipfile.ZipFile(root/'Archives'/'UniversalAnimationLibrary-local-copy.zip','w',zipfile.ZIP_DEFLATED) as z:
 for p in src.rglob('*'):
  if p.is_file():z.write(p,p.relative_to(src))
print('Archived reused local animation files; no download')
