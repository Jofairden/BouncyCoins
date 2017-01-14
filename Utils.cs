using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;


namespace TheDeconstructor
{
	public static class Utils
	{
		public static Color GetTooltipColor(this Item item)
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
