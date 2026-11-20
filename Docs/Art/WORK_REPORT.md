# 免费 3D 美术资源库 V1 执行与验收报告

日期：2026-11-19。项目位置：D:/codex项目/teach。

## 本轮结果

已实际创建 Unity 6000.6.9f1 工程，沿用新工程的 Built-in 渲染管线，无新增第三方插件、无付费内容。后台创建与导入成功，最终编辑器退出码 0；无需用户额外购买或激活其他版本。全程后台下载、文件处理与隐藏编辑器渲染，未控制鼠标键盘、未操作用户窗口。原 Godot 工程和素材源目录未修改。

九个指定来源均已有可使用内容，共 640 个不重复 FBX：

| 资源包 | Unity FBX 数 | 取得方式与状态 |
|---|---:|---|
| Stylized Nature MegaKit | 68 | 官方 itch.io Standard 原始 ZIP，免费版 68/116 |
| Pirate Kit | 72 | Kenney 官方免费接口下载成功，已解决最初缺少正常网页会话导致的拒绝 |
| Watercraft Kit | 46 | 使用用户提供 C:/Users/HT/Downloads/kenney_watercraft-pack.zip，原始包标记 2.1 |
| Universal Base Characters | 10 | 官方 Standard：男女全身角色与 8 个发型/眉毛 |
| Universal Animation Library 第一代 | 2 | 本地复制原地和 Root Motion FBX，各 43 动作；没有重新下载 |
| Ultimate Modular Ruins | 50 | 官方 Drive 已下载部分 FBX、两张纹理和 LICENSE；尚不完整 |
| KayKit Forest Nature | 105 | 官网导向作者 itch.io 免费 1.0 包 |
| KayKit Resource Bits | 76 | 作者 itch.io 免费 1.0 包 |
| KayKit Dungeon Remastered | 211 | 官网链接实际跳转作者 Dungeon Pack，新版免费 1.1；名称变化已记录 |

同一 FBX 中的动作数不计为额外模型。跨资源包完全相同的模型/贴图按 SHA256 去重，引用位置见 FILE_MANIFEST.json。

## 动画复用的证据

原始位置：`D:/codex项目/3D/素材/Universal Animation Library[Standard]`。

保留原始 README、LICENSE、Unity/Godot 使用图以及 Unity 和 Unreal-Godot 文件。Unity 实际只导入第一代 `UAL1_Standard.fbx` 和 `UAL1_Standard_RM.fbx`；原位置未改，没有使用第二代。

两份 FBX 均成功导入，各解析 43 个动作；Avatar 为有效 Humanoid。男女基础角色 Avatar 同样有效。已将 Idle_Loop、Walk_Loop、Sprint_Loop 重定向到男性基础角色，生成真实姿态截图与可在 Play 模式循环的独立预览 Controller。没有修改骨骼结构。ANIMATION_CLIPS.txt、ANIMATION_PREVIEW.tsv 保存证据。尚未逐帧验收全部动作、女性全部动作或游戏移动表现，不能声称全动画正式验收。

## 可直接打开的场景

- `Assets/ArtPreview/Scenes/ArtLibraryPreview.unity`：八个分类区域，实际展示所有已导入模型，保持模型导入尺寸；另有动画重定向预览台。
- `Assets/Scenes/ArtTest/BeachForest_ArtTest.unity`：一个连续场景、一张连续地表 MeshCollider，沙滩逐渐进入草地和泥土小路；海水、棕榈、树木、花草、岩石、船残骸、划艇、木箱、木桶与木板均实际摆放。

这是美术组合验证场景，水面为简洁静态材质；没有开发角色控制、任务、背包、战斗、存档、剧情或完整荒岛。海岸外轮廓与远景仍是测试片段边界，不能当作完整关卡。

## 实际检查与修复

- 最终 640 个 FBX 均加载到 Renderer，未出现空网格对象；空材质/错误 shader 检查为 0。非贴图纯色材质如实计为 untextured_material_slots，并未误判成丢贴图。
- 修复 Quaternius 叶片原始材质在 Built-in 中的黑色透明边框：保留原始贴图，创建独立 Cutout Material 并通过导入器 remap 引用；原 FBX 字节未改。
- 降低展示材质光泽；保留各来源主要颜色与贴图，没有将全场换成统一颜色。
- 修复早期将扁平石路按高度缩放造成巨石的问题：正式测试改用 Rock_Medium，按比例摆放。
- 修复地表 UV 缓存造成的重复色带；最终沙地、草地与小路使用一张连续混合颜色图。
- 路线从沙滩到森林连续采样 151 次，全部命中同一地表碰撞；每 0.48 米采样最大高度差约 0.041 米，无场景切换。该检查验证地表连续性，不替代完整角色胶囊通行测试。
- 保留全部 9 份 ZIP（7 份原始来源压缩包、1 份官方 Ruins 逐文件下载后的本地封装、1 份本地动画复用封装），ZIP CRC 均通过。没有捏造并不存在的官方 Ruins ZIP 或动画原始下载包。
- `Docs/Art/Previews` 有 13 张 Unity Camera.Render 实际场景截图，包含全库、八分类、连续环境三个视角与动画预览。已查看代表性环境、分类和动画结果。
- 仍有一条 Unity 6.6 API 弃用提示（FindFirstObjectByType），不影响编译和运行；无编译错误。最终日志为 `ArtSource/build-art-delivery.log`。

## 授权与目录

各包本地 LICENSE 明确 CC0，允许个人、教育及商业使用，无强制署名。付费 PRO/SOURCE/EXTRA 未获取。完整版本限制、作者、原始授权路径和使用范围见 ASSET_LICENSES.md 与 ASSET_CATALOG.md。原始压缩包、原文件与运行时内容分别在 ArtSource/Archives、ArtSource/OriginalFiles、Assets/Art/External。统一规范为 ART_GUIDELINES.md。

逐文件路径与 SHA256：FILE_MANIFEST.json。压缩包 CRC 与 SHA256：ARCHIVE_MANIFEST.json。逐模型实际尺寸、三角面、材质槽、Avatar 和动作数量：UNITY_IMPORT_AUDIT.tsv。连续场景验证：SCENE_VALIDATION.txt。

## 尚未满足的项与后续选材

1. **Ruins 包尚不完整**：官方公开 Drive 文件夹当前后台页面只能枚举首批 50 个 FBX，已保存成功文件及原始页面；官网介绍约 90 模型。没有将部分下载标成完整，也没有使用第三方来源补齐。现有拱门、柱、地板、木箱、宝箱可用，其余需补取。
2. V1 已证明这些免费素材可以组合出基本协调的明亮卡通海滩森林片段，但尚未达到正式游戏整岛美术完成度。地形、岸线、远景、水体动态和生态细节仍需专门制作。
3. 推荐后续继续使用 CommonTree、Rock_Medium、花草、Kenney 棕榈/残骸/箱桶，以及 KayKit 资源道具和石材组件。不要把针叶林、枯树、现代大船和竞速构件大量混入热带主场景。
4. Superhero 角色适合验证骨骼，不建议作为最终学生冒险主角。还缺儿童/青少年探险角色、正式灯塔、自然洞穴与悬崖、河流/瀑布和更多热带植被。
5. 全库已完成导入与分类展示；逐模型精细人工审核、全部动画逐帧审查、目标设备性能/LOD/碰撞和正式主角尺度下的通行验证仍未完成。不把自动检查通过等同正式生产批准。

## 打开方式

用已安装的 Unity 6000.6.9f1 打开本项目目录，双击上述任一 `.unity` 文件。资源预览场景可自由查看各类别；进入 Play 可看动画预览循环。美术测试场景只有环境与相机，没有正式玩家移动控制，这是本轮范围要求。
