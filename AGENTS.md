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
- 小步提交，commit message 写清楚“为什么改”，而非只写“改了什么”。

> 例外：`dev_log.txt` 是刻意保留的“设计意图 + 踩坑”记录，属文档而非构建日志，可以且应当提交。

## 常见坑（改动前务必确认）

1. **DLL 与 PCK 必须同时更新**：游戏从 `mods/` 加载 `kyxiaofujiu.dll` + `.pck`。只编译 DLL 或只导出 PCK，改动在游戏内无任何效果（这是本仓库最高频的“假 bug”）。
2. **卡牌不参与 `IterateHookListeners` 迭代**：不要在卡牌上重写 `BeforeSideTurnEnd` 等战斗钩子，钩子不会分发到卡牌。卡牌回合末逻辑用 `HasTurnEndInHandEffect` / `OnTurnEndInHand` 方案。
3. **不要误用 mod 自身的 `Seal()`**（定义于 `XiaofujiuCardBase.Seal()`，**不是游戏 API**）：在 `OnPlay` 里调用它会让夫白卡牌卡死在某形态 / 多段打出异常。
4. **不要“自作聪明”改设计意图**：卡牌状态机（如 `CanonicalFufuState = White`）是刻意保持简洁的。改动前先看 `GAME_REFERENCE.md` 与 `dev_log.txt`，确认原设计意图再动手。
5. **游戏路径差异**：`.csproj` 里复制 DLL 的目标路径（`E:\SteamLibrary\...\mods`）是开发机专属，仅当该路径存在才复制；不要依赖它作为其他机器可复现的部署方式。

## 参考文件

- `GAME_REFERENCE.md`：游戏 mod 开发参考手册（数据模型、Hook、命令、模组 API 等），改前先查。
- `dev_log.txt`：开发与踩坑记录，含卡牌设计意图。
