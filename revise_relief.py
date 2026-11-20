from pathlib import Path
p=Path('Assets/Editor/IslandMountainFix.cs');s=p.read_text(encoding='utf-8-sig')
s=s.replace('return Mathf.Max(0,1-radius)*h;','float q=Mathf.Max(0,1-radius);return Mathf.Min(.82f,q)*h;')
s=s.replace('var hs=td.GetHeights(0,0,513,513);','var hs=td.GetHeights(0,0,513,513);')
s=s.replace('float target=old;','float target=old; if(zz>50 || (xx>140&&zz>90))hs[z,x]=bh[z,x];')
s=s.replace('100,155','100,131').replace('78,135','78,117').replace('68,119','68,103')
s=s.replace('float ridge=mountain+7*','float ridge=mountain+11*')
s=s.replace('edge-3,edge+3','edge-6,edge+7')
s=s.replace('old+face*33','old+face*24')
a=s.index('float dx=(xx-205)');b=s.index('float protect=',a)
s=s[:a]+'''float dx=(xx-205)/70,dz=(zz-155)/72,r=Mathf.Sqrt(dx*dx+dz*dz);if(r<1.45f){float rim=1+.13f*Mathf.Sin(zz*.066f)+.08f*Mathf.Cos(xx*.09f);float plateau=72+4*Mathf.Sin(xx*.05f)+3*Mathf.Cos(zz*.08f);float raised=Mathf.Lerp(old,plateau,Sat(rim+.16f,rim-.07f,r));target=Mathf.Max(target,raised);}
 '''+s[b:]
s=s.replace('Sat(7,23,Dist(xx,zz))','Sat(7,36,Dist(xx,zz))')
# use 4 terrain layers for reliable built-in terrain rendering, folding leaf soil and wet sand into corresponding layers
start=s.index('td.SetHeights(0,0,hs);');end=s.index('td.SetBaseMapDirty()',start)
s=s[:start]+'''td.SetHeights(0,0,hs);var previous=td.terrainLayers; TerrainLayer sand=previous[0],grass=previous[2],soil=previous[3],rockLayer=previous[previous.Length-1];td.terrainLayers=new[]{sand,grass,soil,rockLayer};var alpha=new float[512,512,4];for(int z=0;z<512;z++)for(int x=0;x<512;x++){float xx=x/511f*720-360,zz=z/511f*720-360,slope=td.GetSteepness(x/511f,z/511f),h=td.GetInterpolatedHeight(x/511f,z/511f)-25;maxSlope=Mathf.Max(maxSlope,slope);float rock=Mathf.Max(Sat(25,48,slope),Sat(55,90,h))*.96f,beach=1-Sat(3,12,h),noise=Mathf.PerlinNoise(xx*.04f+20,zz*.04f+20);alpha[z,x,0]=beach*(1-rock);alpha[z,x,1]=(1-beach)*(1-rock)*(.75f+.15f*noise);alpha[z,x,2]=(1-beach)*(1-rock)*(.25f-.15f*noise);alpha[z,x,3]=rock;}td.SetAlphamaps(0,0,alpha);'''+s[end:]
p.write_text(s,encoding='utf-8-sig')
