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
		internal Vector2 distance;
		internal int frame;

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

			//animationFrameHeight = 54;
			disableSmartCursor = true;
		}

		public override void RightClick(int i, int j)
		{
			//Main.NewTextMultiline(Main.LocalPlayer.GetModPlayer<DeconPlayer>(mod).DeconDist.ToString());
			TheDeconstructor.instance.TryToggleGUI();
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
			var tile = Main.tile[i, j];
			if (tile.frameX == 0
				&& tile.frameY == 0)
			{
				frame = (frame + 1) % 8;
			}
			return base.PreDraw(i, j, spriteBatch);
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			try
			{
				var inst = TheDeconstructor.instance;
				Color useColor =
					inst.deconGUI.visible
					&& !inst.deconGUI.cubeItemPanel.item.IsAir
					&& inst.deconGUI.cubeItemPanel.item.modItem is QueerLunarCube
						? Utils.DiscoColor()
						: Color.White;

				var sine = (float)Math.Sin(Main.essScale * 0.50f);
				r = 0.05f + 0.35f * sine * useColor.R * 0.01f;
				g = 0.05f + 0.35f * sine * useColor.G * 0.01f;
				b = 0.05f + 0.35f * sine * useColor.B * 0.01f;
			}
			catch (Exception e)
			{
				Main.NewTextMultiline(e.ToString());
			}
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			Item.NewItem(i * 16, j * 16, 20, 28, mod.ItemType<Items.Deconstructor>());
			mod.GetTileEntity("DeconstructorTE").Kill(i, j);
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
		{
			try
			{
				var tile = Main.tile[i, j];
				// Only draw from top left tile
				if (tile.frameX == 0
					&& tile.frameY == 0)
				{
					var inst = TheDeconstructor.instance;
					if (inst.deconGUI.visible
						&& !inst.deconGUI.cubeItemPanel.item.IsAir)
					{
						Color useColor =
							inst.deconGUI.cubeItemPanel.item.modItem is QueerLunarCube
								? Utils.DiscoColor()
								: Color.White;

						Vector2 zero = Main.drawToScreen
							? Vector2.Zero
							: new Vector2(Main.offScreenRange, Main.offScreenRange);

						Texture2D animTexture = mod.GetTexture("Items/LunarCubeFrames");
						const int frameWidth = 20;
						const int frameHeight = 28;
						Vector2 offset = new Vector2(36f, 8f); // offset 2.5 tiles horizontal, 1 tile vertical
						Vector2 position = new Vector2(i, j) * 16f - Main.screenPosition + offset;
						Vector2 origin = new Vector2(frameHeight, frameWidth) * 0.5f;

						spriteBatch.Draw(animTexture, position + zero,
							new Rectangle(0, frameHeight * frame, frameWidth, frameHeight), useColor, 0f, origin, 1f,
							SpriteEffects.None, 0f);
					}
				}

			}
			catch (Exception e)
			{
				Main.NewTextMultiline(e.ToString());
			}
		}
	}
}
