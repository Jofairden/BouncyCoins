using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.UI;

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

	public class TheDeconstructor : Mod
	{
		internal UserInterface deconUI;
		internal DeconstructorGUI deconGUI;
		internal static TheDeconstructor instance;

		public TheDeconstructor()
		{
			Properties = new ModProperties()
			{
				Autoload = true,
				AutoloadSounds = true
			};
		}

		public override void Load()
		{
			instance = this as TheDeconstructor;

			if (Main.dedServ) return;

			deconUI = new UserInterface();
			deconGUI = new DeconstructorGUI();
			deconGUI.Activate();
			deconUI.SetState(deconGUI);
		}

		public override void ModifyInterfaceLayers(List<MethodSequenceListItem> layers)
		{
			int insertLayer = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
			if (insertLayer != -1)
			{
				layers.Insert(insertLayer, new MethodSequenceListItem($"{instance.Name}: UI",
					delegate
					{
						if (deconGUI.visible)
						{
							deconUI.Update(Main._drawInterfaceGameTime);
							deconGUI.Draw(Main.spriteBatch);
						}
						return true;
					},
					null));
			}

			insertLayer = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Interact Item Icon"));
			layers[insertLayer].Skip = insertLayer != -1 && deconGUI.IsMouseHovering;
		}

		internal void TryToggleGUI(bool? state = null)
		{
			bool visible =
				state ?? !deconGUI.visible;

			SoundHelper.PlaySound(
				visible
				? SoundHelper.SoundType.OpenUI
				: SoundHelper.SoundType.CloseUI);

			deconGUI.visible = visible;
			deconGUI.ToggleUI(visible);
		}
	}
}
