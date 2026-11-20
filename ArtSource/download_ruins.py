import urllib.request as u,re,pathlib,concurrent.futures,zipfile,json
root=pathlib.Path(__file__).resolve().parent
out=root/'OriginalFiles'/'UltimateModularRuins';out.mkdir(exist_ok=True)
seen=set(); files=[]
def folder(ident,sub=''):
 if ident in seen:return
 seen.add(ident)
 h=u.urlopen('https://drive.google.com/drive/folders/'+ident,timeout=60).read().decode()
 (root/'OriginalFiles'/('drive-'+ident+'.html')).write_text(h,encoding='utf8')
 for fid,tooltip in re.findall(r'data-id="([^"]+)"[^>]*.*?data-tooltip="([^"]+)"',h):
  name=tooltip.replace(' Shared folder','').replace(' Binary','').replace(' Text','').replace(' Image','')
  if 'Shared folder' in tooltip:
   if name.lower() not in ('obj','blend','gltf'):folder(fid,sub+name+'/')
  elif re.search(r'\.(fbx|png|jpg|txt|md)$',name,re.I):files.append((fid,sub+name))
folder('1ETp2ldaHaP0BkS4FBmkT-g9Yf88T_cIX')
def download(item):
 fid,name=item; dest=out/name;dest.parent.mkdir(parents=True,exist_ok=True)
 try:
  data=u.urlopen('https://drive.google.com/uc?export=download&id='+fid,timeout=60).read()
  if b'<!DOCTYPE' in data[:100] or b'<html' in data[:100]:raise RuntimeError('HTML instead of file')
  dest.write_bytes(data);return {'file':name,'ok':True}
 except Exception as e:return {'file':name,'ok':False,'error':str(e)}
with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool: results=list(pool.map(download,files))
(root/'ruins-results.json').write_text(json.dumps(results,indent=2))
with zipfile.ZipFile(root/'Archives'/'UltimateModularRuins-official-files.zip','w',zipfile.ZIP_DEFLATED) as z:
 for p in out.rglob('*'):
  if p.is_file():z.write(p,p.relative_to(out))
print('Ruins',sum(x['ok'] for x in results),'/',len(results))
