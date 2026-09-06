using Godot;
using Godot.Collections;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;

namespace kyxiaofujiu.ui
{
	/// <summary>
	/// 模组侧粒子容器：_Ready 时自动收集全部 GpuParticles2D 子节点写入 _particles，
	/// 兜底 tscn 导出绑定失败的情况，保证基类 Restart()/SetEmitting() 不空引用。
	/// </summary>
	public partial class XiaofujiuParticlesContainer : NParticlesContainer
	{
		public override void _Ready()
		{
			BindParticles();
		}

		private void BindParticles()
		{
			var list = new Array<GpuParticles2D>();
			foreach (var child in GetChildren())
			{
				Collect(child, list);
			}
			var field = typeof(NParticlesContainer).GetField("_particles",
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			field?.SetValue(this, list);
		}

		private void Collect(Node node, Array<GpuParticles2D> list)
		{
			if (node is GpuParticles2D gp) list.Add(gp);
			foreach (var c in node.GetChildren()) Collect(c, list);
		}
	}
}
