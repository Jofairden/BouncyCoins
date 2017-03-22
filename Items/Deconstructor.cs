using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;


namespace TheDeconstructor.Items
{
	internal class Deconstructor : ModItem
	{
		public override void SetDefaults()
		{
			item.name = "Deconstructor";
			item.toolTip = "Can destroy any item back into materials, for a price";
			item.toolTip2 = "Materials will be put into a goodie bag which can be opened anytime";
			item.useStyle = 1;
			item.useTurn = true;
			item.useAnimation = 15;
			item.useTime = 10;
			item.autoReuse = true;
			item.maxStack = 99;
			item.consumable = true;
			item.width = 20;
			item.height = 24;
			item.value = Item.sellPrice(0, 35, 0, 0);
			item.createTile = mod.TileType<Tiles.Deconstructor>();
			ItemID.Sets.ItemNoGravity[item.type] = true;
		}

		private int worldFC = 0;
		private int worldF = 0;
		private int invFC = 0;
		private int invF = 0;
		private const int invFMax = 8;

		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			var useTexture = mod.GetTexture("Items/DeconstructorFrames");
			if (invFC++ >= 4)
			{
				invF = (invF + 1) % invFMax;
				invFC = 0;
			}
			spriteBatch.Draw(useTexture, position, new Rectangle(0, 28 * invF, 20, 28), drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
			return false;
		}

		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
		{
			var useTexture = mod.GetTexture("Items/DeconstructorFrames");
			worldFC++;
			if (worldFC >= 4)
			{
				worldF = (worldF + 1) % invFMax;
				worldFC = 0;
			}
			spriteBatch.Draw(useTexture, item.position - Main.screenPosition, new Rectangle(1, 28 * worldF, 20, 28), lightColor, 0f, new Vector2(10, 14), scale, SpriteEffects.None, 0f);
			return false;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			var ttl = tooltips.FirstOrDefault(x => x.mod.Equals("Terraria") && x.Name.Equals("ItemName"));
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
