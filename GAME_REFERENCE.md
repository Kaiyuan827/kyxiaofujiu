# 杀戮尖塔2 模组开发参考手册

> 自动生成于 2025-06-29  
> 游戏引擎: Godot 4.5.1 (MegaDot 定制版 C#)  
> 游戏源码位置: `$STS2_SRC`（各机器本地设置，勿写死绝对路径）

---

## 一、项目总览

### 核心目录结构

```
$STS2_SRC/
├── src/
│   ├── Core/
│   │   ├── Models/          ← 数据模型层（CardModel, RelicModel, PowerModel, CharacterModel, 各种 Pool）
│   │   │   ├── Cards/       ← 所有卡牌模型（~250+ 张）
│   │   │   ├── CardPools/   ← 卡池定义
│   │   │   ├── Powers/      ← 所有能力模型
│   │   │   ├── Relics/      ← 所有遗物模型
│   │   │   └── RelicPools/  ← 遗物池定义
│   │   ├── Entities/        ← 运行时实体（玩家、生物、卡牌Node、遗物Node、能力Node）
│   │   ├── Commands/        ← 命令系统（DamageCmd, PowerCmd, CardCmd, PlayerCmd, RelicCmd...）
│   │   │   └── Builders/    ← 命令Builder（AttackCommand, BlockCommand...）
│   │   ├── Modding/         ← 模组API（ModInitializerAttribute, Mod, ModHelper）
│   │   ├── GameActions/     ← GameAction 系统（网络/多人用）
│   │   │   └── Multiplayer/
│   │   ├── Hooks/           ← Hook 系统（Hook.cs, HpLossHookPhase.cs...）
│   │   ├── Localization/    ← 本地化
│   │   │   └── DynamicVars/ ← 动态变量（DamageVar, BlockVar, EnergyVar...）
│   │   ├── ValueProps/      ← 值标志枚举（Unblockable, Unpowered, Move...）
│   │   ├── Saves/           ← 存档
│   │   ├── Runs/            ← 运行状态
│   │   ├── Nodes/           ← Godot 节点（场景/UI）
│   │   └── Combat/          ← 战斗系统
│   ├── gdscript/            ← GDScript 游戏逻辑
│   └── SourceGeneration/    ← 源生成器
├── mods/                    ← 模组加载目录
│   └── kyxiaofujiu.json     ← 你的模组（已在开发）
└── scenes/                  ← Godot 场景文件
```

### 模组加载链路

```
游戏启动 → 扫描 mods/*.json → 加载 DLL/PCK → 
使用 [ModInitializer] 反射调用初始化方法 →
模组用 Harmony Patch 注入内容到 ModelDb
```

---

## 二、核心模型体系

### 2.1 AbstractModel — 所有原型的基类

**命名空间:** `MegaCrit.Sts2.Core.Models`

所有卡牌、遗物、能力、角色都继承自 `AbstractModel`。

**关键属性:**
```
Id: ModelId          ← 类型名作为唯一ID，由 ModelDb 自动分配
IsMutable: bool      ← 是否是可变实例（游戏中使用的是 mutable clone）
IsCanonical: bool    ← 是否是原型实例（模板）
```

**关键方法:**
```
ToMutable(): AbstractModel  ← 从原型生成可变副本
AssertMutable() / AssertCanonical()  ← 断言状态
DeepCloneFields()           ← 克隆时调用
```

### 2.2 ModelDb — 全局模型注册表

```
ModelDb.AllCharacters       ← IEnumerable<CharacterModel>
ModelDb.AllCards            ← IEnumerable<CardModel>
ModelDb.AllPowers           ← IEnumerable<PowerModel>
ModelDb.AllRelics           ← IEnumerable<RelicModel>
ModelDb.AllCardPools        ← IEnumerable<CardPoolModel>
ModelDb.AllRelicPools       ← IEnumerable<RelicPoolModel>

// 唯一查询
ModelDb.Card<T>()           // 例: ModelDb.Card<Dagongren>()
ModelDb.Relic<T>()          // 例: ModelDb.Relic<MyCustomRelic>()
ModelDb.Power<T>()          // 例: ModelDb.Power<DagongPower>()
ModelDb.Character<T>()      // 例: ModelDb.Character<Xiaofujiu>()
ModelDb.CardPool<T>()
ModelDb.RelicPool<T>()

// ID 查询
ModelDb.GetById<CardModel>(id)   // 通过 ModelId 查
ModelDb.GetId(type)              // 获取类型对应的 ModelId
ModelDb.Contains(type)           // 检查类型是否已注册
```

---

## 三、CardModel — 卡牌

**命名空间:** `MegaCrit.Sts2.Core.Models`

### 3.1 构造函数

```csharp
// base(int energyCost, CardType type, CardRarity rarity, TargetType targetType)
public Dagongren()
	: base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
{
}
```

### 3.2 CardType 枚举
```
CardType.Attack, CardType.Skill, CardType.Power, 
CardType.Status, CardType.Curse, CardType.Quest, CardType.None
```

### 3.3 CardRarity 枚举
```
CardRarity.Basic, CardRarity.Common, CardRarity.Uncommon, 
CardRarity.Rare, CardRarity.Curse, CardRarity.Status, 
CardRarity.Event, CardRarity.Quest, CardRarity.Ancient
```

### 3.4 TargetType 枚举
```
TargetType.AnyEnemy, TargetType.Self, TargetType.None, ... 
```

### 3.5 关键属性
```
Owner: Player           ← 卡牌归属玩家
DynamicVars: DynamicVarSet  ← 动态变量（Damage, Block 等）
Keywords: IReadOnlySet<CardKeyword>
Tags: IEnumerable<CardTag>
EnergyCost: CardEnergyCost
Type: CardType
Rarity: CardRarity
TargetType: TargetType
CanBeGeneratedInCombat: bool  ← false 则战斗内不随机生成
CanBeGeneratedByModifiers: bool ← false 则不会被其他卡牌/遗物生成
```

### 3.6 动态变量定义
```csharp
protected override IEnumerable<DynamicVar> CanonicalVars => new[]
{
	new DamageVar(8m, ValueProp.Move)  // 8点伤害
	// new BlockVar(5m)                  // 5点格挡
	// new EnergyVar(1m)                 // 1点能量
};
```

### 3.7 卡牌玩法回调
```csharp
protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
{
	// 卡牌被打出时的逻辑
	
	// 获取目标
	var target = cardPlay.Target;
	
	// 造成伤害
	await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
		.FromCard(this)
		.Targeting(target)
		.WithHitFx("vfx/vfx_attack_slash")
		.Execute(choiceContext);
	
	// 获得格挡
	await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
	
	// 施加能力
	await PowerCmd.Apply<DagongPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
}
```

### 3.8 升级回调
```csharp
protected override void OnUpgrade()
{
	DynamicVars.Damage.UpgradeValueBy(4m);  // 升级+4伤害
	// DynamicVars.Block.UpgradeValueBy(3m); // 升级+3格挡
}
```

### 3.9 重要枚举值
```
CardKeyword: Unplayable, Retain, Ethereal, Exhaust, Sly, Innate, ...

ValueProp 标志:
  ValueProp.Unblockable  ← 穿甲（无法格挡）
  ValueProp.Unpowered    ← 不受力量/敏捷修正
  ValueProp.Move         ← 物理伤害（受力量修正，默认普通攻击用）
  ValueProp.SkipHurtAnim ← 跳过受击动画
```

---

## 四、RelicModel — 遗物

**命名空间:** `MegaCrit.Sts2.Core.Models`

### 4.1 关键属性
```
Owner: Player               ← 所属玩家
Rarity: RelicRarity         ← 稀有度
ShowCounter: bool           ← 是否显示计数器图标
DisplayAmount: int          ← 计数器数值
IsTradable: bool
IsAllowedInShops: bool
IsStackable: bool
FlashSfx: string            ← flash 音效路径
```

### 4.2 RelicRarity 枚举
```
RelicRarity.Common, RelicRarity.Uncommon, RelicRarity.Rare,
RelicRarity.Shop, RelicRarity.Boss, RelicRarity.Starter,
RelicRarity.Event, RelicRarity.Ancient, RelicRarity.None
```

### 4.3 遗物生命周期回调
```csharp
// 战斗相关
public virtual Task BeforeCombatStart() → Task.CompletedTask
public virtual Task AfterDamageReceived(PlayerChoiceContext, Creature target, 
	DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
public virtual Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
// ❌ 以下两个方法不存在，文档有误：
// public virtual Task OnPlayerTurnStart(PlayerChoiceContext choiceContext)
// public virtual Task OnPlayerTurnEnd(PlayerChoiceContext choiceContext)
// ✅ 改用 AfterSideTurnStart 判断玩家回合（参考开心小花 HappyFlower）：
//    participants.Contains(Owner.Creature) → 玩家回合开始
public virtual Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
// ✅ 或用 AfterSideTurnEnd 判断：
//    side == CombatSide.Enemy  → 敌方回合结束 = 玩家回合开始
//    side == CombatSide.Player → 玩家回合结束
public virtual Task AfterSideTurnEnd(PlayerChoiceContext, CombatSide side, 
	IEnumerable<Creature> participants)

// 伤害修正
public virtual decimal ModifyDamageMultiplicative(Creature? target, decimal amount, 
	ValueProp props, Creature? dealer, CardModel? cardSource) → 1m  // 返回倍率
public virtual decimal ModifyDamageAdditive(...) → 0m                // 返回增加值

// 其他回调
public virtual Task AfterObtained()
public virtual Task AfterRemoved()
public virtual bool IsAllowed(IRunState runState)
```

### 4.4 遗物辅助方法
```csharp
Flash()                           // 触发遗物闪烁特效
Flash(IEnumerable<Creature> targets)
InvokeDisplayAmountChanged()      // 更新计数器显示
```

---

## 五、PowerModel — 能力

**命名空间:** `MegaCrit.Sts2.Core.Models`

### 5.1 关键属性
```
Owner: Creature           ← 能力所属的生物
Applier: Creature         ← 能力施加者
Amount: int               ← 层数/数值
Target: Creature?         ← 目标
Type: PowerType           ← Buff 或 Debuff
StackType: PowerStackType ← 叠加方式
IsVisible: bool
```

### 5.2 PowerType
```
PowerType.Buff, PowerType.Debuff
```

### 5.3 PowerStackType
```
PowerStackType.None, PowerStackType.Counter, PowerStackType.Intensity, ...
```

### 5.4 能力生命周期回调
```csharp
public virtual Task AfterApplied(Creature? applier, CardModel? cardSource)
public virtual Task AfterStacked(Creature? applier, CardModel? cardSource)
public virtual Task AfterDurationChanged(int newDuration)
public virtual Task OnTurnStart()
public virtual Task OnTurnEnd()
public virtual Task AfterDamageReceived(...)
public virtual Task BeforeDamageDealt(...)
public virtual Task BeforeCardPlayed(...)
public virtual Task AfterCardPlayed(...)
public virtual decimal ModifyDamageMultiplicative(...) → 1m
public virtual decimal ModifyDamageAdditive(...) → 0m
public virtual decimal ModifyBlockMultiplicative(...) → 1m
```

### 5.5 获取和操作能力
```csharp
// 获取能力
var power = creature.GetPower<DagongPower>();
var powers = creature.GetPowers();

// 移除能力
await PowerCmd.Remove(power);

// 修改层数
await PowerCmd.ModifyAmount(choiceContext, power, amount, applier, cardSource);
```

---

## 六、命令系统 (Commands)

所有命令都在 `MegaCrit.Sts2.Core.Commands` 命名空间下。

### 6.1 DamageCmd — 伤害
```csharp
// 基本攻击
await DamageCmd.Attack(damage)          // damage: decimal
	.FromCard(this)                      // 来源卡牌
	.Targeting(target)                   // 指定单个目标
	.Targeting(target1, target2)         // 多个目标
	.WithHitFx("vfx/vfx_attack_slash")   // 命中特效
	.Hits(count)                         // 攻击次数
	.Execute(choiceContext);

// 计算伤害的变量
await DamageCmd.Attack(calculatedDamageVar).FromCard(this)...

// 直接伤害（非攻击）
await DamageCmd.HpLoss(amount)
	.Targeting(target)
	.WithProp(ValueProp.Unblockable)     // 穿甲
	.Execute(choiceContext);
```

### 6.2 CreatureCmd.GainBlock — 格挡
```csharp
await CreatureCmd.GainBlock(base.Owner.Creature, amount, cardPlay);
```

### 6.3 PowerCmd — 能力
```csharp
// 施加能力
await PowerCmd.Apply<DagongPower>(
	choiceContext,          // PlayerChoiceContext
	target,                 // Creature 目标
	1m,                     // 层数
	applier,                // 施加者
	cardSource              // 来源卡牌（可选）
);

// 移除能力
await PowerCmd.Remove(power);

// 修改层数
await PowerCmd.ModifyAmount(choiceContext, power, deltaAmount, applier, cardSource);
```

### 6.4 CardCmd — 卡牌操作
```csharp
await CardCmd.MoveToHand(choiceContext, card);
await CardCmd.MoveToDiscard(choiceContext, card);
await CardCmd.MoveToExhaust(choiceContext, card);
await CardCmd.GenerateCard<SomeCard>(choiceContext, player);
await CardCmd.UpgradeCard(choiceContext, card);
```

### 6.5 PlayerCmd — 玩家操作
```csharp
await PlayerCmd.GainEnergy(amount, player);
await PlayerCmd.LoseEnergy(amount, player);
await PlayerCmd.GainGold(amount, player);
await PlayerCmd.SetEnergy(amount, player);
```

### 6.6 CreatureCmd — 生物操作
```csharp
await CreatureCmd.Heal(amount, target);
```

---

## 七、模组开发 API

### 7.1 ModInitializer — 入口

```csharp
using MegaCrit.Sts2.Core.Modding;

[ModInitializer(nameof(Initialize))]  // 标记初始化方法
public static class MyModInitializer
{
	public static void Initialize()
	{
		// 注册 Godot 脚本映射
		Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(
			Assembly.GetExecutingAssembly());
		
		// 注入特殊类型到存档缓存
		SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(MyCharacter));
		
		// 应用 Harmony 补丁
		var harmony = new Harmony("my.mod.id");
		harmony.PatchAll();
	}
}
```

### 7.2 ModHelper — 官方辅助

虽然你使用了 Harmony 直接注入，但游戏也提供了官方 API:

```csharp
// 添加模型到池（游戏初始化前调用）
ModHelper.AddModelToPool<SharedRelicPool, MyRelic>();

// 订阅 RunState 钩子
ModHelper.SubscribeForRunStateHooks("my_mod_hook", runState => { ... });

// 订阅 CombatState 钩子
ModHelper.SubscribeForCombatStateHooks("my_mod_hook", combatState => { ... });
```

### 7.3 Harmony Patch 模式（你当前使用的方式）

```csharp
// 注入卡牌到 TokenCardPool（状态类卡牌池）
[HarmonyPatch(typeof(TokenCardPool), "GenerateAllCards")]
public static class TokenCardPoolPatch
{
	static void Postfix(ref CardModel[] __result)
	{
		var list = __result.ToList();
		list.Add(ModelDb.Card<Jiaban>());
		__result = list.ToArray();
	}
}

// 注入角色到 AllCharacters
[HarmonyPatch(typeof(ModelDb), nameof(ModelDb.AllCharacters), MethodType.Getter)]
public static class AllCharactersPatch
{
	static void Postfix(ref IEnumerable<CharacterModel> __result)
	{
		__result = __result.Append(ModelDb.Character<MyCharacter>()).Distinct();
	}
}

// 注入自定义 Epoch
[HarmonyPatch(typeof(EpochModel), nameof(EpochModel.Get), 
	new Type[] { typeof(string) })]
public static class EpochGetPatch
{
	static bool Prefix(string id, ref EpochModel __result)
	{
		if (id == "MY_CUSTOM_EPOCH")
		{
			__result = new MyCustomEpoch();
			return false;
		}
		return true;
	}
}
```

---

## 八、你的模组结构 (kyxiaofujiu)

### 当前位置: 本仓库（kyxiaofujiu），路径因机器而异，勿写死

```
kyxiaofujiu/
├── kyxiaofujiu.json              ← 模组清单
├── kyxiaofujiuInitializer.cs     ← 模组入口 + 所有 Harmony 补丁
├── src/
│   ├── characters/
│   │   └── xiaofujiu.cs          ← 角色定义
│   ├── cardpools/
│   │   ├── xiaofujiucardpool.cs  ← 角色专属卡池
│   │   ├── Dagongren.cs           ← 打工人卡牌
│   │   ├── Chapianrenzhi.cs       ← 差偏认知卡牌
│   │   └── ...                    ← 30+ 张卡牌
│   ├── powers/
│   │   ├── ChapianrenzhiPower.cs  ← 差偏认知能力
│   │   ├── DagongPower.cs         ← 打工人能力
│   │   └── ...                    ← 17 个能力
│   ├── relicpools/
│   │   ├── XiaofujiuRelicPool.cs  ← 遗物池
│   │   └── MyCustomRelic.cs       ← 自定义遗物（完美格挡机制）
│   ├── epochs/
│   │   ├── Xiaofujiu2Epoch.cs
│   │   ├── Xiaofujiu3Epoch.cs
│   │   └── Xiaofujiu4Epoch.cs
│   ├── monsters/
│   │   └── SheHuaXiaoJieMonster.cs ← 自定义怪物
│   └── core/
│       └── nodes/combat/          ← 自定义战斗节点
├── kyxiaofujiu.dll                ← 编译产物
├── kyxiaofujiu.pck                ← Godot 资源包
└── localization/                  ← 本地化文本
```

### 已有 Harmony 补丁列表

1. `ModelDb.AllCharacters` — 注入 Xiaofujiu 角色
2. `ModelDb.AllCardPools` — 注入 XiaofujiuCardPool
3. `ModelDb.AllRelicPools` — 注入 XiaofujiuRelicPool
4. `CharacterModel.EnergyCounterPath` — 自定义能量UI
5. `CharacterModel.TrailPath` — 自定义拖尾
6. `TokenCardPool.GenerateAllCards` — 注入 Jiaban 状态卡
7. `EpochModel.Get` — 注入 3 个自定义纪元
8. `ProgressSaveManager.CheckFifteenElitesDefeatedEpoch` — 跳过纪元检测
9. `ProgressSaveManager.CheckFifteenBossesDefeatedEpoch` — 跳过纪元检测
10. `EpochModel.AllEpochIds` — 注册纪元ID
11. `PlayerCmd.GainGold` — 触发差偏认知征婚
12. `ArchaicTooth.TranscendenceUpgrades` — 欧罗巴斯遗物
13. `PersonalHivePower.AfterDamageReceived` — 宠物攻击防崩溃

---

## 十一、完整命令系统 (Commands) — 从源码提取

所有命令在命名空间 `MegaCrit.Sts2.Core.Commands` 下。

### 11.1 AttackCommand — AttackCommand Builder（核心伤害命令）

`DamageCmd.Attack(decimal)` 返回 `AttackCommand`，支持链式调用：

**链式方法:**
| 方法 | 说明 |
|------|------|
| `.FromCard(CardModel, CardPlay?)` | 设置攻击来自卡牌，自动将攻击者设为卡牌主人 |
| `.FromMonster(MonsterModel)` | 设置攻击来自怪物 |
| `.FromOsty(Creature, CardModel, CardPlay?)` | 设置攻击来自 Osty |
| `.Targeting(Creature)` | 指定单个目标 |
| `.TargetingAllOpponents(ICombatState)` | 攻击所有对手（每次攻击刷新目标列表） |
| `.TargetingRandomOpponents(ICombatState, bool)` | 随机目标攻击 |
| `.Unpowered()` | 标记为不受力量/敏捷修正的伤害 |
| `.WithAttackerAnim(string?, float, Creature?)` | 自定义攻击者动画 |
| `.WithNoAttackerAnim()` | 跳过攻击者动画 |
| `.WithAttackerFx(string? vfx, string? sfx)` | 攻击者身上的VFX/SFX |
| `.WithHitFx(string? vfx, string? sfx)` | 命中特效 |
| `.SpawningHitVfxOnEachCreature()` | 每个目标独立生成VFX |
| `.Hits(int count)` | 攻击次数（多段攻击） |
| `.AfterAttackerAnim(Func<Task>)` | 攻击动画后的自定义逻辑 |
| `.WithWaitBeforeHit(float fast, float standard)` | 每次命中前等待时间 |
| `.Execute(PlayerChoiceContext)` | **执行攻击** |

**关键属性:**
```
Attacker: Creature?           ← 攻击者
ModelSource: AbstractModel?   ← 来源模型（卡牌等）
CardPlay: CardPlay?           ← 关联的卡牌打出
TargetSide: CombatSide        ← 目标阵营
DamageProps: ValueProp        ← 伤害属性（默认 Move）
IsSingleTargeted / IsMultiTargeted / IsRandomlyTargeted
Results: IEnumerable<List<DamageResult>>  ← 执行后的结果
HitSfx / TmpHitSfx / HitVfx  ← 命中音效/特效路径
```

### 11.2 PowerCmd — 能力命令

```csharp
// 施加能力到单个目标
public static async Task<T?> Apply<T>(
    PlayerChoiceContext choiceContext,
    Creature target,
    decimal amount,
    Creature? applier,
    CardModel? cardSource,
    bool silent = false) where T : PowerModel

// 施加能力到多个目标
public static async Task<IReadOnlyList<T>> Apply<T>(
    PlayerChoiceContext choiceContext,
    IEnumerable<Creature>? targets,
    decimal amount,
    Creature? applier,
    CardModel? cardSource,
    bool silent = false) where T : PowerModel

// 直接施加 PowerModel 实例
public static async Task Apply(
    PlayerChoiceContext choiceContext,
    PowerModel power,
    Creature target,
    decimal amount,
    Creature? applier,
    CardModel? cardSource,
    bool silent = false)
```

**自动逻辑:**
- 战斗结束时不施加
- 目标 `CanReceivePowers == false` 时不施加
- 已存在同类能力时自动叠加层数
- 调用 `ModifyPowerAmountGiven` / `ModifyPowerAmountReceived` 钩子
- 多人模式自动缩放 (若 `ShouldScaleInMultiplayer == true`)

### 11.3 CardCmd — 卡牌操作

```csharp
// 自动打出卡牌（免费）
public static async Task AutoPlay(
    PlayerChoiceContext choiceContext,
    CardModel card,
    Creature? target,
    AutoPlayType type = AutoPlayType.Default,
    bool skipXCapture = false,
    bool skipCardPileVisuals = false)

// 弃牌（单张）
public static async Task Discard(PlayerChoiceContext choiceContext, CardModel card)
// 弃牌（多张）
public static async Task Discard(PlayerChoiceContext, IEnumerable<CardModel>)
```

### 11.4 PlayerCmd — 玩家操作

```csharp
// 能量操作
public static async Task GainEnergy(decimal amount, Player player)
public static Task LoseEnergy(decimal amount, Player player)
public static async Task SetEnergy(decimal amount, Player player)

// 星星 (Star) 操作 — STS2 新资源系统
public static async Task GainStars(decimal amount, Player player)
public static Task LoseStars(decimal amount, Player player)
public static async Task SetStars(decimal amount, Player player)

// 金币操作
public static async Task GainGold(decimal amount, Player player, bool wasStolenBack = false)

// 常量
public const string goldSmallSfx = "event:/sfx/ui/gold/gold_1"
public const string goldMediumSfx = "event:/sfx/ui/gold/gold_2"
public const string goldLargeSfx = "event:/sfx/ui/gold/gold_3"
```

### 11.5 RelicCmd — 遗物操作

```csharp
// 获得遗物（通过泛型）
public static async Task<T> Obtain<T>(Player player) where T : RelicModel

// 获得遗物（通过实例）
public static async Task<RelicModel> Obtain(RelicModel relic, Player player, int index = -1)

// 移除遗物
public static async Task Remove(RelicModel relic)

// 替换遗物
public static async Task<RelicModel> Replace(RelicModel original, RelicModel replace)

// 熔化遗物（保留在背包但不可用）
public static async Task Melt(RelicModel relic)
```

### 11.6 CreatureCmd — 生物操作

```csharp
// 添加生物到战斗
public static async Task<Creature> Add<T>(ICombatState combatState, string? slotName = null)
    where T : MonsterModel

public static async Task<Creature> Add(MonsterModel monster, ICombatState combatState,
    CombatSide side = CombatSide.Enemy, string? slotName = null)

public static async Task Add(Creature creature)

// 伤害生物（直接DamageVar方式）
public static async Task<IEnumerable<DamageResult>> Damage(
    PlayerChoiceContext choiceContext,
    Creature target,
    DamageVar damageVar,
    CardModel cardSource,
    CardPlay? cardPlay)
```

### 11.7 其他命令

| 命令 | 说明 |
|------|------|
| `CardPileCmd` | 卡牌堆栈操作 (Add, Remove, Shuffle 等) |
| `CardSelectCmd` | 卡牌选择操作 |
| `ForgeCmd` | 锻造系统（Sovereign Blade 伤害增强） |
| `MapCmd` | 地图操作 |
| `OrbCmd` | 球体操作 (Channel, Evoke) |
| `OstyCmd` | Osty 相关操作 |
| `PotionCmd` | 药水操作 |
| `RelicSelectCmd` | 遗物选择操作 |
| `RewardsCmd` | 奖励操作 |
| `SfxCmd` | 音效播放 |
| `TalkCmd` | 对话系统 |
| `ThinkCmd` | 思考气泡 |
| `VfxCmd` | 特效播放 |

---

## 十二、完整 Hook 系统 — 从源码提取

命名空间 `MegaCrit.Sts2.Core.Hooks`，静态类 `Hook` 分发所有游戏事件。

所有钩子通过 `AbstractModel` 上的虚方法实现。以下是**全部钩子列表**（按执行顺序分组）：

### 12.1 回合生命周期

| 钩子 | 时机 | 参数 | 执行阶段 |
|------|------|------|----------|
| `BeforeSideTurnStart` | 侧回合开始前 | choiceContext, side, participants, combatState | — |
| `AfterSideTurnStart` | 侧回合开始后 | side, participants, combatState | — |
| `AfterSideTurnStartLate` | 侧回合开始后（晚） | side, participants, combatState | Late |
| `AfterEnergyReset` | 玩家能量重置后 | player | — |
| `AfterEnergyResetLate` | 玩家能量重置后（晚） | player | Late |
| `BeforeHandDraw` | 回合抽牌前 | player, choiceContext, combatState | — |
| `BeforeHandDrawLate` | 回合抽牌前（晚） | player, choiceContext, combatState | Late |
| `AfterPlayerTurnStartEarly` | 玩家回合开始后（早） | choiceContext, player | Early |
| `AfterPlayerTurnStart` | 玩家回合开始后 | choiceContext, player | — |
| `AfterPlayerTurnStartLate` | 玩家回合开始后（晚） | choiceContext, player | Late |
| `AfterAutoPrePlayPhaseEnteredEarly` | 自动前打出阶段（早） | choiceContext, player | Early |
| `AfterAutoPrePlayPhaseEntered` | 自动前打出阶段 | choiceContext, player | — |
| `AfterAutoPrePlayPhaseEnteredLate` | 自动前打出阶段（晚） | choiceContext, player | Late |
| `AfterAutoPostPlayPhaseEntered` | 自动后打出阶段 | choiceContext, player | — |
| `BeforeSideTurnEndVeryEarly` | 侧回合结束前（极早） | choiceContext, side, participants | Very Early |
| `BeforeSideTurnEndEarly` | 侧回合结束前（早） | choiceContext, side, participants | Early |
| `BeforeSideTurnEnd` | 侧回合结束前 | choiceContext, side, participants | — |
| `AfterSideTurnEnd` | 侧回合结束后 | choiceContext, side, participants | — |

### 12.2 卡牌生命周期

| 钩子 | 时机 |
|------|------|
| `BeforeCardPlayed` | 卡牌打出前 |
| `AfterCardPlayed` | 卡牌打出后 |
| `AfterCardPlayedLate` | 卡牌打出后（晚） |
| `BeforeCardAutoPlayed` | 卡牌自动打出前 |
| `AfterCardDrawnEarly` | 抽牌后（早） |
| `AfterCardDrawn` | 抽牌后 |
| `AfterCardDiscarded` | 弃牌后 |
| `AfterCardExhausted` | 烧牌后 |
| `AfterCardEnteredCombat` | 卡牌进入战斗堆后 |
| `AfterCardGeneratedForCombat` | 玩家生成战斗卡牌后 |
| `AfterCardChangedPiles` | 卡牌改变堆栈后 |
| `AfterCardChangedPilesLate` | 卡牌改变堆栈后（晚） |
| `BeforeCardRemoved` | 从卡组移除卡牌前 |
| `BeforeFlush` | 手牌清空前 |
| `BeforeFlushLate` | 手牌清空前（晚） |
| `AfterFlush` | 手牌清空后 |

### 12.3 伤害/战斗生命周期

| 钩子 | 时机 |
|------|------|
| `BeforeAttack` | 生物攻击前（多段攻击只触发一次） |
| `AfterAttack` | 生物攻击后（多段攻击只触发一次） |
| `BeforeDamageReceived` | 生物受到伤害前 |
| `AfterDamageReceived` | 生物受到伤害后 |
| `AfterDamageReceivedLate` | 生物受到伤害后（晚） |
| `AfterDamageGiven` | 生物造成伤害后 |
| `BeforeBlockGained` | 获得格挡前 |
| `AfterBlockGained` | 获得格挡后 |
| `AfterBlockCleared` | 格挡被清除后 |
| `AfterBlockBroken` | 格挡被击破后 |
| `AfterCurrentHpChanged` | 生物HP变更后（含delta，负=受伤，正=治疗） |
| `BeforeDeath` | 生物死亡前 |
| `AfterDeath` | 生物死亡后 |
| `AfterDiedToDoom` | 生物因Doom死亡后 |
| `AfterCreatureAddedToCombat` | 新生物加入战斗后 |

### 12.4 能力/能量/资源

| 钩子 | 时机 |
|------|------|
| `BeforePowerAmountChanged` | 能力层数变更前 |
| `AfterPowerAmountChanged` | 能力层数变更后 |
| `AfterEnergySpent` | 消耗能量后 |
| `AfterStarsGained` | 获得星星后 |
| `AfterForge` | 锻造触发后 |
| `AfterSummon` | 召唤后 |
| `AfterTakingExtraTurn` | 获得额外回合后 |

### 12.5 房间/地图/非战斗

| 钩子 | 时机 |
|------|------|
| `BeforeRoomEntered` | 进入房间前 |
| `AfterRoomEntered` | 进入房间后 |
| `AfterActEntered` | 进入新幕后 |
| `AfterMapGenerated` | 地图生成后 |
| `BeforeCombatStart` | 战斗开始前 |
| `BeforeCombatStartLate` | 战斗开始前（晚） |
| `AfterCombatEnd` | 战斗结束后 |
| `BeforeCombatRewardOffered` | 战斗奖励提供前 |
| `AfterCombatVictoryEarly` | 战斗胜利后（早） |
| `AfterCombatVictory` | 战斗胜利后 |
| `AfterRestSiteHeal` | 休息处治疗后 |
| `AfterRestSiteSmith` | 休息处锻造后 |
| `AfterItemPurchased` | 购买物品后 |
| `AfterGoldGained` | 获得金币后 |

### 12.6 药水

| 钩子 | 时机 |
|------|------|
| `BeforePotionUsed` | 使用药水前 |
| `AfterPotionUsed` | 使用药水后 |
| `AfterPotionDiscarded` | 丢弃药水后 |
| `AfterPotionProcured` | 获得药水后 |

### 12.7 奖励/修改器回调

| 钩子 | 时机 |
|------|------|
| `AfterRewardTaken` | 领取奖励后 |
| `AfterShuffle` | 洗牌后 |
| `AfterHandEmptied` | 手牌清空后 |
| `AfterOrbChanneled` | 充能球体后 |
| `AfterOrbEvoked` | 激发球体后 |
| `AfterOstyRevived` | Osty 复活后 |
| `AfterPreventingDeath` | 防止死亡后 |
| `AfterPreventingDraw` | 防止抽牌后 |
| `AfterPreventingBlockClear` | 防止清空格挡后 |
| `AfterTargetingBlockedVfx` | 目标被阻挡后的VFX |

### 12.8 数值修改器 (Modify 系列)

这些钩子返回修改后的数值，并在修改后触发 `AfterModifying*` 回调：

| 静态方法 | 用途 | 默认返回 |
|----------|------|----------|
| `Hook.ModifyDamageMultiplicative(...)` | 伤害倍率修正 | 1m |
| `Hook.ModifyDamageAdditive(...)` | 伤害加法修正 | 0m |
| `Hook.ModifyBlockMultiplicative(...)` | 格挡倍率修正 | 1m |
| `Hook.ModifyBlockAdditive(...)` | 格挡加法修正 | 0m |
| `Hook.ModifyPowerAmountGiven(...)` | 施加能力层数修正 | — |
| `Hook.ModifyPowerAmountReceived(...)` | 接收能力层数修正 | — |
| `Hook.ModifyEnergyGain(...)` | 能量获取修正 | — |
| `Hook.ModifyGoldGained(...)` | 金币获取修正 | — |
| `Hook.ModifyShuffleOrder(...)` | 洗牌顺序修正 | — |

---

## 十三、ModManifest 结构 — 从源码提取

```json
{
  "id": "kyxiaofujiu",
  "name": "小夫鸠",
  "author": "YourName",
  "description": "A custom character mod",
  "version": "1.0.0",
  "has_pck": true,
  "has_dll": true,
  "dependencies": [
    { "id": "ModDependencyId", "min_version": "1.0.0" }
  ],
  "affects_gameplay": true,
  "min_game_version": "0.1.0"
}
```

**ModManifest 字段说明:**
| 字段 | 类型 | 说明 |
|------|------|------|
| `id` | string? | 唯一标识 |
| `name` | string? | 显示名称 |
| `author` | string? | 作者 |
| `description` | string? | 描述 |
| `version` | string? | 语义化版本 |
| `has_pck` | bool | 是否包含 Godot 资源包 |
| `has_dll` | bool | 是否包含 DLL |
| `dependencies` | List\<ModDependency\>? | 依赖列表（每个含 id + min_version） |
| `affects_gameplay` | bool | 是否影响游戏玩法（默认 true） |
| `min_game_version` | string? | 最小游戏版本 |

**ModLoadState 枚举:**
```
None          — 未设置
Loaded        — 成功加载
Failed        — 加载失败
Disabled      — 用户禁用
DisabledDuplicate — Steam + 本地重复
AddedAtRuntime    — 运行时添加（太晚）
```

**ModSource 枚举:**
```
None            — 无来源
ModsDirectory   — 本地 mods 目录
SteamWorkshop   — Steam 创意工坊
```

---

## 十四、ValueProp（伤害/格挡属性）— 从源码提取

```csharp
[Flags]
public enum ValueProp
{
    Unblockable   = 2,    // 穿甲（不可格挡，如中毒）
    Unpowered     = 4,    // 不受力量/敏捷修正（如遗物/药水/能力伤害）
    Move          = 8,    // 物理攻击（受力量修正，攻击卡/怪物攻击）
    SkipHurtAnim  = 0x10  // 跳过受击动画
}
```

---

## 十五、CardModel 完整属性 — 从源码提取

```csharp
// 文本/本地化
TitleLocString: LocString     ← "cards.{Id}.title"
Title: string                 ← 卡牌标题（含升级后缀 +）
Description: LocString        ← "cards.{Id}.description"

// 视觉
PortraitPath: string          ← 卡牌画像路径
BetaPortraitPath: string      ← Beta版画像路径
HasPortrait: bool             ← 是否有画像资源
HasBetaPortrait: bool
Frame: Texture2D              ← 卡牌边框纹理
PortraitBorder: Texture2D     ← 画像边框
BannerTexture: Texture2D      ← 稀有度横幅
BannerMaterial: Material
FrameMaterial: Material
EnergyIcon: Texture2D

// 游戏属性
Type: CardType                ← 卡牌类型（Attack/Skill/Power/Status/Curse/Quest）
Rarity: CardRarity            ← 稀有度
TargetType: TargetType        ← 目标类型
EnergyCost: CardEnergyCost    ← 能量消耗系统
CanonicalEnergyCost: int      ← 初始能量消耗
HasEnergyCostX: bool          ← 是否为X费卡牌

// 星级系统 (STS2 新特性)
CanonicalStarCost: int        ← 初始星星消耗（-1表示无）
HasStarCostX: bool            ← 是否为X星星卡牌
BaseStarCost: int             ← 基础星星消耗
CurrentStarCost: int          ← 当前星星消耗
LastStarsSpent: int           ← 上次花费的星星数

// 关键词和标签
Keywords: IReadOnlySet<CardKeyword>
Tags: IEnumerable<CardTag>

// 池归属
Pool: CardPoolModel           ← 所属卡池
VisualCardPool: CardPoolModel ← 视觉上所属的池（可能不同）

// 所有者
Owner: Player                 ← 归属玩家（仅 mutable 实例）
Pile: CardPile?               ← 当前在哪个牌堆

// 多段攻击
BaseReplayCount: int          ← 额外触发次数（默认0）

// 克隆/复制
CloneOf: CardModel?           ← 克隆源
IsDupe: bool                  ← 是否为复制品

// 升级
CurrentUpgradeLevel: int      ← 当前升级等级
MaxUpgradeLevel: int          ← 最大升级等级（1=常规+，>1=如灼热打击）
IsUpgraded: bool              ← 是否已升级
UpgradePreviewType: CardUpgradePreviewType

// 其他
CanBeGeneratedInCombat: bool  ← 能否在战斗内随机生成（默认 true）
CanBeGeneratedByModifiers: bool ← 能否被其他卡牌生成（默认 true）
FloorAddedToDeck: int?        ← 加入卡组时的层数
MultiplayerConstraint: CardMultiplayerConstraint ← 多人限制
```

---

## 十六、RelicModel 完整属性/回调 — 从源码提取

```csharp
// 文本
Title: LocString              ← 含 Wax 前缀逻辑
Flavor: LocString             ← 风味文本
Description: LocString
DynamicDescription: LocString ← 含动态变量

// 视觉
Icon: Texture2D               ← 小图标
IconOutline: Texture2D        ← 轮廓图标
BigIcon: Texture2D            ← 大图标
PackedIconPath: string        ← 图集图标路径
IconPath: string

// 游戏属性
Rarity: RelicRarity
IsTradable: bool              ← 是否可交易
IsAllowedInShops: bool        ← 是否在商店出现（默认 true）
IsStackable: bool             ← 是否可叠加（如 Circlet）
IsUsedUp: bool                ← 是否已用尽
HasUponPickupEffect: bool     ← 是否有拾取效果
SpawnsPets: bool              ← 是否生成宠物
AddsPet: bool
IsWax: bool                   ← 蜡化遗物
IsMelted: bool                ← 已熔化（不可用）
StackCount: int               ← 叠加计数
ShowCounter: bool             ← 是否显示计数器
DisplayAmount: int            ← 计数器数值
FlashSfx: string              ← 激活音效（默认 "event:/sfx/ui/relic_activate_general"）
ShouldFlashOnPlayer: bool     ← 是否在玩家身上闪烁（默认 true）

// 经济
MerchantCost: int             ← 商店价格（由稀有度自动决定）

// 状态
Status: RelicStatus           ← Normal / Active / Disabled
FloorAddedToDeck: int

// 完整回调列表
public virtual Task AfterObtained()                    // 获得遗物后
public virtual Task AfterRemoved()                     // 移除遗物后
public virtual bool IsAllowed(IRunState runState)       // 是否允许生成（默认 true）
public virtual bool IsAllowedAtNeow(Player player)      // Neow 是否允许

// 辅助方法
Flash()                                                // 闪烁
Flash(IEnumerable<Creature> targets)                   // 闪烁（指定目标）
InvokeDisplayAmountChanged()                           // 更新计数器显示
UpdateTexture(TextureRect)                             // 更新材质纹理
IncrementStackCount()                                  // 增加堆叠计数
RemoveInternal()                                       // 内部移除
ToSerializable()                                       // 序列化
FromSerializable(SerializableRelic)                    // 反序列化
RelicIconChanged()                                     // 图标变更通知
```

---

## 十七、PowerModel 完整属性/回调 — 从源码提取

```csharp
// 文本
Title: LocString              ← "powers.{Id}.title"
Description: LocString        ← "powers.{Id}.description"
SmartDescription: LocString   ← 智能描述（含动态变量）
HasSmartDescription: bool
RemoteDescription: LocString  ← 多人模式远程描述

// 视觉
Icon: Texture2D
BigIcon: Texture2D
PackedIconPath: string
HasSmartDescription: bool

// 核心属性
Amount: int                   ← 层数
AmountOnTurnStart: int        ← 回合开始时的层数
DisplayAmount: int            ← 显示层数（默认 = Amount）
Type: PowerType               ← Buff / Debuff
StackType: PowerStackType     ← 叠加方式
InstanceType: PowerInstanceType ← 实例类型
IsVisible: bool               ← 是否可见
ShouldPlayVfx: bool           ← 是否播放VFX
Owner: Creature               ← 所属生物
Applier: Creature?            ← 施加者
Target: Creature?             ← 目标（多人模式实例化能力用）
CombatState: ICombatState     ← 所属战斗
AllowNegative: bool           ← 是否允许负值
TypeForCurrentAmount: PowerType ← 根据当前层数判断的Type
SkipNextDurationTick: bool    ← 是否跳过下个持续刻
ShouldScaleInMultiplayer: bool ← 多人模式是否缩放

// 完整回调列表
public virtual Task BeforeApplied(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
    // 首次应用前（已存在则叠加时不会调用）

public virtual Task AfterApplied(Creature? applier, CardModel? cardSource)
    // 首次应用后

public virtual Task AfterStacked(Creature? applier, CardModel? cardSource)
    // 叠加层数后

public virtual Task AfterRemoved(Creature oldOwner)
    // 移除后

public virtual Task AfterDurationChanged(int newDuration)
    // 持续时间变更后

public virtual bool ShouldPowerBeRemovedAfterOwnerDeath()
    // 主人死后是否移除（默认 true）

public virtual bool ShouldOwnerDeathTriggerFatal()
    // 主人死亡是否触发致命效果（默认 true）

public virtual bool OwnerIsSecondaryEnemy
    // 是否将敌人归类为次要敌人

// 内部数据
protected virtual object? InitInternalData()           // 初始化私有数据
protected T GetInternalData<T>()                       // 获取私有数据

// 辅助方法
Flash()                                                // 闪烁
InvokeDisplayAmountChanged()                           // 更新显示
StartPulsing() / StopPulsing()                         // 脉冲动画控制
ShouldRemoveDueToAmount()                              // 层数变更后是否应移除
public PowerType GetTypeForAmount(decimal amount)       // 根据层数获取类型
GetScaledAmountForMultiplayer(...)                     // 多人缩放
```

---

## 十八、MonsterModel 怪物模型

**命名空间:** `MegaCrit.Sts2.Core.Models`

```csharp
public abstract class MonsterModel : AbstractModel
{
    public virtual LocString Title         // "monsters.{Id}.name"
    public abstract int MinInitialHp       // 最小初始HP
    public abstract int MaxInitialHp       // 最大初始HP
    public virtual bool IsHealthBarVisible => true
    
    public Rng Rng                         // 怪物专用RNG
    public RunRngSet RunRng                // 运行RNG集
    
    public bool IsPerformingMove           // 是否正在执行行动
    
    public virtual Vector2 ExtraDeathVfxPadding
    public virtual float HpBarSizeReduction
}
```

---

## 十九、CharacterModel 完整属性 — 从源码提取

```csharp
public abstract class CharacterModel : AbstractModel
{
    // 基本属性
    public virtual bool IsPlayable => true       // 是否可选
    public abstract Color NameColor              // 统计界面角色名颜色
    public abstract CharacterGender Gender       // 性别（语法用）
    public abstract int StartingHp               // 起始HP
    public abstract int StartingGold             // 起始金币
    public virtual int MaxEnergy => 3            // 最大能量
    public virtual int BaseOrbSlotCount => 0     // 基础球位（Defect用）
    
    // 文本
    public LocString Title                       // "characters.{Id}.title"
    public LocString TitleObject                 // 宾格形式标题
    public LocString PronounObject               // 代词的宾格
    public LocString PossessiveAdjective         // 所有格形容词
    public LocString PronounPossessive           // 名词性物主代词
    public LocString PronounSubject              // 主格代词
    
    // 池
    public abstract CardPoolModel CardPool       // 角色卡池
    public abstract RelicPoolModel RelicPool     // 遗物池
    public abstract PotionPoolModel PotionPool   // 药水池
    public abstract IEnumerable<CardModel> StartingDeck    // 初始卡组
    public abstract IReadOnlyList<RelicModel> StartingRelics  // 初始遗物
    public virtual IReadOnlyList<PotionModel> StartingPotions // 初始药水
    
    // 视觉路径
    public string TrailPath                      // 卡牌拖尾路径
    public string EnergyCounterPath              // 能量计数器场景
    public string MerchantAnimPath               // 商人动画
    public string RestSiteAnimPath               // 休息处动画
    public string CharacterSelectBg              // 角色选择背景
    public string CharacterSelectTransitionPath  // 过渡材质
    public string MapMarkerPath                  // 地图标记
    
    // 视觉资源
    public Texture2D IconTexture
    public Texture2D IconOutlineTexture
    public Control Icon
    public CompressedTexture2D CharacterSelectIcon
    public CompressedTexture2D CharacterSelectLockedIcon
    
    // 多人模式手势纹理
    public Texture2D ArmPointingTexture
    public Texture2D ArmRockTexture
    public Texture2D ArmPaperTexture
    public Texture2D ArmScissorsTexture
    
    // 对话颜色
    public virtual Color DialogueColor           // 古代对话气泡颜色
    public virtual VfxColor SpeechBubbleColor    // 对话气泡颜色
}
```

---

## 二十、ModelDb 完整 API — 从源码提取

```csharp
public static class ModelDb
{
    // 所有模型实例
    public static IEnumerable<AbstractModel> All
    
    // 查询所有已注册子类型
    public static Type[] AllAbstractModelSubtypes
    
    // 各类模型查询
    public static IEnumerable<CharacterModel> AllCharacters
    public static IEnumerable<CardModel> AllCards
    public static IEnumerable<CardPoolModel> AllCardPools
    public static IEnumerable<RelicModel> AllRelics
    public static IEnumerable<RelicPoolModel> AllRelicPools
    public static IEnumerable<PowerModel> AllPowers
    public static IEnumerable<PotionModel> AllPotions
    public static IEnumerable<PotionPoolModel> AllPotionPools
    public static IEnumerable<EncounterModel> AllEncounters
    public static IEnumerable<EventModel> AllEvents
    public static IEnumerable<ActModel> Acts
    public static IEnumerable<OrbModel> Orbs
    public static IEnumerable<MonsterModel> Monsters
    public static IReadOnlyList<BadgeModel> BadgeModels
    public static IReadOnlyList<AchievementModel> Achievements
    
    // 池分类查询（非角色专属）
    public static IEnumerable<CardPoolModel> AllSharedCardPools
    public static IEnumerable<CardPoolModel> AllCharacterCardPools
    public static IEnumerable<RelicPoolModel> AllSharedRelicPools
    public static IEnumerable<RelicPoolModel> AllCharacterRelicPools
    
    // 泛型查询（推荐方式）
    public static T Card<T>() where T : CardModel
    public static T Relic<T>() where T : RelicModel
    public static T Power<T>() where T : PowerModel
    public static T Character<T>() where T : CharacterModel
    public static T CardPool<T>() where T : CardPoolModel
    public static T RelicPool<T>() where T : RelicPoolModel
    public static T PotionPool<T>() where T : PotionPoolModel
    public static T Encounter<T>() where T : EncounterModel
    public static T Event<T>() where T : EventModel
    public static T Act<T>() where T : ActModel
    public static T Monster<T>() where T : MonsterModel
    public static T Modifier<T>() where T : ModifierModel
    public static T AncientEvent<T>() where T : AncientEventModel
    public static T Orb<T>() where T : OrbModel
    
    // ID 查询
    public static T GetById<T>(ModelId id) where T : AbstractModel
    public static T? GetByIdOrNull<T>(ModelId id) where T : AbstractModel
    public static ModelId GetId(Type type)
    public static ModelId GetId<T>() where T : AbstractModel
    public static bool Contains(Type type)
    
    // 测试/调试用
    public static IEnumerable<AfflictionModel> DebugAfflictions
    public static IEnumerable<EnchantmentModel> DebugEnchantments
}
```

---

## 二十一、ModHelper 官方 API 详解

```csharp
public static class ModHelper
{
    // 添加模型到池（初始化前调用）
    public static void AddModelToPool<TPoolType, TModelType>()
        where TPoolType : AbstractModel, IPoolModel
        where TModelType : AbstractModel

    public static void AddModelToPool(Type poolType, Type modelType)

    // 池内部消费（由池的 GenerateAllCards 调用）
    public static IEnumerable<TModelType> ConcatModelsFromMods<TModelType>(
        IPoolModel poolModel, IEnumerable<TModelType> pool)
        where TModelType : AbstractModel

    // 订阅 RunState 钩子
    public static void SubscribeForRunStateHooks(
        string id, RunHookSubscriptionDelegate del)
    // 委托签名: IEnumerable<AbstractModel> RunHookSubscriptionDelegate(RunState runState)

    // 订阅 CombatState 钩子
    public static void SubscribeForCombatStateHooks(
        string id, CombatHookSubscriptionDelegate del)
    // 委托签名: IEnumerable<AbstractModel> CombatHookSubscriptionDelegate(CombatState combatState)
}
```

---

## 二十二、常用 using 指令完整版

```csharp
using MegaCrit.Sts2.Core.Models;                          // 所有 Model 基类
using MegaCrit.Sts2.Core.Models.Cards;                    // 具体卡牌（如 Dagongren）
using MegaCrit.Sts2.Core.Models.Powers;                   // 具体能力（如 StrengthPower）
using MegaCrit.Sts2.Core.Models.Relics;                   // 具体遗物
using MegaCrit.Sts2.Core.Models.CardPools;                // 卡池
using MegaCrit.Sts2.Core.Models.RelicPools;               // 遗物池
using MegaCrit.Sts2.Core.Models.PotionPools;              // 药水池
using MegaCrit.Sts2.Core.Models.Characters;               // 角色
using MegaCrit.Sts2.Core.Models.Monsters;                 // 怪物
using MegaCrit.Sts2.Core.Models.Orbs;                     // 球体
using MegaCrit.Sts2.Core.Commands;                        // 命令系统
using MegaCrit.Sts2.Core.Commands.Builders;               // 命令Builder
using MegaCrit.Sts2.Core.Entities.Creatures;              // Creature
using MegaCrit.Sts2.Core.Entities.Powers;                 // Power 实体
using MegaCrit.Sts2.Core.Entities.Relics;                 // Relic 实体
using MegaCrit.Sts2.Core.Entities.Cards;                  // CardPlay, CardPile, PileType
using MegaCrit.Sts2.Core.Entities.Players;                // Player
using MegaCrit.Sts2.Core.Combat;                          // CombatState, CombatSide, ICombatState
using MegaCrit.Sts2.Core.Hooks;                           // Hook 系统
using MegaCrit.Sts2.Core.ValueProps;                      // ValueProp 枚举
using MegaCrit.Sts2.Core.Modding;                         // Mod, ModHelper, ModInitializerAttribute
using MegaCrit.Sts2.Core.Localization;                   // LocString
using MegaCrit.Sts2.Core.Localization.DynamicVars;        // DynamicVar, DamageVar, BlockVar, EnergyVar
using MegaCrit.Sts2.Core.GameActions.Multiplayer;         // PlayerChoiceContext, CardPlay
using MegaCrit.Sts2.Core.HoverTips;                      // HoverTip, IHoverTip
using MegaCrit.Sts2.Core.Logging;                        // Log.Info/Error
using MegaCrit.Sts2.Core.Runs;                           // IRunState, RunState
using MegaCrit.Sts2.Core.Rooms;                          // AbstractRoom, CombatRoom
using MegaCrit.Sts2.Core.Rewards;                        // Reward, RewardsSet
using MegaCrit.Sts2.Core.Random;                         // Rng
using MegaCrit.Sts2.Core.Saves;                          // SavedProperty, SerializableRelic
using MegaCrit.Sts2.Core.Saves.Runs;                     // RunState save
using MegaCrit.Sts2.Core.Context;                        // LocalContext
using MegaCrit.Sts2.Core.Assets;                         // ImageHelper, SceneHelper
using MegaCrit.Sts2.Core.Helpers;                        // 各种工具方法
using MegaCrit.Sts2.Core.Extensions;                     // 扩展方法
using MegaCrit.Sts2.Core.Nodes.Combat;                   // CombatManager
using Harmonylib;                                        // Harmony Patch
using static MegaCrit.Sts2.Core.Commands.PlayerCmd;      // GainEnergy 等静态导入
```

---

## 二十三、开发最佳实践速查

### 获取玩家/生物/CardPlay中的上下文
```csharp
// 在 OnPlay 中:
var player = cardPlay.Player;                    // 打出卡牌的玩家
var target = cardPlay.Target;                    // 卡牌目标
var ownerCreature = Owner.Creature;              // 卡牌主人的Creature
var combatState = ownerCreature.CombatState;     // CombatState（可能null）
var runState = Owner.RunState;                   // RunState

// 在遗物/能力回调中:
var player = Owner;                              // 对于 RelicModel
var creature = Owner;                            // 对于 PowerModel
var combatState = creature.CombatState;
```

### 多人模式注意事项
- 使用 `LocalContext.IsMe(creature/player)` 检查是否为本地玩家
- 使用 `LocalContext.NetId` 获取本地玩家网络ID
- 能力设置 `ShouldScaleInMultiplayer = true` 自动缩放到多人
- `MultiplayerScalingModel.GetMultiplayerScaling()` 获取缩放倍率

### 调试技巧
```csharp
using MegaCrit.Sts2.Core.Logging;
Log.Info("Some debug message");                  // 输出日志
Log.Error("Something went wrong");               // 错误日志
```

### 安全性检查
```csharp
// 战斗是否即将结束
if (CombatManager.Instance.IsOverOrEnding) return;
if (CombatManager.Instance.IsEnding) return;

// 目标是否可接收能力
if (!target.CanReceivePowers) return;

// 断言模型状态
model.AssertMutable();       // 确保是可变实例
model.AssertCanonical();     // 确保是原型实例
```

---

*此文档供 Agent 和开发者参考，基于对 STS2 源码（路径 `$STS2_SRC`，因机器而异）分析生成。*  
*最后更新: 2026-07-19*
6. `TokenCardPool.GenerateAllCards` — 注入 Jiaban 状态卡
7. `EpochModel.Get` — 注入 3 个自定义纪元
8. `ProgressSaveManager.CheckFifteenElitesDefeatedEpoch` — 跳过纪元检测
9. `ProgressSaveManager.CheckFifteenBossesDefeatedEpoch` — 跳过纪元检测
10. `EpochModel.AllEpochIds` — 注册纪元ID
11. `PlayerCmd.GainGold` — 触发差偏认知征婚
12. `ArchaicTooth.TranscendenceUpgrades` — 欧罗巴斯遗物
13. `PersonalHivePower.AfterDamageReceived` — 宠物攻击防崩溃

---

## 九、常用开发模式速查

### 添加新卡牌
1. 创建 `class MyCard : CardModel`
2. 定义 `CanonicalVars`（伤害/格挡等）
3. 实现 `OnPlay`（卡牌效果）
4. 实现 `OnUpgrade`（升级效果）
5. 加入 `XiaofujiuCardPool.GenerateAllCards()`
6. 如果需要也能在其他地方生成，加到对应的 CardPool 或 TokenCardPool Patch

### 添加新遗物
1. 创建 `class MyRelic : RelicModel`
2. 设 `Rarity`
3. 覆写需要的回调（`AfterDamageReceived`, `ModifyDamageMultiplicative` 等）
4. 加入 `XiaofujiuRelicPool`
5. 如果需要在开局给予，在 `Xiaofujiu.StartingRelics` 里添加

### 添加新能力 (Power)
1. 创建 `class MyPower : PowerModel`
2. 设定 `Type` 和 `StackType`
3. 覆写需要的回调（`AfterApplied`, `OnTurnEnd` 等）
4. 能力由卡牌或遗物通过 `PowerCmd.Apply<T>()` 施加

### 引用游戏 API 的实用 using
```csharp
using MegaCrit.Sts2.Core.Models;          // 所有 Model 基类
using MegaCrit.Sts2.Core.Commands;        // 命令系统
using MegaCrit.Sts2.Core.Entities.Creatures;  // Creature
using MegaCrit.Sts2.Core.Entities.Powers;     // Power 实体
using MegaCrit.Sts2.Core.Entities.Relics;     // Relic 实体
using MegaCrit.Sts2.Core.Entities.Cards;      // CardPlay, CardPile...
using MegaCrit.Sts2.Core.Entities.Players;    // Player
using MegaCrit.Sts2.Core.Combat;             // CombatState, CombatSide...
using MegaCrit.Sts2.Core.Hooks;              // Hook 系统
using MegaCrit.Sts2.Core.ValueProps;         // ValueProp 枚举
using MegaCrit.Sts2.Core.Modding;            // Mod, ModHelper
using MegaCrit.Sts2.Core.Localization.DynamicVars;  // DynamicVar 系统
using MegaCrit.Sts2.Core.GameActions.Multiplayer;   // PlayerChoiceContext, CardPlay
using MegaCrit.Sts2.Core.HoverTips;          // HoverTip 系统
using MegaCrit.Sts2.Core.Logging;            // Log.Info/Error
using Harmonylib;                            // Harmony Patch
```

---

## 十、开发流程

```
1. 编写 C# 类（卡牌/遗物/能力）
2. 确保类在卡池/遗物池中被引用
3. 如果是全新的内容类型（新角色/新卡池），确认 Harmony Patch 已注册
4. 编译 DLL: dotnet build → kyxiaofujiu.dll
5. 更新 .pck（如果有新的 Godot 资源）
6. 复制 DLL/PCK 到游戏 mods 目录（路径因机器而异）
7. 重启游戏测试
```

---

*此文档供 Agent 和开发者参考，基于反编译的 STS2 源码。*
