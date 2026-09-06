using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using kyxiaofujiu.core;

namespace kyxiaofujiu.powers
{
    /// <summary>
    /// 夫化能力 — 夫黑可免费打出（费用0），打出后进消耗堆（参考 Corruption）
    /// 夫黑以自身状态打出，不触发转化，故不触发创世夫柱
    /// 可打出性由 XiaofujiuCardBase.IsPlayable 动态检查持有者是否有本能力决定
    /// 免费打出：TryModifyEnergyCostInCombat 把持有者的夫黑卡费用改为 0
    /// </summary>
    public sealed class FuhuaPower : PowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.None;

        // 持有者的夫黑卡在战斗中免费打出（费用 → 0）
        public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
        {
            modifiedCost = originalCost;
            if (card.Owner?.Creature != Owner) return false;
            if (card is not XiaofujiuCardBase fc || !fc.IsFufuBlack) return false;
            modifiedCost = 0m;
            return true;
        }

        // 夫黑打出后 → 改目的地为消耗堆（在 GetResultLocationForCardPlay 之后调用）
        public override CardLocation ModifyCardPlayResultLocation(CardModel card, bool isAutoPlay, ResourceInfo resources, CardLocation cardLocation)
        {
            if (card is XiaofujiuCardBase fc && fc.IsFufuBlack && cardLocation.pileType == PileType.Discard)
                cardLocation.pileType = PileType.Exhaust;
            return cardLocation;
        }
    }
}
