# 晓夫九（kyxiaofujiu）

为《杀戮尖塔 2》（Slay the Spire 2）开发的 mod。用 **Godot 4.5 + C#（.NET 9）** 编写，通过 **Harmony** 在运行时注入内容，资源以 Godot **PCK** 形式导出。

## 技术栈

- Godot 4.5（`Godot.NET.Sdk/4.5.1`），GL Compatibility 渲染
- .NET 9（`net9.0`），C#
- Harmony 运行时补丁，依赖 `0Harmony.dll` 与 `sts2.dll`

## 目录结构

```
kyxiaofujiu/
├── kyxiaofujiuInitializer.cs   # mod 入口，使用 [ModInitializer] 反射调用
├── project.godot               # Godot 项目配置
├── src/                        # mod 的 C# 源码
├── scenes/ animations/ images/ audio/   # Godot 资源
├── libs/                       # (本地) 引用库 0Harmony.dll / sts2.dll，不提交
├── GAME_REFERENCE.md           # 游戏 mod 开发参考手册（AI自动生成）
└── AGENTS.md                   # 供 AI 编码代理阅读的项目规范
```

## 构建与部署

### 前置

1. 安装 **Godot 4.5** 与 **.NET SDK 9**。
2. 准备 mod 引用库：`sts2.dll`、`0Harmony.dll`、`GodotSharp.dll` 等**只作为编译期引用，不入库、不拷进产物**（游戏运行时用自己的程序集）。获取方式二选一：

   - **从本地 Steam 安装解析**（官方模板 `Alchyr/ModTemplate-StS2` 的做法）：在 `Directory.Build.props` / `.csproj` 里用 `Sts2Path` 指向本机游戏目录 `<Steam库>...\Slay the Spire 2\data_sts2_windows_x86_64`（各机器自行设置）。
   - **用 NuGet 的“仅编译”引用程序集包**：`sts2` / `GodotSharp` / `0Harmony` 有官方号维护的只编译不部署包，克隆后 `dotnet add package` 即可，不用手翻 Steam 目录。

   （本项目当前是手动把 DLL 放进本地 `libs/`，克隆前请先向维护者确认采用哪种。）

   > 提示：`Directory.Build.props` 是**每台机器各自的本地文件**（已加入 `.gitignore`，不会误传）。它既能放编译期路径 `Sts2Path`，也能放下文“部署”里的 `Sts2ModsDir`；两者互不冲突。

### 编译

```bash
dotnet build
```

产物输出到 `build/kyxiaofujiu.dll`（`bin/`、`obj/`、`build/`、`*.dll` 均不入库）。

### 部署到游戏

游戏的 mod 加载链路：扫描 `mods/*.json` → 装载 **DLL + PCK** → 反射调用 `[ModInitializer]` → Harmony 注入 `ModelDb`。

一个可被游戏识别的 mod，就是**一个目录**，里面放三个文件：

```
mods/kyxiaofujiu/
├── manifest.json      # mod 元数据（id/name/author/version/has_pck/has_dll... 见 GAME_REFERENCE.md）
├── kyxiaofujiu.dll
└── kyxiaofujiu.pck
```

每次改动要**同时更新 DLL 与 PCK**，缺一或版本不一致，改动在游戏内不会生效（这是本仓库最高频的“假 bug”）。

**仓库不写死任何机器路径**——每个人的游戏安装路径不同，所以没有统一假设。构建产物默认只进 `build\`，具体部署到哪里、怎么部署由你按自己的机器来定。两种方式任选。

#### 方式 A：`dotnet build` 时自动复制 DLL（仅 DLL）

`.csproj` 预留了 `Sts2ModsDir` 属性，表示“把 DLL 复制到哪个目录”。默认留空 = 不复制。要启用时三选一（优先级从高到低）：

1. 命令行：
   `dotnet build -p:Sts2ModsDir="<游戏安装目录>\mods\kyxiaofujiu"`
2. 项目根目录放一个被 `.gitignore` 忽略的 `Directory.Build.props`：
   ```xml
   <Project>
     <PropertyGroup>
       <Sts2ModsDir><游戏安装目录>\mods\kyxiaofujiu</Sts2ModsDir>
     </PropertyGroup>
   </Project>
   ```
3. 设置环境变量 `STS2_MODS_DIR` 为上面的目录。

> 注意：方式 A 只复制 DLL，**不含 PCK 与 manifest**，所以仍需手动补另外两个文件才算完整。

#### 方式 B（推荐）：一键构建 + 部署脚本

仓库上级目录的 `build_kyxiaofujiu.ps1` 会一次完成：
`dotnet build` → Godot 导出 `*.pck` → 生成 `manifest.json` → 部署到本机游戏 `mods\kyxiaofujiu\`。

脚本在仓库外（不入库）。每个开发者各自维护脚本开头的两处：

- `$ModsDir`：你本机的游戏 `mods` 目录；
- `$ModVersion`：语义化版本号，控制本地与在线 Workshop 版本的优先级（**本地应 ≥ 在线**，否则游戏优先加载在线版本）。

用法：

```powershell
powershell -ExecutionPolicy Bypass -File <仓库上级目录>\build_kyxiaofujiu.ps1
```

#### 让本地改动生效的关键

本机同时存在“Steam 在线订阅版本”时，游戏会比较 `manifest.json` 里的 `version`。只有本地版本 ≥ 在线版本、或在游戏内禁用在线版本，本地改动才会被加载。因此**每次准备发布新版本前，把 `$ModVersion` / `manifest.json` 的 `version` 手动 +1**；而平时本地测试时也建议保持本地版本高于在线，避免被在线版本覆盖。

## 贡献

欢迎通过 PR 参与。请先阅读 [AGENTS.md](./AGENTS.md) 了解项目规范与常见坑（尤其卡牌钩子、DLL/PCK 同步等），再基于 `master` 开功能分支提交。

## 许可证

**待补充。** 在添加正式许可证文件（LICENSE）之前，请先与项目所有者（Kaiyuan827）确认分发条款。
