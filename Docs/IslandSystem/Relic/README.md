# 随身遗物核心 · 系统入口

使用 `Builds/IslandRelicTrial/试玩.cmd` 启动独立试玩；场景仍为现有灯塔海岸。

## 使用

- 左前臂外侧佩戴实体遗物。鼠标未锁定时，移到装置上可看到青蓝轮廓；点击打开编程系统。
- Tab 或右上角同造型的缩略入口也可打开。鼠标已经控制视角时，先按 Esc 释放鼠标，再点击装置。
- 打开时抬起左臂，晶体浮起，四片外环展开，投影亮起，再显示原有 Scratch 编程面板。关闭后装置收拢并恢复原先的输入与视角状态。
- 仍使用已有自由编程、任务引导、草稿和动作执行功能。

## 资产与代码

本次遗物模型为项目自制，无新增第三方美术依赖。参考图用于外观方向，当前角色模型保持项目现状。

| 文件 | 用途 |
| --- | --- |
| ArtSource/RelicCore/RelicCore.blend | 可继续编辑的三维源文件 |
| ArtSource/RelicCore/build_relic.py | 模型构建源：护框、皮腕带、晶体、四片可动外环、符纹 |
| Assets/IslandSystem/Relic/RelicCore.fbx | 游戏三维模型 |
| Assets/IslandSystem/Relic/Resources/RelicCore.prefab | 可复用装置预制体 |
| Assets/IslandSystem/Relic/*.mat | 象牙白、金色、皮革和晶体材质 |
| Assets/IslandSystem/Relic/RelicCore.cs | 左臂挂点、状态动画、悬停与射线点击、投影、模型渲染缩略图 |
| Assets/IslandSystem/Editor/RelicBuild.cs | 资源接入与独立试玩构建 |
| Assets/IslandSystem/Relic/RelicReview.cs | 后台场景验证和离屏截图 |
| Assets/IslandSystem/ScratchWorkbenchHost.cs | 最小增量：入口调用、等待展开、关闭回收后恢复输入 |
| Assets/IslandSystem/ActionDemo/ActionMotor.cs | 最小增量：点击遗物时不捕获视角鼠标 |
| Tools/IslandScratchHost/Host.cs | 原生面板显示时的短入场过渡 |
| Tools/IslandScratchHost/review-window.ps1 | 支持指定独立试玩文件进行后台验证 |

模型通过 Humanoid LeftLowerArm 挂点随角色运动；四个 Petal 节点和 Crystal 独立运动。HUD 使用相同预制体的透明背景离屏渲染，没有另外画普通按钮。装置碰撞盒仅用于射线交互，不参与角色物理碰撞。

## 验证与截图

`REVIEW.txt` 记录后台 Play Mode 的 16 项检查，包括实体点击、Tab、HUD、展开与收拢、快速重开、关闭后移动、时间与碰撞组件保持。

- 01-Wearing.png：现有角色左臂佩戴
- 02-Idle.png：待机近景
- 03-Hover.png：悬停轮廓
- 04-Activation.png：激活过程
- 05-Projection.png：展开后的三维装置和投影
- 06-HUD.png：游戏中的缩略入口
- 07-System.png：独立试玩中实际嵌入的编程面板内容

01–06 来自游戏离屏渲染；单独的三维投影截图不包含原生 WebView 子窗口，所以显示等待面板的提示。07 直接由试玩内嵌 WebView 导出。均未捕获桌面或注入系统鼠标键盘。

地图、地形、生态、区块、原有场景和编程编辑核心不在本次修改范围。

独立试玩原生窗口验证另见 `NATIVE-REVIEW.txt`：真实 Scratch 加载、1280×720 / 960×540 / 1600×900 的嵌入位置、两条关闭路径的焦点恢复均通过。试玩构建成功时间为 2026-11-23。

## 佩戴与入场修正

关闭了画面的移动、视角、跳跃等常驻操作说明、调试计数，以及遗物入口的文字提示。左臂挂点在角色实际待机姿势就绪后建立，装置朝向左小臂远离身体的一侧，沿前臂贴合；激活视角从角色视线附近俯看自己的腕部。原生面板先准备透明且暂停的动画首帧，再显示并播放一次入场，避免完整界面先闪现。

本次修正复验：16 项场景检查、原生窗口与关闭焦点恢复检查通过；`ENTRANCE-REVIEW.txt` 验证实际入口脚本在显示前准备透明暂停首帧，连续打开三次每次只有一个入场动画。
