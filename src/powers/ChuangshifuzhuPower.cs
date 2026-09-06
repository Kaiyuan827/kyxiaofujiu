using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using kyxiaofujiu.core;
using kyxiaofujiu.utils;

namespace kyxiaofujiu.powers
{
    /// <summary>
    /// 创世夫柱能力 — 每次转换时获得 Amount 点格挡
    /// Amount 叠加：2张则每转换1张得 8 格挡
    /// </summary>
    public sealed class ChuangshifuzhuPower : PowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;

        protected override IEnumerable<IHoverTip> ExtraHoverTips
        {
            get
            {
                foreach (var tip in HoverTipHelper.ConvertTips) yield return tip;
                yield return HoverTipFactory.Static(StaticHoverTip.Block);
            }
        }

        /// <summary>
        /// 由 XiaofujiuCardBase.OnFufuStateChanged 直接调用（非静态事件，随战斗结束自动清理）
        /// </summary>
        public void OnFufuToggled(CardModel card, FufuState oldState, FufuState newState)
        {
            if (Amount <= 0) return;
            TaskHelper.RunSafely(GrantBlock());
        }

        private async Task GrantBlock()
        {
            if (Owner == null || Owner.IsDead) return;
            // 被动触发的格挡不受脆弱影响（脆弱只影响打出的格挡牌）
            await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Move | ValueProp.Unpowered, null);
        }
    }
}
