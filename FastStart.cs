using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FastStart
{
    /*
          Made by Gorateron, (c) 2016 Gorateron
    */

    public class FastStart : Mod
	{
		public FastStart()
		{
			Properties = new ModProperties()
			{
				Autoload = true,
				AutoloadGores = true,
				AutoloadSounds = true
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

        public static int[] Misc2 = new int[Misc.Length];

        /*public static int[,] Extra = new int[,]
        {
            { ItemID.LifeCrystal, Main.rand.Next(4) },
            { ItemID.ManaCrystal, Main.rand.Next(4) },
            { ItemID.WoodenArrow, Main.rand.Next(60, 251) },
            { ItemID.Shuriken, Main.rand.Next(33, 93) },
            { ItemID.ThrowingKnife, Main.rand.Next(33, 93) },
            { ItemID.LesserHealingPotion, Main.rand.Next(3, 7) },
            { ItemID.LesserManaPotion, Main.rand.Next(3, 7) }
        };

        public static int[,] Armor = new int[,]
        {
            { ItemID.GoldHelmet, 1 },
            { ItemID.PlatinumHelmet, 1 },
            { ItemID.IronHelmet, 1 },
            { ItemID.LeadChainmail, 1 },
            { ItemID.CactusBreastplate, 1 },
            { ItemID.GoldGreaves, 1 },
            { ItemID.EbonwoodGreaves, 1 }
        };

        public static int[,] Weapon = new int[,]
        {
            { ItemID.CrystalVileShard, 1 },
            { ItemID.BeeGun, 1 },
            { ItemID.HornetStaff, 1 },
            { ItemID.SlimeStaff, 1 },
            { ItemID.EnchantedBoomerang, 1 },
            { ItemID.GoldShortsword, 1 },
            { ItemID.SilverShortsword, 1 },
            { ItemID.LeadShortsword, 1 },
            { ItemID.IronShortsword, 1 },
            { ItemID.PlatinumShortsword, 1 }
        };

        public static int[,] Accessory = new int[,]
        {
            { ItemID.ShinyRedBalloon, 1 },
            { ItemID.CloudinaBottle, 1 },
            { ItemID.Hook, 1 },
            { ItemID.HermesBoots, 1 },
            { ItemID.LuckyHorseshoe, 1},
            { ItemID.MoneyTrough, 1 }
        };

        public static int[,] Gift = new int[,]
        {
            { ItemID.WinterCape, 1 },
            { ItemID.MysteriousCape, 1 },
            { ItemID.RedCape, 1 },
            { ItemID.CrimsonCloak, 1 },
            { ItemID.DiamondRing, 1 },
            { ItemID.AngelHalo, 1 },
            { ItemID.GingerBeard, 1 }
        };
        */

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
            Misc2[0] = Main.rand.Next(4);
            Misc2[1] = Main.rand.Next(4);
            Misc2[2] = Main.rand.Next(60, 251);
            Misc2[3] = Main.rand.Next(20, 67);
            Misc2[4] = Main.rand.Next(20, 67);
            Misc2[5] = Main.rand.Next(4);
            Misc2[6] = Main.rand.Next(4);
            Misc2[7] = 3;

            PickaxeToolList = PickaxeTool.ToList<int>();
            AxeToolList = AxeTool.ToList<int>();
            HammerToolList = HammerTool.ToList<int>();

            ArmorTopList = ArmorTop.ToList<int>();
            ArmorMiddleList = ArmorMiddle.ToList<int>();
            ArmorBottomList = ArmorBottom.ToList<int>();

            WeaponList = Weapon.ToList<int>();
            WeaponExtraList = WeaponExtra.ToList<int>();

            GiftList = Gift.ToList<int>();
            AccessoryList = Accessory.ToList<int>();
            MiscList = Misc.ToList<int>();
            Misc2List = Misc2.ToList<int>();
        }

        public static void AddToEntries(int entry, int stack)
        {
            EntryList.Add(new Tuple<int, int>(entry, stack));
        }

        public static IList<Item> AddAllEntries(IList<Item> items)
        {
            IList<Item> returnList = items;
            for (int i = 0; i < EntryList.Select(t=>t.Item1).Count(); i++)
            {
                Item entryItem = new Item();
                entryItem.SetDefaults(EntryList[i].Item1);
                entryItem.stack = EntryList[i].Item2;
                returnList.Add(entryItem);
            }
            return returnList;
        }
        
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
                items.Clear();
                SetUpLists();

                int entryIndex;

                //Pickaxe
                entryIndex = Main.rand.Next(PickaxeToolList.Count);
                AddToEntries(PickaxeToolList.ElementAtOrDefault(entryIndex), 1);
                RemoveEntries(0, entryIndex);

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
                        default:
                            break;
                    }

                    if (addItem)
                    {
                        AddToEntries(MiscList.ElementAtOrDefault(entryIndex), Misc2List.ElementAtOrDefault(entryIndex));
                    }
                }

                AddToEntries(MiscList.ElementAtOrDefault(7), Misc2List.ElementAtOrDefault(7));

                items = AddAllEntries(items);
                EntryList.Clear();

                /*for (int i = 0; i < 7; i++)
                {
                    distItem = new Item();
                    entryIndex = i;
                    bool addItem = true;

                    switch (i)
                    { 
                        case 1:
                        case 4:
                        case 6:
                            if ((Misc[entryIndex, 1] - Misc[entryIndex - 1, 1]) <= 0)
                            {
                                addItem = false;
                            }
                            break;
                        default:
                            break;
                    }
                    if (addItem)
                    {
                        AddToEntries(Misc[entryIndex, 0], Misc[entryIndex, 1]);
                    }
                }*/



                /*for (int i = 0; i < 3; i++)
                {
                    item = new Item();
                    int index;
                    switch (i)
                    {
                        case 0:
                            index = Main.rand.Next(i, i + 4);
                            break;
                        case 1:
                            index = Main.rand.Next(i + 3, FastStart.Tool.GetLength(0) - 5);
                            break;
                        case 2:
                            index = Main.rand.Next(i * 5 - 1, FastStart.Tool.GetLength(0));
                            break;
                        default:
                            index = 0;
                            break;
                    }
                    item.SetDefaults(FastStart.Tool[index, 0]);
                    item.stack = FastStart.Tool[index, 1];
                    giveItems.Add(new Tuple<Item, int>(item, item.stack));
                }

                for (int i = 0; i < 3; i++)
                {
                    item = new Item();
                    int index;
                    bool addItem = true;
                    switch (i)
                    {
                        case 0:
                            index = 0;
                            break;
                        case 1:
                            index = 1;
                            if ((FastStart.Extra[index, 1] - FastStart.Extra[0, 1]) <= 0)
                            {
                                addItem = false;
                            }
                            break;
                        case 2:
                            index = i;
                            break;
                        default:
                            index = 0;
                            break;
                    }
                    if (addItem)
                    {
                        item.SetDefaults(FastStart.Extra[index, 0]);
                        item.stack = FastStart.Extra[index, 1];
                        giveItems.Add(new Tuple<Item, int>(item, item.stack));
                    }
                }

                /*int give1 = Main.rand.Next(1, 3);
                item.SetDefaults(FastStart.Extra[give1 - 1, 0]);
                item.stack = FastStart.Extra[give1 - 1, 1];
                giveItems.Add(new Tuple<Item, int>(item, item.stack));
                if ((FastStart.Extra[Math.Abs(give1 % 2 - 1), 1] - FastStart.Extra[give1 % 2, 1]) > 0)
                {
                    item.SetDefaults(FastStart.Extra[Math.Abs(give1 % 2 - 1), 0]);
                    item.stack = FastStart.Extra[Math.Abs(give1 % 2 - 1), 1];
                    giveItems.Add(new Tuple<Item, int>(item, item.stack));
                }*/

                /*
                for (int i = 0; i < 4; i++)
                {
                    item = new Item();
                    int index;
                    bool addItem = true;
                    switch (i)
                    {
                        case 0:
                            index = i + 3;
                            break;
                        case 1:
                            index = i + 3;
                            if ((FastStart.Extra[index, 1] - FastStart.Extra[3, 1]) <= 0)
                            {
                                addItem = false;
                            }
                            break;
                        case 2:
                            index = i * 2 + 1;
                            break;
                        case 3:
                            index = i * 2;
                            if ((FastStart.Extra[index, 1] - FastStart.Extra[5, 1]) <= 0)
                            {
                                addItem = false;
                            }
                            break;
                        default:
                            index = 0;
                            break;
                    }
                    if (addItem)
                    {
                        item.SetDefaults(FastStart.Extra[index, 0]);
                        item.stack = FastStart.Extra[index, 1];
                        giveItems.Add(new Tuple<Item, int>(item, item.stack));
                    }
                }

                for (int i = 0; i < 3; i++)
                {
                    item = new Item();
                    int index;
                    switch (i)
                    {
                        case 0:
                            index = Main.rand.Next(i, i + 3);
                            break;
                        case 1:
                            index = Main.rand.Next(i + 2, FastStart.Armor.GetLength(0) - 2);
                            break;
                        case 2:
                            index = Main.rand.Next(i * 2 + 1, FastStart.Tool.GetLength(0));
                            break;
                        default:
                            index = 0;
                            break;
                    }
                    item.SetDefaults(FastStart.Armor[index, 0]);
                    item.stack = FastStart.Armor[index, 1];
                    giveItems.Add(new Tuple<Item, int>(item, item.stack));
                }

                for (int i = 0; i < 2; i++)
                {
                    item = new Item();
                    int index;
                    switch (i)
                    {
                        case 0:
                            index = Main.rand.Next(i, i + 5);
                            break;
                        case 1:
                            index = Main.rand.Next(i + 4, FastStart.Weapon.GetLength(0));
                            break;
                        default:
                            index = 0;
                            break;
                    }
                    item.SetDefaults(FastStart.Weapon[index, 0]);
                    item.stack = FastStart.Weapon[index, 1];
                    giveItems.Add(new Tuple<Item, int>(item, item.stack));
                }

                item = new Item();
                int useIndex = Main.rand.Next(0, FastStart.Accessory.GetLength(0));
                item.SetDefaults(FastStart.Accessory[useIndex, 0]);
                item.stack = FastStart.Accessory[useIndex, 1];
                giveItems.Add(new Tuple<Item, int>(item, item.stack));

                for (int i = 0; i < 2; i++)
                {
                    item = new Item();
                    int index;
                    switch (i)
                    {
                        case 0:
                            index = Main.rand.Next(i, i + 4);
                            break;
                        case 1:
                            index = Main.rand.Next(i + 3, FastStart.Weapon.GetLength(0));
                            break;
                        default:
                            index = 0;
                            break;
                    }
                    item.SetDefaults(FastStart.Gift[index, 0]);
                    item.stack = FastStart.Gift[index, 1];
                    giveItems.Add(new Tuple<Item, int>(item, item.stack));
                }

                foreach (var giveItem in giveItems)
                {
                    items.Add(giveItem.Item1);
                } */
                //items.Add(item);
            }
            catch (Exception e)
            {
                ErrorLogger.Log(e.ToString());
            }

        }
    }

}
