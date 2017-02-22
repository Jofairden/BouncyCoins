using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace BouncyCoins.Commands
{
	//public class AmplitudeCommand : ModCommand
	//{
	//	public override void Action(CommandCaller caller, string input, string[] args)
	//	{
	//		var split = input.TrimEnd().Split(' ');
	//		if (input.Split(' ').Length < 3)
	//			throw new UsageException($"Please enter the full command.");

	//		float value;
	//		if (!float.TryParse(args[0], out value))
	//			throw new UsageException($"Could not parse value {args[2]} as a float.");

	//		CoinPlayer.GetModPlayer(caller.Player).amplitude = value;
	//	}

	//	public override string Command => "amplitude";

	//	public override CommandType Type => CommandType.Chat;

	//	public override string Usage => "/amplitidue <value>";

	//	public override string Description => "Change the amplitude";
	//}
}
