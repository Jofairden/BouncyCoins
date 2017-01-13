using System;
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
	public class DeconstructBagItem : ModItem
	{
		public BagItemInfo info => item.GetModInfo<BagItemInfo>(mod);

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
					["items"] = info.bagItems.Select(ItemIO.Save).ToList(),
					["source"] = info.sourceItem
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
				list.ForEach(x => info.bagItems.Add(ItemIO.Load(x)));
				info.sourceItem = ItemIO.Load(tag.GetCompound("source"));
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
				if (info.bagItems != null && info.bagItems.Count >= 1)
				{
					// Need to figure out a way how to reset weapon prefixes

					//Item giveItem = new Item();
					foreach (var infoBagItem in info.bagItems)
					{
						//giveItem.SetDefaults(infoBagItem.type);
						//var givenItem = player.GetItem(player.whoAmI, giveItem);
						//givenItem.Prefix(0);
						player.QuickSpawnItem(infoBagItem.type, infoBagItem.stack);
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

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			if (info.sourceItem != null && info.sourceItem.type != 0)
			{
				tooltips.Add(new TooltipLine(mod, $"{mod.Name}: GoodieBag: Source",
						$"Source:[i/s1:{info.sourceItem.type}][c/{GetTTColor(info.sourceItem).ToHexString().Substring(1)}:{info.sourceItem.name} ](x{info.sourceItem.stack})"));
			}
			tooltips.Add(new TooltipLine(mod, $"{mod.Name}: GoodieBag: Title", "Open this bag and receive:"));
			foreach (var infoBagItem in info.bagItems)
			{
				if (infoBagItem.type != 0)
					tooltips.Add(new TooltipLine(mod, $"{mod.Name}: GoodieBag: Content: {infoBagItem.type}",
						$"[i/s1:{infoBagItem.type}][c/{GetTTColor(infoBagItem).ToHexString().Substring(1)}:{infoBagItem.name} ](x{infoBagItem.stack})"));
			}
		}

		private Color GetTTColor(Item item)
		{
			if (item.questItem) return Colors.RarityAmber;
			switch (item.rare)
			{
				default:
				case -1:
					return Colors.RarityTrash;
				case 0:
					return Colors.RarityNormal;
				case 1:
					return Colors.RarityBlue;
				case 2:
					return Colors.RarityGreen;
				case 3:
					return Colors.RarityOrange;
				case 4:
					return Colors.RarityRed*0.75f;
				case 5:
					return Colors.RarityPink;
				case 6:
					return Colors.RarityPurple;
				case 7:
					return Colors.RarityLime;
				case 8:
					return Colors.RarityYellow;
				case 9:
					return Colors.RarityCyan;
				case 10:
					return Colors.RarityRed;
				case 11:
					return Colors.RarityPurple;
			}
		}
	}

	public static class ColorConverterExtensions
	{
		public static string ToHexString(this Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";

		public static string ToRgbString(this Color c) => $"RGB({c.R}, {c.G}, {c.B})";
	}

	public struct ItemValue
	{
		public int copper;
		public int silver;
		public int gold;
		public int platinum;

		public override string ToString()
		{
			string s = "";
			s += copper > 0 ? $"[c/{new Color(205, 133, 63).ToHexString()}:{copper}c]" : "";
			s += silver > 0 ? $"[c/{new Color(220, 220, 220).ToHexString()}:{silver}s]" : "";
			s += gold > 0 ? $"[c/{new Color(255, 218, 155).ToHexString()}:{gold}g]" : "";
			s += platinum > 0 ? $"[c/{new Color(230, 230, 250)}:{platinum}p]" : "";
			return s;
		}

		public static void SetValues(ref ItemValue iV, int c, int s, int g, int p)
		{
			iV.copper = c;
			iV.silver = s;
			iV.gold = g;
			iV.platinum = p;
		}

		public static void SetFromCopper(ref ItemValue iV, int c)
		{
			var totalCopper = c;
			int totalSilver = totalCopper / 100;
			int totalGold = totalCopper / 100 / 100;
			int totalPlatinum = totalCopper/100/100/100;
		}
	}

	public class BagItemInfo : ItemInfo
	{
		public Item sourceItem = new Item();
		public List<Item> bagItems = new List<Item>();

		public override ItemInfo Clone()
		{
			var clone = new BagItemInfo();
			clone.sourceItem = (Item)this.sourceItem.Clone();
			clone.bagItems = new List<Item>(bagItems);
			return clone;
		}
	}

	///  fuck you bitch
	///  (c) gorateron 2017
	///  made in 1 day
	///  suck it
	///  boii
}
