﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace TheDeconstructor
{
	internal class DeconstructBagItem : ModItem
	{
		public Item sourceItem = new Item();
		public List<Item> bagItems = new List<Item>();
		public bool potionSource = false;

		public override void SetDefaults()
		{
			item.name = "Goodie Bag";
			item.width = 26;
			item.height = 34;
			item.rare = 9;
			item.toolTip = "Right click to open";
			item.maxStack = 1;
			item.value = 1;
		}

		public override TagCompound Save()
		{
			try
			{
				TagCompound tc = new TagCompound
				{
					["items"] = bagItems.Select(ItemIO.Save).ToList(),
					["source"] = ItemIO.Save(sourceItem)
				};
				return tc;
			}
			catch (Exception e)
			{
				ErrorLogger.Log(e.ToString());
			}
			return null;
		}

		public override void Load(TagCompound tag)
		{
			try
			{
				var list = tag.GetList<TagCompound>("items").ToList();
				list.ForEach(x => bagItems.Add(ItemIO.Load(x)));
				sourceItem = ItemIO.Load(tag.GetCompound("source"));
			}
			catch (Exception e)
			{
				ErrorLogger.Log(e.ToString());
			}
		}

		public override bool CanRightClick()
		{
			return true;
		}

		public override void RightClick(Player player)
		{
			try
			{
				if (bagItems != null && bagItems.Count >= 1)
				{
					// Need to figure out a way how to reset weapon prefixes
					SoundHelper.PlaySound(SoundHelper.SoundType.Redeem);
					//Item giveItem = new Item();
					foreach (var infoBagItem in bagItems)
					{
						if (infoBagItem.type == 0) break;
						//giveItem.SetDefaults(infoBagItem.type);
						//var givenItem = player.GetItem(player.whoAmI, giveItem);
						//givenItem.Prefix(0);

						//the UI can add materials beyond their maxStack.
						//so this should ensure items are given in multiple stacks if they exceed their maxStack
						int stackDiff = Math.Max(1, (int)Math.Floor((double)(infoBagItem.stack/(double)infoBagItem.maxStack)));
						int useStack = stackDiff > 1 ? infoBagItem.maxStack : infoBagItem.stack;
						int leftOver = stackDiff > 1 ? infoBagItem.stack - useStack*stackDiff : 0;
						for (int i = 0; i < stackDiff; i++)
						{
							if (potionSource && Main.rand.NextFloat() <= 0.2f)
							{
								NotifyLoss(infoBagItem.type, useStack);
								continue;
							}
							player.QuickSpawnItem(infoBagItem.type, useStack);
						}
						if (leftOver > 0)
						{
							if (potionSource && Main.rand.NextFloat() <= 0.2f)
							{
								NotifyLoss(infoBagItem.type, useStack);
								continue;
							}
							player.QuickSpawnItem(infoBagItem.type, leftOver);
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

		private void NotifyLoss(int type, int stack)
		{
			string str = $"Oh noes! You've lost [i/s1:{type}] (x{stack})!";
			Main.NewText(str, 255);
			if (Main.netMode == 1)
				NetMessage.SendData(MessageID.ChatText, -1, -1, str, 255);
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			if (sourceItem != null && sourceItem.type != 0)
			{
				tooltips.Add(new TooltipLine(mod, $"{mod.Name}: GoodieBag: Source",
						$"Source:[i/s1:{sourceItem.type}][c/{sourceItem.GetTooltipColor().ToHexString().Substring(1)}:{sourceItem.name} ](x{sourceItem.stack})"));
			}
			tooltips.Add(new TooltipLine(mod, $"{mod.Name}: GoodieBag: Title", "Open this bag and receive:"));
			foreach (var infoBagItem in bagItems)
			{
				if (infoBagItem.type != 0)
					tooltips.Add(new TooltipLine(mod, $"{mod.Name}: GoodieBag: Content: {infoBagItem.type}",
						$"[i/s1:{infoBagItem.type}][c/{infoBagItem.GetTooltipColor().ToHexString().Substring(1)}:{infoBagItem.name} ](x{infoBagItem.stack})"));
			}

			if (potionSource)
			{
				var tt = new TooltipLine(mod, $"{mod.Name}: GoodieBag: Potion Warning",
					$"Chance of material loss");
				tt.overrideColor = Colors.RarityRed;
				tooltips.Add(tt);
			}
		}

		public override bool CloneNewInstances => true;

		// I'm not actually sure if this is needed
		public override ModItem Clone(Item item)
		{
			var clone = new DeconstructBagItem();
			clone.sourceItem = (Item)this.sourceItem.Clone();
			clone.bagItems = new List<Item>(bagItems);
			clone.potionSource = potionSource;
			return clone;
		}
	}

	public static class ColorConverterExtensions
	{
		public static string ToHexString(this Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";

		public static string ToRgbString(this Color c) => $"RGB({c.R}, {c.G}, {c.B})";
	}
}
