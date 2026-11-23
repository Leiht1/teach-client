using System;
using System.Collections.Generic;
using System.Globalization;
namespace IslandSystem {
public sealed class ProgramIssue {public string blockId,message;public ProgramIssue(Block b,string text){blockId=b==null?null:b.id;message=text;}}
public static class ProgramValidator {
public static List<ProgramIssue> Validate(Draft d){var errors=new List<ProgramIssue>();Action<Block,string> fail=(b,s)=>errors.Add(new ProgramIssue(b,s));if(d.blocks.Count==0||d.blocks[0].kind!=0)fail(d.blocks.Count==0?null:d.blocks[0],"程序开头缺少启动事件");var seen=new HashSet<string>();Action<List<Block>,int> visit=null;visit=(list,depth)=>{foreach(var b in list){if(depth>8){fail(b,"嵌套最多8层");return;}if(!seen.Add(b.id)){fail(b,"积木重复连接或循环引用");return;}if(seen.Count>500){fail(b,"程序最多500个积木");return;}if(b.kind<0||b.kind>=BlockCatalog.All.Length){fail(b,"未知积木定义");continue;}float n;bool number=Number(b.value,out n);if(b.detached)fail(b,"积木连接断开，请吸附到启动程序");if(b.kind==0&&(depth>0||b!=d.blocks[0]))fail(b,"启动事件只能位于程序开头");if(b.kind==2&&(!number||n<1||n>20||n!=(int)n))fail(b,"重复次数须为1–20的整数");if(b.kind==10&&(!number||n==0||n< -15||n>15))fail(b,"移动距离须为 -15 到 15 米，且不能为0");if(b.kind==11&&(!number||n< -360||n>360))fail(b,"转向角度须为 -360 到 360 度");if(b.kind==3&&(!number||n<1||n>60))fail(b,"等待秒数须为1–60");if((b.kind==2||b.kind==4)&&b.children.Count==0)fail(b,"控制积木内部为空");if(b.kind==5||b.kind==6)fail(b,"条件积木只能连接到如果的六边形接口");if(b.kind!=2&&b.kind!=4&&b.children.Count>0)fail(b,"此积木不支持嵌套指令");if(b.kind!=4&&b.condition!=null)fail(b,"此积木不支持条件接口");if(b.kind==4){if(b.condition==null||(b.condition.kind!=5&&b.condition.kind!=6))fail(b,"如果缺少兼容的侦测或比较条件");else if(b.condition.kind==6&&(!Number(b.condition.value,out n)||!Number(b.condition.right,out n)))fail(b,"条件比较参数必须是有限数字");else if(b.condition.kind==5&&b.condition.value!="true"&&b.condition.value!="false")fail(b,"模拟侦测请选择真或假");}visit(b.children,depth+1);}};visit(d.blocks,0);return errors;}
public static bool Number(string text,out float n){return float.TryParse(text,NumberStyles.Float,CultureInfo.InvariantCulture,out n)&&!float.IsNaN(n)&&!float.IsInfinity(n);}
}
}

