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

### 编译

```bash
dotnet build
```

产物输出到 `build/kyxiaofujiu.dll`（`bin/`、`obj/`、`build/`、`*.dll` 均不入库）。

### 部署到游戏

游戏的 mod 加载链路是：扫描 `mods/*.json` → 装载 **DLL + PCK** → 反射调用 `[ModInitializer]` → Harmony 注入 `ModelDb`。

因此每次改动要**同时更新两样**：

- 编译出的 `kyxiaofujiu.dll`
- Godot 导出的资源包 `*.pck`

两者缺一或版本不一致，改动在游戏内不会生效（常被误以为代码没改）。

`.csproj` 不硬编码任何机器路径：默认只把 DLL 输出到 `build\`。要“编译即部署到游戏 mods 目录”，用 MSBuild 属性 `Sts2ModsDir` 指定（优先级从高到低：`dotnet build -p:Sts2ModsDir="..."` > 项目根目录被 `.gitignore` 忽略的 `Directory.Build.props` > 环境变量 `STS2_MODS_DIR`）。

更省事的方式：运行仓库上级目录的 `build_kyxiaofujiu.ps1`，一次性完成 `dotnet build` → Godot 导出 `*.pck` → 写入 `manifest.json` → 部署到本机游戏 `mods\kyxiaofujiu\`。脚本在仓库外，各开发者自行维护本机的 mods 路径与版本号。

## 贡献

欢迎通过 PR 参与。请先阅读 [AGENTS.md](./AGENTS.md) 了解项目规范与常见坑（尤其卡牌钩子、DLL/PCK 同步等），再基于 `master` 开功能分支提交。

## 许可证

**待补充。** 在添加正式许可证文件（LICENSE）之前，请先与项目所有者（Kaiyuan827）确认分发条款。
