from PIL import Image,ImageEnhance
p='Assets/Island_Surface_V02/Textures/'
for name,tint in [('grass_path_2',(0.56,0.95,0.32)),('coast_sand_01',(1.22,1.10,0.78)),('brown_mud_leaves_01',(0.66,0.63,0.48))]:
 im=Image.open(p+name+'_diff.jpg').convert('RGB'); grey=ImageEnhance.Color(im).enhance(.35);r,g,b=grey.split();grey=Image.merge('RGB',tuple(c.point(lambda v,m=m:min(255,int(v*m))) for c,m in zip((r,g,b),tint)));grey.save(p+name+'_diff.jpg',quality=95)
