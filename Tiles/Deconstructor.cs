using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace TheDeconstructor.Tiles
{
	internal sealed class DeconstructorTE : ModTileEntity
	{
		public override bool ValidTile(int i, int j)
		{
			var tile = Main.tile[i, j];
			return
				tile.active()
				&& tile.type == mod.TileType<Deconstructor>()
				&& tile.frameX == 0
				&& tile.frameY == 0;
		}

		public override void Update()
		{
			Main.LocalPlayer.GetModPlayer<DeconPlayer>(mod).DeconDist =
					new Vector2(Position.X, Position.Y) * 16f -
					Main.LocalPlayer.position;
		}

		public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction)
		{
			if (Main.netMode == 1)
			{
				NetMessage.SendTileRange(Main.myPlayer, i, j, 4, 3);
				NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, "", i, j);
				return -1;
			}
			return Place(i, j);
		}
	}


	internal sealed class Deconstructor : ModTile
	{
		public override void SetDefaults()
		{
			Main.tileLighted[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = false;
			Main.tileFrameImportant[Type] = true;

			TileObjectData.newTile.Width = 4;
			TileObjectData.newTile.Height = 3;
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
			TileObjectData.newTile.UsesCustomCanPlace = true;

			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16 };
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(mod.GetTileEntity("DeconstructorTE").Hook_AfterPlacement, -1, 0, false);
			TileObjectData.addTile(Type);

			AddMapEntry(new Color(50, 50, 50), "Deconstructor");

			animationFrameHeight = 54;
			disableSmartCursor = true;
		}

		public override void RightClick(int i, int j)
		{
			//Main.NewTextMultiline(Main.LocalPlayer.GetModPlayer<DeconPlayer>(mod).DeconDist.ToString());
			if (!TheDeconstructor.instance.deconGUI.visible)
			{
				SoundHelper.PlaySound(SoundHelper.SoundType.OpenUI);
				TheDeconstructor.instance.deconGUI.Update(Main._drawInterfaceGameTime);
			}
			TheDeconstructor.instance.deconGUI.visible = !TheDeconstructor.instance.deconGUI.visible;
			TheDeconstructor.instance.deconGUI.ToggleUI();
		}

		public override void AnimateTile(ref int frame, ref int frameCounter)
		{
			frameCounter++;
			if (frameCounter >= 4)
			{
				frame = (frame + 1) % 8;
				frameCounter = 0;
			}
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			var sine = (float)Math.Sin(Main.essScale * 0.50f);
			r = 0.05f + 0.35f * sine;
			g = 0.05f + 0.35f * sine;
			b = 0.05f + 0.35f * sine;
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			Item.NewItem(i * 16, j * 16, 20, 28, mod.ItemType<Items.Deconstructor>());
		}
	}
}
