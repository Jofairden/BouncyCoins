using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;


namespace TheDeconstructor.Items
{
	internal abstract class Cube : ModItem
	{
		private Texture2D animTexture => TheDeconstructor.instance.GetTexture(TexturePath);
		internal Item SealedSource { get; set; } = new Item();
		internal List<Item> SealedItems { get; set; } = new List<Item>();
		internal abstract string TexturePath { get; }
		internal bool CanFail { get; set; }
		internal int InvFC { get; set; }
		internal int InvF { get; set; }
		internal abstract int InvFMax { get; }
		internal CubeState? State { get; set; } = CubeState.Open;

		public enum CubeState
		{
			Sealed,
			Open
		}

		public override bool CloneNewInstances =>
			true;

		public override void SetDefaults()
		{
			item.name = "Cube";
			item.rare = 9;
			item.maxStack = 1;
			item.value = 1;
			item.width = 10;
			item.height = 10;
			ItemID.Sets.ItemNoGravity[item.type] = true;
		}

		public virtual TagCompound CubeSave()
		{
			try
			{
				var tc = new TagCompound
				{
					["SealedItems"] = SealedItems.Select(ItemIO.Save).ToList(),
					["SealedSource"] = ItemIO.Save(SealedSource),
					["CanFail"] = CanFail,
					["State"] = (int?)State ?? 0,
					["Value"] = item.value
				};
				return tc;
			}
			catch (Exception e)
			{
				ErrorLogger.Log(e.ToString());
			}
			return null;
		}

		public override TagCompound Save() =>
			CubeSave();

		public virtual void CubeLoad(TagCompound tag)
		{
			try
			{
				var list = tag.GetList<TagCompound>("SealedItems").ToList();
				list.ForEach(x => SealedItems.Add(ItemIO.Load(x)));
				SealedSource = ItemIO.Load(tag.GetCompound("SealedSource"));
				CanFail = tag.GetBool("CanFail");
				State = (CubeState)tag.GetInt("State");
				item.value = tag.GetInt("Value");
			}
			catch (Exception e)
			{
				ErrorLogger.Log(e.ToString());
			}
		}

		public override void Load(TagCompound tag) =>
			CubeLoad(tag);

		public override void OnCraft(Recipe recipe) =>
			SoundHelper.PlaySound(SoundHelper.SoundType.Receive);

		public override bool CanRightClick() =>
			true;

		public override void RightClick(Player player)
		{
			try
			{
				if (!State.HasValue) return;
				bool isQueer = item.modItem is QueerLunarCube;

				if (State.Value == CubeState.Open)
				{
					if (TheDeconstructor.instance.deconGUI.visible)
					{
						TheDeconstructor.instance.deconGUI.TryGetCube(true);
						TheDeconstructor.instance.deconGUI.TryPutInCube(isQueer);
					}
					else // Prevent consume
						item.stack = 2;
				}
				else if (State.Value == CubeState.Sealed)
				{
					if (TheDeconstructor.instance.deconGUI.visible)
					{
						item.stack = 2;
						return;
					}
					if (SealedItems != null
						&& SealedItems.Count >= 1)
					{
						// Need to figure out a way how to reset weapon prefixes
						SoundHelper.PlaySound(SoundHelper.SoundType.Redeem);
						//Item giveItem = new Item();
						foreach (var infoBagItem in SealedItems)
						{
							if (infoBagItem.type == 0) break;
							//giveItem.SetDefaults(infoBagItem.type);
							//var givenItem = player.GetItem(player.whoAmI, giveItem);
							//givenItem.Prefix(0);

							//the UI can add materials beyond their maxStack.
							//so this should ensure items are given in multiple stacks if they exceed their maxStack
							int stackDiff = Math.Max(1,
								(int)Math.Floor((double)(infoBagItem.stack / (double)infoBagItem.maxStack)));
							int useStack = stackDiff > 1 ? infoBagItem.maxStack : infoBagItem.stack;
							int leftOver = stackDiff > 1 ? infoBagItem.stack - useStack * stackDiff : 0;
							var giveItem = infoBagItem.Clone();
							for (int i = 0; i < stackDiff; i++)
							{
								if (CanFail && Main.rand.NextFloat() <= 0.2f)
								{
									NotifyLoss(infoBagItem.type, useStack);
									continue;
								}
								giveItem.stack = useStack;
								player.GetItem(Main.LocalPlayer.whoAmI, giveItem);
							}
							if (leftOver > 0)
							{
								if (CanFail && Main.rand.NextFloat() <= 0.2f)
								{
									NotifyLoss(infoBagItem.type, leftOver);
									continue;
								}
								giveItem.stack = leftOver;
								player.GetItem(Main.LocalPlayer.whoAmI, giveItem);
							}
						}
						// Queer cube isn't lost upon unsealing
						if (isQueer)
						{
							item.stack = 2;
							var modItem = item.modItem as Cube;
							modItem.State = CubeState.Open;
							modItem.SealedSource.TurnToAir();
							modItem.SealedItems.Clear();
							modItem.CanFail = false;
							item.value = 1;
						}
					}
					//item.ResetStats(item.type);
					//info = new BagItemInfo();
				}
			}
			catch (Exception e)
			{
				Main.NewTextMultiline(e.ToString());
			}
		}

		public virtual void NotifyLoss(int type, int stack)
		{
			string str = $"[i/s1:{type}] (x{stack}) was lost while unsealing!";
			Main.NewText(str, 255);
			if (Main.netMode == NetmodeID.MultiplayerClient)
				NetMessage.SendData(MessageID.ChatText, -1, -1, str, 255);
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			// Remove regular price tooltip
			var priceTT = tooltips.FirstOrDefault(x => x.mod == "Terraria" & x.Name == "Price");
			if (priceTT != null
				&& State == CubeState.Sealed)
				tooltips.Remove(priceTT);

			// Tooltip
			tooltips.Add(State == CubeState.Sealed
				 ? new TooltipLine(mod, $"{mod.Name}: LunarCube: Title", "Sealed\nRight click to break seal") { overrideColor = Color.Yellow }
				 : new TooltipLine(mod, $"{mod.Name}: LunarCube: Title", "Unsealed") { overrideColor = Color.Yellow });

			// Loss warning
			if (CanFail)
			{
				var tt = new TooltipLine(mod, $"{mod.Name}: LunarCube: Loss Warning",
					$"Contents might not survive seal destruction")
				{ overrideColor = Colors.RarityRed };
				tooltips.Add(tt);
			}

			// Value
			if (item.value > 1)
			{
				tooltips.Add(new TooltipLine(mod, $"{mod.Name}: LunarCube: Value",
					"Value:" + new ItemValue().SetFromCopperValue(item.value).ToSellValue().ToTagString()));
			}

			// Source item
			if (SealedSource != null
				&& !SealedSource.IsAir)
			{
				tooltips.Add(new TooltipLine(mod, $"{mod.Name}: LunarCube: Source",
						$"Sealed item:[i/s1:{SealedSource.type}][c/{SealedSource.GetTooltipColor().ToHexString().Substring(1)}:{SealedSource.name} ](x{SealedSource.stack})"));
			}

			// Contents
			if (SealedItems.Any<Item>())
			{
				tooltips.Add(new TooltipLine(mod, $"{mod.Name}: LunarCube: Content Title", "Contains:"));
				foreach (var infoBagItem in SealedItems)
				{
					if (!infoBagItem.IsAir)
						tooltips.Add(new TooltipLine(mod, $"{mod.Name}: LunarCube: Content: {infoBagItem.type}",
							$"[i/s1:{infoBagItem.type}][c/{infoBagItem.GetTooltipColor().ToHexString().Substring(1)}:{infoBagItem.name} ](x{infoBagItem.stack})"));
				}
			}

		}

		public virtual Cube CubeClone<T>() where T : Cube, new()
		{
			Cube clone = new T
			{
				SealedSource = (Item)this.SealedSource.Clone(),
				SealedItems = new List<Item>(this.SealedItems),
				CanFail = this.CanFail,
				State = this.State
			};
			return clone;
		}

		public virtual bool CubeDrawInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame,
			Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			Color useColor =
				item.type == mod.ItemType<QueerLunarCube>()
					? Tools.DiscoColor()
					: drawColor;

			if (InvFC++ >= 4)
			{
				InvF = (InvF + 1) % InvFMax;
				InvFC = 0;
			}
			spriteBatch.Draw(animTexture, position, new Rectangle(0, item.height * InvF, item.width, item.height), useColor, 0f, origin, scale, SpriteEffects.None, 0f);
			return false;
		}

		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame,
			Color drawColor, Color itemColor, Vector2 origin, float scale)
			=> CubeDrawInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale);

		public virtual bool CubeDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor,
			ref float rotation, ref float scale, int whoAmI)
		{
			Color useColor =
				item.type == mod.ItemType<QueerLunarCube>()
					? Tools.DiscoColor()
					: lightColor;
			Main.itemFrameCounter[whoAmI]++;
			if (Main.itemFrameCounter[whoAmI] >= 4)
			{
				Main.itemFrame[whoAmI] = (Main.itemFrame[whoAmI] + 1) % InvFMax;
				Main.itemFrameCounter[whoAmI] = 0;
			}
			spriteBatch.Draw(animTexture, item.position - Main.screenPosition, new Rectangle(0, item.height * Main.itemFrame[whoAmI], item.width, item.height), useColor, 0f, new Vector2(item.width / 2f, item.height / 2f), scale, SpriteEffects.None, 0f);
			return false;
		}

		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor,
			ref float rotation, ref float scale, int whoAmI) =>
			CubeDrawInWorld(spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);

		public override void PostUpdate()
		{
			Color useColor =
			   item.type == mod.ItemType<QueerLunarCube>()
				   ? Tools.DiscoColor()
				   : Color.White;
			var sine = (float)Math.Sin(Main.essScale * 0.50f);
			var r = 0.05f + 0.35f * sine * useColor.R * 0.01f;
			var g = 0.05f + 0.35f * sine * useColor.G * 0.01f;
			var b = 0.05f + 0.35f * sine * useColor.B * 0.01f;
			Lighting.AddLight(item.Center, new Vector3(r, g, b));
		}
	}
}
