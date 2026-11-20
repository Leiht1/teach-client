# 本轮资产清单与来源

| 资产 | 实际来源 | 使用方式与许可依据 |
|---|---|---|
| 分层悬崖、草冠、草沿、海边岩块 | 本轮新建 Blender 网格；`ArtSource/Lighthouse_Authored/Lighthouse_Coast_Authored.blend` | 独立模型，固定布局；FBX 和预制体保存在 `Assets/Lighthouse_Authored` |
| 沙滩、草台、山路及支撑、石平台 | 同上 | 独立地表网格；草沙道路有顶点混合 |
| 灯塔、灯室、栏杆、门窗、附属小屋 | 同上 | 原创几何组合，非外部成品灯塔 |
| 弯曲棕榈 | 同上 | 连续树干和曲面叶片，不再使用 Kenney 棕榈 |
| 四分区手绘地表图集 | 本轮通过图像生成工具制作；`Assets/Lighthouse_Authored/PaintedSurfaceAtlas.png` | 包含沙地、草地、岩石、铺石。AI 生成纹理，不是照片采样，也不是下载的第三方贴图 |
| 阔叶树 `CommonTree_1`、`CommonTree_2` | [Quaternius Stylized Nature MegaKit](https://quaternius.com/packs/stylizednaturemegakit.html) | 已有项目资源，免费标准版；本地许可 `ArtSource/Licenses/StylizedNatureMegaKit/License_Standard.txt`，CC0 1.0 |
| 草花 `Bush_Common_Flowers`、`Grass_Common_Short`、`Grass_Wispy_Short` | 同上 | 同上；16 个预先指定位置的簇，每簇三种模型 |
| 尺度测试角色 `Superhero_Male_FullBody` | [Quaternius Universal Base Characters](https://quaternius.com/packs/universalbasecharacters.html) | 已有项目资源；许可 `ArtSource/Licenses/UniversalBaseCharacters/Universal Base Characters[Standard]/License_Standard.txt`，CC0。测试角色不是最终角色交付 |
| 待机与行走动画 | [Quaternius Universal Animation Library](https://quaternius.com/packs/universalanimationlibrary.html) | 使用 `UAL1_Standard.fbx` 中 Idle_Loop、Walk_Loop；本地许可 `ArtSource/Licenses/UniversalAnimationLibrary/License.txt`，CC0 |
| 地表、水面、天空材质 | 本轮编写的 Unity Built-in 着色器 | 保存在 `Assets/Lighthouse_Authored`；非照片级 PBR 素材 |

本轮优先复用了已经导入、附带许可的资源。没有下载新的外部模型，也没有购买付费资产。外部资产未统一宣称为本轮原创。

## 资源缺口记录

- 当前没有现成且完全匹配参考图的高质量卡通灯塔和层叠悬崖，因此本轮制作专用几何资产。
- 原先候选球形树冠及粗折线棕榈近景细节不足：球形阔叶树换成已有叶片树；棕榈改为本轮曲面叶片模型。
- 没有最终游戏角色；保留第三方测试角色作为尺度与通行检查工具。
- 本轮自制模块没有配套多级 LOD；没有声称完成移动端或主机性能认证。

未选入的外部资源不是本场景依赖；项目旧版本资源不属于本次资产清单。

迁移快照更新：基础灌木已统一改用 Quaternius `Bush_Common_Flowers.fbx`（16 处），另有上述 16 处草花簇；KayKit 球形灌木和 Kenney 棕榈已不用于当前场景。
