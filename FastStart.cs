using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FastStart
{
	/*
        Made by Jofairden, (c) 2017
		Sorry for the lack of documentation

		Small update 6-1-2017
		Fix FastStart for newer version

		3-20-2017
		v 0.4
    */

	public class FastStart : Mod
	{
		public FastStart()
		{
			Properties = new ModProperties()
			{
				Autoload = true,
			};
		}
	}

	public class FastStartPlayer : ModPlayer
	{
		public static List<Tuple<int, int>> EntryList = new List<Tuple<int, int>>();

		public static int[] PickaxeTool = new int[]
		{
			ItemID.IronPickaxe,
			ItemID.LeadPickaxe,
			ItemID.SilverPickaxe,
			ItemID.GoldPickaxe,
			ItemID.PlatinumPickaxe
		};

		public static int[] AxeTool = new int[]
		{
			ItemID.IronAxe,
			ItemID.LeadAxe,
			ItemID.SilverAxe,
			ItemID.GoldAxe,
			ItemID.PlatinumAxe
		};

		public static int[] HammerTool = new int[]
		{
			ItemID.IronHammer,
			ItemID.LeadHammer,
			ItemID.SilverHammer,
			ItemID.GoldHammer,
			ItemID.PlatinumHammer
		};

		public static int[] ArmorTop = new int[]
		{
			ItemID.IronHelmet,
			ItemID.LeadHelmet,
			ItemID.SilverHelmet,
			ItemID.GoldHelmet,
			ItemID.PlatinumHelmet
		};

		public static int[] ArmorMiddle = new int[]
		{
			ItemID.IronChainmail,
			ItemID.LeadChainmail,
			ItemID.SilverChainmail,
			ItemID.GoldChainmail,
			ItemID.PlatinumChainmail
		};

		public static int[] ArmorBottom = new int[]
		{
			ItemID.IronGreaves,
			ItemID.LeadGreaves,
			ItemID.SilverGreaves,
			ItemID.GoldGreaves,
			ItemID.PlatinumGreaves
		};

		public static int[] Weapon = new int[]
		{
			ItemID.IronShortsword,
			ItemID.LeadShortsword,
			ItemID.SilverShortsword,
			ItemID.GoldShortsword,
			ItemID.PlatinumShortsword
		};

		public static int[] WeaponExtra = new int[]
		{
			ItemID.EnchantedBoomerang,
			ItemID.BeeGun,
			ItemID.SlimeStaff,
			ItemID.HornetStaff,
			ItemID.CrystalVileShard
		};

		public static int[] Gift = new int[]
		{
			 ItemID.WinterCape,
			 ItemID.MysteriousCape,
			 ItemID.RedCape,
			 ItemID.CrimsonCloak,
			 ItemID.DiamondRing,
			 ItemID.AngelHalo,
			 ItemID.GingerBeard,
		};

		public static int[] Accessory = new int[]
		{
			ItemID.ShinyRedBalloon,
			ItemID.CloudinaBottle,
			ItemID.GrapplingHook,
			ItemID.HermesBoots,
			ItemID.LuckyHorseshoe,
			ItemID.MoneyTrough
		};

		public static int[] Misc = new int[]
		{
			ItemID.LifeCrystal,
			ItemID.ManaCrystal,
			ItemID.WoodenArrow,
			ItemID.Shuriken,
			ItemID.ThrowingKnife,
			ItemID.LesserHealingPotion,
			ItemID.LesserManaPotion,
			ItemID.RecallPotion
		};

		public static int[] MiscStacks = new int[Misc.Length];
		public static List<int> PickaxeToolList;
		public static List<int> AxeToolList;
		public static List<int> HammerToolList;

		public static List<int> ArmorTopList;
		public static List<int> ArmorMiddleList;
		public static List<int> ArmorBottomList;

		public static List<int> WeaponList;
		public static List<int> WeaponExtraList;

		public static List<int> GiftList;
		public static List<int> AccessoryList;
		public static List<int> MiscList;
		public static List<int> Misc2List;

		public static void SetUpLists()
		{
			MiscStacks[0] = Main.rand.Next(4);
			MiscStacks[1] = Main.rand.Next(4);
			MiscStacks[2] = Main.rand.Next(60, 251);
			MiscStacks[3] = Main.rand.Next(20, 67);
			MiscStacks[4] = Main.rand.Next(20, 67);
			MiscStacks[5] = Main.rand.Next(4);
			MiscStacks[6] = Main.rand.Next(4);
			MiscStacks[7] = 3;

			PickaxeToolList = PickaxeTool.ToList();
			AxeToolList = AxeTool.ToList();
			HammerToolList = HammerTool.ToList();

			ArmorTopList = ArmorTop.ToList();
			ArmorMiddleList = ArmorMiddle.ToList();
			ArmorBottomList = ArmorBottom.ToList();

			WeaponList = Weapon.ToList();
			WeaponExtraList = WeaponExtra.ToList();

			GiftList = Gift.ToList();
			AccessoryList = Accessory.ToList();
			MiscList = Misc.ToList();
			Misc2List = MiscStacks.ToList();
		}

		public static void AddToEntries(int entry, int stack)
		{
			EntryList.Add(new Tuple<int, int>(entry, stack));
		}

		// Add all items which were added  to the entry list, the user's inventory item list
		public static void AddAllEntries(ref IList<Item> items)
		{
			foreach (Tuple<int, int> tuple in EntryList)
			{
				Item entryItem = new Item();
				entryItem.SetDefaults(tuple.Item1);
				entryItem.stack = tuple.Item2;
				items.Add(entryItem);
			}
		}

		// You get a platinum pickaxe?
		// Can't get platinum axe/hammer
		// Get a copper top? Can't get copper middle/bottom
		// And so on..
		// Makes our inventory truly random
		public static void RemoveEntries(int entryType, int entryIndex)
		{
			if (entryType == 0)
			{
				PickaxeToolList.RemoveAt(entryIndex);
				AxeToolList.RemoveAt(entryIndex);
				HammerToolList.RemoveAt(entryIndex);
			}
			else if (entryType == 1)
			{
				ArmorTopList.RemoveAt(entryIndex);
				ArmorMiddleList.RemoveAt(entryIndex);
				ArmorBottomList.RemoveAt(entryIndex);
			}
			else if (entryType == 2)
			{
				WeaponList.RemoveAt(entryIndex);
				WeaponExtraList.RemoveAt(entryIndex);
			}
			else if (entryType == 3)
			{
				GiftList.RemoveAt(entryIndex);
			}
			else if (entryType == 4)
			{
				AccessoryList.RemoveAt(entryIndex);
			}
		}

		public override void SetupStartInventory(IList<Item> items)
		{
			try
			{
				items.Clear(); // Remove default vanilla items
				SetUpLists(); // Setup (sort of ctor)

				// Grab random
				// Pickaxe
				var entryIndex = Main.rand.Next(PickaxeToolList.Count); // Select a random index from the list
				AddToEntries(PickaxeToolList.ElementAtOrDefault(entryIndex), 1); // Add to the selected entry to the entry list
				RemoveEntries(0, entryIndex); // Remove from other entries

				//Axe
				entryIndex = Main.rand.Next(AxeToolList.Count);
				AddToEntries(AxeToolList.ElementAtOrDefault(entryIndex), 1);
				RemoveEntries(0, entryIndex);

				//Hammer
				entryIndex = Main.rand.Next(HammerToolList.Count);
				AddToEntries(HammerToolList.ElementAtOrDefault(entryIndex), 1);
				RemoveEntries(0, entryIndex);

				//ArmorTop
				entryIndex = Main.rand.Next(ArmorTopList.Count);
				AddToEntries(ArmorTopList.ElementAtOrDefault(entryIndex), 1);
				RemoveEntries(1, entryIndex);

				//ArmorMiddle
				entryIndex = Main.rand.Next(ArmorMiddleList.Count);
				AddToEntries(ArmorMiddleList.ElementAtOrDefault(entryIndex), 1);
				RemoveEntries(1, entryIndex);

				//ArmorBottom
				entryIndex = Main.rand.Next(ArmorBottomList.Count);
				AddToEntries(ArmorBottomList.ElementAtOrDefault(entryIndex), 1);
				RemoveEntries(1, entryIndex);

				//Weapon
				entryIndex = Main.rand.Next(WeaponList.Count);
				AddToEntries(WeaponList.ElementAtOrDefault(entryIndex), 1);
				RemoveEntries(2, entryIndex);

				//Weapon
				entryIndex = Main.rand.Next(WeaponExtraList.Count);
				AddToEntries(WeaponExtraList.ElementAtOrDefault(entryIndex), 1);
				RemoveEntries(2, entryIndex);

				//Gift
				entryIndex = Main.rand.Next(GiftList.Count);
				AddToEntries(GiftList.ElementAtOrDefault(entryIndex), 1);
				RemoveEntries(3, entryIndex);
				entryIndex = Main.rand.Next(GiftList.Count);
				AddToEntries(GiftList.ElementAtOrDefault(entryIndex), 1);
				RemoveEntries(3, entryIndex);

				//Accessory
				entryIndex = Main.rand.Next(AccessoryList.Count);
				AddToEntries(AccessoryList.ElementAtOrDefault(entryIndex), 1);
				RemoveEntries(4, entryIndex);

				for (int i = 0; i < MiscList.Count - 2; i++)
				{
					entryIndex = i;
					bool addItem = true;

					switch (i)
					{
						case 1:
						case 4:
						case 6:
							if ((Misc2List[entryIndex] - Misc2List[entryIndex - 1]) < 0)
							{
								addItem = false;
							}
							else if ((Misc2List[entryIndex] - Misc2List[entryIndex - 1]) == 0)
							{
								Misc2List[entryIndex] = 1;
							}
							break;
					}

					if (addItem)
					{
						AddToEntries(MiscList.ElementAtOrDefault(entryIndex), Misc2List.ElementAtOrDefault(entryIndex));
					}
				}

				AddToEntries(MiscList.ElementAtOrDefault(7), Misc2List.ElementAtOrDefault(7));

				AddAllEntries(ref items);
				EntryList.Clear();
			}
			catch (Exception e)
			{
				ErrorLogger.Log(e.ToString());
			}

		}
	}

}
