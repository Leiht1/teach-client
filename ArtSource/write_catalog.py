from pathlib import Path
import json,csv,hashlib,zipfile,collections
root=Path(__file__).resolve().parent.parent
manifest=json.loads((root/'Docs/Art/FILE_MANIFEST.json').read_text(encoding='utf8'))
info={
'StylizedNatureMegaKit':('Stylized Nature MegaKit','https://quaternius.com/packs/stylizednaturemegakit.html','Standard，68/116 模型；不含 PRO 增量和 SOURCE 引擎工程/风动着色器','树木、灌木、草、花、蘑菇、岩石、石路','海滩森林测试：树、花草、岩石'),
'UltimateModularRuins':('Ultimate Modular Ruins Pack','https://quaternius.com/packs/ultimatemodularruins.html','官方 Drive 当前取得 50 个 FBX（官网介绍约 90 模型），属于部分下载；未将本地打包文件冒称官方原始 ZIP','石拱门、柱、门、地板、桥段、木箱、书架、宝箱、植被','分类预览；未接入海滩森林'),
'UniversalBaseCharacters':('Universal Base Characters','https://quaternius.com/packs/universalbasecharacters.html','Standard；男女 Superhero 全身角色 + 8 个原点发型/眉毛；不含 SOURCE 工程与 rigged Blend','角色、发型和眉毛','分类预览、第一代 UAL 动作重定向预览'),
'UniversalAnimationLibrary':('Universal Animation Library','https://quaternius.com/packs/universalanimationlibrary.html','本地第一代 Standard，未下载；43 个动作各有原地及 Root Motion 两份 FBX','待机、行走、慢跑、冲刺、跳跃、交互、游泳等','动画展示区及角色重定向预览'),
'PirateKit':('Pirate Kit','https://kenney.nl/assets/pirate-kit','官方免费包，版本以包内 License 为准；未获取 All-in-1 付费合集','棕榈、残骸、船、木箱、桶、宝箱、船桅、栈板','海滩森林：棕榈、船残骸、木箱、木桶、木板'),
'WatercraftKit':('Watercraft Kit','https://kenney.nl/assets/watercraft-kit','用户提供的 watercraft-pack ZIP，License 标记 2.1；未声称由本轮网络下载','划艇、帆船、快艇、货船、浮标、运输零件','海滩森林：小划艇；其余分类展示'),
'ForestNature':('KayKit Forest Nature Pack','https://kaylousberg.com/game-assets/forest-nature-pack','1.0 FREE；仅通过无需付款的公开下载列表获取 Free，不含 EXTRA/SOURCE','多种树木、灌木、岩石、地表自然道具','分类展示；测试中的补充灌木花草'),
'ResourceBits':('KayKit Resource Bits','https://kaylousberg.com/game-assets/resource-bits','1.0 FREE；不含付费扩展','木材、石料、矿石、金属与资源道具，详见逐模型表','资源展示区；未制作背包或采集系统'),
'DungeonRemastered':('KayKit Dungeon Remastered','https://kaylousberg.com/game-assets/dungeon-remastered','官网链接经作者跳转至 kaykit-dungeon-pack；实际包为 Dungeon Pack 1.1 FREE（2026-07-16），保留名称差异；不含 EXTRA/SOURCE','墙、地板、门、楼梯、木箱、宝箱、陷阱等','古代建筑分类展示；未制作地牢关卡')}
audit=list(csv.DictReader((root/'Docs/Art/UNITY_IMPORT_AUDIT.tsv').open(encoding='utf8'),delimiter='\t'))
counts=collections.Counter(x['asset'].split('/')[4] for x in audit)
cat=['# 美术资源登记表 V1','', '日期：2026-11-19。工程 Unity 6000.6.9f1 / Built-in。本库总计 '+str(len(audit))+' 个唯一 FBX；素材来自九个指定资源包（其中动画为本地复用，Watercraft 为用户已下载，Ruins 为部分下载）。','', '| 资源名称 | 作者 | 来源 | 授权 | 分类 | 状态 |','|---|---|---|---|---|---|']
lic=['# 授权登记表 V1','','日期：2026-11-19。以下结论依据每包保留的原始 LICENSE。CC0 允许个人、教育与商业使用，无强制署名；作者支持/捐赠请求不等于付费使用门槛。未购买、未登录绕过、未下载付费专属包。授权不涵盖第三方商标或后续新增内容的独立权利，新增资源仍须逐包核对。','']
for r in manifest:
 key=r['name'];title,url,version,categories,usage=info[key];author='Kay Lousberg' if r['author']=='KayKit' else r['author']
 assets=f"Assets/Art/External/{r['author']}/{key}"
 licenses=[p for p in (root/'ArtSource/Licenses'/key).rglob('*') if p.is_file() and 'licen' in p.name.lower()]
 cat.append(f'| {title} | {author} | [官方]({url}) | CC0 1.0 | {categories} | {counts[key]} FBX 导入可见；'+('部分下载' if key=='UltimateModularRuins' else '已取得')+' |')
 cat.extend([])
 lic.extend([f'## {title}',f'- 作者/发布者：{author}。',f'- 官方页面：{url}',f'- 实际版本及免费限制：{version}。','- 授权：CC0 1.0；允许商业用途；无需署名，建议在致谢中自愿保留作者名。', '- 原始授权副本：'+ '；'.join('`'+str(p.relative_to(root)).replace('\\','/')+'`' for p in licenses),f'- 使用位置：`{assets}`。实际范围：{usage}。',''])
cat.extend(['','## 实际文件和审核','', '逐文件原始位置、Unity 位置、SHA256、字节数与去重映射见 [FILE_MANIFEST.json](FILE_MANIFEST.json)。逐模型尺寸、网格数量、三角形数、材质空引用、无主贴图槽数量、Humanoid 与动作数见 [UNITY_IMPORT_AUDIT.tsv](UNITY_IMPORT_AUDIT.tsv)。','', '**无主贴图槽不直接等于丢贴图**：纯色材质（特别是 Ruins、发型、动画人偶）可以不使用贴图。自动检查未发现空材质或错误 shader；分类截图和验证场景已实际生成，但不代表全部模型逐面、全部 43 动作和所有性能情形均已人工批准。',''])
for r in manifest:
 key=r['name'];title,url,version,categories,usage=info[key];assets=f"Assets/Art/External/{r['author']}/{key}"
 cat.extend([f'### {title}',f'- 取得状态/版本：{version}。',f'- Unity 实际文件：`{assets}`（{counts[key]} 个唯一 FBX；只导入 Unity 版，公共重复纹理按 SHA256 复用）。',f'- 原始文件：`{str(Path(r["source"]).relative_to(root)).replace(chr(92),"/")}`。',f'- 重要模型类别：{categories}。',f'- 实际用途：{usage}。','- 导入：批处理真实导入、渲染；材质适配为独立 Material + 导入器映射，原 FBX 未改。','- 审核：通过分类可见性预览；选入测试场景的资产完成整体构图观察，其余仍为正式接入前候选。',''])
cat.extend(['## 推荐和保留问题','','- 优先候选：Quaternius CommonTree、Rock_Medium、花草；Kenney 棕榈、残骸、木箱、木桶与划艇；KayKit 资源道具和地牢基础组件。','- 不推荐直接作为最终主角：Superhero 男女角色的体型与服装不符合学生荒岛主角定位，现阶段只验证骨骼与动作。','- 不建议在热带主场景大量使用针叶树、枯树、红叶树、现代大型货船或赛车组件；它们保留在库中供选择。','- Ruins 还缺后半部分文件，不能宣称完整下载。石材构件可作遗迹，但不能直接等同自然洞穴岩壁。','- 原始叶片材质曾出现透明区域黑边，已用独立 Cutout 材质适配；错误地将扁平 RockPath 按高度拉成巨石的问题已通过改用 Rock_Medium 修正。','- 当前库可支撑海滩、森林、遗迹的基础搭建；整岛仍缺自然洞穴/悬崖套件、瀑布与河流水体、正式灯塔、儿童探险主角、部分热带植被及完整地形与性能方案。',''])
(root/'Docs/Art/ASSET_CATALOG.md').write_text('\n'.join(cat),encoding='utf8')
(root/'Docs/Art/ASSET_LICENSES.md').write_text('\n'.join(lic),encoding='utf8')
archives=[]
for p in (root/'ArtSource/Archives').glob('*.zip'):
 with zipfile.ZipFile(p) as z:
  bad=z.testzip()
 archives.append({'archive':str(p.relative_to(root)),'bytes':p.stat().st_size,'sha256':hashlib.sha256(p.read_bytes()).hexdigest(),'crc_ok':bad is None,'note':'本地将官方逐文件下载封装，非官方原始压缩包' if p.name.startswith('Ultimate') else ('本地将复用的第一代动画文件封装，非重新下载' if p.name.startswith('UniversalAnimationLibrary') else '原始压缩包')})
(root/'Docs/Art/ARCHIVE_MANIFEST.json').write_text(json.dumps(archives,ensure_ascii=False,indent=2),encoding='utf8')
print('Catalog, licenses, archive CRC checks written',len(audit),len(archives))

