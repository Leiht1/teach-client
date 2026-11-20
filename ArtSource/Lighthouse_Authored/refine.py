from pathlib import Path
p=Path('ArtSource/Lighthouse_Authored/build_dcc.py');s=p.read_text(encoding="utf-8-sig");s=s.replace('scale[2]+.015,.17,grass','scale[2]+.24,.30,grass');p.write_text(s,encoding="utf-8")
p=Path('Assets/Editor/CoastAuthored.cs');s=p.read_text(encoding="utf-8-sig");s=s.replace('root.name="Authored cliff and lighthouse assemblies";','root.name="Authored cliff and lighthouse assemblies";root.transform.localScale=new Vector3(1,1,-1);')
s=s.replace('"Tree_4_A_Color1.fbx":"Tree_4_C_Color1.fbx"','"Tree_1_A_Color1.fbx":"Tree_1_C_Color1.fbx"').replace('"Bush_4_C_Color1.fbx":"Bush_2_C_Color1.fbx"','"Bush_1_C_Color1.fbx":"Bush_1_A_Color1.fbx"')
s=s.replace('m.color=col;m.SetFloat','m.color=col;string low=n.ToLowerInvariant();if(low.Contains("sand")||low.Contains("meadow")||low.Contains("grass")||low.Contains("limestone")){m.shader=Shader.Find("Coast/Painted");m.SetTexture("_Atlas",AssetDatabase.LoadAssetAtPath<Texture2D>(D+"/PaintedSurfaceAtlas.png"));m.SetFloat("_Kind",low.Contains("limestone")?2:low.Contains("grass")||low.Contains("meadow")?1:0);m.SetFloat("_Scale",low.Contains("limestone")?7:low.Contains("sand")?9:4.5f);m.color=low.Contains("grass")||low.Contains("meadow")?new Color(.80f,.91f,.72f):low.Contains("limestone")?new Color(.80f,.86f,.94f):new Color(.92f,.94f,.91f);}m.SetFloat')
s=s.replace('AssetDatabase.CreateAsset(mesh,D+"/OceanPlane.asset");','AssetDatabase.DeleteAsset(D+"/OceanPlane.asset");AssetDatabase.CreateAsset(mesh,D+"/OceanPlane.asset");')
s=s.replace('new Color(.43f,.62f,.82f)','new Color(.50f,.50f,.50f)').replace('sky.SetFloat("_AtmosphereThickness",.85f)','sky.SetFloat("_AtmosphereThickness",.65f)')
p.write_text(s,encoding="utf-8")

