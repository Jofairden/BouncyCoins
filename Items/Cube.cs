using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace TheDeconstructor.Items
{
	internal abstract class Cube : ModItem
	{
		internal Item SealedSource { get; set; } = new Item();
		internal List<Item> SealedItems { get; set; } = new List<Item>();
		internal bool CanFail { get; set; }
		internal CubeState? State { get; set; } = CubeState.Open;

		public enum CubeState
		{
			Sealed,
			Open
		}

		// Sync infos for multiplayer
		public override void NetSend(BinaryWriter writer)
		{
			writer.WriteItem(SealedSource, true);
			writer.WriteVarInt(SealedItems.Count);
			for (int i = 0; i < SealedItems.Count; i++)
			{
				writer.WriteItem(SealedItems[i], true);
			}
			writer.Write(CanFail);
			writer.WriteVarInt((int)State);
		}

		public override void NetRecieve(BinaryReader reader)
		{
			SealedSource = reader.ReadItem(true).Clone();
			int count = reader.ReadVarInt();
			SealedItems = new List<Item>();
			for (int i = 0; i < count; i++)
			{
				SealedItems.Add(reader.ReadItem(true).Clone());
			}
			CanFail = reader.ReadBoolean();
			State = (CubeState)reader.ReadVarInt();
		}

		public override bool CloneNewInstances =>
			true;

		public override void SetDefaults()
		{
			item.name = "Cube";
			item.rare = 9;
			item.maxStack = 1;
			item.value = 1;
			item.width = 20;
			item.height = 28;
			ItemID.Sets.ItemNoGravity[item.type] = true;
		}

		public override DrawAnimation GetAnimation()
		{
			return new DrawAnimationVertical(4, 8);
		}

		public virtual TagCompound CubeSave()
		{
			var tc = new TagCompound
			{
				["SealedItems"] = SealedItems.Select(ItemIO.Save).ToList(),
				["SealedSource"] = ItemIO.Save(SealedSource),
				["CanFail"] = CanFail,
				["State"] = (int?)State ?? 0,
				["Value"] = item.value
			};
			return tc;
		}

		public override TagCompound Save() =>
			CubeSave();

		public virtual void CubeLoad(TagCompound tag)
		{
			var list = tag.GetList<TagCompound>("SealedItems").ToList();
			list.ForEach(x => SealedItems.Add(ItemIO.Load(x)));
			SealedSource = ItemIO.Load(tag.GetCompound("SealedSource"));
			CanFail = tag.GetBool("CanFail");
			State = (CubeState)tag.GetInt("State");
			item.value = tag.GetInt("Value");
		}

		public override void Load(TagCompound tag) =>
			CubeLoad(tag);

		public override void OnCraft(Recipe recipe) =>
			SoundHelper.PlaySound(SoundHelper.SoundType.Receive);

		public override bool CanRightClick() =>
			true;

		public override void RightClick(Player player)
		{
			if (!State.HasValue) return;
			bool isQueer = item.modItem is QueerLunarCube;

			if (State.Value == CubeState.Open)
			{
				if (TheDeconstructor.instance.deconGUI.visible)
				{
					TheDeconstructor.instance.deconGUI.TryGetCube(true);
					TheDeconstructor.instance.deconGUI.TryPutInCube(isQueer);
				}
				else // Prevent consume
					item.stack = 2;
			}
			else if (State.Value == CubeState.Sealed)
			{
				if (TheDeconstructor.instance.deconGUI.visible)
				{
					item.stack = 2;
					return;
				}
				if (SealedItems != null
					&& SealedItems.Count >= 1)
				{
					// Need to figure out a way how to reset weapon prefixes
					SoundHelper.PlaySound(SoundHelper.SoundType.Redeem);
					//Item giveItem = new Item();
					foreach (var infoBagItem in SealedItems)
					{
						if (infoBagItem.type == 0) break;
						//giveItem.SetDefaults(infoBagItem.type);
						//var givenItem = player.GetItem(player.whoAmI, giveItem);
						//givenItem.Prefix(0);

						//the UI can add materials beyond their maxStack.
						//so this should ensure items are given in multiple stacks if they exceed their maxStack
						int stackDiff = Math.Max(1,
							(int)Math.Floor((double)(infoBagItem.stack / (double)infoBagItem.maxStack)));
						int useStack = stackDiff > 1 ? infoBagItem.maxStack : infoBagItem.stack;
						int leftOver = stackDiff > 1 ? infoBagItem.stack - useStack * stackDiff : 0;
						var giveItem = infoBagItem.Clone();
						for (int i = 0; i < stackDiff; i++)
						{
							if (CanFail && Main.rand.NextFloat() <= 0.3f)
							{
								NotifyLoss(infoBagItem.type, useStack);
								continue;
							}
							giveItem.stack = useStack;
							Main.LocalPlayer.GiveClonedItem(giveItem, useStack);
							//player.GetItem(Main.LocalPlayer.whoAmI, giveItem);
						}
						if (leftOver > 0)
						{
							if (CanFail && Main.rand.NextFloat() <= 0.3f)
							{
								NotifyLoss(infoBagItem.type, leftOver);
								continue;
							}
							giveItem.stack = leftOver;
							Main.LocalPlayer.GiveClonedItem(giveItem, leftOver);
							//player.GetItem(Main.LocalPlayer.whoAmI, giveItem);
						}
					}
					// Queer cube isn't lost upon unsealing
					if (isQueer)
					{
						item.stack = 2;
						var modItem = item.modItem as Cube;
						modItem.State = CubeState.Open;
						modItem.SealedSource.TurnToAir();
						modItem.SealedItems.Clear();
						modItem.CanFail = false;
						item.value = 1;
					}
				}
				//item.ResetStats(item.type);
				//info = new BagItemInfo();
			}
		}

		public virtual void NotifyLoss(int type, int stack)
		{
			string str = $"[i/s1:{type}] (x{stack}) was lost while unsealing!";
			Main.NewText(str, 255);
			if (Main.netMode == NetmodeID.MultiplayerClient)
				NetMessage.SendData(MessageID.ChatText, -1, -1, str, 255);
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			// Remove regular price tooltip
			var priceTT = tooltips.FirstOrDefault(x => x.mod == "Terraria" & x.Name == "Price");
			if (priceTT != null
				&& State == CubeState.Sealed)
				tooltips.Remove(priceTT);

			// Tooltip
			tooltips.Add(State == CubeState.Sealed
				 ? new TooltipLine(mod, $"{mod.Name}: LunarCube: Title", "Sealed\nRight click to break seal") { overrideColor = Color.Yellow }
				 : new TooltipLine(mod, $"{mod.Name}: LunarCube: Title", "Unsealed") { overrideColor = Color.Yellow });

			// Loss warning
			if (CanFail)
			{
				var tt = new TooltipLine(mod, $"{mod.Name}: LunarCube: Loss Warning",
					$"30% chance of failure")
				{ overrideColor = Colors.RarityRed };
				tooltips.Add(tt);
			}

			// Value
			if (item.value > 1)
			{
				tooltips.Add(new TooltipLine(mod, $"{mod.Name}: LunarCube: Value",
					"Value:" + new ItemValue().SetFromCopperValue(item.value).ToSellValue().ToTagString()));
			}

			// Source item
			if (SealedSource != null
				&& !SealedSource.IsAir)
			{
				tooltips.Add(new TooltipLine(mod, $"{mod.Name}: LunarCube: Source",
						$"Sealed item:[i/s1:{SealedSource.type}][c/{SealedSource.GetTooltipColor().ToHexString().Substring(1)}:{SealedSource.name} ](x{SealedSource.stack})"));
			}

			// Contents
			if (SealedItems.Any<Item>())
			{
				tooltips.Add(new TooltipLine(mod, $"{mod.Name}: LunarCube: Content Title", "Contains:"));
				foreach (var infoBagItem in SealedItems)
				{
					if (!infoBagItem.IsAir)
						tooltips.Add(new TooltipLine(mod, $"{mod.Name}: LunarCube: Content: {infoBagItem.type}",
							$"[i/s1:{infoBagItem.type}][c/{infoBagItem.GetTooltipColor().ToHexString().Substring(1)}:{infoBagItem.name} ](x{infoBagItem.stack})"));
				}
			}

		}

		public virtual Cube CubeClone<T>() where T : Cube, new()
		{
			Cube clone = new T
			{
				SealedSource = (Item)this.SealedSource.Clone(),
				SealedItems = new List<Item>(this.SealedItems),
				CanFail = this.CanFail,
				State = this.State
			};
			return clone;
		}

		public override void PostUpdate()
		{
			Color useColor =
			   item.type == mod.ItemType<QueerLunarCube>()
				   ? Tools.DiscoColor()
				   : Color.White;
			var sine = (float)Math.Sin(Main.essScale * 0.50f);
			var r = 0.05f + 0.35f * sine * useColor.R * 0.01f;
			var g = 0.05f + 0.35f * sine * useColor.G * 0.01f;
			var b = 0.05f + 0.35f * sine * useColor.B * 0.01f;
			Lighting.AddLight(item.Center, new Vector3(r, g, b));
		}
	}
}
