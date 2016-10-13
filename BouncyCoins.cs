using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using System.Collections.Generic;

namespace BouncyCoins
{
	class BouncyCoins : Mod
	{

        // (c) gorateron
        // version 0.1.2

		public BouncyCoins()
		{
			Properties = new ModProperties()
			{
				Autoload = true,
				AutoloadGores = true,
				AutoloadSounds = true,
                AutoloadBackgrounds = true
			};
		}

        public override void Load()
        {
            Version reqVer = new Version(0, 8, 3, 5);
            if (ModLoader.version < reqVer)
            {
                string message = "\nBouncy Coins uses a functionality only present in tModLoader version " + reqVer.ToString() + " or higher. Please update tModLoader to use this mod.\n\n";
                throw new Exception(message);
            }
        }
    }

    class CoinItem : GlobalItem
    {
        // Bouncy coins <3
        public override bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            if (item.modItem == null && item.type >= 71 && item.type <= 74)
            {
                Texture2D texture = Main.itemTexture[item.type];
                Texture2D animTexture = Main.coinTexture[item.type - 71];
                rotation = item.velocity.X * 0.2f;
                float offsetY = item.height - texture.Height;
                float offsetX = item.width * 0.5f - texture.Width * 0.5f;
                DoCoinAnimation(whoAmI);
                int frameHeight = animTexture.Height / 8;
                const float amplitude = 5f;
                const float speed = 0.05f;
                int angle = whoAmI % 60 + item.spawnTime;
                Rectangle? frameRect = new Rectangle?(
                                                      new Rectangle(
                                                        0, 
                                                        Main.itemFrame[whoAmI] * frameHeight + 1, 
                                                        texture.Width, 
                                                        frameHeight
                                                      ));

                Vector2 center = new Vector2(0f, 
                                             frameHeight * 0.5f);

                Vector2 offset = new Vector2(0f, 
                                             amplitude * (float)Math.Cos(angle * speed));

                Vector2 pos = new Vector2(item.position.X - Main.screenPosition.X + animTexture.Width * 0.5f + offsetX, 
                                          item.position.Y - Main.screenPosition.Y + frameHeight * 0.5f + offsetY) 
                                          - center + offset;

                Vector2 origin = new Vector2(texture.Width * 0.5f, 
                                             frameHeight * 0.5f);

                Main.spriteBatch.Draw(animTexture, pos, frameRect, alphaColor, rotation, origin, scale, SpriteEffects.None, 0f);
                return false;
            }
            else
                return base.PreDrawInWorld(item, spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);
        }

        private void DoCoinAnimation(int whoAmI)
        {
            Main.itemFrameCounter[whoAmI] += 1;
            if (Main.itemFrameCounter[whoAmI] > 5)
            {
                Main.itemFrameCounter[whoAmI] = 0;
                Main.itemFrame[whoAmI] += 1;
            }
            if (Main.itemFrame[whoAmI] > 7)
            {
                Main.itemFrame[whoAmI] = 0;
            }
        }
    }
}
