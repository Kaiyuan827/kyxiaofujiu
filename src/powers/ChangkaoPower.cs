using MegaCrit.Sts2.Core.Models;  // ✅ AbstractModel 在此
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using kyxiaofujiu.cardpools;  // ✅ Changkao 卡牌在此

namespace kyxiaofujiu.powers
{
	public sealed class ChangkaoPower : TemporaryDexterityPower
	{
		public override AbstractModel OriginModel => ModelDb.Card<Changkao>();
	}
}
