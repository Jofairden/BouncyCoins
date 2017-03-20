using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SatisfyingCoins
{
	public class SatisfyingCoins : Mod
	{
		public SatisfyingCoins()
		{
			Properties = new ModProperties()
			{
				Autoload = true,
				AutoloadSounds = true
			};
		}
	}

	public class CoinGI : GlobalItem
	{
		private static readonly int[] CoinSounds =
		{
			4, 5, 2, 20, 21, 23, 30, 31, 32, 44
		};

		public override bool OnPickup(Item item, Player player)
		{
			if (new int[] {ItemID.CopperCoin, ItemID.SilverCoin, ItemID.GoldCoin, ItemID.PlatinumCoin}.Contains(item.type))
			{
				int soundType = CoinSounds[Main.rand.Next(0, CoinSounds.Length)];
				Main.PlaySound(SoundLoader.customSoundType, -1, -1, mod.GetSoundSlot(SoundType.Custom, $"Sounds/Custom/Pickup_Coin{soundType}"));
			}
			return base.OnPickup(item, player);
		}
	}
}
