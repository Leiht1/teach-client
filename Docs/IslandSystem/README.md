# 荒岛智能系统 · Scratch 官方积木版

2026-11-23。正常 Windows 游戏及 Unity Editor Play 入口改为使用 **Scratch Blocks 2.2.13 官方核心**。积木轮廓、C 形凹槽、六边形条件、参数输入、整串拖动、吸附预览、嵌套、原生右键复制/删除以及撤销重做均来自官方库；外围保留荒岛系统的暖色工作台。没有复制整个 Scratch 网站，也没有接入 Scratch VM。

## 灯塔地图真实动作试玩（2026-11-23）

当前已接入真实执行器。仍然打开原有 `Lighthouse_Coast_ArtBenchmark_V01`，没有另建试玩场景，也没有改写灯塔场景文件或地形资源。详见 [动作试玩说明](ActionTrial/README.md)。

- 新增蓝色跳跃、向前移动（米）、右转（度）和粉色砍伐四种真实动作。点击“运行角色动作”执行；原“预览 [F5]”仍只输出文本。
- WASD 按镜头方向移动；Shift 加速；Space 跳跃；E 手动砍伐；滚轮调视距。点击游戏画面进入鼠标视角控制，Esc 释放并停止程序，Tab 打开编程界面。
- 原有演示移动、模拟侦测、待接入采集仍明确标注；真实执行会拒绝这些占位指令。

## 试玩

双击 `Builds/IslandScratchTrial/IslandScratchTrial.exe`。建议使用窗口模式、1280×720。Tab 或右上角“系统”打开；Tab / Esc / × 关闭。左侧六类积木共用原生连续指令库，滚轮缩放工作区，拖动空白平移。右键复制会连同下方积木及内部凹槽复制；删除单块会保留其外部后续指令。两种模式共用同一工作区。

界面通过本机 WebView2 子窗口嵌入 Unity，不打开浏览器标签页，不连接外网服务。要求 Windows x64、.NET Framework 4.6.2+ 与 Microsoft Edge WebView2 Runtime。本机已具备并通过隐藏窗口初始化测试。当前没有 macOS、Linux 或移动端宿主；其他平台仍保留旧原型回退。Unity Editor 的嵌入入口已接入，自动验证覆盖后台 Play Mode 与 Windows 宿主初始化；编辑器停靠布局、不同显示器 DPI 的实际前台使用尚需人工体验。

## 功能与边界

- 官方形状及拖拽，不再使用手绘 IMGUI 积木替代正常 Windows 编辑器。
- 启动事件；演示动作；重复、等待、如果；模拟通行侦测；双参数数值大于比较；粉色采集占位积木。
- 数字输入和下拉选择、整串复制、删除、撤销/重做、缩放/平移、模式切换、教学提示。
- 独立 v2 自动草稿，保存完整原生工作区、断开片段、嵌套、参数、位置、视口、模式、提示进度及程序名。
- Unity 检查启动入口、断开片段、空控制槽、条件兼容性、有限数值、循环 1–20、等待 1–60 和安全嵌套上限；错误按钮定位对应原生积木。
- 预览按控制结构展开，最多 400 条。比较支持左右两个参数。文本预览不会操作世界；另有 `ActionProgramRunner : IProgramExecutor` 负责灯塔地图的真实动作。只对练习树产生砍伐结果，不改背包或世界存档。
- 灯塔地图运行时由新 `ActionMotor` 接管原角色，旧 `LighthouseWalker` 源码及场景引用保留。打开系统取消当前程序并屏蔽新控制器的手动输入，保留重力；关闭在原生窗口隐藏确认后恢复输入与键盘焦点，不改变 timeScale。其他原型场景仍恢复其原有 walker 状态。嵌入窗口获得焦点期间临时允许 Unity 后台更新，确保保存与关闭请求响应；关闭时恢复原有 runInBackground 设置。

## 保存

实际路径：`Application.persistentDataPath/island-scratch-v2.json`，`version=2`。包含原生 workspace JSON 与独立的 v1 执行图 JSON。写入临时文件后原子替换，保留 `.bak`，失败明确显示。第一次打开若没有 v2，会读取原有 `island-program-v1.json` 并迁移，**原文件保留**。不能读取的草稿提示恢复失败，不当成空程序成功恢复。

## 源码与重建

| 位置 | 用途 |
| --- | --- |
| `Tools/IslandScratchWeb/src/` | 游戏工作台、原生积木定义、保存及执行图适配 |
| `Tools/IslandScratchWeb/package-lock.json` | 精确固定依赖 |
| `Tools/IslandScratchWeb/build.mjs` | 离线 bundle、媒体及许可打包 |
| `Tools/IslandScratchHost/Host.cs` | Windows WebView2 子窗口宿主 |
| `Tools/IslandScratchHost/build.ps1` | 固定 SDK 版本与宿主重建 |
| `Assets/StreamingAssets/IslandSystem/` | 可直接随 Unity 打包的网页、媒体、授权、宿主 |
| `Assets/IslandSystem/ScratchWorkbenchHost.cs` | 本机随机端口及随机令牌桥接、独立存档、输入屏蔽、检查和预览 |
| `Assets/IslandSystem/ProgramModel.cs` | 独立程序图及双参数比较的兼容扩展 |
| `Assets/IslandSystem/ProgramValidator.cs` | 执行图检查 |
| `Assets/IslandSystem/Workbench.cs` | 最小入口切换；原型回退及执行接口 |
| `Assets/IslandSystem/Editor/ScratchPlayerBuild.cs` | 独立新版试玩构建，不覆盖旧试玩包 |
| `Assets/IslandSystem/ScratchBridgeReview.cs` | 后台 Play Mode 联调 |

重建网页：`npm ci --prefix Tools/IslandScratchWeb`，然后 `npm run build --prefix Tools/IslandScratchWeb`。重建宿主：PowerShell 执行 `Tools/IslandScratchHost/build.ps1`。原生依赖已随 StreamingAssets 放入，日常运行 Unity 不要求 Node 或网络。宿主使用固定 Microsoft.Web.WebView2 SDK 1.0.3537.50。

动作资源生成入口：`-batchmode -quit -executeMethod IslandSystem.ActionDemoBuild.Prepare`（资源已生成，无需日常重复执行）。试玩构建入口：`-batchmode -quit -executeMethod IslandSystem.ActionDemoBuild.Build`。编辑器应使用项目版本 6000.6.9f1。不要更改世界场景或 PlayerSettings。

## 验证与截图

- `Scratch/WebReview.txt`：官方工作区后台浏览器操作测试。包含真正的虚拟页面指针拖入、吸附、嵌套、整串移动、六边形连接、参数输入、下拉菜单、右键说明/删除、撤销重做、缩放/平移及 960×540 / 1600×900 布局。
- `Scratch/UnityBridgeReview.txt`：实际 Unity Play Mode 联调，网页调用真实检查器、预览和磁盘保存，验证迁移、刷新恢复、错误定位、关闭恢复输入及时间倍率不变。
- `Scratch/NativeReview.txt`：隐藏 Windows WebView2 宿主成功加载官方工作区并直接生成自身内容快照。
- 截图见 `Scratch/01-Free.png`、`02-Nested-Guide.png`、`03-960.png`、`03-1600.png`、`04-Unity-Preview.png`、`05-Unity-Validation.png`、`06-Conditional.png`。它们是后台网页/原生内容渲染，不是桌面截屏。未模拟实体键鼠或捕获系统鼠标。

所有原有已跟踪的地图、地形、场景、资源、角色与采集文件均未修改。现有未提交及 `_Recovery` 文件保留。

## 开源来源

- [Scratch Blocks 官方仓库](https://github.com/scratchfoundation/scratch-blocks)，Apache-2.0，固定 2.2.13。
- [Blockly](https://github.com/RaspberryPiFoundation/blockly)，Apache-2.0，固定 12.5.1。
- continuous-toolbox 7.0.9、field-colour 6.0.12，Apache-2.0。
- Microsoft WebView2 SDK 1.0.3537.50，原始再分发许可随 Host 目录提供。

许可正文、原始商标政策、版本及说明位于 `Assets/StreamingAssets/IslandSystem/Web/licenses/`。本项目是独立游戏 UI，不使用 Scratch 标志或猫角色，不表示 Scratch 官方认可。上游渲染和拖拽代码未修改。


## 2026-11-23 · Tab 后灰屏修复

修复 Windows Player 将已是客户区坐标的面板位置再次按桌面坐标转换的问题；改用实际游戏窗口客户区像素计算居中位置。WebView2 窗体改为真正的 WinForms 子控件，避免首次 Show 继续按独立顶层窗口定位。Player 的定位和显示同步放在 Update 中，不再依赖窗口被遮挡时可能跳过的 OnGUI 渲染回调；页面完成 ready 后才显示，等待期间提供加载提示和 Esc 取消。

新增真实 Windows Player 的隐藏父窗口回归：`Tools/IslandScratchHost/review-window.ps1`。父窗口始终隐藏，真实 Unity Player 和 WebView2 都作为其后代运行；不激活窗口、不模拟系统键鼠、不截取桌面。只通过开发版测试入口调用相同开关路径并读取本应用窗口几何信息。

通过结果在 `Scratch/PlayerWindowReview.txt`：1280×720、960×540、1600×900 下，验证子面板实际客户区坐标与尺寸、WS_VISIBLE 显示状态、关闭和重新打开。另从实际嵌入的 WebView2 直接抓取其内容并人工检查，见 `Scratch/07-Embedded-Player.png`。这弥补了上一版仅分别验证网页和宿主初始化、没有验证最终 Player 窗口嵌入的缺口。测试入口仅在开发版且显式传入 `-scratch-window-review` 时启用，正常试玩不自动打开界面或写测试草稿。

## 2026-11-23 · 关闭编程界面后人物无法移动

修复原生 WebView 隐藏后只激活游戏窗口、未明确恢复游戏键盘焦点的问题。宿主隐藏完成后通过标准输出发送带序号的 HIDDEN 确认；Unity 维持更新直到确认到达，在自己的主线程归还键盘焦点、恢复原先启用的角色输入组件及原有 runInBackground / 光标状态。关闭过程包含两秒兜底，组件销毁时也恢复输入；快速重新打开会忽略上一次过期的关闭确认。

真实隐藏 Player 回归增加：先将键盘焦点放入本应用的隐藏编程子窗口，分别通过直接关闭和网页 RPC 关闭，检查焦点 HWND 回到 Unity，角色 enabled 状态与原状态一致，runInBackground 和 timeScale 恢复/保持。整个窗口树隐藏，未注入系统键鼠事件，桌面前台窗口未改变。结果在 `Scratch/PlayerWindowReview.txt`。此测试验证原生焦点及输入恢复，不伪称进行了实体键盘行走测试。没有修改角色移动、物理、碰撞或地图文件。
