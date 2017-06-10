using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using System.Collections.Generic;
using Terraria.ModLoader.IO;

namespace BouncyCoins
{
	public class BouncyCoins : Mod
	{

		// (c) gorateron/jofairden
		// version 0.1.2.3

		public static int[] VanillaCoinSet =
		{
			71,72,73,74
		};
		public static BouncyCoins instance { get; protected set; }

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
			instance = this;
			Version reqVer = new Version(0, 8, 3, 5);
			if (ModLoader.version >= reqVer) return;
			string message = $"\nBouncy Coins uses a functionality only present in tModLoader version {reqVer} or higher. Please update tModLoader to use this mod.\n\n";
			throw new Exception(message);
		}
	}

	public class CoinItem : GlobalItem
	{
		internal static bool IsVanillaCoin(int type)
			=> BouncyCoins.VanillaCoinSet.Any(x => x == type);

		// Bouncy coins <3
		public override bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
		{
			var player = CoinPlayer.GetModPlayer(Main.LocalPlayer);

			if ((player.disallowModItems && item.modItem == null)
				|| !player.bouncyItems.Contains(item.type)
				|| item.velocity.Length() > 0f)
				return true;

			Texture2D texture = Main.itemTexture[item.type];
			var keyFrame = (player.keyFrameActions.ContainsKey(item.type) && player.keyFrameActions[item.type] != null)
				? player.keyFrameActions[item.type].Invoke(item, whoAmI)
				: CoinPlayer.coinKeyFrameAction(item, whoAmI);

			Texture2D animTexture = keyFrame.AnimationTexture;
			rotation = keyFrame.Rotation;
			float offsetY = item.height - texture.Height;
			float offsetX = item.width * 0.5f - texture.Width * 0.5f;
			int frameHeight = keyFrame.FrameHeight;
			int angle = player.bounceEvenly ? (int)Main.time : whoAmI % 60 + item.spawnTime;
			//angle += (int)player.universalOffset;

			Rectangle? frameRect = new Rectangle?(
				new Rectangle(
					0,
					Main.itemFrame[whoAmI] * frameHeight + 1,
					texture.Width,
					frameHeight
				));

			var math_offset = player.amplitude * (float)Math.Cos(angle * player.speed);
			Vector2 center = new Vector2(0f, frameHeight * 0.5f);
			Vector2 offset = new Vector2(0f, math_offset - player.amplitude + player.universalOffset);

			Vector2 pos = new Vector2(item.position.X - Main.screenPosition.X + animTexture.Width * 0.5f + offsetX,
							item.position.Y - Main.screenPosition.Y + frameHeight * 0.5f + offsetY)
						  - center + offset;

			Vector2 origin = new Vector2(texture.Width * 0.5f, frameHeight * 0.5f);

			Main.spriteBatch.Draw(animTexture, pos, frameRect, alphaColor, rotation, origin, scale, SpriteEffects.None, 0f);
			return false;
		}
	}

	public class CoinPlayer : ModPlayer
	{
		public static CoinPlayer GetModPlayer(Player player) => player.GetModPlayer<CoinPlayer>(BouncyCoins.instance);

		public delegate KeyFrameActionResult keyFrameActionDelegate(Item item, int whoAmI);

		internal static keyFrameActionDelegate coinKeyFrameAction => GenerateBasicKeyFrameActionDelegate(1, 5, 8);

		public class KeyFrameActionResult
		{
			public KeyFrameActionResult(Texture2D text, float rot, int height)
			{
				AnimationTexture = text;
				Rotation = rot;
				FrameHeight = height;
			}

			public Texture2D AnimationTexture { get; protected set; }
			public float Rotation { get; protected set; }
			public int FrameHeight { get; protected set; }
		}
		/// <summary>
		/// Will generate a basic keyframeaction delegate for you, but if it's not a vanilla coin the offset returned will be 0.
		/// </summary>
		/// <param name="frameIncrement">How fast the itemFrameCounter increments</param>
		/// <param name="maxFrameCount">The amount of frames itemFrameCounter will count to</param>
		/// <param name="maxFrames">The maximum amount of frames</param>
		/// <returns></returns>
		public static keyFrameActionDelegate GenerateBasicKeyFrameActionDelegate(int frameIncrement, int maxFrameCount, int maxFrames)
		{
			return delegate (Item item, int whoAmI)
			{
				Main.itemFrameCounter[whoAmI] += frameIncrement;
				if (Main.itemFrameCounter[whoAmI] > maxFrameCount)
				{
					Main.itemFrameCounter[whoAmI] = 0;
					Main.itemFrame[whoAmI] = (Main.itemFrame[whoAmI] + 1) % maxFrames;
				}
				return CoinItem.IsVanillaCoin(item.type) 
				? new KeyFrameActionResult(Main.coinTexture[item.type - 71], item.velocity.X* 0.2f, Main.coinTexture[item.type - 71].Height / maxFrames)
				: new KeyFrameActionResult(Main.itemTexture[item.type], 0f, Main.itemTexture[item.type].Height / maxFrames);
			};
		}

		public void AddBouncyItem(int type, keyFrameActionDelegate keyFrameAction)
		{
			if (bouncyItems.Contains(type)) return;
			bouncyItems.Add(type);
			keyFrameActions.Add(type, keyFrameAction);
		}

		public bool RemoveBouncyItem(int type)
		{
			if (!bouncyItems.Contains(type)) return false;
			bool b = bouncyItems.Remove(type);
			return keyFrameActions.Remove(type) && b;
		}

		internal bool bounceEvenly; // bounce evenly?
		internal bool disallowModItems; // do not bounce moditems?
		internal List<int> bouncyItems; // list of items that will bounce
		internal float amplitude; // amp of bounce
		internal float speed; // total speed of bounce
		internal float universalOffset; // some offset added to angle
		internal Dictionary<int, keyFrameActionDelegate> keyFrameActions; // keyframing

		public override void Initialize()
		{
			bounceEvenly = false;
			disallowModItems = false;
			bouncyItems = new List<int>(BouncyCoins.VanillaCoinSet);
			amplitude = 5f;
			speed = 0.05f;
			universalOffset = 0f;
			keyFrameActions = new Dictionary<int, keyFrameActionDelegate>();
		}

		public override TagCompound Save()
		{
			return new TagCompound()
			{
				["bounceEvenly"] = bounceEvenly,
				["disallowModItems"] = disallowModItems,
				["bouncyItems"] = new List<int>(bouncyItems),
				["amplitude"] = amplitude,
				["speed"] = speed,
				["universalOffset"] = universalOffset
			};
		}

		public override void Load(TagCompound tag)
		{
			bounceEvenly = tag.GetBool("bounceEvenly");
			disallowModItems = tag.GetBool("disallowModItems");
			bouncyItems = new List<int>(tag.GetList<int>("bouncyItems"));
			amplitude = tag.GetFloat("amplitude");
			speed = tag.GetFloat("speed");
			universalOffset = tag.GetFloat("universalOffset");
		}
	}
}
