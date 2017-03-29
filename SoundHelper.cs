﻿using Terraria;
using Terraria.ModLoader;


namespace TheDeconstructor
{
	internal static class SoundHelper
	{
		internal static string[] Sounds =
		{
			"CloseUI",
			"Decline",
			"Notif",
			"OpenUI",
			"Receive",
			"Redeem"
		};

		internal enum SoundType
		{
			CloseUI,
			Decline,
			Notif,
			OpenUI,
			Receive,
			Redeem
		}

		internal static void PlaySound(SoundType type)
		{
			Main.PlaySound(SoundLoader.customSoundType, -1, -1,
				TheDeconstructor.instance.GetSoundSlot(Terraria.ModLoader.SoundType.Custom, "Sounds/Custom/" + Sounds[(int)type]));
		}
	}
}
