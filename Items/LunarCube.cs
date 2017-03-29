using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;

namespace TheDeconstructor.Items
{
	internal sealed class LunarCube : Cube
	{
		public override void SetDefaults()
		{
			base.SetDefaults();
			item.name = "Lunar Cube";
		}

		public override ModItem Clone() => 
			CubeClone<LunarCube>();

		public override Color? GetAlpha(Color lightColor) =>
			CubeColor<LunarCube>();

		public override void PostUpdate() =>
			CubeLighting<LunarCube>();

		public override void AddRecipes()
		{
			var recipe = new ModRecipe(mod);
			recipe.AddRecipeGroup("Fragment");
			recipe.AddIngredient(ItemID.LunarBar);
			recipe.AddTile(mod.TileType<Tiles.Deconstructor>());
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
