using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;


namespace TheDeconstructor
{
	public static class Utils
	{
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
				case 4: //todo: find real color
					return Colors.RarityRed * 0.75f;
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
}
