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
	public class BouncyCoins : Mod
	{

        // (c) gorateron/jofairden
        // version 0.2

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
                string message = $"\nBouncy Coins uses a functionality only present in tModLoader version {reqVer} or higher. Please update tModLoader to use this mod.\n\n";
                throw new Exception(message);
            }
        }
    }

	public class CoinItem : GlobalItem
	{
		public delegate void keyFrameActionDelegate(int whoAmI);

		private keyFrameActionDelegate coinKeyFrameAction => GenerateBasicKeyFrameActionDelegate(1, 5, 7);

		public static keyFrameActionDelegate GenerateBasicKeyFrameActionDelegate(int frameIncrement, int maxFrameCount, int maxFrames)
		{
			return delegate(int whoAmI)
			{
				Main.itemFrameCounter[whoAmI] += frameIncrement;
				if (Main.itemFrameCounter[whoAmI] > maxFrameCount)
				{
					Main.itemFrameCounter[whoAmI] = 0;
					Main.itemFrame[whoAmI] += 1;
				}
				if (Main.itemFrame[whoAmI] > maxFrames)
				{
					Main.itemFrame[whoAmI] = 0;
				}
			};
		}

	    public static void AddNewItem(int type, keyFrameActionDelegate keyFrameAction)
	    {
			if (bouncyItems.Contains(type)) return;
		    bouncyItems.Add(type);
		    keyFrameActions.Add(type, keyFrameAction);
	    }

	    public static void RemoveItem(int type)
	    {
			if (!bouncyItems.Contains(type)) return;
		    bouncyItems.Remove(type);
		    keyFrameActions.Remove(type);
	    }

		public static bool bounceEvenly = false;
		public static bool disallowModItems = false;
	    public static List<int> bouncyItems = new List<int>(new int[]
	    {
		    71,
		    72,
		    73,
		    74
	    }
	    );
	    public static float amplitude = 5f; // amp of bounce
	    public static float speed = 0.05f; // total speed of bounce
	    public static float universalOffset = 0f; // some offset added to angle
	    public static Dictionary<int, keyFrameActionDelegate> keyFrameActions = new Dictionary<int, keyFrameActionDelegate>();

		// Bouncy coins <3
		public override bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
			if ((disallowModItems && item.modItem == null) || !bouncyItems.Contains(item.type) || item.velocity.Length() > 0f)
				return base.PreDrawInWorld(item, spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);

			Texture2D texture = Main.itemTexture[item.type];
            Texture2D animTexture = Main.coinTexture[item.type - 71];
            rotation = item.velocity.X * 0.2f;
            float offsetY = item.height - texture.Height;
            float offsetX = item.width * 0.5f - texture.Width * 0.5f;
			int frameHeight = animTexture.Height / 8;
			int angle = bounceEvenly ? (int)Main.time : whoAmI % 60 + item.spawnTime;
	        angle += (int)universalOffset;

	        if (keyFrameActions.ContainsKey(item.type) && keyFrameActions[item.type] != null)
		        keyFrameActions[item.type].Invoke(whoAmI);
	        else coinKeyFrameAction(whoAmI);

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
    }
}
