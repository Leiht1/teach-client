using System;
using System.Collections.Generic;
using UnityEngine;
namespace IslandSystem {
// Runtime tree is independent from IMGUI. Disk records use IDs to avoid Unity recursive serialization limits.
public class Block { public string id=Guid.NewGuid().ToString(); public int kind; public string value="3"; public string right="5"; public bool detached; public Vector2 position; public Block condition; public List<Block> children=new List<Block>(); }
public class Draft { public int version=1; public string name="我的荒岛程序"; public List<Block> blocks=new List<Block>(); }
public static class ProgramCodec {
[Serializable] class Record {public string id,value,right,condition; public int kind;public bool detached;public Vector2 position;public List<string> children=new List<string>();}
[Serializable] class FileData {public int version=1;public string name;public List<string> roots=new List<string>();public List<Record> nodes=new List<Record>();}
public static string Write(Draft d){var f=new FileData{name=d.name,version=d.version};var seen=new HashSet<string>();Action<Block> add=null;add=b=>{if(seen.Count>=500)throw new Exception("程序最多500个积木");if(!seen.Add(b.id))throw new Exception("重复积木标识或循环连接");var r=new Record{id=b.id,kind=b.kind,value=b.value,right=b.right,detached=b.detached,position=b.position,condition=b.condition==null?null:b.condition.id};f.nodes.Add(r);if(b.condition!=null)add(b.condition);foreach(var c in b.children){r.children.Add(c.id);add(c);}};foreach(var b in d.blocks){f.roots.Add(b.id);add(b);}return JsonUtility.ToJson(f,true);}
public static Draft Read(string json){if(string.IsNullOrEmpty(json)||!json.Contains("\"nodes\"")||!json.Contains("\"roots\""))throw new Exception("草稿缺少积木图数据");var f=JsonUtility.FromJson<FileData>(json);if(f==null||f.version!=1||f.nodes==null||f.roots==null)throw new Exception("草稿格式或版本不支持");var map=new Dictionary<string,Block>();foreach(var r in f.nodes){if(r.kind<0||r.kind>=BlockCatalog.All.Length||string.IsNullOrEmpty(r.id)||map.ContainsKey(r.id))throw new Exception("无效积木定义");map.Add(r.id,new Block{id=r.id,kind=r.kind,value=r.value,right=r.right??"5",detached=r.detached,position=r.position});}foreach(var r in f.nodes){var b=map[r.id];if(!string.IsNullOrEmpty(r.condition))b.condition=map[r.condition];foreach(var id in r.children)b.children.Add(map[id]);}var d=new Draft{name=f.name};foreach(var id in f.roots)d.blocks.Add(map[id]);Write(d);return d;}
public static Block Clone(Block b){var d=new Draft();d.blocks.Add(b);return Read(Write(d)).blocks[0];}
}
}

