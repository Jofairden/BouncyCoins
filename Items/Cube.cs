using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;


namespace TheDeconstructor.Items
{
    internal abstract class Cube : ModItem
    {
        private Texture2D animTexture => TheDeconstructor.instance.GetTexture(TexturePath);
        public Item SealedSource { get; set; } = new Item();
        public List<Item> SealedItems { get; set; } = new List<Item>();
        public abstract string TexturePath { get; }
        public bool CanFail { get; set; }
        public int InvFC { get; set; }
        public int InvF { get; set; }
        public abstract int InvFMax { get; }
        public CubeState? State { get; set; }

        public enum CubeState
        {
            Sealed,
            Open
        }

        public override bool CloneNewInstances =>
            true;

        public override void SetDefaults()
        {
            item.name = "Cube";
            item.rare = 9;
            item.maxStack = 1;
            item.value = 1;
            item.width = 10;
            item.height = 10;
            ItemID.Sets.ItemNoGravity[item.type] = true;
        }

        public virtual TagCompound CubeSave()
        {
            try
            {
                TagCompound tc = new TagCompound
                {
                    ["items"] = SealedItems.Select(ItemIO.Save).ToList(),
                    ["source"] = ItemIO.Save(SealedSource)
                };
                return tc;
            }
            catch (Exception e)
            {
                ErrorLogger.Log(e.ToString());
            }
            return null;
        }

        public override TagCompound Save() =>
            CubeSave();

        public virtual void CubeLoad(TagCompound tag)
        {
            try
            {
                var list = tag.GetList<TagCompound>("items").ToList();
                list.ForEach(x => SealedItems.Add(ItemIO.Load(x)));
                SealedSource = ItemIO.Load(tag.GetCompound("source"));
            }
            catch (Exception e)
            {
                ErrorLogger.Log(e.ToString());
            }
        }

        public override void Load(TagCompound tag) =>
            CubeLoad(tag);

        public override void RightClick(Player player)
        {
            try
            {
                if (!State.HasValue) return;

                if (State.Value == CubeState.Open)
                {
                    item.stack = 2;
                    // todo
                }
                else if (State.Value == CubeState.Sealed)
                {
                    if (SealedItems != null && SealedItems.Count >= 1)
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
                            for (int i = 0; i < stackDiff; i++)
                            {
                                if (CanFail && Main.rand.NextFloat() <= 0.2f)
                                {
                                    NotifyLoss(infoBagItem.type, useStack);
                                    continue;
                                }
                                player.QuickSpawnItem(infoBagItem.type, useStack);
                            }
                            if (leftOver > 0)
                            {
                                if (CanFail && Main.rand.NextFloat() <= 0.2f)
                                {
                                    NotifyLoss(infoBagItem.type, useStack);
                                    continue;
                                }
                                player.QuickSpawnItem(infoBagItem.type, leftOver);
                            }
                        }
                    }
                    //item.ResetStats(item.type);
                    //info = new BagItemInfo();
                }
            }
            catch (Exception e)
            {
                Main.NewTextMultiline(e.ToString());
            }
        }

        public virtual void NotifyLoss(int type, int stack)
        {
            string str = $"Oh noes! You've lost [i/s1:{type}] (x{stack})!";
            Main.NewText(str, 255);
            if (Main.netMode == NetmodeID.MultiplayerClient)
                NetMessage.SendData(MessageID.ChatText, -1, -1, str, 255);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (SealedSource != null && !SealedSource.IsAir)
            {
                tooltips.Add(new TooltipLine(mod, $"{mod.Name}: LunarCube: Source",
                        $"Source:[i/s1:{SealedSource.type}][c/{SealedSource.GetTooltipColor().ToHexString().Substring(1)}:{SealedSource.name} ](x{SealedSource.stack})"));
            }
            tooltips.Add(new TooltipLine(mod, $"{mod.Name}: LunarCube: Title", "Open the seal to receive:"));
            foreach (var infoBagItem in SealedItems)
            {
                if (!infoBagItem.IsAir)
                    tooltips.Add(new TooltipLine(mod, $"{mod.Name}: LunarCube: Content: {infoBagItem.type}",
                        $"[i/s1:{infoBagItem.type}][c/{infoBagItem.GetTooltipColor().ToHexString().Substring(1)}:{infoBagItem.name} ](x{infoBagItem.stack})"));
            }

            if (CanFail)
            {
                var tt = new TooltipLine(mod, $"{mod.Name}: LunarCube: Loss Warning",
                    $"Chance of material loss") {overrideColor = Colors.RarityRed};
                tooltips.Add(tt);
            }
        }

        public virtual Cube CubeClone<T>() where T : Cube, new()
        {
            var clone = (Cube)new T()
            {
                SealedSource = (Item)this.SealedSource.Clone(),
                SealedItems = new List<Item>(this.SealedItems),
                CanFail = this.CanFail,
                State = this.State
            };
            return clone;
        }

        public override bool CanRightClick() => 
            true;

        public virtual bool CubeDrawInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame,
            Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (InvFC++ >= 4)
            {
                InvF = (InvF + 1) % InvFMax;
                InvFC = 0;
            }
            spriteBatch.Draw(animTexture, position, new Rectangle(0, item.height * InvF, item.width, item.height), drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
            return false;
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame,
            Color drawColor, Color itemColor, Vector2 origin, float scale)
            => CubeDrawInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale);

        public virtual bool CubeDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor,
            ref float rotation, ref float scale, int whoAmI)
        {
            Main.itemFrameCounter[whoAmI]++;
            if (Main.itemFrameCounter[whoAmI] >= 4)
            {
                Main.itemFrame[whoAmI] = (Main.itemFrame[whoAmI] + 1) % InvFMax;
                Main.itemFrameCounter[whoAmI] = 0;
            }
            spriteBatch.Draw(animTexture, item.position - Main.screenPosition, new Rectangle(0, item.height * Main.itemFrame[whoAmI], item.width, item.height), lightColor, 0f, new Vector2(item.width / 2f, item.height / 2f), scale, SpriteEffects.None, 0f);
            return false;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor,
            ref float rotation, ref float scale, int whoAmI) =>
            CubeDrawInWorld(spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);

        public override void PostUpdate()
        {
            var sine = (float)Math.Sin(Main.essScale * 0.50f);
            var r = 0.05f + 0.35f * sine;
            var g = 0.05f + 0.35f * sine;
            var b = 0.05f + 0.35f * sine;
            Lighting.AddLight(item.Center, new Vector3(r, g, b));
        }
    }
}
