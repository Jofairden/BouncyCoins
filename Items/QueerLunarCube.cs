using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;


namespace TheDeconstructor.Items
{
	internal sealed class QueerLunarCube : Cube
	{
		public override string Texture => $"{mod.Name}/Items/LunarCube";

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Queer Lunar Cube");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.rare = 10;
		}

		public override Color? GetAlpha(Color lightColor) =>
			CubeColor<QueerLunarCube>();

		public override void PostUpdate() =>
			CubeLighting<QueerLunarCube>();

		public override void AddRecipes()
		{
			var recipe = new ModRecipe(mod);
			recipe.AddRecipeGroup("Fragment");
			recipe.AddIngredient(mod.ItemType<LunarCube>(), 1);
			recipe.AddTile(mod.TileType<Tiles.Deconstructor>());
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
