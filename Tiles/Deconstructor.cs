using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using TheDeconstructor.Items;

namespace TheDeconstructor.Tiles
{
	internal sealed class DeconstructorTE : ModTileEntity
	{
		public int frame = 0;
		public Vector2 DistanceToLocalPlayer = Vector2.Zero;

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
			DistanceToLocalPlayer =
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
		private const int w = 16;
		private const int p = 2;

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

			TileObjectData.newTile.CoordinateWidth = w;
			TileObjectData.newTile.CoordinatePadding = p;
			TileObjectData.newTile.CoordinateHeights = new int[] { w, w, w };
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(mod.GetTileEntity("DeconstructorTE").Hook_AfterPlacement, -1, 0, false);
			TileObjectData.addTile(Type);

			AddMapEntry(new Color(50, 50, 50), "Deconstructor");

			//animationFrameHeight = 54;
			disableSmartCursor = true;
		}

		public override void RightClick(int i, int j)
		{
			//Main.NewTextMultiline(Main.LocalPlayer.GetModPlayer<DeconPlayer>(mod).DeconDist.ToString());
			Tile tile = Main.tile[i, j];
			if (tile.type == Type)
			{
				// Try to get top left frame (0,0)
				var x = i - tile.frameX / (w + p);
				var y = j - tile.frameY / (w + p);
				TheDeconstructor.instance.deconGUI.tileData = new short[] { (short)x, (short)y };
				TheDeconstructor.instance.TryToggleGUI();
			}
		}

		//public override void AnimateTile(ref int frame, ref int frameCounter)
		//{
		//	frameCounter++;
		//	if (frameCounter >= 4)
		//	{
		//		frame = (frame + 1) % 8;
		//		frameCounter = 0;
		//	}
		//}

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
		{
			Tile tile = Main.tile[i, j];
			if (tile.type == Type
				&& tile.IsTopLeftFrame())
			{
				var te = (TileEntity.ByPosition[new Point16(i, j)] as DeconstructorTE);
				te.frame = (te.frame + 1) % 8;
			}
			return true;
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			Color useColor = Color.White;
			Tile tile = Main.tile[i, j];
			if (tile.type == Type)
			{
				// Try to get top left frame (0,0)
				var x = i - tile.frameX / (w + p);
				var y = j - tile.frameY / (w + p);
				var inst = TheDeconstructor.instance;
				if (inst.deconGUI.tileData != null
					&& inst.deconGUI.tileData[0] == x
					&& inst.deconGUI.tileData[1] == y
					&& inst.deconGUI.visible
					&& !inst.deconGUI.cubeItemPanel.item.IsAir
					&& inst.deconGUI.cubeItemPanel.item.modItem is QueerLunarCube)
				{
					useColor = Tools.DiscoColor();
				}
			}

			var sine = (float)Math.Sin(Main.essScale * 0.50f);
			r = 0.05f + 0.35f * sine * useColor.R * 0.01f;
			g = 0.05f + 0.35f * sine * useColor.G * 0.01f;
			b = 0.05f + 0.35f * sine * useColor.B * 0.01f;
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			TheDeconstructor.instance.TryToggleGUI(false);
			Item.NewItem(i * 16, j * 16, 20, 28, mod.ItemType<Items.Deconstructor>());
			mod.GetTileEntity("DeconstructorTE").Kill(i, j);
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
		{
			var tile = Main.tile[i, j];
			// Only draw from top left tile
			if (tile.IsTopLeftFrame())
			{
				var TE = TileEntity.ByPosition[new Point16(i, j)] as DeconstructorTE;
				var inst = TheDeconstructor.instance;
				if (inst.deconGUI.visible
					&& !inst.deconGUI.cubeItemPanel.item.IsAir
					&& inst.deconGUI.tileData != null
					&& inst.deconGUI.tileData[0] == i
					&& inst.deconGUI.tileData[1] == j)
				{
					Color useColor =
						inst.deconGUI.cubeItemPanel.item.modItem is QueerLunarCube
							? Tools.DiscoColor()
							: Color.White;

					Vector2 zero = Main.drawToScreen
						? Vector2.Zero
						: new Vector2(Main.offScreenRange, Main.offScreenRange);

					Texture2D animTexture = mod.GetTexture("Items/LunarCubeFrames");
					const int frameWidth = 20;
					const int frameHeight = 28;
					Vector2 offset = new Vector2(36f, 8f); // offset 2.5 tiles horizontal, 0.5 tile vertical
					Vector2 position = new Vector2(i, j) * 16f - Main.screenPosition + offset;
					Vector2 origin = new Vector2(frameHeight, frameWidth) * 0.5f;

					spriteBatch.Draw(animTexture, position + zero,
						new Rectangle(0, frameHeight * TE.frame, frameWidth, frameHeight), useColor, 0f, origin, 1f,
						SpriteEffects.None, 0f);
				}
			}
		}
	}
}
