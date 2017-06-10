using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.UI;
using TheDeconstructor.UI;

namespace TheDeconstructor
{
	public class TheDeconstructor : Mod
	{
		internal UserInterface deconUI;
		internal DeconstructorGUI deconGUI;
		internal static TheDeconstructor instance;
		internal static Texture2D DogeTexture;

		public TheDeconstructor()
		{
			Properties = new ModProperties
			{
				Autoload = true,
				AutoloadSounds = true
			};
		}

		public override void Load()
		{
			instance = this;
			if (Main.dedServ) return;

			DogeTexture = GetTexture("EmptyDoge");
			DogeTexture.MultiplyColorsByAlpha();

			deconUI = new UserInterface();
			deconGUI = new DeconstructorGUI();
			deconGUI.Activate();
			deconUI.SetState(deconGUI);
		}


		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			int insertLayer = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
			if (insertLayer != -1)
			{
				layers.Insert(insertLayer, new LegacyGameInterfaceLayer($"{instance.Name}: UI",
					delegate
					{
						if (deconGUI.visible)
						{
							deconUI.Update(Main._drawInterfaceGameTime);
							deconGUI.Draw(Main.spriteBatch);
						}
						return true;
					},
					InterfaceScaleType.UI));
			}

			insertLayer = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Interact Item Icon"));
			layers[insertLayer].Active = !(insertLayer != -1 && deconGUI.visible && deconGUI.IsMouseHovering);
		}

		/// <summary>
		/// Try toggling our UI
		/// </summary>
		/// <param name="state"></param>
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
