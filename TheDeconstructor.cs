using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.UI;

namespace TheDeconstructor
{
	public class TheDeconstructor : Mod
	{
		internal UserInterface deconUI;
		internal DeconstructorGUI deconGUI;
		internal static TheDeconstructor instance;
		internal static ModHotKey deconHotkey;

		public TheDeconstructor()
		{
			Properties = new ModProperties()
			{
				Autoload = true,
			};
		}

		public override void Load()
		{
			instance = this as TheDeconstructor;
			deconHotkey = RegisterHotKey("Toggle Deconstructor UI", "U");

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
				layers.Insert(insertLayer, new MethodSequenceListItem("Deconstructor: UI",
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

		internal class DeconPlayer : ModPlayer
		{
			public override void ProcessTriggers(TriggersSet triggersSet)
			{
				if (!deconHotkey.JustPressed) return;

				if (!instance.deconGUI.visible)
				{
					instance.deconGUI.Update(Main._drawInterfaceGameTime);
				}
				instance.deconGUI.visible = !instance.deconGUI.visible;
				instance.deconGUI.ToggleUI();
			}
		}
	}


	/// <summary>
	///  fuck you bitch
	///  (c) gorateron 2017
	///  made in 1 day
	///  suck it
	///  boii
	/// </summary>
	internal static class ModHelper
	{
		public static void Invert(this bool bl)
		{
			bl = !bl;
		}
	}
}
