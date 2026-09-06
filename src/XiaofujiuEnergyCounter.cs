using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace kyxiaofujiu.ui
{
	/// <summary>
	/// 晓夫九能量计数器：工资槽节点在场景里静态声明（xiaofujiu_energy_counter.tscn 的 SalarySlot），
	/// 引擎在战斗资源预加载时创建，代码只负责刷新工资数字。
	/// 避免运行时动态创建/同步加载资源（曾导致进入战斗卡死）。
	/// </summary>
	public partial class XiaofujiuEnergyCounter : NEnergyCounter
	{
		private Label? _salaryLabel;
		private object? _playerRef;
		private int _lastSalary = -1;
		private static bool _vfxIssueLogged;

		public override void _Ready()
		{
			try
			{
				base._Ready();
			}
			catch (System.Exception ex)
			{
				// 游戏基类 NEnergyCounter._Ready() 会把 %EnergyVfxBack / %EnergyVfxFront 强转成 NParticlesContainer，
				// 但 mod 场景实例化后这俩节点因根场景类型解析失败而变成 Godot.Control（Godot 用占位符替代），
				// 强转抛 InvalidCastException，导致 _Ready 中断。
				// 能量数字/图层/旋转在前几行已赋值、_EnterTree 也已连好能量变化事件，
				// 所以吞掉异常即可让计数器继续工作，只是能量爆闪 VFX 与悬停气泡不可用。
				// 若要恢复 VFX，需修正 xiaofujiu_energy_counter.tscn 里 EnergyVfxBack/Front 的节点类型（见 dev_log）。
				if (!_vfxIssueLogged)
				{
					_vfxIssueLogged = true;
					MegaCrit.Sts2.Core.Logging.Log.Warn($"[XiaofujiuEnergyCounter] 基类能量计数器初始化异常（VFX 节点非 NParticlesContainer），VFX/悬停已降级：{ex.Message}");
				}
			}
			// 工资槽 label 在场景里静态声明；这里 GetNode 可能被 base._Ready() 的 VFX 异常中断，
			// 改在 _Process 里兜底获取（GetNodeOrNull 不抛异常）。
		}

		public override void _Process(double delta)
		{
			base._Process(delta);
			if (_salaryLabel == null)
			{
				_salaryLabel = GetNodeOrNull<Label>("%SalaryLabel");
				if (_salaryLabel == null) return;
			}
			RefreshSalary();
		}

		private void RefreshSalary()
		{
			if (_salaryLabel == null) return;
			try
			{
				if (_playerRef == null)
				{
					var f = typeof(NEnergyCounter).GetField("_player", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
					_playerRef = f?.GetValue(this);
				}
				if (_playerRef == null) return;
				int salary = GetSalary(_playerRef);
				if (salary != _lastSalary)
				{
					_lastSalary = salary;
					_salaryLabel!.Text = salary.ToString();
				}
			}
			catch
			{
			}
		}

		private static int GetSalary(object player)
		{
			var relics = player.GetType().GetProperty("Relics")?.GetValue(player) as System.Collections.IEnumerable;
			if (relics == null) return 0;
			foreach (var r in relics)
			{
				if (r != null && r.GetType().Name == "LuckyLandlordRelic")
				{
					if (r.GetType().GetProperty("Salary")?.GetValue(r) is int s) return s;
				}
			}
			return 0;
		}
	}
}
