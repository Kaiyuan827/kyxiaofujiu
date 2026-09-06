using Godot;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace kyxiaofujiu.utils
{
	public static class CustomAudioManager
	{
		/// <summary>
		/// 播放自定义音效（支持 mp3/wav/ogg）
		/// </summary>
		/// <param name="path">音效文件路径，如 "res://audio/jimi.mp3"</param>
		public static void PlaySfx(string path)
		{
			if (NCombatRoom.Instance == null)
			{
				Log.Warn($"CustomAudioManager: NCombatRoom.Instance 为空，无法播放音效 {path}");
				return;
			}

			var audioPlayer = new AudioStreamPlayer();
			audioPlayer.Stream = ResourceLoader.Load<AudioStream>(path);
			
			if (audioPlayer.Stream == null)
			{
				Log.Error($"CustomAudioManager: 无法加载音效 {path}");
				audioPlayer.QueueFree();
				return;
			}

			NCombatRoom.Instance.AddChild(audioPlayer);
			audioPlayer.Play();

			// 播放完成后自动清理
			_ = Task.Run(async () =>
			{
				// ✅ 显式转换为 float
				float waitTime = (float)audioPlayer.Stream.GetLength();
				if (waitTime <= 0) waitTime = 2f;
				await Cmd.Wait(waitTime + 0.1f);
				audioPlayer.QueueFree();
			});
		}

		/// <summary>
		/// 播放自定义音效（指定音量）
		/// </summary>
		public static void PlaySfx(string path, float volumeDb)
		{
			if (NCombatRoom.Instance == null) return;

			var audioPlayer = new AudioStreamPlayer();
			audioPlayer.Stream = ResourceLoader.Load<AudioStream>(path);
			
			if (audioPlayer.Stream == null)
			{
				Log.Error($"CustomAudioManager: 无法加载音效 {path}");
				audioPlayer.QueueFree();
				return;
			}

			audioPlayer.VolumeDb = volumeDb;
			NCombatRoom.Instance.AddChild(audioPlayer);
			audioPlayer.Play();

			_ = Task.Run(async () =>
			{
				// ✅ 显式转换为 float
				float waitTime = (float)audioPlayer.Stream.GetLength();
				if (waitTime <= 0) waitTime = 2f;
				await Cmd.Wait(waitTime + 0.1f);
				audioPlayer.QueueFree();
			});
		}

		private static AudioStreamPlayer? _bgmPlayer;

		/// <summary>
		/// 播放自定义背景音乐（循环），如 "res://audio/xxx.mp3"
		/// </summary>
		public static void PlayBgm(string path)
		{
			StopBgm();
			if (NCombatRoom.Instance == null)
			{
				Log.Warn($"CustomAudioManager: NCombatRoom.Instance 为空，无法播放BGM {path}");
				return;
			}

			var player = new AudioStreamPlayer();
			player.Stream = ResourceLoader.Load<AudioStream>(path);

			if (player.Stream == null)
			{
				Log.Error($"CustomAudioManager: 无法加载BGM {path}");
				player.QueueFree();
				return;
			}

			// 播放结束后重新播放实现循环
			player.Finished += () =>
			{
				if (GodotObject.IsInstanceValid(player) && player.Stream != null)
				{
					player.Play();
				}
			};

			NCombatRoom.Instance.AddChild(player);
			player.Play();
			_bgmPlayer = player;
		}

		/// <summary>
		/// 停止自定义背景音乐
		/// </summary>
		public static void StopBgm()
		{
			if (_bgmPlayer != null)
			{
				if (GodotObject.IsInstanceValid(_bgmPlayer))
				{
					_bgmPlayer.Stop();
					_bgmPlayer.QueueFree();
				}
				_bgmPlayer = null;
			}
		}
	}
}
