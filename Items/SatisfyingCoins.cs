using Terraria.ID;
using Terraria.ModLoader;

namespace SatisfyingCoins.Items
{
	public class SatisfyingCoins : ModItem
	{
		public override void SetDefaults()
		{
			item.name = "SatisfyingCoins";
			item.damage = 50;
			item.melee = true;
			item.width = 40;
			item.height = 40;
			item.toolTip = "This is a modded sword.";
			item.useTime = 20;
			item.useAnimation = 20;
			item.useStyle = 1;
			item.knockBack = 6;
			item.value = 10000;
			item.rare = 2;
			item.UseSound = SoundID.Item1;
			item.autoReuse = true;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.DirtBlock, 10);
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
