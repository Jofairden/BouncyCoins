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
				&& tile.IsTopLeftFrame();
		}

		public override void Update()
		{
			DistanceToLocalPlayer =
					new Vector2(Position.X, Position.Y) * 16f -
					Main.LocalPlayer.position;
		}

		public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction)
		{
			// Subtract the origin
			i -= 2;
			j -= 2;
			// Singleplayer
			if (Main.netMode != NetmodeID.MultiplayerClient)
				return Place(i, j);
			// Multiplayer
			NetMessage.SendTileSquare(Main.myPlayer, i, j, 4, TileChangeType.None);
			NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, "", i, j, 0f, 0f, 0, 0, 0);
			return -1;
		}
	}


	internal sealed class Deconstructor : ModTile
	{
		private const int s = 16;
		private const int p = 2;

		public override void SetDefaults()
		{
			Main.tileLighted[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileFrameImportant[Type] = true;

			TileObjectData.newTile.Width = 4;
			TileObjectData.newTile.Height = 3;
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
			TileObjectData.newTile.UsesCustomCanPlace = true;
			TileObjectData.newTile.CoordinateWidth = s;
			TileObjectData.newTile.CoordinatePadding = p;
			TileObjectData.newTile.CoordinateHeights = new int[] { s, s, s };
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(mod.GetTileEntity("DeconstructorTE").Hook_AfterPlacement, -1, 0, false);
			TileObjectData.newTile.Origin = new Point16(2, 2);
			TileObjectData.newTile.DrawYOffset = 2;
			TileObjectData.addTile(Type);
			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);
			AddMapEntry(new Color(50, 50, 50), "Lunar Deconstructor");

			disableSmartCursor = true;
		}

		public override void RightClick(int i, int j)
		{
			Tile tile = Main.tile[i, j];
			if (tile.type == Type)
			{
				// Try to get top left frame (0,0)
				var x = i - tile.frameX / (s + p);
				var y = j - tile.frameY / (s + p);
				TheDeconstructor.instance.deconGUI.tileData = new Point16(x, y);
				TheDeconstructor.instance.TryToggleGUI();
			}
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			Color useColor = Color.White;
			Tile tile = Main.tile[i, j];
			if (tile.type == Type
				&& tile.IsTopLeftFrame())
			{
				// Try to get top left frame (0,0)
				var inst = TheDeconstructor.instance;
				if (inst.deconGUI.tileData != null
					&& inst.deconGUI.tileData == new Point16(i, j)
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
			if (tile.type == Type
				&& tile.IsTopLeftFrame())
			{
				var inst = TheDeconstructor.instance;
				if (inst.deconGUI.visible
					&& !inst.deconGUI.cubeItemPanel.item.IsAir
					&& inst.deconGUI.tileData != null
					&& inst.deconGUI.tileData == new Point16(i, j))
				{
					var TE = TileEntity.ByPosition[inst.deconGUI.tileData.Value] as DeconstructorTE;
					Color useColor =
						inst.deconGUI.cubeItemPanel.item.modItem is QueerLunarCube
							? Tools.DiscoColor()
							: Color.White;

					Vector2 zero = Main.drawToScreen
						? Vector2.Zero
						: new Vector2(Main.offScreenRange, Main.offScreenRange);

					Texture2D animTexture = mod.GetTexture("Items/LunarCube");
					const int frameWidth = 20;
					const int frameHeight = 28;
					Vector2 offset = new Vector2(36f, 8f); // offset 2.5 tiles horizontal, 0.5 tile vertical
					Vector2 position = new Vector2(i, j) * 16f - Main.screenPosition + offset;
					Vector2 origin = new Vector2(frameHeight, frameWidth) * 0.5f;
					// tiles draw every 5 ticks, so we can safely increment here
					TE.frame = (TE.frame + 1) % 8;
					spriteBatch.Draw(animTexture, position + zero,
						new Rectangle(0, frameHeight * TE.frame, frameWidth, frameHeight), useColor, 0f, origin, 1f,
						SpriteEffects.None, 0f);

				}
			}
		}
	}
}
