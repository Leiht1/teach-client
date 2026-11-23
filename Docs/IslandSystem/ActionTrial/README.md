# 灯塔地图 · 真实积木动作试玩

2026-11-23。继续使用现有灯塔海岸，不新增场景、不重新生成地图。

## 奔跑与跳跃修复（2026-11-23）

- 在独立的 `Animation/UAL1_Action.fbx` 导入副本中显式开启待机、行走、奔跑和空中动画循环；落地动画仍为单次播放。原素材库文件未改动。
- 跳跃与落地动画采用脚底高度基准，烘焙根部高度，由 CharacterController 负责真实垂直位移，避免动画把可见模型压进地面。
- 落地计数改为本次移动产生向下碰撞接触后才确认；上方碰撞会结束上升速度。
- 后台针对性检查：持续奔跑4秒，动画累计约6个周期且小腿旋转持续变化；6次起跳全部落地。按渲染网格顶点采样，脚底最低距碰撞地面约0.055米，控制器原点最低约0.080米，未穿地。结果见 `LocomotionReview.txt`，截图见 `06-Sprint-Loop.png`、`07-Jump-Clearance.png`、`08-Landing-Clearance.png`。
- 原有真实积木执行链、砍伐、移动、转向、开关界面输入恢复已重新通过后台 Play Mode 回归。

## 奔跑朝向修复（2026-11-23）

奔跑导入时采用身体朝向计算根方向，造成可见步态相对移动方向平均偏右约20.33度。现在只对 Sprint_Loop 保留原始根轴，并将该素材的反向根轴对齐到角色 +Z；移动控制与其他动作朝向不变。

后台对24个周期相位采样，并分别检查角色朝向0/90/180/270度，骨盆平均前向与角色前向误差均小于0.08度。报告：`SprintHeadingReview.txt`；实际游戏相机截图：`09-Sprint-Aligned.png`。持续奔跑与6次跳跃的回归仍由 `LocomotionReview.txt` 记录。

## 试玩入口

双击 `Builds/IslandScratchTrial/试玩.cmd`。Unity 中打开原有 `Assets/Scenes/Lighthouse_Coast_ArtBenchmark_V01.unity` 点击 Play 也会自动接入。

点击游戏画面后鼠标控制视角；滚轮调节视距。WASD 是镜头相对的四向移动，Shift 加速，Space 跳跃，E 砍伐。Esc 释放鼠标并停止程序。Tab 或右上角系统按钮进入原生 Scratch 工作台；Tab / Esc / × 关闭后恢复角色输入。

## 四个真实动作

| 积木 | 行为与范围 |
| --- | --- |
| 跳跃（蓝） | CharacterController 真实起跳，等待落地后继续。循环3次就是3次完整起跳和落地。 |
| 砍伐（粉） | 面向1.8米内的练习树，播放素材库砍树动作并在挥斧时命中。3次命中树倒下，掉落3件场景木材。 |
| 向前移动（蓝） | 沿角色朝向移动，-15～15米；负数后退，0非法。碰撞由同一角色控制器处理，遇阻超时停止。 |
| 右转（蓝） | -360～360度；正数右转、负数左转。 |

最简测试：黄色“当程序启动”下接橙色“重复执行3次”，凹槽里放蓝色“跳跃”，点击“运行角色动作”。默认不替玩家放置完整程序。

出生点前方及两侧有3棵带进度标记的练习树，可用 E 或粉色积木砍伐。练习树倒下20秒后自动复位，并清理这轮掉落物，方便反复测试。原地图其他树木未改成资源系统。掉落物只在本次场景中存在，未连接拾取、背包和世界存档。

事件、顺序、循环、等待和数值比较条件由独立执行器解释。模拟通行侦测、旧演示移动和待接入采集只能预览，真实运行会指出对应积木。展开最多400条、预计运行最多180秒，运行时也有限额；动作失败会停止并释放控制。重新打开工作台会取消正在运行的程序，防止后台继续移动。

## 文件与接入方式

- `Assets/IslandSystem/ActionDemo/ActionSceneSetup.cs`：只在原灯塔场景加载后挂接现有 Explorer，放置可砍伐练习目标。没有保存或覆盖场景。
- `ActionMotor.cs`：手动与程序共用的移动、跳跃、转向、砍伐和相机控制。打开工作台屏蔽手动输入，物理重力继续。
- `ActionProgramRunner.cs`：执行树结构、参数及运行预算检查、取消和错误恢复，实现 `IProgramExecutor`。
- `ChopTarget.cs`：练习树的命中、倒下、木材生成和复位。
- `ActionLocomotion.controller`、`PracticeTree.prefab`、`Axe.prefab`、`DroppedWood.prefab`：独立可复用资源；`Resources/IslandActionSetup.prefab` 保存接入引用。
- `Assets/IslandSystem/Editor/ActionDemoBuild.cs`：资源构建、原灯塔场景试玩打包。
- `ProgramModel.cs`、`BlockCatalog.cs`、`ProgramValidator.cs`：稳定指令ID追加8～11，旧ID不改，旧草稿保留。
- `ScratchWorkbenchHost.cs` 与 `Tools/IslandScratchWeb/src/`：原工作台增加真实动作积木、运行请求和输入恢复；保持官方拖拽核心。
- `ActionReview.cs` 与 `Tools/IslandScratchWeb/action-review.mjs`：后台真实动作联调；`ScratchWindowReview.cs` 补充新控制器打开/关闭的输入状态检查。

## 素材来源

| 内容 | 来源与许可 |
| --- | --- |
| 角色 | 项目原有 Explorer / Quaternius Superhero_Male_FullBody，不替换角色资产。 |
| 待机、走路、跑步、空中和落地 | 项目原有 Quaternius Universal Animation Library 1，CC0。 |
| TreeChopping_Loop | 从本机 `D:/codex项目/3D/素材/Universal Animation Library 2[Standard]/Unity/UAL2_Standard.fbx` 接入的非根运动版本，Quaternius，CC0；包内许可保存在 `Animation/UAL2_License.txt`。不依赖另一项目的运行环境。 |
| 练习树 | 项目原有 Quaternius Stylized Nature MegaKit / CommonTree_1，CC0。 |
| 木材 | 项目原有 KayKit ResourceBits / Wood_Log_A，CC0。 |
| 手斧 | 本轮本地制作的简单工具模型及材质；没有引入未授权的角色或武器素材。 |
| 积木 | 已接入的 Scratch Blocks 2.2.13 官方核心；许可沿用 StreamingAssets 内授权文件。 |

## 验证与截图

- `RuntimeReview.txt`：后台 Unity Play Mode 与真实本机HTTP桥接结果，包括循环3次跳跃/落地、3次砍伐与倒树掉落、转90度后移动2米、横向移动和加速、镜头参数、输入恢复及取消。
- `WebReview.txt`：原生 Scratch 拖动、嵌套、参数、复制删除、撤销重做、保存与窗口适配回归。
- `PlayerWindowReview.txt`：隐藏的实际 Windows 游戏窗口与 WebView2 嵌入、开关及键盘焦点回归。
- `01-Lighthouse-Play.png`、`02-Jump.png`、`03-Chop.png`、`04-Tree-Felled.png` 是游戏相机直接渲染截图；`05-Real-Blocks.png` 为实际运行中的官方积木工作区。

自动测试不操作桌面，不注入系统键鼠，不捕获鼠标。运动测试通过同一控制器接口输入方向，相机直接离屏渲染；实际键盘和鼠标手感留给用户试玩检查。手动试玩中只有点击画面后才锁定鼠标，可随时按 Esc 释放。
