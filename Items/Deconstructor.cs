using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TheDeconstructor.Items
{
	internal sealed class Deconstructor : ModItem
	{
		public override void SetDefaults()
		{
			item.name = "Lunar Deconstructor";
			item.toolTip = "Can seal an item to return it to it's former state, for a price";
			item.toolTip2 = "Materials will be put into a sealed Lunar Cube";
			item.useStyle = 1;
			item.useTurn = true;
			item.useAnimation = 15;
			item.useTime = 10;
			item.autoReuse = true;
			item.maxStack = 99;
			item.consumable = true;
			item.width = 40;
			item.height = 30;
			item.value = Item.sellPrice(0, 35, 0, 0);
			item.createTile = mod.TileType<Tiles.Deconstructor>();
			ItemID.Sets.ItemNoGravity[item.type] = true;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
		    var ttl = tooltips.FirstOrDefault(x =>
		        x.mod.Equals("Terraria", System.StringComparison.OrdinalIgnoreCase)
		        && x.Name.Equals("ItemName", System.StringComparison.OrdinalIgnoreCase));

			if (ttl != null)
				ttl.overrideColor = new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB);
		}

		public override void AddRecipes()
		{
			var recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.FragmentSolar, 10);
			recipe.AddIngredient(ItemID.FragmentNebula, 10);
			recipe.AddIngredient(ItemID.FragmentStardust, 10);
			recipe.AddIngredient(ItemID.FragmentVortex, 10);
			recipe.AddIngredient(ItemID.LunarBar, 10);
			recipe.AddTile(412);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
