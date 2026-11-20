from pathlib import Path
p=Path('Assets/Editor/CoastAuthored.cs');s=p.read_text(encoding='utf-8-sig');needle='root.transform.localScale=new Vector3(1,1,-1);';s=s.replace(needle,needle+'''
 var bakedRoot=new GameObject("Authored coast geometry");int meshIndex=0;
 foreach(var f in root.GetComponentsInChildren<MeshFilter>()){var go=new GameObject(f.name);go.transform.SetParent(bakedRoot.transform);var source=f.sharedMesh;var copy=UnityEngine.Object.Instantiate(source);copy.name=f.name;copy.vertices=source.vertices.Select(v=>f.transform.TransformPoint(v)).ToArray();if(f.transform.localToWorldMatrix.determinant<0){var indices=copy.triangles;for(int k=0;k<indices.Length;k+=3){int t=indices[k];indices[k]=indices[k+2];indices[k+2]=t;}copy.triangles=indices;}copy.RecalculateNormals();copy.RecalculateBounds();string mp=D+"/BakedMesh"+(meshIndex++)+".asset";AssetDatabase.DeleteAsset(mp);AssetDatabase.CreateAsset(copy,mp);go.AddComponent<MeshFilter>().sharedMesh=copy;go.AddComponent<MeshRenderer>().sharedMaterials=f.GetComponent<Renderer>().sharedMaterials;}
 UnityEngine.Object.DestroyImmediate(root);root=bakedRoot;
''');p.write_text(s,encoding='utf-8')
