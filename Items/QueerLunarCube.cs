using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;


namespace TheDeconstructor.Items
{
	internal sealed class QueerLunarCube : Cube
	{
		public override bool Autoload(ref string name, ref string texture, IList<EquipType> equips)
		{
			texture = $"{mod.Name}/Items/LunarCube";
			return base.Autoload(ref name, ref texture, equips);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.name = "Queer Lunar Cube";
			item.rare = 10;
		}

		public override ModItem Clone()
			 => CubeClone<QueerLunarCube>() as ModItem;

		public override Color? GetAlpha(Color lightColor)
		{
			var c = Tools.DiscoColor();
			c.A = lightColor.A;
			return c;
		}

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
