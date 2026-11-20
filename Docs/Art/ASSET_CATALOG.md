# 美术资源登记表 V1

日期：2026-11-19。工程 Unity 6000.6.9f1 / Built-in。本库总计 640 个唯一 FBX；素材来自九个指定资源包（其中动画为本地复用，Watercraft 为用户已下载，Ruins 为部分下载）。

| 资源名称 | 作者 | 来源 | 授权 | 分类 | 状态 |
|---|---|---|---|---|---|
| Stylized Nature MegaKit | Quaternius | [官方](https://quaternius.com/packs/stylizednaturemegakit.html) | CC0 1.0 | 树木、灌木、草、花、蘑菇、岩石、石路 | 68 FBX 导入可见；已取得 |
| Ultimate Modular Ruins Pack | Quaternius | [官方](https://quaternius.com/packs/ultimatemodularruins.html) | CC0 1.0 | 石拱门、柱、门、地板、桥段、木箱、书架、宝箱、植被 | 50 FBX 导入可见；部分下载 |
| Universal Base Characters | Quaternius | [官方](https://quaternius.com/packs/universalbasecharacters.html) | CC0 1.0 | 角色、发型和眉毛 | 10 FBX 导入可见；已取得 |
| Universal Animation Library | Quaternius | [官方](https://quaternius.com/packs/universalanimationlibrary.html) | CC0 1.0 | 待机、行走、慢跑、冲刺、跳跃、交互、游泳等 | 2 FBX 导入可见；已取得 |
| Pirate Kit | Kenney | [官方](https://kenney.nl/assets/pirate-kit) | CC0 1.0 | 棕榈、残骸、船、木箱、桶、宝箱、船桅、栈板 | 72 FBX 导入可见；已取得 |
| Watercraft Kit | Kenney | [官方](https://kenney.nl/assets/watercraft-kit) | CC0 1.0 | 划艇、帆船、快艇、货船、浮标、运输零件 | 46 FBX 导入可见；已取得 |
| KayKit Forest Nature Pack | Kay Lousberg | [官方](https://kaylousberg.com/game-assets/forest-nature-pack) | CC0 1.0 | 多种树木、灌木、岩石、地表自然道具 | 105 FBX 导入可见；已取得 |
| KayKit Resource Bits | Kay Lousberg | [官方](https://kaylousberg.com/game-assets/resource-bits) | CC0 1.0 | 木材、石料、矿石、金属与资源道具，详见逐模型表 | 76 FBX 导入可见；已取得 |
| KayKit Dungeon Remastered | Kay Lousberg | [官方](https://kaylousberg.com/game-assets/dungeon-remastered) | CC0 1.0 | 墙、地板、门、楼梯、木箱、宝箱、陷阱等 | 211 FBX 导入可见；已取得 |

## 实际文件和审核

逐文件原始位置、Unity 位置、SHA256、字节数与去重映射见 [FILE_MANIFEST.json](FILE_MANIFEST.json)。逐模型尺寸、网格数量、三角形数、材质空引用、无主贴图槽数量、Humanoid 与动作数见 [UNITY_IMPORT_AUDIT.tsv](UNITY_IMPORT_AUDIT.tsv)。

**无主贴图槽不直接等于丢贴图**：纯色材质（特别是 Ruins、发型、动画人偶）可以不使用贴图。自动检查未发现空材质或错误 shader；分类截图和验证场景已实际生成，但不代表全部模型逐面、全部 43 动作和所有性能情形均已人工批准。

### Stylized Nature MegaKit
- 取得状态/版本：Standard，68/116 模型；不含 PRO 增量和 SOURCE 引擎工程/风动着色器。
- Unity 实际文件：`Assets/Art/External/Quaternius/StylizedNatureMegaKit`（68 个唯一 FBX；只导入 Unity 版，公共重复纹理按 SHA256 复用）。
- 原始文件：`ArtSource/OriginalFiles/StylizedNatureMegaKit`。
- 重要模型类别：树木、灌木、草、花、蘑菇、岩石、石路。
- 实际用途：海滩森林测试：树、花草、岩石。
- 导入：批处理真实导入、渲染；材质适配为独立 Material + 导入器映射，原 FBX 未改。
- 审核：通过分类可见性预览；选入测试场景的资产完成整体构图观察，其余仍为正式接入前候选。

### Ultimate Modular Ruins Pack
- 取得状态/版本：官方 Drive 当前取得 50 个 FBX（官网介绍约 90 模型），属于部分下载；未将本地打包文件冒称官方原始 ZIP。
- Unity 实际文件：`Assets/Art/External/Quaternius/UltimateModularRuins`（50 个唯一 FBX；只导入 Unity 版，公共重复纹理按 SHA256 复用）。
- 原始文件：`ArtSource/OriginalFiles/UltimateModularRuins`。
- 重要模型类别：石拱门、柱、门、地板、桥段、木箱、书架、宝箱、植被。
- 实际用途：分类预览；未接入海滩森林。
- 导入：批处理真实导入、渲染；材质适配为独立 Material + 导入器映射，原 FBX 未改。
- 审核：通过分类可见性预览；选入测试场景的资产完成整体构图观察，其余仍为正式接入前候选。

### Universal Base Characters
- 取得状态/版本：Standard；男女 Superhero 全身角色 + 8 个原点发型/眉毛；不含 SOURCE 工程与 rigged Blend。
- Unity 实际文件：`Assets/Art/External/Quaternius/UniversalBaseCharacters`（10 个唯一 FBX；只导入 Unity 版，公共重复纹理按 SHA256 复用）。
- 原始文件：`ArtSource/OriginalFiles/UniversalBaseCharacters`。
- 重要模型类别：角色、发型和眉毛。
- 实际用途：分类预览、第一代 UAL 动作重定向预览。
- 导入：批处理真实导入、渲染；材质适配为独立 Material + 导入器映射，原 FBX 未改。
- 审核：通过分类可见性预览；选入测试场景的资产完成整体构图观察，其余仍为正式接入前候选。

### Universal Animation Library
- 取得状态/版本：本地第一代 Standard，未下载；43 个动作各有原地及 Root Motion 两份 FBX。
- Unity 实际文件：`Assets/Art/External/Quaternius/UniversalAnimationLibrary`（2 个唯一 FBX；只导入 Unity 版，公共重复纹理按 SHA256 复用）。
- 原始文件：`ArtSource/OriginalFiles/Universal Animation Library[Standard]`。
- 重要模型类别：待机、行走、慢跑、冲刺、跳跃、交互、游泳等。
- 实际用途：动画展示区及角色重定向预览。
- 导入：批处理真实导入、渲染；材质适配为独立 Material + 导入器映射，原 FBX 未改。
- 审核：通过分类可见性预览；选入测试场景的资产完成整体构图观察，其余仍为正式接入前候选。

### Pirate Kit
- 取得状态/版本：官方免费包，版本以包内 License 为准；未获取 All-in-1 付费合集。
- Unity 实际文件：`Assets/Art/External/Kenney/PirateKit`（72 个唯一 FBX；只导入 Unity 版，公共重复纹理按 SHA256 复用）。
- 原始文件：`ArtSource/OriginalFiles/PirateKit`。
- 重要模型类别：棕榈、残骸、船、木箱、桶、宝箱、船桅、栈板。
- 实际用途：海滩森林：棕榈、船残骸、木箱、木桶、木板。
- 导入：批处理真实导入、渲染；材质适配为独立 Material + 导入器映射，原 FBX 未改。
- 审核：通过分类可见性预览；选入测试场景的资产完成整体构图观察，其余仍为正式接入前候选。

### Watercraft Kit
- 取得状态/版本：用户提供的 watercraft-pack ZIP，License 标记 2.1；未声称由本轮网络下载。
- Unity 实际文件：`Assets/Art/External/Kenney/WatercraftKit`（46 个唯一 FBX；只导入 Unity 版，公共重复纹理按 SHA256 复用）。
- 原始文件：`ArtSource/OriginalFiles/WatercraftKit`。
- 重要模型类别：划艇、帆船、快艇、货船、浮标、运输零件。
- 实际用途：海滩森林：小划艇；其余分类展示。
- 导入：批处理真实导入、渲染；材质适配为独立 Material + 导入器映射，原 FBX 未改。
- 审核：通过分类可见性预览；选入测试场景的资产完成整体构图观察，其余仍为正式接入前候选。

### KayKit Forest Nature Pack
- 取得状态/版本：1.0 FREE；仅通过无需付款的公开下载列表获取 Free，不含 EXTRA/SOURCE。
- Unity 实际文件：`Assets/Art/External/KayKit/ForestNature`（105 个唯一 FBX；只导入 Unity 版，公共重复纹理按 SHA256 复用）。
- 原始文件：`ArtSource/OriginalFiles/ForestNature`。
- 重要模型类别：多种树木、灌木、岩石、地表自然道具。
- 实际用途：分类展示；测试中的补充灌木花草。
- 导入：批处理真实导入、渲染；材质适配为独立 Material + 导入器映射，原 FBX 未改。
- 审核：通过分类可见性预览；选入测试场景的资产完成整体构图观察，其余仍为正式接入前候选。

### KayKit Resource Bits
- 取得状态/版本：1.0 FREE；不含付费扩展。
- Unity 实际文件：`Assets/Art/External/KayKit/ResourceBits`（76 个唯一 FBX；只导入 Unity 版，公共重复纹理按 SHA256 复用）。
- 原始文件：`ArtSource/OriginalFiles/ResourceBits`。
- 重要模型类别：木材、石料、矿石、金属与资源道具，详见逐模型表。
- 实际用途：资源展示区；未制作背包或采集系统。
- 导入：批处理真实导入、渲染；材质适配为独立 Material + 导入器映射，原 FBX 未改。
- 审核：通过分类可见性预览；选入测试场景的资产完成整体构图观察，其余仍为正式接入前候选。

### KayKit Dungeon Remastered
- 取得状态/版本：官网链接经作者跳转至 kaykit-dungeon-pack；实际包为 Dungeon Pack 1.1 FREE（2026-07-16），保留名称差异；不含 EXTRA/SOURCE。
- Unity 实际文件：`Assets/Art/External/KayKit/DungeonRemastered`（211 个唯一 FBX；只导入 Unity 版，公共重复纹理按 SHA256 复用）。
- 原始文件：`ArtSource/OriginalFiles/DungeonRemastered`。
- 重要模型类别：墙、地板、门、楼梯、木箱、宝箱、陷阱等。
- 实际用途：古代建筑分类展示；未制作地牢关卡。
- 导入：批处理真实导入、渲染；材质适配为独立 Material + 导入器映射，原 FBX 未改。
- 审核：通过分类可见性预览；选入测试场景的资产完成整体构图观察，其余仍为正式接入前候选。

## 推荐和保留问题

- 优先候选：Quaternius CommonTree、Rock_Medium、花草；Kenney 棕榈、残骸、木箱、木桶与划艇；KayKit 资源道具和地牢基础组件。
- 不推荐直接作为最终主角：Superhero 男女角色的体型与服装不符合学生荒岛主角定位，现阶段只验证骨骼与动作。
- 不建议在热带主场景大量使用针叶树、枯树、红叶树、现代大型货船或赛车组件；它们保留在库中供选择。
- Ruins 还缺后半部分文件，不能宣称完整下载。石材构件可作遗迹，但不能直接等同自然洞穴岩壁。
- 原始叶片材质曾出现透明区域黑边，已用独立 Cutout 材质适配；错误地将扁平 RockPath 按高度拉成巨石的问题已通过改用 Rock_Medium 修正。
- 当前库可支撑海滩、森林、遗迹的基础搭建；整岛仍缺自然洞穴/悬崖套件、瀑布与河流水体、正式灯塔、儿童探险主角、部分热带植被及完整地形与性能方案。
