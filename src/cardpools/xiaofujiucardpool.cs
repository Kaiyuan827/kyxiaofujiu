using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;


namespace kyxiaofujiu.cardpools
{
	public sealed class XiaofujiuCardPool : CardPoolModel
	{
		public override string Title => "xiaofujiu";
		public override string EnergyColorName => "xiaofujiu";
		public override string CardFrameMaterialPath => "card_frame_yellow";
		public override Color DeckEntryCardColor => new Color("ba7d14ff");
		public override Color EnergyOutlineColor => new Color("995000ff");
		public override bool IsColorless => false;

		protected override CardModel[] GenerateAllCards()
		{
			return new CardModel[]
			{
				ModelDb.Card<Strikexiaofujiu>(),
				ModelDb.Card<Defendxiaofujiu>(),
				ModelDb.Card<Heizhuanbaizhuanhei>(),
				ModelDb.Card<Changkao>(),
				ModelDb.Card<Fuheishuohua>(),
				ModelDb.Card<Oniupi>(),
				ModelDb.Card<Nongbai>(),
				ModelDb.Card<Fubaimaomei>(),
				ModelDb.Card<Dapaitaiwenle>(),
				ModelDb.Card<Bzhanzhenghun>(),
				ModelDb.Card<Huoduilingdianwu>(),
				ModelDb.Card<Baohei>(),
				ModelDb.Card<Fuhuangsc>(),
				ModelDb.Card<Fengkuangxuanzhuan>(),
				ModelDb.Card<Zhuangtang>(),
				ModelDb.Card<Xiaojiujiu>(),
				ModelDb.Card<Jueburenshu>(),
				ModelDb.Card<Yayi>(),
				ModelDb.Card<Dachangchengxuyuan>(),
				ModelDb.Card<Dagongren>(),
				ModelDb.Card<Xiatounan>(),
				ModelDb.Card<Qinggetanchang>(),
				ModelDb.Card<Zhenghunzhe>(),
				ModelDb.Card<Sanshisuidenanren>(),
				ModelDb.Card<Biedaraofuge>(),
				ModelDb.Card<Yayixingtai>(),
				ModelDb.Card<Xiaofujiushiliansheng>(),
				ModelDb.Card<Nizenmehaizaizhe>(),
				ModelDb.Card<Wushisc>(),
				ModelDb.Card<Chidafen>(),
				ModelDb.Card<Caokongyaokongqi>(),
				ModelDb.Card<Neimaerdexinwei>(),
				ModelDb.Card<Baomi>(),
				ModelDb.Card<Fubaishangong>(),
				ModelDb.Card<Chapianrenzhi>(),
				ModelDb.Card<Tianxuandagongren>(),
				ModelDb.Card<Jimi>(),
				ModelDb.Card<Shuangmianfuge>(),
				ModelDb.Card<Yiciyige>(),
				ModelDb.Card<Yaojiajiujia>(),
				ModelDb.Card<Fugebaoshe>(),
				ModelDb.Card<Muhouheishou>(),
				ModelDb.Card<Tufaeji>(),
				ModelDb.Card<Ktvgewang>(),
				ModelDb.Card<Zhenggewelai>(),
				ModelDb.Card<Zhengchangjianghua>(),
				ModelDb.Card<Daibaduodai>(),
				ModelDb.Card<Diyukuangfu>(),
				ModelDb.Card<Fugedajianbing>(),
				ModelDb.Card<Kumingyuanyang>(),
				ModelDb.Card<Datouerzi>(),
				ModelDb.Card<Huikajiu>(),
				ModelDb.Card<Shiwanfenfuli>(),
				ModelDb.Card<Chunqingnvda>(),
				ModelDb.Card<Chuangshifuzhu>(),
				ModelDb.Card<Sanpaidayuqieqie>(),
				ModelDb.Card<Caitoudazhan>(),
				ModelDb.Card<Gangzuodemeijia>(),
				ModelDb.Card<Fuhua>(),
				ModelDb.Card<Tuiding>(),
				ModelDb.Card<Bangyishenmedongxi>(),
				ModelDb.Card<Fudenver>(),
				ModelDb.Card<Niannongshi>(),
				ModelDb.Card<Zhuizongzhishe>(),
				ModelDb.Card<Lejinwo>(),
				ModelDb.Card<Buweinierersi>(),
				ModelDb.Card<Yongweimi>(),
				ModelDb.Card<Wuzhonghenyi>(),
				ModelDb.Card<Lage>(),
				ModelDb.Card<Zhandouzhiying>(),
				ModelDb.Card<Rufumen>(),
				ModelDb.Card<Kaifuyan>(),
				ModelDb.Card<Suixinzhe>(),
				ModelDb.Card<Nongfushebao>(),
				ModelDb.Card<Tiantiantian>(),
				ModelDb.Card<Kukuku>(),
				ModelDb.Card<Touzi>(),
				ModelDb.Card<Niuma>(),
				ModelDb.Card<Fangganfuxue>(),
				ModelDb.Card<Oujinjin>(),
				ModelDb.Card<Liangyuansc>(),
				ModelDb.Card<Shututonggui>(),
				ModelDb.Card<Wuqunlaile>(),
				ModelDb.Card<Huangpifucaidei>(),
			};
		}
	}
}
