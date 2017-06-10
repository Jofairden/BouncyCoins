using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace TheDeconstructor
{
	internal class DeconGlobalItem : GlobalItem
	{
		public bool addValueTooltip = false;

		public override void NetSend(Item item, BinaryWriter writer)
		{
			writer.Write(addValueTooltip);
		}

		public override void NetReceive(Item item, BinaryReader reader)
		{
			item.GetGlobalItem<DeconGlobalItem>(mod).addValueTooltip = reader.ReadBoolean();
		}

		public override bool CloneNewInstances => true;
		public override bool InstancePerEntity => true;

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			if (!addValueTooltip) return;
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
