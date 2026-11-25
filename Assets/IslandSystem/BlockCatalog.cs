using System;
namespace IslandSystem {
public sealed class BlockDefinition {
 public readonly string name,description;public readonly int category;
 public BlockDefinition(string title,int group,string help){name=title;category=group;description=help;}
}
public static class BlockCatalog {
 // Stable array indices are the V1 save-format instruction IDs. Append new IDs; never reorder.
 public static readonly BlockDefinition[] All={
 new BlockDefinition("当程序启动",0,"程序唯一入口，放在已连接指令的最上方。"),
 new BlockDefinition("前进一步 · 演示",1,"只在文本预览中记录前进一步；尚未连接角色移动。"),
 new BlockDefinition("重复",2,"将指令放进凹槽，按顺序重复1–20次；次数必须为整数。"),
 new BlockDefinition("等待（秒）",2,"等待1–60秒；真实执行时暂停当前程序，不改变世界时间倍率。"),
 new BlockDefinition("如果",2,"把六边形条件放入接口，条件为真时执行凹槽内的指令。"),
 new BlockDefinition("前方可通行 · 模拟",3,"六边形模拟侦测；点击真/假编辑预览值，不读取真实世界。"),
 new BlockDefinition("数值比较",4,"六边形条件：左侧数值大于右侧数值时为真。只能放入如果条件接口。"),
 new BlockDefinition("采集 · 待接入",5,"荒岛专属占位指令；尚未接入采集能力，不会改动背包或世界。"),
 new BlockDefinition("跳跃",1,"垂直起跳，等待落地后才执行下一条指令。"),
 new BlockDefinition("砍伐",5,"面向1.8米内的练习树挥斧；3次命中砍倒并掉落木材，不修改背包。"),
 new BlockDefinition("向前移动（米）",1,"沿角色朝向移动，可填负数后退；遇障碍停止，不穿墙。"),
 new BlockDefinition("右转（度）",1,"正数向右、负数向左转动角色。"),
 new BlockDefinition("向前跳",5,"沿角色朝向跃过低矮障碍，落地后继续。"),
 new BlockDefinition("移动 2 步",1,"沿当前朝向走两步。")};
}
}

