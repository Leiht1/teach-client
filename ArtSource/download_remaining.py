import urllib.request as u, urllib.parse as p, http.cookiejar,re,json,pathlib,zipfile,hashlib
root=pathlib.Path(__file__).resolve().parent
packs=[('Quaternius','StylizedNatureMegaKit','https://quaternius.itch.io/stylized-nature-megakit'),('Quaternius','UniversalBaseCharacters','https://quaternius.itch.io/universal-base-characters'),('KayKit','ForestNature','https://kaylousberg.itch.io/kaykit-forest'),('KayKit','ResourceBits','https://kaylousberg.itch.io/resource-bits'),('KayKit','DungeonRemastered','https://kaylousberg.itch.io/kaykit-dungeon-pack')]
results=[]
for author,name,url in packs:
 record=dict(author=author,name=name,url=url,status='failed')
 try:
  if name in ('StylizedNatureMegaKit','UniversalBaseCharacters','ForestNature'):continue
  s=u.build_opener(u.HTTPCookieProcessor(http.cookiejar.CookieJar()))
  def get(a): return s.open(a,timeout=60).read()
  def post(a,data,ref):return s.open(u.Request(a,data=p.urlencode(data).encode(),headers={'Referer':ref}),timeout=60).read()
  h=get(url).decode(); (root/'OriginalFiles'/f'{name}-itch.html').write_text(h,encoding='utf8')
  token=re.search(r'name="csrf_token" value="([^"]+)',h)[1]
  d=json.loads(post(url+'/download_url',{'csrf_token':token},url)); print(name,d,flush=True); dl=d['url']; h=get(dl).decode(); (root/'OriginalFiles'/f'{name}-download.html').write_text(h,encoding='utf8')
  token=re.search(r'name="csrf_token" value="([^"]+)',h)[1]
  matches=re.findall(r'data-upload_id="(\d+)".*?<strong[^>]*title="([^"]+)"',h,re.S)
  record['public_files']=matches
  if not matches: raise RuntimeError('No public downloadable files')
  files=[]
  for ident,filename in matches:
   if filename.lower()=='free': filename=name+'-Free.zip'
   if not filename.lower().endswith('.zip'):continue
   if re.search(r'\bsource\b|deluxe|prototype|godot|unreal|unitypackage',filename,re.I):continue
   link=json.loads(post(url+'/file/'+ident,{'csrf_token':token},dl))['url']
   target=root/'Archives'/filename
   if not target.exists():
    with s.open(link,timeout=120) as r,open(target,'wb') as f:
     while block:=r.read(1024*1024): f.write(block)
   with zipfile.ZipFile(target) as z:
    if z.testzip():raise RuntimeError('ZIP CRC failed')
    z.extractall(root/'OriginalFiles'/name)
   files.append(str(target.relative_to(root.parent)))
  record.update(status='downloaded' if files else 'no suitable free zip',archives=files)
 except Exception as e:record['error']=str(e)
 results.append(record);print(name,record['status'],record.get('error',''),flush=True)
 (root/'download-results.json').write_text(json.dumps(results,ensure_ascii=False,indent=2),encoding='utf8')


