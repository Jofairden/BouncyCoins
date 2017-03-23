using System;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ID;


namespace TheDeconstructor
{
	public struct ItemValue
	{
		public int RawValue { get; private set; }
		public int Copper { get; private set; }
		public int Silver { get; private set; }
		public int Gold { get; private set; }
		public int Platinum { get; private set; }

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();

			if (Platinum > 0)
				stringBuilder.Append($"{Platinum}p");
			if (Gold > 0)
				stringBuilder.Append($"{Gold}g");
			if (Silver > 0)
				stringBuilder.Append($"{Silver}s");
			if (Copper > 0)
				stringBuilder.Append($"{Copper}c");

			if (stringBuilder.Length <= 0)
				return " [No value]";

			return string.Concat(stringBuilder.ToString().Select(c => $"{c}" + (char.IsLetter(c) ? " " : ""))).TrimEnd(' ');
		}

		public string ToTagString()
		{
			StringBuilder stringBuilder = new StringBuilder();

			if (Platinum > 0)
				stringBuilder.Append($"[i/s1:{ItemID.PlatinumCoin}]{Platinum}");
			if (Gold > 0)
				stringBuilder.Append($"[i/s1:{ItemID.GoldCoin}]{Gold}");
			if (Silver > 0)
				stringBuilder.Append($"[i/s1:{ItemID.SilverCoin}]{Silver}");
			if (Copper > 0)
				stringBuilder.Append($"[i/s1:{ItemID.CopperCoin}]{Copper}");

			if (stringBuilder.Length <= 0)
				return " [No value]";

			return $"{stringBuilder}";
		}

		public ItemValue SetValues(int copper, int silver = 0, int gold = 0, int platinum = 0)
		{
			SetFromCopperValue(GetRawCopperValue(copper, silver, gold, platinum));
			return this;
		}

		public ItemValue AddValues(int copper, int silver = 0, int gold = 0, int platinum = 0)
		{
			SetFromCopperValue(RawValue + GetRawCopperValue(copper, silver, gold, platinum));
			return this;
		}

		private int GetRawCopperValue(int copper, int silver = 0, int gold = 0, int platinum = 0)
		{
			return (int)(copper + silver * 100 + gold * Math.Pow(100, 2) + platinum * Math.Pow(100, 3));
		}

		public ItemValue ToSellValue()
		{
			RawValue /= 5;
			SetFromCopperValue(RawValue);
			return this;
		}

		public ItemValue ApplyDiscount(Player player)
		{
			if (player.discount)
			{
				RawValue = (int)(RawValue * 0.8f);
				SetFromCopperValue(RawValue);
			}
			return this;
		}

		public ItemValue SetFromCopperValue(int value)
		{
			RawValue = value;
			int copper = value;
			int silver = 0;
			int gold = 0;
			int platinum = 0;

			if (copper >= 100)
			{
				silver = copper / 100;
				copper %= 100;

				if (silver >= 100)
				{
					gold = silver / 100;
					silver %= 100;

					if (gold >= 100)
					{
						platinum = gold / 100;
						gold %= 100;
					}
				}
			}

			this.Copper = copper;
			this.Silver = silver;
			this.Gold = gold;
			this.Platinum = platinum;
			return this;
		}

		public static implicit operator ItemValue(int rawValue)
		{
			return new ItemValue().SetFromCopperValue(rawValue);
		}

		public static ItemValue operator +(ItemValue first, ItemValue second)
		{
			return new ItemValue().SetFromCopperValue(first.RawValue + second.RawValue);
		}

		public static ItemValue operator +(ItemValue first, float second)
		{
			return new ItemValue().SetFromCopperValue((int) (first.RawValue + second));
		}

		public static ItemValue operator -(ItemValue first, ItemValue second)
		{
			return new ItemValue().SetFromCopperValue(first.RawValue - second.RawValue);
		}

		public static ItemValue operator -(ItemValue first, float second)
		{
			return new ItemValue().SetFromCopperValue((int)(first.RawValue - second));
		}

		public static ItemValue operator *(ItemValue first, ItemValue second)
		{
			return new ItemValue().SetFromCopperValue(first.RawValue * second.RawValue);
		}

		public static ItemValue operator *(ItemValue first, float second)
		{
			return new ItemValue().SetFromCopperValue((int)(first.RawValue * second));
		}

		public static ItemValue operator /(ItemValue first, ItemValue second)
		{
			return new ItemValue().SetFromCopperValue(first.RawValue / second.RawValue);
		}

		public static ItemValue operator /(ItemValue first, float second)
		{
			return new ItemValue().SetFromCopperValue((int)(first.RawValue / second));
		}

		public static ItemValue operator %(ItemValue first, ItemValue second)
		{
			return new ItemValue().SetFromCopperValue(first.RawValue % second.RawValue);
		}

		public static ItemValue operator %(ItemValue first, float second)
		{
			return new ItemValue().SetFromCopperValue((int)(first.RawValue % second));
		}
	}
}
