using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace BouncyCoins.Commands
{
	public class AmplitudeCommand : ModCommand
	{
		public override void Action(CommandCaller caller, string input, string[] args)
		{
			if (args.Length < 2)
				throw new UsageException($"Please enter the full command.");

			float value;

			if (args[0] == "amp" || args[0] == "amplitude")
			{
				if (!float.TryParse(args[1], out value))
					throw new UsageException($"Could not parse value {args[1]} as a float.");

				CoinPlayer.GetModPlayer(caller.Player).amplitude = value;

			}
			else if (args[0] == "speed")
			{
				if (!float.TryParse(args[1], out value))
					throw new UsageException($"Could not parse value {args[1]} as a float.");

				CoinPlayer.GetModPlayer(caller.Player).speed = value / 100f;
			}
			else if (args[0] == "offset")
			{
				if (!float.TryParse(args[1], out value))
					throw new UsageException($"Could not parse value {args[1]} as a float.");

				CoinPlayer.GetModPlayer(caller.Player).universalOffset = value;
			}
			else if (args[0] == "disallowmoditems")
			{
				bool parsed;
				if (!bool.TryParse(args[1], out parsed))
					throw new UsageException($"Could not parse value {args[1]} as a bool.");

				CoinPlayer.GetModPlayer(caller.Player).disallowModItems = parsed;
			}
			else if (args[0] == "evenbounce")
			{
				bool parsed;
				if (!bool.TryParse(args[1], out parsed))
					throw new UsageException($"Could not parse value {args[1]} as a bool.");

				CoinPlayer.GetModPlayer(caller.Player).bounceEvenly = parsed;
			}
		}

		public override string Command => "bc";

		public override CommandType Type => CommandType.Chat;

		public override string Usage => "/bc <command> <args>";

		public override string Description => "Perform a command";
	}
}
