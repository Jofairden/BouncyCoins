using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.UI;
using Terraria.ID;


namespace TheDeconstructor
{
	public static class Tools
	{
		public static void MultiplyColorsByAlpha(this Texture2D texture)
		{
			Color[] data = new Color[texture.Width * texture.Height];
			texture.GetData(data);
			for (int i = 0; i < data.Length; i++)
			{
				Vector4 we = data[i].ToVector4();
				data[i] = new Color(we.X * we.W, we.Y * we.W, we.Z * we.W, we.W);
			}
			texture.SetData(data);
		}

		public static bool IsTopLeftFrame(this Tile tile) =>
			tile.frameX == 0
			&& tile.frameY == 0;

		public static Point16 GetTopLeftFrame(this Tile tile, int i, int j, int size = 16, int padding = 2) =>
			new Point16(
				i - tile.frameX / (size + padding), 
				j - tile.frameY / (size + padding));

		public static string ToHexString(this Color c) =>
			$"#{c.R:X2}{c.G:X2}{c.B:X2}";

		public static string ToRgbString(this Color c) =>
			$"RGB({c.R}, {c.G}, {c.B})";

		public static Color GetTooltipColor(this Item item)
		{
			if (item.questItem)
				return Colors.RarityAmber;
			if (item.expert 
				|| item.type == TheDeconstructor.instance.ItemType<Items.Deconstructor>()
				|| item.type == TheDeconstructor.instance.ItemType<Items.QueerLunarCube>())
				return Main.DiscoColor;

			return ItemRarity.GetColor(item.rare);
		}
	}
}
