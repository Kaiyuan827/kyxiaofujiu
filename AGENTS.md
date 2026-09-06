# AGENTS.md — 供 AI 编码代理阅读

本文档为本仓库中工作的 AI 编码助手（如 Codex）提供项目上下文、操作规范与已知坑。人类开发者也可参考。

## 项目定位

- 《杀戮尖塔 2》mod「晓夫九」，Godot 4.5 + C#（.NET 9），Harmony 运行时补丁。
- mod 入口：`kyxiaofujiuInitializer.cs`，使用 `[ModInitializer]` 属性，游戏启动时反射调用初始化。
- 游戏加载链路：`mods/*.json` → 装载 DLL + PCK → `[ModInitializer]` 初始化 → Harmony 把内容注入 `ModelDb`（详见 `GAME_REFERENCE.md`）。
- 主要玩法：卡牌「要嫁就嫁」，夫白 / 夫黑形态转换，格挡与 Retain 机制。

## 构建

- 依赖：**Godot 4.5** + **.NET SDK 9**。
- 引用库：`sts2.dll`、`0Harmony.dll`、`GodotSharp.dll` 等**仅编译期引用，不入库**（游戏运行时用自己的程序集）。克隆后按 README“构建前置”二选一：从本地 Steam 路径解析（用 `STS2_DATA` 指向 `data_sts2_*`），或加 NuGet“仅编译”引用程序集包。当前项目用的是本地 `libs/`，克隆前先确认采用哪种。
- 编译：`dotnet build`，输出 `build/kyxiaofujiu.dll`。
- 部署：需**同时**更新 Godot 导出的 `.pck` 与编译出的 `.dll` 到游戏 `mods/` 目录。只改一个在游戏里不会生效。

## 路径约定（重要）

- 本仓库**不含游戏源码**，`GAME_REFERENCE.md` 只是生成出来的参考文档。
- 游戏源码 / 游戏安装路径**因开发者机器而异**，不要硬编码绝对路径。
- 约定用符号路径指向本地游戏：
  - `%STS2_SRC%`（Windows）/ `$STS2_SRC`（Linux / macOS）→ 游戏反编译源码根目录
  - `%STS2_DATA%`（Windows）/ `$STS2_DATA`（Linux / macOS）→ 游戏的 `data_sts2_*` 目录（含 `sts2.dll` 等）
- Agent 需要游戏源码时，按下面的顺序定位，找到即用；**找不到就直接停下来问用户，不要自行全局搜索或瞎猜**：
  1. 环境变量：`STS2_SRC`（Windows 为 `%STS2_SRC%`）。
  2. 仓库 / 工作区的上级目录及常见相邻目录（确认是含 `src/` 的 STS2 源码根目录）。
  3. 已知默认位置 `E:\sts2\src`。
  4. 都没有 → 在对话里**直接向用户询问路径**，不要继续猜测。
- 编译引用应优先从 `STS2_DATA` 解析 `sts2.dll`、`0Harmony.dll`，或使用 NuGet 上的“仅编译”引用程序集包（见 README“构建前置”）。

## 提交约定

- 遵循 `.editorconfig`。
- 只提交必要源码 / 资源。**不要**提交：`build/`、`bin/`、`obj/`、`*.pck`、`*.dll`、`libs/`、`backup/`、`src_bak/`、`*.zip`、临时构建日志（如 `build_log.txt`、`export_log.txt`、`import_diff.txt`）、编辑器 / 系统杂项。
- **必须提交** Godot 的 `.import` 与 `.uid` 文件（导入配置 / UID 需要入库），不要随意改动或删除。
- 基于 `master` 开功能分支，走 PR 合并；不要直接推 `master`。
- **硬性规定：每次修改完成后必须小步本地提交**（改一处、提交一处，不要攒一堆改动再一次性提交）；commit message 写清楚“为什么改”，而非只写“改了什么”。

> 例外：`dev_log.txt` 是刻意保留的“设计意图 + 踩坑”记录，属文档而非构建日志，可以且应当提交。

## 常见坑（改动前务必确认）

1. **DLL 与 PCK 必须同时更新**：游戏从 `mods/` 加载 `kyxiaofujiu.dll` + `.pck`。只编译 DLL 或只导出 PCK，改动在游戏内无任何效果（这是本仓库最高频的“假 bug”）。
2. **卡牌不参与 `IterateHookListeners` 迭代**：不要在卡牌上重写 `BeforeSideTurnEnd` 等战斗钩子，钩子不会分发到卡牌。卡牌回合末逻辑用 `HasTurnEndInHandEffect` / `OnTurnEndInHand` 方案。
3. **不要误用 mod 自身的 `Seal()`**（定义于 `XiaofujiuCardBase.Seal()`，**不是游戏 API**）：在 `OnPlay` 里调用它会让夫白卡牌卡死在某形态 / 多段打出异常。
4. **不要“自作聪明”改设计意图**：卡牌状态机（如 `CanonicalFufuState = White`）是刻意保持简洁的。改动前先看 `GAME_REFERENCE.md` 与 `dev_log.txt`，确认原设计意图再动手。
5. **游戏路径差异 / mods 部署**：`.csproj` 不再硬编码任何机器路径。默认只把 DLL 输出到项目 `build\`；要“编译即部署到游戏 mods 目录”，用 MSBuild 属性 `Sts2ModsDir` 指定（优先级从高到低：`-p:Sts2ModsDir=...` > 项目根目录被 gitignore 的 `Directory.Build.props` > 环境变量 `STS2_MODS_DIR`）。不配也完全可用：直接跑仓库旁的 `build_kyxiaofujiu.ps1`，它会一次性编出 DLL + PCK + 清单（`kyxiaofujiu.json`，文件名须为 `<modid>.json`）并部署到你本机的 mods 目录。不要依赖某个开发机专属的绝对路径作为可复现部署方式。
6. **`CardSelectorPrefs` 的“手动确认”由 min/max 决定**：`new CardSelectorPrefs(prompt, min, max)` 中 `RequireManualConfirmation = (min >= 0 && min != max)`。想让玩家“点卡即取”就传 `(prompt, 1)`（min==max）；若传 `(prompt, 0, 1)` 还需额外点一次“确定”（对应官方涅奥之怒），对单张选择很冗余。官方宇宙冷漠 `CosmicIndifference` 用 `(prompt, 1)`，参考实现。
7. **卡组（Deck）里的牌也会收到 `BeforeCombatStart`/`AfterCombatEnd`**：`IterateHookListeners(combatState)` 会遍历 `player.Deck.Cards` + 战斗牌堆全部卡。若你在卡牌上订阅静态事件并对卡组牌做 `AddThisCombat(-n)`，`EndOfCombat` 修饰符会留在常驻的卡组牌上且无清理逻辑（游戏没有 `EndOfCombatCleanup`），费用会跨战斗越减越低，从“本场战斗”变成“全局永久”。减费前先 `if (CombatState == null) return;`，只对战斗中的实例操作（参考 `Bangyishenmedongxi`）。注意：战斗回合类钩子（如 `BeforeSideTurnEnd`）只遍历 `combatState.IterateHookListeners()`，不会到卡组牌。
8. **事件/遗物给“稀有卡牌奖励”的图标**：`CardReward.IconPath` 仅在 `Source == CardCreationSource.Encounter && RarityOdds == CardRarityOddsType.BossEncounter` 时显示稀有卡图标（`reward_icon_rare.png`）。用 `ForNonCombatWithUniformOdds`（Source=Other）会回落到普通卡图标（`reward_icon_card.png`）。要稀有图标请直接 `new CardCreationOptions(cardPools, CardCreationSource.Encounter, CardRarityOddsType.BossEncounter)`（参考 Boss 奖励）。
9. **多部位 Boss 的“第一个存活怪物”不一定是视觉左侧**：`state.Enemies.FirstOrDefault(c => !c.IsDead)` 可能命中左侧部位（如帝皇蟹的 Crusher）。需指定说话者时用类型/部位判断优先（如 `c.Monster is Rocket`），否则气泡会贴着屏幕最左（参考夫黄SC 的 `PickSpeaker`）。
10. **`CardSelectCmd.FromCombatPile` 在“恰好满足 min 且无需手动确认”时返回牌堆内部 List 而非副本**：`new CardSelectorPrefs(prompt, 1)`（min==max → `RequireManualConfirmation=false`）且牌堆恰好 1 张时走“点卡即取”快捷路径，直接返回 `pile.Cards`（活的内部 List）。若你在 `OnPlay` 里 `foreach` 它、循环内又 `CardPileCmd.Add(card, PileType.Hand)` 移动该牌，等于迭代期间修改同一 List，抛 `InvalidOperationException: Collection was modified`，OnPlay 中断导致牌卡死在打出牌位（画面顶部）。**凡对牌堆选择结果 `foreach` 且循环内要移动这些牌的，务必先 `.ToList()` 拍快照再遍历**（`(await CardSelectCmd.FromCombatPile(...)).ToList()`）。参考 `Wushisc` 满手/弃牌堆仅 1 张卡死案例——根因是 `(0,1)`→`(1,1)` 关掉手动确认后引入的回归。

## 已知问题（非阻塞，供排查参考）

- **能量计数器 VFX**：`XiaofujiuEnergyCounter` 继承游戏 `NEnergyCounter`，其 `_Ready()` 会把 `%EnergyVfxBack/%EnergyVfxFront` 强转成 `NParticlesContainer`，但 mod 场景 `xiaofujiu_energy_counter.tscn` 实例化后这两节点因根场景类型解析失败变成 `Godot.Control`（Godot 用占位符替代），强转抛 `InvalidCastException`、每场战斗刷一条红错。已在 `_Ready()` 用 try/catch 降级：数字/旋转/工资正常，能量爆闪 VFX 与悬停气泡不可用。要恢复 VFX 需改该场景里 `EnergyVfxBack/Front` 的节点类型并重导 PCK（见 dev_log 补充119）。

## 参考文件

- `GAME_REFERENCE.md`：游戏 mod 开发参考手册（数据模型、Hook、命令、模组 API 等），改前先查。
- `dev_log.txt`：开发与踩坑记录，含卡牌设计意图。

## AI 一键构建 / 部署

- 仓库上级目录的 `build_kyxiaofujiu.ps1`：`dotnet build` → Godot 导出 `.pck` → 写入 `kyxiaofujiu.json`（商店清单元信息，文件名须为 `<modid>.json`）并部署到本机游戏 `mods\kyxiaofujiu\`。每个开发者各自维护脚本里的本机 `$ModsDir`，并用 `$ManifestVersion` 控制本地与在线 Workshop 版本的优先级（本地应 ≥ 在线，否则游戏优先加载在线版本）。该脚本在仓库外，不入库。

## 修复完成后的收尾 / 发布（Agent 务必在完成时提醒用户）

Agent 修完一个 bug 并**小步本地提交后**，主动提醒用户完成“代码侧”与“模组侧”两套发布动作，不要改完代码就收工：

- **代码侧（提交 PR）**
  1. 基于 `master`（或当前功能分支）确定/创建功能分支；改动小步、本地提交。
  2. `git push origin <branch>` 创建远程 PR 分支；在仓库上级目录写一份 `PR_DESCRIPTION.md`（含改动/原因/测试）供用户提交 PR。
- **模组侧（发布到 Steam 创意工坊）**
  1. 编辑仓库外的 `build_kyxiaofujiu.ps1`，把 `$ManifestVersion` +1（本地需 ≥ 在线版本，否则游戏优先加载在线版本）。
  2. 运行 `build_kyxiaofujiu.ps1` 重编 DLL + PCK 并部署（生成 `<modid>.json` 清单）。
  3. 运行 `upload_workshop.ps1` 上传（AppID 2868840 / PublishedFileId 3796187683）。**上传需 SteamCMD 交互登录（密码 + Steam Guard），Agent 无法代为完成**，提醒用户在本机执行，可先用 `-DryRun` 演练；**上传前先关闭游戏**，避免 `mods\kyxiaofujiu.dll` 被占用。

> 参考：本仓库“无视SC 满手卡死 + 能量计数器 VFX”修复即按上述流程收尾（见 dev_log 补充119）。
