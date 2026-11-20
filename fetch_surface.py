import urllib.request,json,os
out='Assets/Island_Surface_V02/Textures';os.makedirs(out,exist_ok=True)
ids=['coast_sand_01','forest_ground_04','brown_mud_leaves_01','rocky_terrain_02','grass_path_2']
for name in ids:
 try:
  data=json.load(urllib.request.urlopen(urllib.request.Request('https://api.polyhaven.com/files/'+name,headers={'User-Agent':'IslandSurface/1.0 contact local-editor'})))
  for kind in ['diff','nor_gl']:
   variants=data['Diffuse' if kind=='diff' else 'nor_gl']['1k']; fmt='jpg' if 'jpg' in variants else 'png';u=variants[fmt]['url'];urllib.request.urlretrieve(u,out+'/'+name+'_'+kind+'.'+fmt)
  print(name,'OK')
 except Exception as e: print(name,str(e))



