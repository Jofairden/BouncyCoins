using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.ID;


namespace TheDeconstructor
{
	public static class Tools
	{
		public static bool IsTopLeftFrame(this Tile tile) =>
			tile.frameX == 0
			&& tile.frameY == 0;

		public static string ToHexString(this Color c) =>
			$"#{c.R:X2}{c.G:X2}{c.B:X2}";

		public static string ToRgbString(this Color c) =>
			$"RGB({c.R}, {c.G}, {c.B})";

		public static Color DiscoColor() =>
			new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB);

		public static Color GetTooltipColor(this Item item)
		{
			if (item.questItem)
				return Colors.RarityAmber;
			if (item.expert || item.type == TheDeconstructor.instance.ItemType<Items.Deconstructor>())
				return DiscoColor();

			return ItemRarity.GetColor(item.rare);
		}
	}
}
