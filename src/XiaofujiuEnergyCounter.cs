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

		public override void _Ready()
		{
			base._Ready();
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
