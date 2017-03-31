using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace TheDeconstructor
{
	internal class DeconGlobalItem : GlobalItem
	{
		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			var info = item.GetModInfo<DeconItemInfo>(mod);
			if (info.addValueTooltip)
			{
				var tt = tooltips.FirstOrDefault(x=> 
				x.mod.Equals("Terraria", System.StringComparison.OrdinalIgnoreCase)
				&& x.Name.Equals("ItemName", System.StringComparison.OrdinalIgnoreCase));
				if (tt != null)
				{
					string value = new ItemValue().SetFromCopperValue(item.value * item.stack).ToSellValue().ToTagString();
					//tooltips[0].text = $"[i/s1:{item.type}]{tooltips[0].text}";
					//tooltips.Insert(1, new TooltipLine(mod, $"{mod.Name}: ValueTooltip", $"{value}"));
					tooltips[0].text += value == "[No value]" ? $" {value}" : value;
				}
			}
		}
	}

	internal class DeconItemInfo : ItemInfo
	{
		public bool addValueTooltip = false;

		public override ItemInfo Clone()
		{
			var clone = new DeconItemInfo { addValueTooltip = this.addValueTooltip };
			return clone;
		}
	}
}
